using System;
using System.Diagnostics;

namespace ViruBoy
{
    public class IOController
    {
        public P1 P1 { get; set; }
        public byte SB { get; set; } // Not used
        public byte SC { get; set; } // Not used
        public byte DIV { get; set; }
        public byte TIMA { get; set; }
        public byte TMA { get; set; } // TODO: Use it
        public TAC TAC { get; set; }
        public LCDC LCDC { get; set; } = (LCDC)0x91;
        public byte LY { get; set; }
        public InterruptEnableRegister IF { get; set; }
        public STAT STAT { get; set; } // TODO: Everything below this line
        public byte LYC { get; set; } 
        public byte SCY { get; set; }
        public byte SCX { get; set; }
        public BGP BGP { get; set; }
        public BGP OBP0 { get; set; }
        public BGP OBP1 { get; set; }
        public byte WY { get; set; }
        public byte WX { get; set; }
        public byte DMA { get; set; }

        public bool IsVBlank => LY >= 144;
        public byte InputState
        {
            get
            {
                if ((P1 & P1.P14Out) == 0)
                {
                    return (byte)((byte)P1 | ((byte)~_inputState & 0x0F));
                }
                else if ((P1 & P1.P15Out) == 0)
                {
                    return (byte)((byte)P1 | (((byte)~_inputState & 0xF0) >> 4));
                }
                else return (byte)((byte)P1 | 0x0F);
            }
        }

        private InputEnum _inputState;

        public int TimerClockSpeed
        {
            get
            {
                byte clockSpeed = (byte)(TAC & (TAC.ClockSelectSpeed1 | TAC.ClockSelectSpeed2));

                switch (clockSpeed)
                {
                    case 0:
                        return 1024;
                    case 1:
                        return 16;
                    case 2:
                        return 64;
                    case 3:
                        return 256;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private const double _vsync = 57.93f;
        private const int _lines = 153;
        private const int _divSpeed = 16384;
        private const double _milliPerFrame = 1000.0 / _vsync;

        private int _divLeft = 256;
        private int _lyLeft = 456;
        private int _tacLeft = 0;
        private ulong _lastMachineClocks;

        public void Step(ulong clock)
        {
            int clockDifference = (int)(clock - _lastMachineClocks);
            _divLeft -= clockDifference;
            _lyLeft -= clockDifference;

            if (_lyLeft <= 0)
            {
                _lyLeft += 456;


                LY++;
                if (LY == 154) LY = 0;
                else if (LY == 144) IF |= InterruptEnableRegister.VBlank;

                if (LYC == LY) STAT |= STAT.LYCEqLYCoincidence;
                else STAT &= ~STAT.LYCEqLYCoincidence;
            }

            if (_divLeft <= 0)
            {
                _divLeft += 256;

                DIV++;
            }

            if ((TAC & TAC.TimeStop) != 0)
            {
                _tacLeft -= clockDifference;
                if (_tacLeft <= 0)
                {
                    _tacLeft += TimerClockSpeed;

                    if (TIMA + 1 > 0xFF)
                    {
                        TIMA = TMA;
                        IF |= InterruptEnableRegister.TimerOverflow;
                    }
                    else TIMA++;
                }
            }

            _lastMachineClocks = clock;
        }

        public byte ReadMemory(ushort address)
        {
            switch (address)
            {
                case 0xFF00:
                    return InputState;
                case 0xFF01:
                    return SB;
                case 0xFF02:
                    return SC;
                case 0xFF04:
                    return DIV;
                case 0xFF05:
                    return TIMA;
                case 0xFF06:
                    return TMA;
                case 0xFF07:
                    return (byte)TAC;
                case 0xFF0F:
                    return (byte)IF;
                case 0xFF40:
                    return (byte)LCDC;
                case 0xFF41:
                    return (byte)STAT;
                case 0xFF42:
                    return SCY;
                case 0xFF43:
                    return SCX;
                case 0xFF44:
                    return LY;
                case 0xFF45:
                    return LYC;
                case 0xFF47:
                    return (byte)BGP;
                case 0xFF48:
                    return (byte)OBP0;
                case 0xFF49:
                    return (byte)OBP1;
                case 0xFF4A:
                    return WY;
                case 0xFF4B:
                    return WX;
                case 0xFF4D:
                    return 0xFF; // TODO: Speed switching
                default:
                    throw new NotImplementedException("Unknown IO register");
            }
        }

        public void WriteMemory(ushort address, byte data)
        {
            if (address >= 0xFF10 && address <= 0xFF3F)
            {
                //Debug.WriteLine("Beep boop, sound shit");
                return;
            }

            switch (address)
            {
                case 0xFF00:
                    P1 = (P1)data;
                    break;
                case 0xFF01:
                    SB = data;
                    break;
                case 0xFF02:
                    SC = data;
                    break;
                case 0xFF04:
                    DIV = 0;
                    break;
                case 0xFF05:
                    TIMA = data;
                    break;
                case 0xFF06:
                    TMA = data;
                    break;
                case 0xFF07:
                    TAC = (TAC)data;
                    break;
                case 0xFF0F:
                    IF = (InterruptEnableRegister)data;
                    break;
                case 0xFF40:
                    LCDC = (LCDC)data;
                    break;
                case 0xFF41:
                    STAT = (STAT)data;
                    break;
                case 0xFF42:
                    SCY = data;
                    break;
                case 0xFF43:
                    SCX = data;
                    break;
                case 0xFF44:
                    LY = 0;
                    break;
                case 0xFF45:
                    LYC = data;
                    break;
                case 0xFF46:
                    DMA = data;
                    break;
                case 0xFF47:
                    BGP = (BGP) data;
                    break;
                case 0xFF48:
                    OBP0 = (BGP)data;
                    break;
                case 0xFF49:
                    OBP1 = (BGP)data;
                    break;
                case 0xFF4A:
                    WY = data;
                    break;
                case 0xFF4B:
                    WX = data;
                    break;
                default:
                    //throw new NotImplementedException("Unknown IO register");
                    Debug.WriteLine("Unknown IO regiser write");
                    return;
            }
        }

        [Flags]
        public enum InputEnum
        {
            Right = 1,
            Left = 2,
            Up = 4,
            Down = 8,
            A = 16,
            B = 32,
            Select = 64,
            Start = 128

        }

        public void UpdateInput(InputEnum input)
        {
            _inputState = input;
        }
    }
}
