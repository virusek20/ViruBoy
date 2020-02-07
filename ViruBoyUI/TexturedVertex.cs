using OpenTK;

namespace ViruBoyUI
{
    public struct TexturedVertex
    {
        public const int Size = (4 + 2) * 4;

        public Vector4 Position;
        public Vector2 UV;

        public TexturedVertex(Vector4 pos, Vector2 uv)
        {
            Position = pos;
            UV = uv;
        }
    }
}
