using System;
using System.Diagnostics;

namespace ViruBoy
{
    public class VirtualCpu
    {
        public const ushort IEAddress = 0xFFFF;
        public const ushort HighRamAddress = 0xFF80;
        public const ushort EmptyIO2Address = 0xFF4C;
        public const ushort IOAddress = 0xFF00;
        public const ushort EmptyIO1Address = 0xFEA0;
        public const ushort SpriteAttribAddress = 0xFE00;
        public const ushort InternalRamEchoAddress = 0xE000;
        public const ushort InternalRamAddress = 0xC000;
        public const ushort SwitchableRamAddress = 0xA000;
        public const ushort VideoRamAddress = 0x8000;
        public const ushort SwitchableRomBankAddress = 0x4000;
        public const ushort RomBank0Address = 0x0000;


        private const byte Low4Bitmask = 0b0000_1111;

        private const ushort Low12Bitmask = 0b0000_1111_1111_1111;

        private FlagRegister _f = (FlagRegister)0xB0;

        public FlagRegister F {
            get => _f;
            set => _f = (FlagRegister)((byte)value & 0b11110000);
        }
        public InterruptEnableRegister IE { get; private set; } // TODO: Maybe move?

        public bool NewInterruptState { get; private set; }
        public bool InterruptsEnabled { get; private set; } = true;
        public bool InterruptChangeWait { get; private set; } = true;
        public bool IsHalted = false;

        public Cartridge LoadedCartridge { get; private set; }
        public IOController IOController { get; } = new IOController();
        public ushort PC { get; set; } = Cartridge.CodeStartAddress;

        public byte[] Registers { get; } = new byte[7]; // A, B, C, D, E, H, L
        public ushort SP { get; set; } = 0xFFFE;
        public ushort HL => (ushort)((Registers[5] << 8) | Registers[6]);

        private readonly byte[] _ram = new byte[8192];
        private readonly byte[] _vram = new byte[8192];
        private readonly byte[] _hram = new byte[128];
        private readonly byte[] _ao = new byte[160];

        public ulong MachineCycles { get; private set; } = 0;

        public VirtualCpu(Cartridge cartridge)
        {
            LoadedCartridge = cartridge ?? throw new ArgumentNullException(nameof(cartridge));

            Registers[0] = 0x01;
            Registers[1] = 0x00;
            Registers[2] = 0x13;
            Registers[4] = 0xD8;
            Registers[5] = 0x01;
            Registers[6] = 0x4D;

            WriteMemory(0xFF05, 0x00); // TIMA
            WriteMemory(0xFF06, 0x00); // TMA
            WriteMemory(0xFF07, 0x00); // TAC
            WriteMemory(0xFF10, 0x80); // NR10
            WriteMemory(0xFF11, 0xBF); // NR11
            WriteMemory(0xFF12, 0xF3); // NR12
            WriteMemory(0xFF14, 0xBF); // NR14
            WriteMemory(0xFF16, 0x3F); // NR21
            WriteMemory(0xFF17, 0x00); // NR22
            WriteMemory(0xFF19, 0xBF); // NR24
            WriteMemory(0xFF1A, 0x7F); // NR30
            WriteMemory(0xFF1B, 0xFF); // NR31
            WriteMemory(0xFF1C, 0x9F); // NR32
            WriteMemory(0xFF1E, 0xBF); // NR33
            WriteMemory(0xFF20, 0xFF); // NR41
            WriteMemory(0xFF21, 0x00); // NR42
            WriteMemory(0xFF22, 0x00); // NR43
            WriteMemory(0xFF23, 0xBF); // NR30
            WriteMemory(0xFF24, 0x77); // NR50
            WriteMemory(0xFF25, 0xF3); // NR51
            WriteMemory(0xFF26, 0xF1); // NR52
            WriteMemory(0xFF40, 0x91); // LCDC
            WriteMemory(0xFF42, 0x00); // SCY
            WriteMemory(0xFF43, 0x00); // SCX
            WriteMemory(0xFF45, 0x00); // LYC
            WriteMemory(0xFF47, 0xFC); // BGP
            WriteMemory(0xFF48, 0xFF); // OBP0
            WriteMemory(0xFF49, 0xFF); // OBP1
            WriteMemory(0xFF4A, 0x00); // WY
            WriteMemory(0xFF4B, 0x00); // WX
            WriteMemory(0xFFFF, 0x00); // IE
        }

        public void Run()
        {
            while (true)
            {
                Step();
            }
        }

        public void Step()
        {
            if (!IsHalted)
            {
                Span<byte> instruction;
                if (PC < SwitchableRomBankAddress) instruction = LoadedCartridge.Bank0.AsSpan().Slice(PC, 3);
                else if (PC >= HighRamAddress) instruction = _hram.AsSpan().Slice(PC - HighRamAddress, 3);
                else if (PC >= InternalRamAddress) instruction = _ram.AsSpan().Slice(PC - InternalRamAddress, 3);
                else instruction = LoadedCartridge.SwitchBank.AsSpan().Slice(PC - SwitchableRomBankAddress, 3);
                InterpretOpcode(instruction);
            }
            else MachineCycles += 4;

            if (InterruptChangeWait == false) InterruptChangeWait = true;
            else InterruptsEnabled = NewInterruptState;

            IOController.Step(MachineCycles);
            Interrupt(IOController.IF);
            
            if (IOController.DMA != 0)
            {
                int start = IOController.DMA << 8;

                for (int i = 0; i < 160; i++)
                {
                    _ao[i] = ResolveMemory((ushort)(start + i));
                }
                
                IOController.DMA = 0;
            }
        }

