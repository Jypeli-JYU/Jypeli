using System;
using System.Diagnostics;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

using Matrix = System.Numerics.Matrix4x4;

namespace Jypeli
{
    // TODO: Tämän nimeä ja näkyvyyttä voisi hieman pohtia. Nimeäminen ei täsmää 2D-vektoriin, sekä tämä on floateilla eikä doubleilla.
    // Pitäisikö Jypelillä edes olla tätä, vai käyttäisikö ainoastaan System.Numericsin tarjoamaa, kuten matriisien kohdalla?
    // Vai pitäisikö Jypelille tehdä oma matriisi?
    /// <summary>
    /// Hyvin minimalistinen 3D-Vektori.
    /// </summary>
    public struct Vector3
    {
        private static readonly Vector3 zero = new Vector3(0f, 0f, 0f);
        private static readonly Vector3 one = new Vector3(1f, 1f, 1f);
        private static readonly Vector3 up = new Vector3(0f, 1f, 0f);
        private static readonly Vector3 down = new Vector3(0f, -1f, 0f);
        private static readonly Vector3 right = new Vector3(1f, 0f, 0f);
        private static readonly Vector3 left = new Vector3(-1f, 0f, 0f);
        private static readonly Vector3 forward = new Vector3(0f, 0f, -1f);
        private static readonly Vector3 backward = new Vector3(0f, 0f, 1f);

        public float X;
        public float Y;
        public float Z;

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3 Zero
        {
            get { return zero; }
        }

        public static Vector3 One
        {
            get { return one; }
        }

        public static Vector3 Up
        {
            get { return up; }
        }

        public static Vector3 Forward
        {
            get { return forward; }
        }

        public static Vector3 Right
        {
            get { return right; }
        }

        public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
        {
            var x = vector1.Y * vector2.Z - vector2.Y * vector1.Z;
            var y = -(vector1.X * vector2.Z - vector2.X * vector1.Z);
            var z = vector1.X * vector2.Y - vector2.X * vector1.Y;

            return new Vector3(x, y, z);
        }

        public static float Dot(Vector3 vector1, Vector3 vector2)
        {
            return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
        }

        public float LengthSquared()
        {
            return (X * X) + (Y * Y) + (Z * Z);
        }

        public static Vector3 Multiply(Vector3 vector1, Vector3 vector2)
        {
            vector1.X *= vector2.X;
            vector1.Y *= vector2.Y;
            vector1.Z *= vector2.Z;
            return vector1;
        }

        public static Vector3 Normalize(Vector3 vector)
        {
            float factor = (float)Math.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y) + (vector.Z * vector.Z));
            factor = 1f / factor;
            return new Vector3(vector.X * factor, vector.Y * factor, vector.Z * factor);
        }

        public static void Transform(Vector3[] vectors, ref Matrix matrix, Vector3[] transformedPoints)
        {
            for (var i = 0; i < vectors.Length; i++)
            {
                transformedPoints[i] = Transform(vectors[i], matrix);
            }
        }

        public static Vector3 Transform(Vector3 position, Matrix matrix)
        {
            var x = (position.X * matrix.M11) + (position.Y * matrix.M21) + (position.Z * matrix.M31) + matrix.M41;
            var y = (position.X * matrix.M12) + (position.Y * matrix.M22) + (position.Z * matrix.M32) + matrix.M42;
            var z = (position.X * matrix.M13) + (position.Y * matrix.M23) + (position.Z * matrix.M33) + matrix.M43;
            return new Vector3(x, y, z);
        }

        public static bool operator ==(Vector3 vector1, Vector3 vector2)
        {
            return vector1.X == vector2.X
                && vector1.Y == vector2.Y
                && vector1.Z == vector2.Z;
        }

        public static bool operator !=(Vector3 vector1, Vector3 vector2)
        {
            return !(vector1 == vector2);
        }

        public static Vector3 operator +(Vector3 vector1, Vector3 vector2)
        {
            vector1.X += vector2.X;
            vector1.Y += vector2.Y;
            vector1.Z += vector2.Z;
            return vector1;
        }

        public static Vector3 operator -(Vector3 vector)
        {
            vector = new Vector3(-vector.X, -vector.Y, -vector.Z);
            return vector;
        }

        public static Vector3 operator *(Vector3 vector1, float value)
        {
            vector1.X *= value;
            vector1.Y *= value;
            vector1.Z *= value;
            return vector1;
        }

        public static Vector3 operator -(Vector3 vector1, Vector3 vector2)
        {
            vector1.X -= vector2.X;
            vector1.Y -= vector2.Y;
            vector1.Z -= vector2.Z;
            return vector1;
        }

        public static implicit operator System.Numerics.Vector3(Vector3 vector)
        {
            return new System.Numerics.Vector3(vector.X, vector.Y, vector.Z);
        }
    }
}
