using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using OpenTK.Graphics.OpenGL4;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using ViruBoy;
using System.Threading.Tasks;
using OpenTK.Audio.OpenAL;

namespace ViruBoyUI
{
    sealed class EmulatorUi : GameWindow
    {
        private const int _scale = 6;
        private readonly Color4 _clearColor = new Color4(224, 248, 208, 255);

        private int _program;
        private VirtualCpu _cpu = new VirtualCpu(new Cartridge("Tetris"));
        private VideoDataManager _videoDataManager;

        private readonly List<Sprite> _sprites = new List<Sprite>();
        private Background _background;
        private Matrix4 _projection;

        public EmulatorUi() : base(160, 144, GraphicsMode.Default, "ViruBoy", GameWindowFlags.FixedWindow, DisplayDevice.Default, 4, 0, GraphicsContextFlags.ForwardCompatible)
        {
            Title += ": OpenGL Version: " + GL.GetString(StringName.Version);

            Width = 160 * _scale;
            Height = 144 * _scale;
            var off = 0x10;

            for (int i = 0; i < Cartridge.ScrollingGraphic.Length; i++)
            {
                byte compressed = Cartridge.ScrollingGraphic[i];
                int[] bits = new int[8];
                for (int j = 0; j < 8; j++) bits[j] = (compressed & (1 << j)) != 0 ? 1 : 0;
                _cpu.TilePatternTable1[off] = (byte)((bits[7] << 7) | (bits[7] << 6) | (bits[6] << 5) | (bits[6] << 4) | (bits[5] << 3) | (bits[5] << 2) | (bits[4] << 1) | (bits[4]));
                _cpu.TilePatternTable1[off + 0x02] = _cpu.TilePatternTable1[off];

                _cpu.TilePatternTable1[off + 0x04] = (byte)((bits[3] << 7) | (bits[3] << 6) | (bits[2] << 5) | (bits[2] << 4) | (bits[1] << 3) | (bits[1] << 2) | (bits[0] << 1) | (bits[0]));
                _cpu.TilePatternTable1[off + 0x06] = _cpu.TilePatternTable1[off + 0x04];

                off += 0x08;
            }

            _cpu.BackgroundTileMap1[0x104] = 0x01;
            _cpu.BackgroundTileMap1[0x105] = 0x02;
            _cpu.BackgroundTileMap1[0x106] = 0x03;
            _cpu.BackgroundTileMap1[0x107] = 0x04;
            _cpu.BackgroundTileMap1[0x108] = 0x05;
            _cpu.BackgroundTileMap1[0x109] = 0x06;
            _cpu.BackgroundTileMap1[0x10A] = 0x07;
            _cpu.BackgroundTileMap1[0x10B] = 0x08;
            _cpu.BackgroundTileMap1[0x10C] = 0x09;
            _cpu.BackgroundTileMap1[0x10D] = 0x0A;
            _cpu.BackgroundTileMap1[0x10E] = 0x0B;
            _cpu.BackgroundTileMap1[0x10F] = 0x0C;
            _cpu.BackgroundTileMap1[0x110] = 0x19;

            _cpu.BackgroundTileMap1[0x124] = 0x0D;
            _cpu.BackgroundTileMap1[0x125] = 0x0E;
            _cpu.BackgroundTileMap1[0x126] = 0x0F;
            _cpu.BackgroundTileMap1[0x127] = 0x10;

            _cpu.BackgroundTileMap1[0x128] = 0x11;
            _cpu.BackgroundTileMap1[0x129] = 0x12;
            _cpu.BackgroundTileMap1[0x12A] = 0x13;
            _cpu.BackgroundTileMap1[0x12B] = 0x14;
            _cpu.BackgroundTileMap1[0x12C] = 0x15;
            _cpu.BackgroundTileMap1[0x12D] = 0x16;
            _cpu.BackgroundTileMap1[0x12E] = 0x17;
            _cpu.BackgroundTileMap1[0x12F] = 0x18;

            
            /*
            unsafe
            {
                var device = Alc.OpenDevice(null);
                var context = Alc.CreateContext(device, (int*)null);

                Alc.MakeContextCurrent(context);


                int sampleFreq = 44100;
                double dt = 2 * Math.PI / sampleFreq;
                var dataCount = 100;
                double amp = 0.5;

                for (int freq = 440; freq < 10000; freq += 100)
                {
                    int source;
                    int buffers;
                    AL.GenBuffers(1, out buffers);
                    AL.GenSources(1, out source);

                    var sinData = new short[dataCount];
                    for (int i = 0;
                        i < sinData.Length; ++i)
                    {
                        sinData[i] = (short)(amp * short.MaxValue * Math.Sin(i * dt * freq));
                    }

                    AL.BufferData(buffers, ALFormat.Mono16, sinData, sinData.Length, sampleFreq);
                    AL.Source(source, ALSourcei.Buffer, buffers);
                    AL.Source(source, ALSourceb.Looping, true);

                    AL.SourcePlay(source);
                    Thread.Sleep(1000);
                }

                Console.ReadLine();

                if (context != ContextHandle.Zero)
                {
                    Alc.MakeContextCurrent(ContextHandle.Zero);
                    Alc.DestroyContext(context);
                }
                context = ContextHandle.Zero;

                if (device != IntPtr.Zero)
                {
                    Alc.CloseDevice(device);
                }
                device = IntPtr.Zero;
            }
            */

            //Task.Run(() => _cpu.Run());
        }

        private void CreateProjection()
        {
            // TODO: Unfuck
            /*
            var scx = _cpu.IOController.SCX;
            var scy = _cpu.IOController.SCY;
            _projection = Matrix4.CreateOrthographicOffCenter(scx, 160 + scx, 144 + scy, scy, 0.1f, 4000f);
            */

            _projection = Matrix4.CreateOrthographicOffCenter(0, 160, 144, 0, 0.1f, 4000f);
        }