        public void Interrupt(InterruptEnableRegister type)
        {
            if (!InterruptsEnabled) return;
            if ((type & IE) == 0) return;

            IsHalted = false;
            InterruptsEnabled = false;
            IOController.IF = 0;

            byte lo = (byte)((PC & 0xFF00) >> 8);
            byte hi = (byte)(PC & 0x00FF);

            WriteMemory(--SP, lo);
            WriteMemory(--SP, hi);

            switch (type)
            {
                case InterruptEnableRegister.VBlank:
                    PC = 0x40;
                    break;
                case InterruptEnableRegister.LCDC:
                    PC = 0x48;
                    break;
                case InterruptEnableRegister.TimerOverflow:
                    PC = 0x50;
                    break;
                case InterruptEnableRegister.SerialComplete:
                    PC = 0x58;
                    break;
                case InterruptEnableRegister.TransitionHToLP10To13:
                    PC = 0x60;
                    break;
            }
        }

        public Span<byte> TilePatternTable1 => _vram.AsSpan(0x800, 0x1000);
        public Span<byte> TilePatternTable2 => _vram.AsSpan(0x0, 0x1000);
        public Span<byte> BackgroundTileMap1 => _vram.AsSpan(0x1800, 0x400);
        public Span<byte> BackgroundTileMap2 => _vram.AsSpan(0x1C00, 0x400);
        public Span<byte> SpritePatternTable => _vram.AsSpan(0x0, 0x1000);
        public Span<byte> ObjectAttributeMemory => _ao.AsSpan();

        public byte ResolveMemory(ushort address)
        {
            // TODO: Off by one hurting juice
            if (address == IEAddress) return (byte)IE;
            else if (address >= HighRamAddress) return _hram[address - HighRamAddress];
            else if (address >= EmptyIO2Address) return 0xFF; // This is actually empty
            else if (address >= IOAddress) return IOController.ReadMemory(address);
            else if (address >= EmptyIO1Address) return 0xFF; // This is also empty
            else if (address >= SpriteAttribAddress) return _ao[address - SpriteAttribAddress];
            else if (address >= InternalRamEchoAddress) return _ram[address - InternalRamEchoAddress];
            else if (address >= InternalRamAddress) return _ram[address - InternalRamAddress];
            else if (address >= SwitchableRamAddress) return LoadedCartridge.RamBank[address - SwitchableRamAddress];
            else if (address >= VideoRamAddress) return _vram[address - VideoRamAddress];
            else if (address >= SwitchableRomBankAddress) return LoadedCartridge.SwitchBank[address - SwitchableRomBankAddress];
            else return LoadedCartridge.Bank0[address];
        }

        public void WriteMemory(ushort address, byte data)
        {
            if (address < VideoRamAddress) LoadedCartridge.WriteMemory(address, data);
            else if (address == IEAddress) IE = (InterruptEnableRegister) data;
            else if (address >= HighRamAddress) _hram[address - HighRamAddress] = data;
            else if (address >= EmptyIO2Address) return;
            else if (address >= IOAddress) IOController.WriteMemory(address, data);
            else if (address >= EmptyIO1Address) return;
            else if (address >= SpriteAttribAddress) _ao[address - SpriteAttribAddress] = data;
            else if (address >= InternalRamEchoAddress) _ram[address - InternalRamEchoAddress] = data;
            else if (address >= InternalRamAddress) _ram[address - InternalRamAddress] = data;
            else if (address >= SwitchableRamAddress) LoadedCartridge.RamBank[address - SwitchableRamAddress] = data;
            else if (address >= VideoRamAddress) _vram[address - VideoRamAddress] = data;
            else if (address >= SwitchableRomBankAddress) IOController.WriteMemory(address, data);
        }

        private byte CarryAdd => (byte)((byte)(F & FlagRegister.Carry) >> 4);

