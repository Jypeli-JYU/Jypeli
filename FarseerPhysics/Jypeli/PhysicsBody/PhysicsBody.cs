#region MIT License
/*
 * Copyright (c) 2009 University of Jyväskylä, Department of Mathematical
 * Information Technology.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

/*
 * Original Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 * Modified for Farseer engine by Mikko Röyskö
 */

using System;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision.Shapes;
using Jypeli.Farseer;
using Jypeli.Physics;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Jypeli
{
    /// <summary>
    /// Peliolio, joka noudattaa fysiikkamoottorin määräämiä fysiikan lakeja.
    /// Voidaan kuitenkin myös laittaa noudattamaan lakeja valikoidusti.
    /// </summary>
    public partial class PhysicsBody : IPhysicsBody
    {
        /// <summary>
        /// Kappaleen omistajaolio.
        /// </summary>
        public IPhysicsObject Owner { get; internal set; }

        /// <summary>
        /// Luotavien fysiikkakappaleiden oletustiheys.
        /// Oletuksena 0.001.
        /// </summary>
        public static float DefaultDensity { get; set; } = 0.001f;

        /// <summary>
        /// Jättääkö olio painovoiman huomioimatta.
        /// </summary>
        public bool IgnoresGravity
        {
            get { return FSBody.IgnoreGravity; }
            set { FSBody.IgnoreGravity = value; }
        }

        /// <summary>
        /// Jättääkö olio kaikki fysiikkalogiikat (ks. <c>AddPhysicsLogic</c>)
        /// huomiotta. Vaikuttaa esim. painovoimaan, mutta ei törmäyksiin.
        /// </summary>
        public bool IgnoresPhysicsLogics
        {
            get { return FSBody.IgnoreGravity; } // TODO: Mitä tän käytännössä pitäisi tehdä?
            set { FSBody.IgnoreGravity = value; }
        }

        /// <summary>
        /// Fysiikkamuodon muodostavat verteksit.
        /// </summary>
        public List<List<Vector2>> Vertices
        {
            get
            {
                List<List<Vector2>> vert = new List<List<Vector2>>();
                for(int i = 0; i < FSBody.FixtureList.Count; i++)
                {
                    vert.Add(new List<Vector2>());
                    Fixture f = FSBody.FixtureList[i];
                    if (f.Shape is PolygonShape shape)
                        vert[i].AddRange(shape.Vertices);
                }
                return vert;
            }
        }

        #region Constructors

        /// <summary>
        /// Luo uuden fysiikkaolion.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        /// <param name="world">Fysiikkamoottorin maailma johon kappale luodaan.</param>
        public PhysicsBody(double width, double height, World world)
            : this(width, height, Shape.Rectangle, world)
        {
        }

        /// <summary>
        /// Luo uuden fysiikkaolion.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        /// <param name="shape">Muoto.</param>
        /// <param name="world">Fysiikkamoottorin maailma johon kappale luodaan.</param>
        public PhysicsBody(double width, double height, Shape shape, World world)
        {
            this._size = new Vector(width, height) * FSConvert.DisplayToSim;
            this._shape = shape;

            FSBody = world.CreateBody(bodyType: BodyType.Dynamic);// BodyFactory.CreateBody(world, bodyType: BodyType.Dynamic);
            FSBody.owner = this;
            FSBody.Enabled = false;
            if (shape is Ellipse && width == height)
            {
                Fixture f = FixtureFactory.AttachCircle((float)height * FSConvert.DisplayToSim / 2, DefaultDensity, FSBody);
                f.Tag = FSBody;
            }
            else
            {
                List<Vertices> vertices = CreatePhysicsShape(shape, this._size);
                List<Fixture> fixtures = FixtureFactory.AttachCompoundPolygon(vertices, DefaultDensity, FSBody);
                fixtures.ForEach((f) => f.Tag = FSBody);
            }
        }


        /// <summary>
        /// Luo fysiikkaolion, jonka muotona on säde.
        /// </summary>
        /// <param name="raySegment">Säde.</param>
        /// <param name="world">Fysiikkamoottorin maailma johon kappale luodaan</param>
        public PhysicsBody(RaySegment raySegment, World world)
            : this(1, 1, raySegment, world)
        {
            this._size = Vector.One;
            this._shape = raySegment;
        }

        #endregion

        public void Update(Time time)
        {
        }

        public void SetCollisionIgnorer(Ignorer ignorer)
        {
            if(ignorer is null)
            {
                FSBody.JypeliGroupIgnorer = null;
                FSBody.ObjectIgnorer = null;
                return;
            }

            if (ignorer is JypeliGroupIgnorer)
            {
                JypeliGroupIgnorer ign = ignorer as JypeliGroupIgnorer;
                FSBody.JypeliGroupIgnorer = ign;
            }
            else if (ignorer is ObjectIgnorer || ignorer is null)
            {
                ObjectIgnorer ign = ignorer as ObjectIgnorer;
                FSBody.ObjectIgnorer = ign;
            }
            else
                throw new NotImplementedException("Annettu Ignore ei ole toteutettu.");
        }

        /// <summary>
        /// Muodostaa uudelleen yhdistettyjen fysiikkakappaleiden fysiikkamuodot.
        /// Kutsuttava jos esim. lapsiolion paikkaa tai kokoa muutetaan.
        /// </summary>
        /// <param name="physObj">Kappale jonka ominaisuuksia muutettiin.</param>
        public void RegenerateConnectedFixtures()
        {
            /**
             * "Post-order" puuhakualgoritmi.
             * Jos muokataan obj3, tälle voidaan antaa parametrina obj3, obj2 tai obj1 ilman ongelmia.
             * obj1
             *     obj2
             *          obj3
             *          obj4
             *     obj5
             *     obj6
             */
            PhysicsObject physObj = this.Owner as PhysicsObject;

            SynchronousList<GameObject> childs = physObj.Objects;
            foreach (var child in childs)
            {
                if (child is PhysicsObject)
                {
                    if (!child.IsAddedToGame) continue;
                    PhysicsObject physChild = child as PhysicsObject;
                    physChild.Parent.Size *= 1;
                    physChild.Body.RegenerateConnectedFixtures();
                }
            }
            PhysicsBody physMainParent = (PhysicsBody)((PhysicsObject)physObj.GetMainParent()).Body;
            List<Fixture> fs = physMainParent.FSBody.FixtureList._list;

            for (int i = 0; i < fs.Count; i++)
            {
                if ((Body)fs[i].Tag == ((PhysicsBody)physObj.Body).FSBody)
                    physMainParent.FSBody.Remove(fs[i]);
            }
            if (physObj.Parent != null)
                PhysicsGame.Instance.Engine.ConnectBodies((PhysicsObject)physObj.Parent, physObj);
        }
    }

    /*
    class CollisionIgnorerAdapter : Physics2DDotNet.Ignorers.Ignorer
    {
        private Jypeli.Ignorer innerIgnorer;

        public override bool BothNeeded
        {
            get { return innerIgnorer.BothNeeded; }
        }

        protected override bool CanCollide( Body thisBody, Body otherBody, Physics2DDotNet.Ignorers.Ignorer other )
        {
            var body1 = (Jypeli.PhysicsBody)( thisBody.Tag );
            var body2 = (Jypeli.PhysicsBody)( otherBody.Tag );
            var otherIgnorer = other == null ? null : ( (CollisionIgnorerAdapter)other ).innerIgnorer;

            return innerIgnorer.CanCollide( body1, body2, otherIgnorer );
        }

        public CollisionIgnorerAdapter( Jypeli.Ignorer adaptee )
        {
            this.innerIgnorer = adaptee;
        }
    }*/
}
