using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace ViruBoyUI
{
    public class Background : IDisposable
    {
        public byte this[int i]
        {
            get
            {
                int vertexNum = i * 4;

                int x = (int)(Vertices[vertexNum].UV.X / 8);
                int y = (int)(Vertices[vertexNum].UV.Y / 8);

                return (byte)(y * 16 + x);
            }

            set
            {
                Vertices[i * 4 + 0].UV = new Vector2(value % 16 * 8, value / 16 * 8);
                Vertices[i * 4 + 1].UV = new Vector2(value % 16 * 8 + 8, value / 16 * 8);
                Vertices[i * 4 + 2].UV = new Vector2(value % 16 * 8 + 8, value / 16 * 8 + 8);
                Vertices[i * 4 + 3].UV = new Vector2(value % 16 * 8, value / 16 * 8 + 8);
            }
        }

        public TexturedVertex[] Vertices { get; } = new TexturedVertex[4096];

        private readonly int _vertexArray;
        private readonly int _buffer;

        private bool _initialized;

        public Background()
        {
            for (int i = 0; i < 1024; i++)
            {
                int x = i % 32;
                int y = i / 32;

                Vertices[i * 4 + 0].Position = new Vector4(x * 8, y * 8, -1, 1);
                Vertices[i * 4 + 1].Position = new Vector4(x * 8 + 8, y * 8, -1, 1);
                Vertices[i * 4 + 2].Position = new Vector4(x * 8 + 8, y * 8 + 8, -1, 1);
                Vertices[i * 4 + 3].Position = new Vector4(x * 8, y * 8 + 8, -1, 1);
            }

            _vertexArray = GL.GenVertexArray();
            _buffer = GL.GenBuffer();

            GL.BindVertexArray(_vertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);

            GL.NamedBufferStorage(_buffer, 4096 * TexturedVertex.Size, Vertices, BufferStorageFlags.DynamicStorageBit);

            GL.VertexArrayAttribBinding(_vertexArray, 0, 0);
            GL.EnableVertexArrayAttrib(_vertexArray, 0);
            GL.VertexArrayAttribFormat(_vertexArray, 0, 4, VertexAttribType.Float, false, 0);

            GL.VertexArrayAttribBinding(_vertexArray, 1, 0);
            GL.EnableVertexArrayAttrib(_vertexArray, 1);
            GL.VertexArrayAttribFormat(_vertexArray, 1, 2, VertexAttribType.Float, false, 16);

            GL.VertexArrayVertexBuffer(_vertexArray, 0, _buffer, IntPtr.Zero, TexturedVertex.Size);

            _initialized = true;
        }

        public void Update()
        {
            //GL.NamedBufferStorage(_buffer, 4096 * TexturedVertex.Size, Vertices, BufferStorageFlags.DynamicStorageBit);
            GL.NamedBufferSubData(_buffer, IntPtr.Zero, 4096 * TexturedVertex.Size, Vertices);
        }

        public void Render()
        {
            GL.BindVertexArray(_vertexArray);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4096);
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