        private void InterpretOpcode(Span<byte> opcode)
        {
            sbyte offset;
            ushort address; // TODO: Get that shit outta here

            switch (opcode[0])
            {
                case 0x00: // NOP
                    PC++;
                    MachineCycles += 4;
                    return;
                case 0x07: // RLCA
                    if ((Registers[0] & 0x80) > 0) F |= FlagRegister.Carry;
                    else F &= ~FlagRegister.Carry;

                    Registers[0] = (byte)((Registers[0] << 1) | ((Registers[0] & 0x80) >> 7));

                    F &= ~FlagRegister.NSubstract;
                    F &= ~FlagRegister.HalfCarry;
                    F &= ~FlagRegister.Zero;

                    PC++;
                    MachineCycles += 4;
                    return;
                case 0x08: // LD (a16),SP
                    ushort value = (ushort)((opcode[2] << 8) | opcode[1]);
                    WriteMemory(value, (byte)(SP & 0x00FF));
                    WriteMemory((ushort)(value + 1), (byte)((SP & 0xFF00) >> 8));

                    PC += 3;
                    MachineCycles += 20;
                    return;
                case 0x0A: // LD A,(BC)
                    Registers[0] = ResolveMemory(ReadRPTable(0));
                    PC++;
                    MachineCycles += 8;
                    return;
                case 0x0F: // RRCA
                    if ((Registers[0] & 0x01) > 0) F |= FlagRegister.Carry;
                    else F &= ~FlagRegister.Carry;

                    Registers[0] = (byte)((Registers[0] >> 1) | ((Registers[0] & 0x01) << 7));

                    F &= ~FlagRegister.Zero;
                    F &= ~FlagRegister.NSubstract;
                    F &= ~FlagRegister.HalfCarry;

                    PC++;
                    MachineCycles += 4;
                    return;
                case 0x10: // STOP
                    Debug.WriteLine("Stopping...");
                    IsHalted = true;

                    MachineCycles += 4;
                    PC += 2;
                    return;
                case 0x17: // RLA
                    bool oldCarry = (F & FlagRegister.Carry) > 0;

                    if ((Registers[0] & 0x80) > 0) F |= FlagRegister.Carry;
                    else F &= ~FlagRegister.Carry;

                    Registers[0] <<= 1;
                    if (oldCarry) Registers[0] |= 1;

                    F &= ~FlagRegister.Zero;
                    F &= ~FlagRegister.NSubstract;
                    F &= ~FlagRegister.HalfCarry;

                    PC++;
                    MachineCycles += 4;
                    return;
                case 0x18: // JR 8
                    sbyte relJump = (sbyte) opcode[1];
                    PC = (ushort)(PC + relJump);

                    PC += 2;
                    MachineCycles += 12;
                    return;
                case 0x1A: // LD A,(DE)
                    Registers[0] = ResolveMemory(ReadRPTable(1));

                    PC++;
                    MachineCycles += 8;
                    return;
                case 0x1F: // RRA
                    bool oldCarryRight = (F & FlagRegister.Carry) > 0;

                    if ((Registers[0] & 0x01) > 0) F |= FlagRegister.Carry;
                    else F &= ~FlagRegister.Carry;

                    Registers[0] >>= 1;
                    if (oldCarryRight) Registers[0] |= 0x80;

                    F &= ~FlagRegister.Zero;
                    F &= ~FlagRegister.NSubstract;
                    F &= ~FlagRegister.HalfCarry;

                    PC++;
                    MachineCycles += 4;
                    return;
                case 0x20: // JR NZ, r8
                    offset = (sbyte)opcode[1];
                    if ((byte)(F & FlagRegister.Zero) == 0)
                    {
                        PC = (ushort)(PC + offset);
                        MachineCycles += 4;
                    }

                    MachineCycles += 8;
                    PC += 2;
                    return;
                case 0x27: // DAA
                    // TODO: Maybe figure out what the fuck it's doing?
                    int tmp = Registers[0];

                    if ((F & FlagRegister.NSubstract) == 0)
                    {
                        if ((F & FlagRegister.HalfCarry) > 0 || (tmp & 0x0F) > 9) tmp += 6;
                        if ((F & FlagRegister.Carry) > 0 || tmp > 0x9F) tmp += 0x60;
                    }
                    else
                    {
                        if ((F & FlagRegister.HalfCarry) > 0)
                        {
                            tmp -= 6;
                            if ((F & FlagRegister.Carry) == 0)tmp &= 0xFF;
                        }
                        if ((F & FlagRegister.Carry) > 0) tmp -= 0x60;
                    }
                    F &= ~(FlagRegister.HalfCarry | FlagRegister.Zero);
                    if ((tmp & 0x100) > 0) F |= FlagRegister.Carry;
                    Registers[0] = (byte)(tmp & 0xFF);

                    if (Registers[0] == 0) F |= FlagRegister.Zero;
                    else F &= ~FlagRegister.Zero;

                    PC++;
                    MachineCycles += 4;
                    return;
                case 0x28: // JR Z,r8
                    offset = (sbyte)opcode[1];
                    if ((byte)(F & FlagRegister.Zero) != 0)
                    {
                        PC = (ushort)(PC + offset);
                        MachineCycles += 4;
                    }

                    MachineCycles += 8;
                    PC += 2;
                    return;
                case 0x2A: // LD A,(HL+)
                    Registers[0] = ResolveMemory(HL);
                    WriteRPTable(2, (ushort)(HL + 1));

                    PC++;
                    MachineCycles += 8;
                    return;
                case 0x2F: // CPL
                    Registers[0] = (byte)~Registers[0];

                    F |= FlagRegister.NSubstract;
                    F |= FlagRegister.HalfCarry;

                    PC++;
                    MachineCycles += 4;
                    return;
                case 0x30: // JR NC,r8
                    offset = (sbyte)opcode[1];
                    if ((F & FlagRegister.Carry) == 0)
                    {
                        PC = (ushort)(PC + offset);
                        MachineCycles += 4;
                    }

                    MachineCycles += 8;
                    PC += 2;
                    return;
                case 0x37: // SCF
                    F |= FlagRegister.Carry;
                    F &= ~FlagRegister.NSubstract;
                    F &= ~FlagRegister.HalfCarry;

                    MachineCycles += 4;
                    PC++;
                    return;
                case 0x38: // JR C,r8
                    offset = (sbyte)opcode[1];
                    if ((byte)(F & FlagRegister.Carry) != 0)
                    {
                        PC = (ushort)(PC + offset);
                        MachineCycles += 4;
                    }

                    MachineCycles += 8;
                    PC += 2;
                    return;
                case 0x3A: // LD A,(HL-)
                    Registers[0] = ResolveMemory(HL);
                    WriteRPTable(2, (ushort)(HL - 1));

                    PC++;
                    MachineCycles += 8;
                    return;
                case 0x3F: // CCF

                    if ((F & FlagRegister.Carry) == 0) F |= FlagRegister.Carry;
                    else F &= ~FlagRegister.Carry;

                    F &= ~FlagRegister.NSubstract;
                    F &= ~FlagRegister.HalfCarry;

                    PC++;
                    MachineCycles += 4;
                    return;
                case 0xC0: // RET NZ

                    if ((F & FlagRegister.Zero) == 0)
                    {
                        byte hiRZ = ResolveMemory(SP++);
                        byte loRZ = ResolveMemory(SP++);

                        PC = (ushort) ((loRZ << 8) | hiRZ);
                        MachineCycles += 12;
                    }
                    else PC++;

                    MachineCycles += 8;
                    return;
                case 0xC2: // JP NZ,a16
                    if ((F & FlagRegister.Zero) == 0)
                    {
                        PC = (ushort) ((opcode[2] << 8) | opcode[1]);
                        MachineCycles += 4;
                    }
                    else PC += 3;

                    MachineCycles += 12;
                    return;
                case 0xC3: // JP a16
                    PC = (ushort)((opcode[2] << 8) | opcode[1]);
                    MachineCycles += 16;
                    return;
                case 0xC4: // CALL NZ,a16
                    if ((F & FlagRegister.Zero) == 0)
                    {
                        address = (ushort) ((opcode[2] << 8) | opcode[1]);
                        ushort returnAddressNz = (ushort) (PC + 3);
                        byte loNz = (byte) ((returnAddressNz & 0xFF00) >> 8);
                        byte hiNz = (byte) (returnAddressNz & 0x00FF);

                        WriteMemory(--SP, loNz);
                        WriteMemory(--SP, hiNz);
                        PC = address;
                        MachineCycles += 12;
                    }
                    else PC += 3;

                    MachineCycles += 12;
                    return;
                case 0xC8: // RET Z
                    if ((F & FlagRegister.Zero) != 0)
                    {
                        byte hiRZ = ResolveMemory(SP++);
                        byte loRZ = ResolveMemory(SP++);

                        PC = (ushort)((loRZ << 8) | hiRZ);
                        MachineCycles += 12;
                    }
                    else PC++;

                    MachineCycles += 8;
                    return;
                case 0xC9: // RET
                    byte hiR = ResolveMemory(SP++);
                    byte loR = ResolveMemory(SP++);

                    PC = (ushort)((loR << 8) | hiR);
                    MachineCycles += 16;
                    return;
                case 0xCA: // JP Z,a16
                    if ((F & FlagRegister.Zero) != 0)
                    {
                        address = (ushort)((opcode[2] << 8) | opcode[1]);
                        PC = address;

                        MachineCycles += 12;
                    }
                    else PC += 3;

                    MachineCycles += 4;
                    return;
                case 0xCB: // CB Prefix
                    CBPrefix(opcode.Slice(1));
                    return;
                case 0xCC: // CALL Z,a16
                    if ((F & FlagRegister.Zero) > 0)
                    {
                        address = (ushort)((opcode[2] << 8) | opcode[1]);
                        ushort returnAddressZ = (ushort)(PC + 3);
                        byte loZ = (byte)((returnAddressZ & 0xFF00) >> 8);
                        byte hiZ = (byte)(returnAddressZ & 0x00FF);

                        WriteMemory(--SP, loZ);
                        WriteMemory(--SP, hiZ);
                        PC = address;
                        MachineCycles += 24;
                    }
                    else PC += 3;

                    MachineCycles += 12;
                    return;
                case 0xCD: // CALL a16
                    address = (ushort)((opcode[2] << 8) | opcode[1]);
                    ushort returnAddress = (ushort)(PC + 3);
                    byte lo = (byte)((returnAddress & 0xFF00) >> 8);
                    byte hi = (byte)(returnAddress & 0x00FF);

                    WriteMemory(--SP, lo);
                    WriteMemory(--SP, hi);
                    PC = address;
                    MachineCycles += 24;
                    return;
                case 0xD0: // RET NC
                    if ((F & FlagRegister.Carry) == 0)
                    {
                        byte hiRiNC = ResolveMemory(SP++);
                        byte loRiNC = ResolveMemory(SP++);

                        PC = (ushort) ((loRiNC << 8) | hiRiNC);
                        MachineCycles += 12;
                    }
                    else PC++;

                    MachineCycles += 8;
                    return;
                case 0xD2: // JP NC,a16
                    if ((F & FlagRegister.Carry) == 0)
                    {
                        PC = (ushort)((opcode[2] << 8) | opcode[1]);
                        MachineCycles += 4;
                    }
                    else PC += 3;

                    MachineCycles += 12;
                    return;
                case 0xD4: // CALL NC,a16
                    if ((F & FlagRegister.Carry) == 0)
                    {
                        address = (ushort)((opcode[2] << 8) | opcode[1]);
                        ushort returnAddressNC = (ushort)(PC + 3);
                        byte loNC = (byte)((returnAddressNC & 0xFF00) >> 8);
                        byte hiNC = (byte)(returnAddressNC & 0x00FF);

                        WriteMemory(--SP, loNC);
                        WriteMemory(--SP, hiNC);
                        PC = address;

                        MachineCycles += 12;
                    }
                    else PC += 3;

                    MachineCycles += 12;
                    return;
                case 0xD8: // RET C
                    if ((F & FlagRegister.Carry) > 0)
                    {
                        byte hiRc = ResolveMemory(SP++);
                        byte loRc = ResolveMemory(SP++);

                        PC = (ushort)((loRc << 8) | hiRc);
                        MachineCycles += 12;
                    }
                    else PC++;

                    MachineCycles += 8;
                    return;
                case 0xD9: // RETI
                    byte hiRi = ResolveMemory(SP++);
                    byte loRi = ResolveMemory(SP++);

                    PC = (ushort)((loRi << 8) | hiRi);

                    NewInterruptState = true;
                    InterruptChangeWait = false;
                    MachineCycles += 8;
                    return;
                case 0xDA: // JP C,a16
                    if ((F & FlagRegister.Carry) > 0)
                    {
                        PC = (ushort)((opcode[2] << 8) | opcode[1]);
                        MachineCycles += 4;
                    }
                    else PC += 3;

                    MachineCycles += 12;
                    return;
                case 0xDC: // CALL C,a16
                    if ((F & FlagRegister.Carry) > 0)
                    {
                        address = (ushort)((opcode[2] << 8) | opcode[1]);
                        ushort returnAddressNC = (ushort)(PC + 3);
                        byte loNC = (byte)((returnAddressNC & 0xFF00) >> 8);
                        byte hiNC = (byte)(returnAddressNC & 0x00FF);

                        WriteMemory(--SP, loNC);
                        WriteMemory(--SP, hiNC);
                        PC = address;

                        MachineCycles += 12;
                    }
                    else PC += 3;

                    MachineCycles += 12;
                    return;
                case 0xE0: // LDH (a8),A
                    WriteMemory((ushort)(0xFF00 + opcode[1]), Registers[0]);
                    PC += 2;
                    MachineCycles += 12;
                    return;
                case 0xE2: // LD (C),A
                    WriteMemory((ushort)(0xFF00 + Registers[2]), Registers[0]);
                    PC++;
                    MachineCycles += 8;
                    return;
                case 0xE8: // ADD SP,r8
                    F &= ~ FlagRegister.NSubstract;
                    F &= ~ FlagRegister.Zero;

                    sbyte spOffset = (sbyte) opcode[1];
                    ushort oldSp = SP;

                    SP = (ushort)(SP + spOffset);

                    if ((oldSp & 0xFF) + opcode[1] > byte.MaxValue) F |= FlagRegister.Carry;
                    else F &= ~FlagRegister.Carry;

                    if ((oldSp & Low4Bitmask) + (opcode[1] & Low4Bitmask) >= 0x10) F |= FlagRegister.HalfCarry;
                    else F &= ~FlagRegister.HalfCarry;

                    PC += 2;
                    MachineCycles += 16;
                    return;
                case 0xE9: // JP (HL) | ACTUALLY JP HL
                    PC = HL; // THIS IS ABSOLUTELY CORRECT, FUTURE ME: DON'T TOUCH THIS, K?
                    MachineCycles += 4;
                    return;
                case 0xEA: // LD (a16),A
                    address = (ushort)((opcode[2] << 8) | opcode[1]);
                    WriteMemory(address, Registers[0]);
                    PC += 3;
                    MachineCycles += 16;
                    return;
                case 0xF0: // LDH A,(a8)
                    Registers[0] = ResolveMemory((ushort)(0xFF00 + opcode[1]));
                    PC += 2;
                    MachineCycles += 12;
                    return;
                case 0xF2: // LD A,(C)
                    Registers[0] = ResolveMemory((ushort)(0xFF00 + Registers[2]));

                    PC++;
                    MachineCycles += 8;
                    return;
                case 0xF3: // DI
                    NewInterruptState = false;
                    InterruptChangeWait = false;
                    PC++;
                    MachineCycles += 4;
                    return;
                case 0xF8: // LD HL,SP+r8
                    sbyte spOffset2 = (sbyte) opcode[1];
                    ushort oldSp2 = SP;
                    WriteRP2Table(2, (ushort)(SP + spOffset2));

                    F &= ~FlagRegister.Zero;
                    F &= ~FlagRegister.NSubstract;

                    if ((oldSp2 & 0xFF) + opcode[1] > byte.MaxValue) F |= FlagRegister.Carry;
                    else F &= ~FlagRegister.Carry;

                    if ((oldSp2 & Low4Bitmask) + (opcode[1] & Low4Bitmask) >= 0x10) F |= FlagRegister.HalfCarry;
                    else F &= ~FlagRegister.HalfCarry;

                    PC += 2;
                    MachineCycles += 12;
                    return;
                case 0xF9: // LD SP,HL
                    SP = HL;
                    PC++;
                    MachineCycles += 8;
                    return;
                case 0xFA: // LD A,(a16)
                    address = (ushort)((opcode[2] << 8) | opcode[1]);
                    Registers[0] = ResolveMemory(address);

                    PC += 3;
                    MachineCycles += 16;
                    return;
                case 0xFB: // EI
                    NewInterruptState = true;
                    InterruptChangeWait = false;
                    PC++;
                    MachineCycles += 4;
                    return;
            }

            byte x = (byte)((opcode[0] & 0b11_000_000) >> 6);
            byte y = (byte)((opcode[0] & 0b00_111_000) >> 3);
            byte z = (byte)(opcode[0] & 0b00_000_111);
            byte p = (byte)((y & 0b110) >> 1);
            byte q = (byte)(y & 0b001);


            if (x == 2) Alu(y, z); // ADD A, | ADC A, | SUB | SBC A, | AND | XOR | OR | CP
            else if (x == 0 && z == 1)
            {
                ushort val = (ushort)((opcode[2] << 8) | opcode[1]);

                if (q == 0) // LD rp[p], nn
                {
                    WriteRPTable(p, val);
                    PC += 3;

                    MachineCycles += 12;
                }
                else // ADD HL, rp[p]
                {
                    ushort before = HL;
                    ushort added = ReadRPTable(p);
                    WriteRPTable(2, (ushort)(added + HL));

                    if (HL <= before && added != 0 && before != 0) F |= FlagRegister.Carry;
                    else F &= ~FlagRegister.Carry;

                    ushort mask1 = (ushort)((before & Low12Bitmask) << 4);
                    ushort mask2 = (ushort)((added & Low12Bitmask) << 4);
                    ushort masks = (ushort)(mask1 + mask2);
                    if (masks <= mask1 && mask1 != 0 && mask2 != 0) F |= FlagRegister.HalfCarry;
                    else F &= ~FlagRegister.HalfCarry;

                    F &= ~FlagRegister.NSubstract;

                    PC++;
                    MachineCycles += 8;
                }
            }

            else if (x == 0 && z == 2 && q == 0) // TODO: Some GB specific shit?
            {
                ushort rp = ReadRPTable(Math.Min(p, (byte) 2)); // It goes 0,1,2,2
                WriteMemory(rp, Registers[0]);

                if (p == 2) WriteRPTable(2, (ushort)(rp + 1)); // LD (nn), HL
                else if (p == 3) WriteRPTable(2, (ushort)(rp - 1)); // LD (nn), A

                MachineCycles += 8;
                PC++;
            }
            else if (x == 0 && z == 3 && q == 0) // INC rp[p]
            {
                WriteRPTable(p, (ushort)(ReadRPTable(p) + 1));

                PC++;
                MachineCycles += 8;
            }
            else if (x == 0 && z == 3 && q == 1) // DEC rp[p]
            {
                WriteRPTable(p, (ushort)(ReadRPTable(p) - 1));

                PC++;
                MachineCycles += 8;
            }
            else if (x == 0 && z == 4) // INC r[y]
            {
                byte val = ReadRTable(y);

                if ((byte)(val + 1) == 0) F |= FlagRegister.Zero;
                else F &= ~FlagRegister.Zero;

                if ((val & 0x0F) == 0x0F) F |= FlagRegister.HalfCarry;
                else F &= ~FlagRegister.HalfCarry;

                F &= ~FlagRegister.NSubstract;

                MachineCycles += 4;
                if (y == 6) MachineCycles += 8;

                WriteRTable(y, (byte)(val + 1));
                PC++;
            }
            else if (x == 0 && z == 5) // DEC r[y]
            {
                byte val = ReadRTable(y);
                if ((byte)(val - 1) == 0) F |= FlagRegister.Zero;
                else F &= ~FlagRegister.Zero;

                if ((val & 0x0F) == 0x00) F |= FlagRegister.HalfCarry;
                else F &= ~FlagRegister.HalfCarry;

                F |= FlagRegister.NSubstract;

                MachineCycles += 4;
                if (y == 6) MachineCycles += 8;

                WriteRTable(y, (byte)(val - 1));
                PC++;
            }
            else if (x == 0 && z == 6) // LD r[y], n
            {
                WriteRTable(y, opcode[1]);
                PC += 2;
                MachineCycles += 8;
                if (y == 6) MachineCycles += 4;
            }
            else if (x == 1)
            {
                if (y == 6 && z == 6)
                {
                    MachineCycles += 4;
                    PC++;
                    //Debug.WriteLine("Halting...");
                    IsHalted = true;

                    return;
                }

                WriteRTable(y, ReadRTable(z)); // LD r[y], r[z]

                if (y == 6 || z == 6) MachineCycles += 8;
                else MachineCycles += 4;
                PC++;
            }
            else if (x == 3 && z == 6) AluARegister(y, opcode[1]); // TODO: Maybe move the PC++ out? | alu[y] n
            else if (x == 3 && z == 1 && q == 0) // POP rp2[p]
            {
                byte hiR = ResolveMemory(SP++);
                byte loR = ResolveMemory(SP++);

                WriteRP2Table(p, (ushort)((loR << 8) | hiR));

                PC++;
                MachineCycles += 12;
            }
            else if (x == 3 && z == 5 && q == 0) // PUSH rp2[p]
            {
                ushort data = ReadRP2Table(p);

                byte lo = (byte)((data & 0xFF00) >> 8);
                byte hi = (byte)(data & 0x00FF);

                WriteMemory(--SP, lo);
                WriteMemory(--SP, hi);

                PC++;
            }
            else if (x == 3 && z == 7)
            {
                ushort resetLocation = (ushort)(y * 8);
                //Debug.Write("Reset 0x" + resetLocation.ToString("X"));
                PC++;

                byte lo = (byte)((PC & 0xFF00) >> 8);
                byte hi = (byte)(PC & 0x00FF);

                WriteMemory(--SP, lo);
                WriteMemory(--SP, hi);
                PC = resetLocation;
                MachineCycles += 16;
            }
            else throw new NotImplementedException("Unknown opcode");
        }

