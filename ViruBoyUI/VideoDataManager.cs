using System;
using OpenTK.Graphics.OpenGL4;

using ViruBoy;

namespace ViruBoyUI
{
    public class VideoDataManager
    {
        public readonly int TileTexture;
        public readonly int SpriteTexture;

        public float[] retroPalette =
        {

            155, 188, 15, 255f,
            139, 172, 15, 255,
            48, 98, 48, 255,
            15, 56, 15, 255,
        };

        public float[] palette =
        {

            224, 248, 208, 255,
            136, 192, 112, 255,
            52, 104, 86, 255,
            8, 24, 32, 255f
        };


        private readonly VirtualCpu _cpu;
        private float[] _dataTile = new float[256 * 64 * 4];
        private float[] _dataSprite = new float[256 * 64 * 4];

        public VideoDataManager(VirtualCpu cpu)
        {
            _cpu = cpu;

            GL.CreateTextures(TextureTarget.Texture2D, 1, out TileTexture);
            GL.TextureStorage2D(TileTexture, 1, SizedInternalFormat.Rgba32f, 128, 128);

            GL.CreateTextures(TextureTarget.Texture2D, 1, out SpriteTexture);
            GL.TextureStorage2D(SpriteTexture, 1, SizedInternalFormat.Rgba32f, 128, 128);

            ReloadTileData();
            ReloadSpriteData();
        }

        public void ReloadTileData()
        {
            int x = 0;
            int y = 0;

            for (int i = 0; i < 256; i++)
            {
                int xPos = x * 8 * 4;
                int oneLine = 128 * 4;
                int yPos = oneLine * 8 * y;

                LoadTile(i, ref _dataTile, xPos + yPos);
                x++;
                if (x == 16)
                {
                    x = 0;
                    y++;
                }
            }

            GL.BindTexture(TextureTarget.Texture2D, TileTexture);
            GL.TextureSubImage2D(TileTexture, 0, 0, 0, 128, 128, PixelFormat.Rgba, PixelType.Float, _dataTile);
        }

        public void ReloadSpriteData()
        {
            int x = 0;
            int y = 0;

            for (int i = 0; i < 256; i++)
            {
                int xPos = x * 8 * 4;
                int oneLine = 128 * 4;
                int yPos = oneLine * 8 * y;

                LoadSprite(i, ref _dataSprite, xPos + yPos);
                x++;
                if (x == 16)
                {
                    x = 0;
                    y++;
                }
            }

            GL.BindTexture(TextureTarget.Texture2D, SpriteTexture);
            GL.TextureSubImage2D(SpriteTexture, 0, 0, 0, 128, 128, PixelFormat.Rgba, PixelType.Float, _dataSprite);
        }

        private void ResolveColor(int colorIndex, ref float[] texture, int offset)
        {
            texture[offset] = palette[colorIndex * 4] / 255f;
            texture[offset + 1] = palette[colorIndex * 4 + 1] / 255f;
            texture[offset + 2] = palette[colorIndex * 4 + 2] / 255f;
            texture[offset + 3] = palette[colorIndex * 4 + 3] / 255f;
        }

        private void LoadTile(int index, ref float[] texture, int offset)
        {
            Span<byte> tile;
            
            if ((_cpu.IOController.LCDC & LCDC.BGWindowTileDataSelect) == 0) tile = _cpu.TilePatternTable1.Slice(index * 16, 16);
            else tile = _cpu.TilePatternTable2.Slice(index * 16, 16);

            for (int line = 0; line < 8; line++)
            {
                byte lower = tile[line * 2];
                byte upper = tile[line * 2 + 1];
                int off = 0;

                for (int pixel = 0; pixel < 8; pixel++)
                {
                    int colorLower = (lower & (1 << (7 - pixel))) >> (7 - pixel);
                    int colorUpper = (upper & (1 << (7 - pixel))) >> (7 - pixel);

                    int color = colorLower | (colorUpper << 1);
                    ResolveColor(color, ref texture, off + offset);
                    off += 4;
                }

                offset += 128 * 4;
            }
        }

        private void LoadSprite(int index, ref float[] texture, int offset)
        {
            Span<byte> tile = _cpu.SpritePatternTable.Slice(index * 16, 16);

            for (int line = 0; line < 8; line++)
            {
                byte lower = tile[line * 2];
                byte upper = tile[line * 2 + 1];
                int off = 0;

                for (int pixel = 0; pixel < 8; pixel++)
                {
                    int colorLower = (lower & (1 << (7 - pixel))) >> (7 - pixel);
                    int colorUpper = (upper & (1 << (7 - pixel))) >> (7 - pixel);

                    int color = colorLower | (colorUpper << 1);
                    ResolveColor(color, ref texture, off + offset);
                    off += 4;
                }

                offset += 128 * 4;
            }
        }
    }
}
