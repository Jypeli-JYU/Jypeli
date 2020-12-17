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
using FarseerPhysics.Dynamics;
using AdvanceMath;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Jypeli.Farseer;
using FarseerPhysics.Common;
using FarseerPhysics;

namespace Jypeli
{
    public partial class PhysicsBody
    {
        Shape _shape;
        Vector _size;

        /// <summary>
        /// Fysiikkamoottorin käyttämä tietorakenne.
        /// </summary>
        public Body Body { get; private set; }

        /// <summary>
        /// Olion paikka koordinaatistossa. Käsittää sekä X- että Y-koordinaatin.
        /// </summary>
        [Save]
        public Vector Position
        {
            get => Body._xf.P * FSConvert.SimToDisplay;
            set
            {
                Vector2 temp = new Vector2((float)value.X, (float)value.Y) * FSConvert.DisplayToSim;
                Body.IsAwake = true;
                Velocity = Vector.Zero; // Farseer ei hirveästi arvosta kappaleen raahaamista hiirellä suoraan sijaintia muuttamalla.
                // TODO: Kappaleille jonkin sortin "EnableDragging" tms. ja FixedMouseJoint
                Body.SetTransform(ref temp, (float)Angle);
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
                // TODO:Body.Shape = CreatePhysicsShape(_shape, value * FSConvert.DisplayToSim);
                _size = value * FSConvert.DisplayToSim;
            }
        }

        /// <summary>
        /// Kulma, jossa olio on. Oliota voi pyörittää kulmaa vaihtamalla.
        /// </summary>
        [Save]
        public double Angle
        {
            get { return Body.Rotation; }
            set { Body.Rotation = (float)value; }
        }

        /// <summary>
        /// Olion muoto.
        /// </summary>
        public Shape Shape
        {
            get { return _shape; }
            set
            {
                SetShape(value, GetDefaultParameters(Size.X, Size.Y));
            }
        }

        internal void SetShape(Shape shape, CollisionShapeParameters parameters)
        {
            // TODO: Tää on vähän huono ratkaisu.
            _shape = shape;
            var collisionHandlers = Body.FixtureList[0].OnCollision;
            Body.DestroyFixture(Body.FixtureList[0]);
            Vertices vertices = CreatePhysicsShape(shape, this._size);
            Fixture f = Body.CreateFixture(new FarseerPhysics.Collision.Shapes.PolygonShape(vertices, 0.01f));
            f.OnCollision += collisionHandlers;
            //Body.Shape = CreatePhysicsShape( shape, Size, parameters );
        }

        internal static Vertices CreatePhysicsShape(Shape shape, Vector size)
        {
            return CreatePhysicsShape(shape, size, GetDefaultParameters(size.X, size.Y));
        }

        /// <summary>
        /// Creates a shape to be used in the Physics Body. A physics shape is scaled to the
        /// size of the object. In addition, it has more vertices and some additional info
        /// that is used in collision detection.
        /// </summary>
        internal static Vertices CreatePhysicsShape(Shape shape, Vector size, CollisionShapeParameters parameters)
        {
            if (shape is RaySegment)
            {
                RaySegment raySegment = (RaySegment)shape;
                return PolygonTools.CreateLine(raySegment.Origin, raySegment.Origin + raySegment.Direction*raySegment.Length);

            }
            else if (shape is Rectangle)
            {
                return PolygonTools.CreateRectangle((float)size.X/2, (float)size.Y/2);
            }
            else if (shape is Ellipse)
            {
                Debug.Assert(shape.IsUnitSize);

                double smaller = Math.Min(size.X/2, size.Y/2);
                double bigger = Math.Max(size.X/2, size.Y/2);
                // Average between width and height.
                double r = smaller / 2 + (bigger - smaller) / 2;
                int vertexCount = Math.Min(Settings.MaxPolygonVertices, (int)Math.Ceiling((2 * Math.PI * r) / parameters.MaxVertexDistance));

                return PolygonTools.CreateEllipse((float)size.X/2, (float)size.Y/2, vertexCount);

            }
            else
            {
                // TODO: ei toimi oikein erikoisemmille muodoille, kuten tähdelle.
                Vertices vertices = new Vertices();
                for (int i = 0; i < shape.Cache.Vertices.Length; i++)
                {
                    Vector v = shape.Cache.OutlineVertices[i];
                    if (shape.IsUnitSize)
                    {
                        v.X *= size.X;
                        v.Y *= size.Y;
                    }
                    vertices.Add(new Vector2((float)v.X, (float)v.Y));
                }
                return vertices;
            }
        }
    }
}