        private void AluARegister(byte operation, byte data)
        {
            MachineCycles += 8;
            byte original = Registers[0];

            switch (operation)
            {
                case 0: // ADD A, n
                    Registers[0] += data;

                    if (Registers[0] == 0) F |= FlagRegister.Zero;
                    else F &= ~FlagRegister.Zero;

                    F &= ~FlagRegister.NSubstract;

                    if (original + data > byte.MaxValue) F |= FlagRegister.Carry;
                    else F &= ~FlagRegister.Carry;

                    byte mask1 = (byte)((original & Low4Bitmask) << 4);
                    byte mask2 = (byte)((data & Low4Bitmask) << 4);
                    byte masks = (byte)(mask1 + mask2);
                    if (masks <= mask1 && mask1 != 0 && mask2 != 0) F |= FlagRegister.HalfCarry;
                    else F &= ~FlagRegister.HalfCarry;

                    break;
                case 1: // ADC A, n
                    Registers[0] += (byte)(data + CarryAdd);

                    if (Registers[0] == 0) F |= FlagRegister.Zero;
                    else F &= ~FlagRegister.Zero;

                    F &= ~FlagRegister.NSubstract;

                    if ((((original & 0x0F) + (data & 0x0F) + CarryAdd) & 0xF0) != 0) F |= FlagRegister.HalfCarry;
                    else F &= ~FlagRegister.HalfCarry;

                    if (original + data + CarryAdd > byte.MaxValue) F |= FlagRegister.Carry;
                    else F &= ~FlagRegister.Carry;
                    break;
                case 2: // SUB A, n
                    Registers[0] -= data;

                    if (Registers[0] == 0) F |= FlagRegister.Zero;
                    else F &= ~FlagRegister.Zero;

                    F |= FlagRegister.NSubstract;

                    if (data > original) F |= FlagRegister.Carry;
                    else F &= ~FlagRegister.Carry;

                    if ((original & 0x0F) < (data & 0x0F)) F |= FlagRegister.HalfCarry;
                    else F &= ~FlagRegister.HalfCarry;
                    break;
                case 3: // SBC A, n
                    Registers[0] -= (byte)(data + CarryAdd);

                    if (Registers[0] == 0) F |= FlagRegister.Zero;
                    else F &= ~FlagRegister.Zero;

                    F |= FlagRegister.NSubstract;

                    if ((((original & 0x0F) - (data & 0x0F) - CarryAdd) & 0xF0) != 0) F |= FlagRegister.HalfCarry;
                    else F &= ~FlagRegister.HalfCarry;

                    if (original - data - CarryAdd < 0) F |= FlagRegister.Carry;
                    else F &= ~FlagRegister.Carry;
                    break;
                case 4: // AND A, n
                    Registers[0] &= data;

                    if (Registers[0] == 0) F |= FlagRegister.Zero;
                    else F &= ~FlagRegister.Zero;

                    F &= ~FlagRegister.NSubstract;
                    F |= FlagRegister.HalfCarry;
                    F &= ~FlagRegister.Carry;
                    break;
                case 5: // XOR A, n
                    Registers[0] ^= data;

                    if (Registers[0] == 0) F |= FlagRegister.Zero;
                    else F &= ~FlagRegister.Zero;

                    F &= ~FlagRegister.NSubstract;
                    F &= ~FlagRegister.HalfCarry;
                    F &= ~FlagRegister.Carry;
                    break;
                case 6: // OR A, n
                    Registers[0] |= data;

                    if (Registers[0] == 0) F |= FlagRegister.Zero;
                    else F &= ~FlagRegister.Zero;

                    F &= ~FlagRegister.NSubstract;
                    F &= ~FlagRegister.HalfCarry;
                    F &= ~FlagRegister.Carry;
                    break;
                case 7: // CP A, n
                    if (Registers[0] == data) F |= FlagRegister.Zero;
                    else F &= ~FlagRegister.Zero;

                    F |= FlagRegister.NSubstract;

                    if (data > original) F |= FlagRegister.Carry;
                    else F &= ~FlagRegister.Carry;

                    if ((original & 0x0F) < (data & 0x0F)) F |= FlagRegister.HalfCarry;
                    else F &= ~FlagRegister.HalfCarry;
                    break;
            }

            PC += 2;
        }

