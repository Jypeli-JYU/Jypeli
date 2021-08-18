using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenGL;

namespace Jypeli.Rendering.OpenGl
{
    public class Shader : IShader, IDisposable
    {
        //Our handle and the GL instance this class will use, these are private because they have no reason to be public.
        //Most of the time you would want to abstract items to make things like this invisible.
        private uint _handle;
        private GL _gl;

        private static Shader ActiveShader;

        public Shader(GL gl, string vrt, string frag)
        {
            _gl = gl;

            //Load the individual shaders.
            uint vertex = LoadShader(ShaderType.VertexShader, vrt);
            uint fragment = LoadShader(ShaderType.FragmentShader, frag);
            //Create the shader program.
            _handle = _gl.CreateProgram();
            //Attach the individual shaders.
            _gl.AttachShader(_handle, vertex);
            _gl.AttachShader(_handle, fragment);
            _gl.LinkProgram(_handle);
            //Check for linking errors.
            _gl.GetProgram(_handle, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                throw new Exception($"Program failed to link with error: {_gl.GetProgramInfoLog(_handle)}");
            }
            //Detach and delete the shaders
            _gl.DetachShader(_handle, vertex);
            _gl.DetachShader(_handle, fragment);
            _gl.DeleteShader(vertex);
            _gl.DeleteShader(fragment);
        }

        /// <inheritdoc/>
        public void Use()
        {
            // Shaderin vaihto on melko raskas operaatio, erityisesti mobiililaitteilla
            if (ActiveShader == this)
                return;

            _gl.UseProgram(_handle);
            ActiveShader = this;
        }

        /// <inheritdoc/>
        public void SetUniform(string name, int value)
        {
            //Setting a uniform on a shader using a name.
            int location = _gl.GetUniformLocation(_handle, name);
            if (location == -1) //If GetUniformLocation returns -1 the uniform is not found.
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            Use();
            _gl.Uniform1(location, value);
        }

        /// <inheritdoc/>
        public void SetUniform(string name, float value)
        {
            int location = _gl.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            Use();
            _gl.Uniform1(location, value);
        }

        /// <inheritdoc/>
        public void SetUniform(string name, Vector4 value)
        {
            int location = _gl.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            Use();
            _gl.Uniform4(location, value);
        }

        /// <inheritdoc/>
        public unsafe void SetUniform(string name, Vector3[] value)
        {
            int location = _gl.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            Use();

            _gl.Uniform3(location, (uint)value.Length, Unsafe.AsRef<float>(Unsafe.AsPointer(ref value)));
        }

        /// <inheritdoc/>
        public void SetUniform(string name, Vector2 value)
        {
            int location = _gl.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            Use();
            _gl.Uniform2(location, value);
        }

        /// <inheritdoc/>
        public unsafe void SetUniform(string name, Matrix4x4 value)
        {
            //A new overload has been created for setting a uniform so we can use the transform in our shader.
            int location = _gl.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            Use();
            _gl.UniformMatrix4(location, 1, false, (float*)&value);
        }

        /// <inheritdoc/>
        public unsafe void SetUniform(string name, float[] value)
        {
            //A new overload has been created for setting a uniform so we can use the transform in our shader.
            int location = _gl.GetUniformLocation(_handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            Use();
            _gl.Uniform1(location, 10, value);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            //Remember to delete the program when we are done.
            _gl.DeleteProgram(_handle);
        }

        private uint LoadShader(ShaderType type, string shadersrc)
        {
            //To load a single shader we need to load
            //1) Load the shader from a file.
            //2) Create the handle.
            //3) Upload the source to opengl.
            //4) Compile the shader.
            //5) Check for errors.
            uint handle = _gl.CreateShader(type);
            _gl.ShaderSource(handle, shadersrc);
            _gl.CompileShader(handle);
            string infoLog = _gl.GetShaderInfoLog(handle);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
            }

            return handle;
        }
    }
}
