#region licenses
/* Original source Aether Physics 2D:
 * Copyright (c) 2020 Kastellanos Nikolaos
 * https://github.com/tainicom/Aether.Physics2D
*/

/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/
#endregion

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;


namespace FarseerPhysics.Common
{
    //Contributed by Matthew Bettcher

    /// <summary>
    /// Path:
    /// Very similar to Vertices, but this
    /// class contains vectors describing
    /// control points on a Catmull-Rom
    /// curve.
    /// </summary>
    public class Path
    {
        /// <summary>
        /// All the points that makes up the curve
        /// </summary>
        public List<Vector2> ControlPoints;

        private float _deltaT;

        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        public Path()
        {
            ControlPoints = new List<Vector2>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        /// <param name="vertices">The vertices to created the path from.</param>
        public Path(Vector2[] vertices)
        {
            ControlPoints = new List<Vector2>(vertices.Length);

            for (int i = 0; i < vertices.Length; i++)
            {
                Add(vertices[i]);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        /// <param name="vertices">The vertices to created the path from.</param>
        public Path(IList<Vector2> vertices)
        {
            ControlPoints = new List<Vector2>(vertices.Count);
            for (int i = 0; i < vertices.Count; i++)
            {
                Add(vertices[i]);
            }
        }

        /// <summary>
        /// True if the curve is closed.
        /// </summary>
        /// <value><c>true</c> if closed; otherwise, <c>false</c>.</value>
        public bool Closed { get; set; }

        /// <summary>
        /// Gets the next index of a controlpoint
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public int NextIndex(int index)
        {
            if (index == ControlPoints.Count - 1)
            {
                return 0;
            }
            return index + 1;
        }

        /// <summary>
        /// Gets the previous index of a controlpoint
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public int PreviousIndex(int index)
        {
            if (index == 0)
            {
                return ControlPoints.Count - 1;
            }
            return index - 1;
        }

        /// <summary>
        /// Translates the control points by the specified vector.
        /// </summary>
        /// <param name="vector">The vector.</param>
        public void Translate(ref Vector2 vector)
        {
            for (int i = 0; i < ControlPoints.Count; i++)
                ControlPoints[i] = ControlPoints[i] + vector;
        }

        /// <summary>
        /// Scales the control points by the specified vector.
        /// </summary>
        /// <param name="value">The Value.</param>
        public void Scale(ref Vector2 value)
        {
            for (int i = 0; i < ControlPoints.Count; i++)
                ControlPoints[i] = ControlPoints[i] * value;
        }

        /// <summary>
        /// Rotate the control points by the defined value in radians.
        /// </summary>
        /// <param name="value">The amount to rotate by in radians.</param>
        public void Rotate(float value)
        {
            var rotation = Complex.FromAngle(value);

            for (int i = 0; i < ControlPoints.Count; i++)
                ControlPoints[i] = Complex.Multiply(ControlPoints[i], ref rotation);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < ControlPoints.Count; i++)
            {
                builder.Append(ControlPoints[i].ToString());
                if (i < ControlPoints.Count - 1)
                {
                    builder.Append(" ");
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Returns a set of points defining the
        /// curve with the specifed number of divisions
        /// between each control point.
        /// </summary>
        /// <param name="divisions">Number of divisions between each control point.</param>
        /// <returns></returns>
        public Vertices GetVertices(int divisions)
        {
            Vertices verts = new Vertices();

            float timeStep = 1f / divisions;

            for (float i = 0; i < 1f; i += timeStep)
            {
                verts.Add(GetPosition(i));
            }

            return verts;
        }

        public Vector2 GetPosition(float time)
        {
            Vector2 temp;

            if (ControlPoints.Count < 2)
                throw new Exception("You need at least 2 control points to calculate a position.");

            if (Closed)
            {
                Add(ControlPoints[0]);

                _deltaT = 1f / (ControlPoints.Count - 1);

                int p = (int)(time / _deltaT);

                // use a circular indexing system
                int p0 = p - 1;
                if (p0 < 0)
                    p0 = p0 + (ControlPoints.Count - 1);
                else if (p0 >= ControlPoints.Count - 1)
                    p0 = p0 - (ControlPoints.Count - 1);
                int p1 = p;
                if (p1 < 0)
                    p1 = p1 + (ControlPoints.Count - 1);
                else if (p1 >= ControlPoints.Count - 1)
                    p1 = p1 - (ControlPoints.Count - 1);
                int p2 = p + 1;
                if (p2 < 0)
                    p2 = p2 + (ControlPoints.Count - 1);
                else if (p2 >= ControlPoints.Count - 1)
                    p2 = p2 - (ControlPoints.Count - 1);
                int p3 = p + 2;
                if (p3 < 0)
                    p3 = p3 + (ControlPoints.Count - 1);
                else if (p3 >= ControlPoints.Count - 1)
                    p3 = p3 - (ControlPoints.Count - 1);

                // relative time
                float lt = (time - _deltaT * p) / _deltaT;

                CalcCatmullRom(ControlPoints[p0], ControlPoints[p1], ControlPoints[p2], ControlPoints[p3], lt, out temp);

                RemoveAt(ControlPoints.Count - 1);
            }
            else
            {
                int p = (int)(time / _deltaT);

                // 
                int p0 = p - 1;
                if (p0 < 0)
                    p0 = 0;
                else if (p0 >= ControlPoints.Count - 1)
                    p0 = ControlPoints.Count - 1;
                int p1 = p;
                if (p1 < 0)
                    p1 = 0;
                else if (p1 >= ControlPoints.Count - 1)
                    p1 = ControlPoints.Count - 1;
                int p2 = p + 1;
                if (p2 < 0)
                    p2 = 0;
                else if (p2 >= ControlPoints.Count - 1)
                    p2 = ControlPoints.Count - 1;
                int p3 = p + 2;
                if (p3 < 0)
                    p3 = 0;
                else if (p3 >= ControlPoints.Count - 1)
                    p3 = ControlPoints.Count - 1;

                // relative time
                float lt = (time - _deltaT * p) / _deltaT;

                CalcCatmullRom(ControlPoints[p0], ControlPoints[p1], ControlPoints[p2], ControlPoints[p3], lt, out temp);
            }

            return temp;
        }

        private void CalcCatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float amount, out Vector2 result)
        {
            double sqAmount = amount * amount;
            double cuAmount = sqAmount * amount;

            double x;
            double y;
            x = 2.0 * p1.X;
            y = 2.0 * p1.Y;
            x += (p2.X - p0.X) * amount;
            y += (p2.Y - p0.Y) * amount;
            x += (2.0 * p0.X - 5.0 * p1.X + 4.0 * p2.X - p3.X) * sqAmount;
            y += (2.0 * p0.Y - 5.0 * p1.Y + 4.0 * p2.Y - p3.Y) * sqAmount;
            x += (3.0 * p1.X - p0.X - 3.0 * p2.X + p3.X) * cuAmount;
            y += (3.0 * p1.Y - p0.Y - 3.0 * p2.Y + p3.Y) * cuAmount;
            x *= 0.5;
            y *= 0.5;

            result.X = (float)x;
            result.Y = (float)y;
        }

        /// <summary>
        /// Gets the normal for the given time.
        /// </summary>
        /// <param name="time">The time</param>
        /// <returns>The normal.</returns>
        public Vector2 GetPositionNormal(float time)
        {
            float offsetTime = time + 0.0001f;

            Vector2 a = GetPosition(time);
            Vector2 b = GetPosition(offsetTime);

            Vector2 output, temp;

            temp = Vector2.Subtract(a, b);

            output.X = -temp.Y;
            output.Y = temp.X;

            output = Vector2.Normalize(output);

            return output;
        }

        public void Add(Vector2 point)
        {
            ControlPoints.Add(point);
            _deltaT = 1f / (ControlPoints.Count - 1);
        }

        public void Remove(Vector2 point)
        {
            ControlPoints.Remove(point);
            _deltaT = 1f / (ControlPoints.Count - 1);
        }

        public void RemoveAt(int index)
        {
            ControlPoints.RemoveAt(index);
            _deltaT = 1f / (ControlPoints.Count - 1);
        }

        public float GetLength()
        {
            List<Vector2> verts = GetVertices(ControlPoints.Count * 25);
            float length = 0;

            for (int i = 1; i < verts.Count; i++)
            {
                length += Vector2.Distance(verts[i - 1], verts[i]);
            }

            if (Closed)
                length += Vector2.Distance(verts[ControlPoints.Count - 1], verts[0]);

            return length;
        }

        public List<Vector3> SubdivideEvenly(int divisions)
        {
            List<Vector3> verts = new List<Vector3>();

            float length = GetLength();

            float deltaLength = length / divisions + 0.001f;
            float t = 0.000f;

            // we always start at the first control point
            Vector2 start = ControlPoints[0];
            Vector2 end = GetPosition(t);

            // increment t until we are at half the distance
            while (deltaLength * 0.5f >= Vector2.Distance(start, end))
            {
                end = GetPosition(t);
                t += 0.0001f;

                if (t >= 1f)
                    break;
            }

            start = end;

            // for each box
            for (int i = 1; i < divisions; i++)
            {
                Vector2 normal = GetPositionNormal(t);
                float angle = (float)Math.Atan2(normal.Y, normal.X);

                verts.Add(new Vector3(end.X, end.Y, angle));

                // until we reach the correct distance down the curve
                while (deltaLength >= Vector2.Distance(start, end))
                {
                    end = GetPosition(t);
                    t += 0.00001f;

                    if (t >= 1f)
                        break;
                }
                if (t >= 1f)
                    break;

                start = end;
            }
            return verts;
        }
    }
}