        private void Alu(byte operation, byte register)
        {
            MachineCycles += 4;
            byte operand = ReadRTable(register);
            byte original = Registers[0];

            switch (operation)
            {
                case 0: // ADD A, r[p]
                    Registers[0] += operand;

                    if (Registers[0] == 0) F |= FlagRegister.Zero;
                    else F &= ~FlagRegister.Zero;

                    F &= ~FlagRegister.NSubstract;

                    if (original + operand > byte.MaxValue) F |= FlagRegister.Carry;
                    else F &= ~FlagRegister.Carry;

                    byte mask1 = (byte)((original & Low4Bitmask) << 4);
                    byte mask2 = (byte)((operand & Low4Bitmask) << 4);
                    byte masks = (byte)(mask1 + mask2);
                    if (masks <= mask1 && mask1 != 0 && mask2 != 0) F |= FlagRegister.HalfCarry;
                    else F &= ~FlagRegister.HalfCarry;

                    break;
                case 1: // ADC A, r[p]
                    Registers[0] += (byte)(operand + CarryAdd);

                    if (Registers[0] == 0) F |= FlagRegister.Zero;
                    else F &= ~FlagRegister.Zero;

                    F &= ~FlagRegister.NSubstract;


                    if ((((original & 0x0F) + (operand & 0x0F) + CarryAdd) & 0xF0) != 0) F |= FlagRegister.HalfCarry;
                    else F &= ~FlagRegister.HalfCarry;

                    if (original + operand + CarryAdd > byte.MaxValue) F |= FlagRegister.Carry;
                    else F &= ~FlagRegister.Carry;
                    break;
                case 2: // SUB r[p]
                    Registers[0] -= operand;

                    if (Registers[0] == 0) F |= FlagRegister.Zero;
                    else F &= ~FlagRegister.Zero;

                    F |= FlagRegister.NSubstract;


                    if (operand > original) F |= FlagRegister.Carry;
                    else F &= ~FlagRegister.Carry;

                    if ((original & 0x0F) < (operand & 0x0F)) F |= FlagRegister.HalfCarry;
                    else F &= ~FlagRegister.HalfCarry;
                    break;
                case 3: // SBC A, r[p]
                    Registers[0] -= (byte)(operand + CarryAdd);

                    if (Registers[0] == 0) F |= FlagRegister.Zero;
                    else F &= ~FlagRegister.Zero;

                    F |= FlagRegister.NSubstract;

                    if ((((original & 0x0F) - (operand & 0x0F) - CarryAdd) & 0xF0) != 0) F |= FlagRegister.HalfCarry;
                    else F &= ~FlagRegister.HalfCarry;

                    if (original - operand - CarryAdd < 0) F |= FlagRegister.Carry;
                    else F &= ~FlagRegister.Carry;
                    break;
                case 4: // AND r[p]
                    Registers[0] &= operand;

                    if (Registers[0] == 0) F |= FlagRegister.Zero;
                    else F &= ~FlagRegister.Zero;

                    F &= ~FlagRegister.NSubstract;
                    F |= FlagRegister.HalfCarry;
                    F &= ~FlagRegister.Carry;
                    break;
                case 5: // XOR r[p]
                    Registers[0] ^= operand;

                    if (Registers[0] == 0) F |= FlagRegister.Zero;
                    else F &= ~FlagRegister.Zero;

                    F &= ~FlagRegister.NSubstract;
                    F &= ~FlagRegister.HalfCarry;
                    F &= ~FlagRegister.Carry;
                    break;
                case 6: // OR r[p]
                    Registers[0] |= operand;

                    if (Registers[0] == 0) F |= FlagRegister.Zero;
                    else F &= ~FlagRegister.Zero;

                    F &= ~FlagRegister.NSubstract;
                    F &= ~FlagRegister.HalfCarry;
                    F &= ~FlagRegister.Carry;
                    break;
                case 7: // CP r[p]
                    if (original == operand) F |= FlagRegister.Zero;
                    else F &= ~FlagRegister.Zero;

                    F |= FlagRegister.NSubstract;

                    if (operand > original) F |= FlagRegister.Carry;
                    else F &= ~FlagRegister.Carry;

                    if ((original & 0x0F) < (operand & 0x0F)) F |= FlagRegister.HalfCarry;
                    else F &= ~FlagRegister.HalfCarry;
                    break;
            }

            PC++;
        }

