using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenGL;

namespace Jypeli.Rendering
{
    /// <summary>
    /// Rajapinta shadereita varten
    /// </summary>
    public interface IShader
    {
        /// <summary>
        /// Asettaa shaderin käyttöön
        /// </summary>
        public void Use();

        // TODO: Mikä on tätä vastaava jollain toisella rajapinnalla, mikä olisi järkevä nimeäminen?
        /// <summary>
        /// Asettaa uniformin arvon
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetUniform(string name, int value);

        /// <summary>
        /// Asettaa uniformin arvon
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetUniform(string name, float value);

        /// <summary>
        /// Asettaa uniformin arvon
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetUniform(string name, Vector4 value);

        /// <summary>
        /// Asettaa uniformin arvon
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public unsafe void SetUniform(string name, Vector3[] value);

        /// <summary>
        /// Asettaa uniformin arvon
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetUniform(string name, Vector2 value);

        /// <summary>
        /// Asettaa uniformin arvon
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public unsafe void SetUniform(string name, Matrix4x4 value);

        /// <summary>
        /// Asettaa uniformin arvon
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public unsafe void SetUniform(string name, float[] value);

        /// <summary>
        /// Poistaa shaderin ja vapauttaa sen näytönohjaimelta
        /// </summary>
        public void Dispose();
    }
}
