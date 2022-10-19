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

using FarseerPhysics.Dynamics;
using Jypeli.Farseer;
using FarseerPhysics.Common;
using FarseerPhysics;
using System.Collections.Generic;
using FarseerPhysics.Factories;
using FarseerPhysics.Common.Decomposition;
using System.Numerics;

namespace Jypeli
{
    public partial class PhysicsBody
    {
        Shape _shape;
        Vector _size;

        /// <summary>
        /// Fysiikkamoottorin käyttämä tietorakenne.
        /// </summary>
        public Body FSBody { get; private set; }

        /// <summary>
        /// Olion paikka koordinaatistossa. Käsittää sekä X- että Y-koordinaatin.
        /// </summary>
        [Save]
        public Vector Position
        {
            get => FSBody._xf.p * FSConvert.SimToDisplay;
            set
            {
                Vector2 temp = new Vector2((float)value.X, (float)value.Y) * FSConvert.DisplayToSim;
                if (Owner.Parent is null)
                    FSBody.Awake = true;
                // Farseer ei hirveästi arvosta kappaleen raahaamista hiirellä suoraan sijaintia muuttamalla.
                // TODO: Kappaleille jonkin sortin "EnableDragging" tms. ja FixedMouseJoint
                FSBody.Position = temp;
            }
        }

        /// <summary>
        /// Olion koko (x on leveys, y on korkeus).
        /// </summary>
        [Save]
        public Vector Size
        {
            get
            {
                return _size * FSConvert.SimToDisplay;
            }
            set
            {
                _size = value * FSConvert.DisplayToSim;
                Shape = _shape; // Muodostaa fysiikkamuodon uudestaan uudella koolla.
            }
        }

        /// <summary>
        /// Kulma, jossa olio on. Oliota voi pyörittää kulmaa vaihtamalla.
        /// </summary>
        [Save]
        public double Angle
        {
            get { return FSBody.Rotation; }
            set { FSBody.Rotation = (float)value; }
        }

        /// <summary>
        /// Olion muoto.
        /// </summary>
        public Shape Shape
        {
            get { return _shape; }
            set
            {
                SetShape(value);
            }
        }

        internal void SetShape(Shape shape)
        {
            bool ignoresCollisionResponseTemp = IgnoresCollisionResponse;
            _shape = shape;
            var collisionHandlers = FSBody.FixtureList[0].OnCollision;

            for (int i = 0; i < FSBody.FixtureList.Count; i++)
            {
                FSBody.Remove(FSBody.FixtureList[i]);
            }

            List<Fixture> fs = new List<Fixture>();
            if (shape == Shape.Circle || shape == Shape.Ellipse) // Ympyrä on oikeasti ellipsi ja ellipsi voi olla ympyrä
                if (Size.X == Size.Y)
                    fs.Add(FixtureFactory.AttachCircle((float)Size.X / 2 * FSConvert.DisplayToSim, DefaultDensity, FSBody));
                else
                    fs.Add(FixtureFactory.AttachEllipse((float)Size.X / 2 * FSConvert.DisplayToSim, (float)Size.Y / 2 * FSConvert.DisplayToSim, Settings.MaxPolygonVertices, DefaultDensity, FSBody));
            else
            {
                List<Vertices> vertices = CreatePhysicsShape(shape, this._size);
                fs.AddRange(FixtureFactory.AttachCompoundPolygon(vertices, DefaultDensity, FSBody));
            }
            fs.ForEach((f) => { f.OnCollision += collisionHandlers; f.Tag = FSBody; });
            IgnoresCollisionResponse = ignoresCollisionResponseTemp;
        }

        /// <summary>
        /// Creates a shape to be used in the Physics Body. A physics shape is scaled to the
        /// size of the object. In addition, it has more vertices and some additional info
        /// that is used in collision detection.
        /// </summary>
        internal static List<Vertices> CreatePhysicsShape(Shape shape, Vector size)
        {
            List<Vertices> res = new List<Vertices>();
            if (shape is RaySegment raySegment)
            {
                res.Add(PolygonTools.CreateLine(raySegment.Origin, raySegment.Origin + raySegment.Direction * raySegment.Length * FSConvert.DisplayToSim));
                return res;
            }
            else if (shape is Rectangle)
            {
                res.Add(PolygonTools.CreateRectangle((float)size.X / 2, (float)size.Y / 2));
                return res;
            }
            else
            {
                Vertices vertices = new Vertices();
                for (int i = 0; i < shape.Cache.OutlineVertices.Length; i++)
                {
                    Vector v = shape.Cache.OutlineVertices[i];
                    if (shape.IsUnitSize)
                    {
                        v.X *= size.X;
                        v.Y *= size.Y;
                    }
                    else
                    {
                        v.X *= FSConvert.DisplayToSim;
                        v.Y *= FSConvert.DisplayToSim;
                    }
                    vertices.Add(new Vector2((float)v.X, (float)v.Y));
                }

                // TODO: Mille kaikille muodoille tää tarvii tehdä?
                // TODO: Mikä on paras algoritmi?
                // TODO: Tuottaa tietyissä tilanteissa hyvin raskaita muotoja.
                res.AddRange(CDTDecomposer.ConvexPartition(vertices));
                return res;
            }
        }
    }
}