        public byte ReadRTable(byte r)
        {
            if (r == 6) return ResolveMemory(HL);
            else if (r == 7) return Registers[0];
            else return Registers[r + 1];
        }

        private void WriteRTable(byte r, byte data)
        {
            if (r == 6) WriteMemory(HL, data);
            else if (r == 7) Registers[0] = data;
            else Registers[r + 1] = data;
        }

        private ushort ReadRPTable(byte rp)
        {
            switch (rp)
            {
                case 0:
                    return (ushort)((Registers[1] << 8) | Registers[2]);
                case 1:
                    return (ushort)((Registers[3] << 8) | Registers[4]);
                case 2:
                    return (ushort)((Registers[5] << 8) | Registers[6]);
                case 3:
                    return SP;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void WriteRPTable(byte rp, ushort data)
        {
            byte lo = (byte)((data & 0xFF00) >> 8);
            byte hi = (byte)(data & 0x00FF);

            switch (rp)
            {
                case 0:
                    Registers[1] = lo;
                    Registers[2] = hi;
                    break;
                case 1:
                    Registers[3] = lo;
                    Registers[4] = hi;
                    break;
                case 2:
                    Registers[5] = lo;
                    Registers[6] = hi;
                    break;
                case 3:
                    SP = data;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private ushort ReadRP2Table(byte rp2)
        {
            switch (rp2)
            {
                case 0:
                    return (ushort)((Registers[1] << 8) | Registers[2]);
                case 1:
                    return (ushort)((Registers[3] << 8) | Registers[4]);
                case 2:
                    return (ushort)((Registers[5] << 8) | Registers[6]);
                case 3:
                    return (ushort)((Registers[0] << 8) | (byte)F); // TODO: This too
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void WriteRP2Table(byte rp2, ushort data)
        {
            byte lo = (byte)((data & 0xFF00) >> 8);
            byte hi = (byte)(data & 0x00FF);

            switch (rp2)
            {
                case 0:
                    Registers[1] = lo;
                    Registers[2] = hi;
                    break;
                case 1:
                    Registers[3] = lo;
                    Registers[4] = hi;
                    break;
                case 2:
                    Registers[5] = lo;
                    Registers[6] = hi;
                    break;
                case 3:
                    Registers[0] = lo;
                    F = (FlagRegister)hi; // TODO: Are you sure about this
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CBPrefix(Span<byte> opcode)
        {
            byte x = (byte)((opcode[0] & 0b11_000_000) >> 6);
            byte y = (byte)((opcode[0] & 0b00_111_000) >> 3);
            byte z = (byte)(opcode[0] & 0b00_000_111);

            switch (x)
            {
                case 0:
                    if (z == 6) MachineCycles += 8;
                    Rot(y, z);
                    break;
                case 1:
                    if (z == 6) MachineCycles += 8;
                    if ((ReadRTable(z) & (1 << y)) == 0) F |= FlagRegister.Zero;
                    else F &= ~FlagRegister.Zero;

                    F &= ~FlagRegister.NSubstract;
                    F |= FlagRegister.HalfCarry;
                    break;
                case 2:
                    byte reset = ReadRTable(z);
                    reset &= (byte)(~(1 << y));

                    if (z == 6) MachineCycles += 8;
                    WriteRTable(z, reset);
                    break;
                case 3:
                    byte set = ReadRTable(z);
                    set |= (byte)(1 << y);

                    if (z == 6) MachineCycles += 8;
                    WriteRTable(z, set);
                    break;
                default:
                    throw new NotImplementedException();
            }

            MachineCycles += 8;
            PC += 2;
        }

        private void Rot(byte y, byte z)
        {
            byte data = ReadRTable(z);
            byte carryData = (F & FlagRegister.Carry) == 0 ? (byte) 0 : (byte) 1;

            F &= ~FlagRegister.HalfCarry;
            F &= ~FlagRegister.NSubstract;

            byte result;

            switch (y)
            {
                case 0: // RLC
                    if ((data & 0b10000000) == 0) F &= ~FlagRegister.Carry;
                    else F |= FlagRegister.Carry;

                    result = (byte)((data << 1) | (data >> 7));
                    break;
                case 1: // RRC
                    if ((data & 0b00000001) == 0) F &= ~FlagRegister.Carry;
                    else F |= FlagRegister.Carry;

                    result = (byte)((data >> 1) | (data << 7));
                    break;
                case 2: // RL
                    if ((data & 0b10000000) == 0) F &= ~FlagRegister.Carry;
                    else F |= FlagRegister.Carry;

                    result = (byte)((data << 1) | carryData);
                    break;
                case 3: // RR
                    if ((data & 0b00000001) == 0) F &= ~FlagRegister.Carry;
                    else F |= FlagRegister.Carry;

                    result = (byte)((data >> 1) | (carryData << 7));
                    break;
                case 4: // SLA
                    if ((data & 0b10000000) == 0) F &= ~FlagRegister.Carry;
                    else F |= FlagRegister.Carry;

                    result = (byte)(data << 1);
                    break;
                case 5: // SRA
                    if ((data & 0b00000001) == 0) F &= ~FlagRegister.Carry;
                    else F |= FlagRegister.Carry;

                    result = (byte)((data >> 1) | (data & 0b10000000));
                    break;
                case 6: // SWAP
                    byte lo = (byte)(data & 0x0F);
                    byte hi = (byte) (data & 0xF0);
                    result = (byte)((lo << 4) | (hi >> 4));

                    F &= ~FlagRegister.Carry;
                    break;
                case 7: // SRL
                    if ((data & 0b00000001) == 0) F &= ~FlagRegister.Carry;
                    else F |= FlagRegister.Carry;

                    result = (byte)(data >> 1);
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (result == 0) F |= FlagRegister.Zero;
            else F &= ~FlagRegister.Zero;
            WriteRTable(z, result);
        }
    }
}