        private int CompileShader()
        {
            int vs = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs, File.ReadAllText("Shader/vertexShader.vert"));
            GL.CompileShader(vs);

            int fs = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs, File.ReadAllText("Shader/fragmentShader.frag"));
            GL.CompileShader(fs);

            int program = GL.CreateProgram();
            GL.AttachShader(program, fs);
            GL.AttachShader(program, vs);
            GL.LinkProgram(program);

            GL.DetachShader(program, fs);
            GL.DetachShader(program, vs);
            GL.DeleteShader(fs);
            GL.DeleteShader(vs);

            return program;
        }

        protected override void OnLoad(EventArgs e)
        {
            CursorVisible = true;

            CreateProjection();
            GL.Viewport(0, 0, 160 * _scale, 144 * _scale);

            _program = CompileShader();
            _videoDataManager = new VideoDataManager(_cpu);

            _background = new Background();

            for (int i = 0; i < 40; i++)
            {
                //_sprites.Add(new Sprite(new Vector2((i % 16) * 8, (i / 16) * 8), (byte)i, false));
                _sprites.Add(new Sprite(new Vector2(-8, -8), 0, false));
            }

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.PatchParameter(PatchParameterInt.PatchVertices, 4);
        }

        private void GenerateTiles()
        {
            if (!_cpu.IOController.IsVBlank)
            {
                if ((_cpu.IOController.LCDC & LCDC.BGTileMapDisplaySelect) == 0)
                {
                    if ((_cpu.IOController.LCDC & LCDC.BGWindowTileDataSelect) == 0)
                    {
                        for (int i = 0; i < 1024; i++)
                        {
                            sbyte tile = (sbyte)_cpu.BackgroundTileMap1[i];
                            _background[i] = (byte)(tile + 128);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 1024; i++) _background[i] = _cpu.BackgroundTileMap1[i];
                    }
                }
                else
                {
                    if ((_cpu.IOController.LCDC & LCDC.BGWindowTileDataSelect) == 0)
                    {
                        for (int i = 0; i < 1024; i++)
                        {
                            sbyte tile = (sbyte)_cpu.BackgroundTileMap2[i];
                            _background[i] = (byte)(tile + 128);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 1024; i++) _background[i] = _cpu.BackgroundTileMap2[i];
                    }
                }
            }
            _videoDataManager.ReloadTileData();
            _background.Update();
        }

        private void GenerateSprites()
        {
            if (!_cpu.IOController.IsVBlank)
            {
                for (int i = 0; i < 40; i++)
                {
                    int yPos = _cpu.ObjectAttributeMemory[i * 4];
                    int xPos = _cpu.ObjectAttributeMemory[i * 4 + 1];
                    byte pattern = _cpu.ObjectAttributeMemory[i * 4 + 2];

                    _sprites[i]?.Dispose();
                    _sprites[i] = new Sprite(new Vector2(xPos - 8, yPos - 16), pattern, false);
                }

                //_sprites[0] = new Sprite(new Vector2(0x10 - 8, 0x80 - 16), 0x58, false);
            }

            _videoDataManager.ReloadSpriteData();
        }

        protected override void OnClosed(EventArgs e)
        {
            GL.DeleteProgram(_program);

            foreach (var sprite in _sprites) sprite.Dispose();
            _background.Dispose();

            base.OnClosed(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            while (_cpu.IOController.IsVBlank) _cpu.Step();

            CreateProjection();
            GenerateTiles();
            GenerateSprites();
            HandleKeyboard();

            while (!_cpu.IOController.IsVBlank) _cpu.Step();
        }

        private void HandleKeyboard()
        {
            var keyState = Keyboard.GetState();

            IOController.InputEnum gbKeyState = 0;

            if (keyState.IsKeyDown(Key.Escape)) Exit();
            if (keyState.IsKeyDown(Key.Up)) gbKeyState |= IOController.InputEnum.Up;
            if (keyState.IsKeyDown(Key.Down)) gbKeyState |= IOController.InputEnum.Down;
            if (keyState.IsKeyDown(Key.Left)) gbKeyState |= IOController.InputEnum.Left;
            if (keyState.IsKeyDown(Key.Right)) gbKeyState |= IOController.InputEnum.Right;
            if (keyState.IsKeyDown(Key.A)) gbKeyState |= IOController.InputEnum.A;
            if (keyState.IsKeyDown(Key.S)) gbKeyState |= IOController.InputEnum.B;
            if (keyState.IsKeyDown(Key.Enter)) gbKeyState |= IOController.InputEnum.Start;
            if (keyState.IsKeyDown(Key.ShiftLeft)) gbKeyState |= IOController.InputEnum.Select;

            _cpu.IOController.UpdateInput(gbKeyState);
            if (gbKeyState != 0) _cpu.IsHalted = false;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(_clearColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_program);
            GL.UniformMatrix4(20, false, ref _projection);

            GL.BindTexture(TextureTarget.Texture2D, _videoDataManager.TileTexture);
            if ((_cpu.IOController.LCDC & LCDC.BgAndWindowDisplay) > 0 && (_cpu.IOController.LCDC & LCDC.LCDControlOperation) > 0) _background.Render();

            GL.BindTexture(TextureTarget.Texture2D, _videoDataManager.SpriteTexture);
            if ((_cpu.IOController.LCDC & LCDC.OBJDisplay) > 0 && (_cpu.IOController.LCDC & LCDC.LCDControlOperation) > 0) foreach (var sprite in _sprites) sprite.Render();

            SwapBuffers();
        }
    }
}
