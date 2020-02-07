using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;

namespace ViruBoyUI
{
    public class Sprite : IDisposable
    {
        private readonly int _vertexArray;
        private readonly int _buffer;

        private bool _initialized;

        public Sprite(Vector2 pos, byte pattern, bool high)
        {
            Vector4 v4pos = new Vector4(pos.X, pos.Y, -1, 1);


            /*
            TexturedVertex[] vertices = {
                new TexturedVertex(v4pos, uv),
                new TexturedVertex(v4pos + new Vector4(8, 0, 0, 0), uv + new Vector2(8, 0)),
                new TexturedVertex(v4pos + new Vector4(8, high ? 16 : 8, 0, 0), uv + new Vector2(8, high ? 16 : 8)),
                new TexturedVertex(v4pos + new Vector4(0, high ? 16 : 8, 0, 0), uv + new Vector2(0, high ? 16 : 8))
            };
            */

            TexturedVertex[] vertices = {
                new TexturedVertex(v4pos, Vector2.Zero),
                new TexturedVertex(v4pos + new Vector4(8, 0, 0, 0), Vector2.Zero),
                new TexturedVertex(v4pos + new Vector4(8, high ? 16 : 8, 0, 0), Vector2.Zero),
                new TexturedVertex(v4pos + new Vector4(0, high ? 16 : 8, 0, 0), Vector2.Zero)
            };

            vertices[0].UV = new Vector2(pattern % 16 * 8, pattern / 16 * 8);
            vertices[1].UV = new Vector2(pattern % 16 * 8 + 8, pattern / 16 * 8);
            vertices[2].UV = new Vector2(pattern % 16 * 8 + 8, pattern / 16 * 8 + 8);
            vertices[3].UV = new Vector2(pattern % 16 * 8, pattern / 16 * 8 + 8);

            /*
            TexturedVertex[] vertices = new TexturedVertex[4]
            {
                new TexturedVertex(v4pos, new Vector2(0, 0)),
                new TexturedVertex(v4pos + new Vector4(128, 0, 0, 0), new Vector2(128, 0)),
                new TexturedVertex(v4pos + new Vector4(128, 128, 0, 0), new Vector2(128, 128)),
                new TexturedVertex(v4pos + new Vector4(0, 128, 0, 0), new Vector2(0, 128))
            };
            */

            _vertexArray = GL.GenVertexArray();
            _buffer = GL.GenBuffer();

            GL.BindVertexArray(_vertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);

            GL.NamedBufferStorage(_buffer, 4 * TexturedVertex.Size, vertices, BufferStorageFlags.MapWriteBit);

            GL.VertexArrayAttribBinding(_vertexArray, 0, 0);
            GL.EnableVertexArrayAttrib(_vertexArray, 0);
            GL.VertexArrayAttribFormat(_vertexArray, 0, 4, VertexAttribType.Float, false, 0);

            GL.VertexArrayAttribBinding(_vertexArray, 1, 0);
            GL.EnableVertexArrayAttrib(_vertexArray, 1);
            GL.VertexArrayAttribFormat(_vertexArray, 1, 2, VertexAttribType.Float, false, 16);

            GL.VertexArrayVertexBuffer(_vertexArray, 0, _buffer, IntPtr.Zero, TexturedVertex.Size);

            _initialized = true;
        }

        public void Render()
        {
            GL.BindVertexArray(_vertexArray);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_initialized)
                {
                    GL.DeleteVertexArray(_vertexArray);
                    GL.DeleteBuffer(_buffer);
                    _initialized = false;
                }
            }
        }
    }
}
