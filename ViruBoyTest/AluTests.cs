using Microsoft.VisualStudio.TestTools.UnitTesting;
using ViruBoy;

namespace ViruBoyTest
{
    [TestClass]
    public class AluTests
    {
        private VirtualCpu _cpu;

        [TestInitialize]
        public void Initialize()
        {
            _cpu = new VirtualCpu(new Cartridge());
        }

        [TestMethod]
        public void IncrementDoubleRegister()
        {
            _cpu.Registers[1] = 0xC0; // B
            _cpu.Registers[2] = 0x10; // C = 0x0110

            _cpu.Registers[3] = 0xC0; // D
            _cpu.Registers[4] = 0x11; // E = 0x0111

            _cpu.Registers[5] = 0xFF; // H
            _cpu.Registers[6] = 0xFF; // L = 0x0112

            _cpu.SP = 0xC013;

            for (int i = 0; i < 4; i++)
            {
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + i] = (byte)(0x03 + i * 0x10);
            }

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 1, 8UL);
            Assert.AreEqual(0xC0, _cpu.Registers[1]);
            Assert.AreEqual(0x11, _cpu.Registers[2]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 2, 16UL);
            Assert.AreEqual(0xC0, _cpu.Registers[3]);
            Assert.AreEqual(0x12, _cpu.Registers[4]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 3, 24UL);
            Assert.AreEqual(0x00, _cpu.Registers[5]);
            Assert.AreEqual(0x00, _cpu.Registers[6]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 4, 32UL);
            Assert.AreEqual((ushort)0xC014, _cpu.SP);
        }

        [TestMethod]
        public void DecrementDoubleRegister()
        {
            _cpu.Registers[1] = 0xC0; // B
            _cpu.Registers[2] = 0x10; // C = 0x0110

            _cpu.Registers[3] = 0xC0; // D
            _cpu.Registers[4] = 0x11; // E = 0x0111

            _cpu.Registers[5] = 0x00; // H
            _cpu.Registers[6] = 0x00; // L = 0x0112

            _cpu.SP = 0xC013;

            for (int i = 0; i < 4; i++)
            {
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + i] = (byte)(0x0B + i * 0x10);
            }

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 1, 8UL);
            Assert.AreEqual(0xC0, _cpu.Registers[1]);
            Assert.AreEqual(0x0F, _cpu.Registers[2]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 2, 16UL);
            Assert.AreEqual(0xC0, _cpu.Registers[3]);
            Assert.AreEqual(0x10, _cpu.Registers[4]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 3, 24UL);
            Assert.AreEqual(0xFF, _cpu.Registers[5]);
            Assert.AreEqual(0xFF, _cpu.Registers[6]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 4, 32UL);
            Assert.AreEqual((ushort)0xC012, _cpu.SP);
        }

        [TestMethod]
        public void DecrementRegister()
        {
            _cpu.Registers[0] = 1;
            _cpu.Registers[1] = 2;
            _cpu.Registers[2] = 3;
            _cpu.Registers[3] = 4;
            _cpu.Registers[4] = 5;
            _cpu.Registers[5] = 16;
            _cpu.Registers[6] = 0;

            for (int i = 0; i < 3; i++)
            {
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + i] = (byte)(0x05 + i * 0x10);
            }

            for (int i = 0; i < 4; i++)
            {
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 3 + i] = (byte)(0x0D + i * 0x10);
            }

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 1, 4, FlagRegister.Carry | FlagRegister.NSubstract);
            Assert.AreEqual((byte)0x01, _cpu.Registers[1]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 2, 8, FlagRegister.Carry | FlagRegister.NSubstract);
            Assert.AreEqual((byte)0x03, _cpu.Registers[3]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 3, 12, FlagRegister.Carry | FlagRegister.HalfCarry | FlagRegister.NSubstract);
            Assert.AreEqual((byte)15, _cpu.Registers[5]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 4, 16, FlagRegister.Carry | FlagRegister.NSubstract);
            Assert.AreEqual((byte)0x02, _cpu.Registers[2]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 5, 20, FlagRegister.Carry | FlagRegister.NSubstract);
            Assert.AreEqual((byte)0x04, _cpu.Registers[4]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 6, 24, FlagRegister.Carry | FlagRegister.HalfCarry | FlagRegister.NSubstract);
            Assert.AreEqual((byte)0xFF, _cpu.Registers[6]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 7, 28, FlagRegister.Carry | FlagRegister.NSubstract | FlagRegister.Zero);
            Assert.AreEqual((byte)0x00, _cpu.Registers[0]);
        }

        [TestMethod]
        public void DecrementIndirect()
        {
            _cpu.Registers[5] = 0xC0; // H
            _cpu.Registers[6] = 0x12; // L = 0xC012

            _cpu.WriteMemory(0xC012, 0xA0);
            _cpu.WriteMemory(0xC013, 0x10);
            _cpu.WriteMemory(0xC014, 0x00);
            _cpu.WriteMemory(0xC015, 0x01);

            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 0] = 0x35;
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 1] = 0x35;
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 2] = 0x35;
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 3] = 0x35;

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 1, 12, FlagRegister.Carry | FlagRegister.HalfCarry | FlagRegister.NSubstract);
            Assert.AreEqual((byte)0x9F, _cpu.ResolveMemory(0xC012));

            _cpu.Registers[6] = 0x13;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 2, 24, FlagRegister.Carry | FlagRegister.NSubstract | FlagRegister.HalfCarry);
            Assert.AreEqual((byte)0x0F, _cpu.ResolveMemory(0xC013));

            _cpu.Registers[6] = 0x14;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 3, 36, FlagRegister.Carry | FlagRegister.HalfCarry | FlagRegister.NSubstract);
            Assert.AreEqual((byte)0xFF, _cpu.ResolveMemory(0xC014));

            _cpu.Registers[6] = 0x15;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 4, 48, FlagRegister.Carry | FlagRegister.Zero | FlagRegister.NSubstract);
            Assert.AreEqual((byte)0x00, _cpu.ResolveMemory(0xC015));
        }

        [TestMethod]
        public void IncrementIndirect()
        {
            _cpu.Registers[5] = 0xC0; // H
            _cpu.Registers[6] = 0x12; // L = 0xC012

            _cpu.WriteMemory(0xC012, 0xAA);
            _cpu.WriteMemory(0xC013, 0x0F);
            _cpu.WriteMemory(0xC014, 0xFF);

            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 0] = 0x34;
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 1] = 0x34;
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 2] = 0x34;

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 1, 12, FlagRegister.Carry);
            Assert.AreEqual((byte)0xAB, _cpu.ResolveMemory(0xC012));

            _cpu.Registers[6] = 0x13;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 2, 24, FlagRegister.Carry | FlagRegister.HalfCarry);
            Assert.AreEqual((byte)0x10, _cpu.ResolveMemory(0xC013));

            _cpu.Registers[6] = 0x14;
            _cpu.Step();
            // ReSharper disable once RedundantArgumentDefaultValue
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 3, 36, FlagRegister.Carry | FlagRegister.Zero | FlagRegister.HalfCarry);
            Assert.AreEqual((byte)0x00, _cpu.ResolveMemory(0xC014));
        }

        [TestMethod]
        public void IncrementRegister()
        {
            _cpu.Registers[0] = 0;
            _cpu.Registers[1] = 1;
            _cpu.Registers[2] = 2;
            _cpu.Registers[3] = 3;
            _cpu.Registers[4] = 4;
            _cpu.Registers[5] = 15;
            _cpu.Registers[6] = 255;

            for (int i = 0; i < 3; i++)
            {
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + i] = (byte)(0x04 + i * 0x10);
            }

            for (int i = 0; i < 4; i++)
            {
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 3 + i] = (byte)(0x0C + i * 0x10);
            }

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 1, 4, FlagRegister.Carry);
            Assert.AreEqual((byte)0x02, _cpu.Registers[1]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 2, 8, FlagRegister.Carry);
            Assert.AreEqual((byte)0x04, _cpu.Registers[3]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 3, 12, FlagRegister.Carry | FlagRegister.HalfCarry);
            Assert.AreEqual((byte)16, _cpu.Registers[5]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 4, 16, FlagRegister.Carry);
            Assert.AreEqual((byte)0x03, _cpu.Registers[2]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 5, 20, FlagRegister.Carry);
            Assert.AreEqual((byte)0x05, _cpu.Registers[4]);

            _cpu.Step();
            // ReSharper disable once RedundantArgumentDefaultValue
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 6, 24, FlagRegister.Carry | FlagRegister.Zero | FlagRegister.HalfCarry);
            Assert.AreEqual((byte)0x00, _cpu.Registers[6]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 7, 28, FlagRegister.Carry);
            Assert.AreEqual((byte)0x01, _cpu.Registers[0]);
        }

        [TestMethod]
        public void IncrementHLIndirect()
        {
            _cpu.Registers[5] = 0xC0;
            _cpu.Registers[6] = 0x00;

            _cpu.WriteMemory(VirtualCpu.InternalRamAddress, 0x0F);

            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress] = 0x34;

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 1, 12, FlagRegister.Carry | FlagRegister.HalfCarry);
            Assert.AreEqual((byte)0x10, _cpu.ResolveMemory(0xC000));
        }

        [TestMethod]
        public void RotateALeftCarry()
        {
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 0] = 0x07; // RLCA
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 1] = 0x07; // RLCA
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 2] = 0x07; // RLCA
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 3] = 0x07; // RLCA

            _cpu.Registers[0] = 0x00;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 1, 4, FlagRegister.Zero);
            Assert.AreEqual((byte)0x00, _cpu.Registers[0]);

            _cpu.Registers[0] = 0x01;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 2, 8, 0);
            Assert.AreEqual((byte)0x02, _cpu.Registers[0]);

            _cpu.Registers[0] = 0xB4;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 3, 12, FlagRegister.Carry);
            Assert.AreEqual((byte)0x69, _cpu.Registers[0]);

            _cpu.Registers[0] = 0x80;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 4, 16, FlagRegister.Carry);
            Assert.AreEqual((byte)0x01, _cpu.Registers[0]);
        }

        [TestMethod]
        public void RotateALeft()
        {
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 0] = 0x17; // RLA
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 1] = 0x17; // RLA
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 2] = 0x17; // RLA
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 3] = 0x17; // RLA
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 4] = 0x17; // RLA
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 5] = 0x17; // RLA

            _cpu.F &= ~FlagRegister.Carry;

            _cpu.Registers[0] = 0x00;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 1, 4, FlagRegister.Zero);
            Assert.AreEqual((byte)0x00, _cpu.Registers[0]);

            _cpu.Registers[0] = 0x01;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 2, 8, 0);
            Assert.AreEqual((byte)0x02, _cpu.Registers[0]);

            _cpu.Registers[0] = 0xB4;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 3, 12, FlagRegister.Carry);
            Assert.AreEqual((byte)0x68, _cpu.Registers[0]);

            _cpu.F &= ~FlagRegister.Carry;
            _cpu.Registers[0] = 0x80;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 4, 16, FlagRegister.Carry | FlagRegister.Zero);
            Assert.AreEqual((byte)0x00, _cpu.Registers[0]);

            _cpu.F |= FlagRegister.Carry;
            _cpu.Registers[0] = 0x00;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 5, 20, 0);
            Assert.AreEqual((byte)0x01, _cpu.Registers[0]);

            _cpu.F |= FlagRegister.Carry;
            _cpu.Registers[0] = 0x80;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 6, 24, FlagRegister.Carry);
            Assert.AreEqual((byte)0x01, _cpu.Registers[0]);
        }

        [TestMethod]
        public void RotateARightCarry()
        {
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 0] = 0x0F; // RRCA
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 1] = 0x0F; // RRCA
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 2] = 0x0F; // RRCA
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 3] = 0x0F; // RRCA

            _cpu.Registers[0] = 0x00;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 1, 4, FlagRegister.Zero);
            Assert.AreEqual((byte)0x00, _cpu.Registers[0]);

            _cpu.Registers[0] = 0x80;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 2, 8, 0);
            Assert.AreEqual((byte)0x40, _cpu.Registers[0]);

            _cpu.Registers[0] = 0xD5;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 3, 12, FlagRegister.Carry);
            Assert.AreEqual((byte)0xEA, _cpu.Registers[0]);

            _cpu.Registers[0] = 0x01;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 4, 16, FlagRegister.Carry);
            Assert.AreEqual((byte)0x80, _cpu.Registers[0]);
        }

        [TestMethod]
        public void RotateARight()
        {
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 0] = 0x1F; // RRA
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 1] = 0x1F; // RRA
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 2] = 0x1F; // RRA
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 3] = 0x1F; // RRA
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 4] = 0x1F; // RRA
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 5] = 0x1F; // RRA

            _cpu.F &= ~FlagRegister.Carry;

            _cpu.Registers[0] = 0x00;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 1, 4, FlagRegister.Zero);
            Assert.AreEqual((byte)0x00, _cpu.Registers[0]);

            _cpu.Registers[0] = 0x80;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 2, 8, 0);
            Assert.AreEqual((byte)0x40, _cpu.Registers[0]);

            _cpu.Registers[0] = 0xD5;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 3, 12, FlagRegister.Carry);
            Assert.AreEqual((byte)0x6A, _cpu.Registers[0]);

            _cpu.F &= ~FlagRegister.Carry;
            _cpu.Registers[0] = 0x01;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 4, 16, FlagRegister.Carry | FlagRegister.Zero);
            Assert.AreEqual((byte)0x00, _cpu.Registers[0]);

            _cpu.F |= FlagRegister.Carry;
            _cpu.Registers[0] = 0x00;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 5, 20, 0);
            Assert.AreEqual((byte)0x80, _cpu.Registers[0]);

            _cpu.F |= FlagRegister.Carry;
            _cpu.Registers[0] = 0x01;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 6, 24, FlagRegister.Carry);
            Assert.AreEqual((byte)0x80, _cpu.Registers[0]);
        }

        [TestMethod]
        public void AddDoubleRegisterToHL()
        {
            // It's overflow from the.. 11th bit?

            int pos = Cartridge.CodeStartAddress;
            for (int i = 0; i < 4; i++) // 3 times each of the 4 instruction (0 + x | 11th byte to 12th | high byte to overflow)
            {
                byte instr = (byte)(0x09 + i * 0x10);

                for (int j = 0; j < 3; j++)
                {
                    _cpu.LoadedCartridge.Bank0[pos] = instr;
                    pos++;
                }
            }

            // ADD HL, BC

            _cpu.Registers[1] = 0x00; // B
            _cpu.Registers[2] = 0x10; // C = 0x0010
            _cpu.Registers[5] = 0x00; // H
            _cpu.Registers[6] = 0x00; // L = 0x0000

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 1, 8, 0);
            Assert.AreEqual((ushort)0x0010, _cpu.HL);

            _cpu.Registers[1] = 0x00; // B
            _cpu.Registers[2] = 0x01; // C = 0x0001
            _cpu.Registers[5] = 0x0F; // H
            _cpu.Registers[6] = 0xFF; // L = 0x0FFF
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 2, 16, FlagRegister.HalfCarry);
            Assert.AreEqual((ushort)0x1000, _cpu.HL);

            _cpu.Registers[1] = 0x10; // D
            _cpu.Registers[2] = 0x00; // E = 0x1000
            _cpu.Registers[5] = 0xF0; // H
            _cpu.Registers[6] = 0x00; // L = 0xF000
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 3, 24, FlagRegister.Carry | FlagRegister.Zero);
            Assert.AreEqual((ushort)0x0000, _cpu.HL);

            // ADD HL, DE

            _cpu.Registers[3] = 0x00; // D
            _cpu.Registers[4] = 0x10; // E = 0x0010
            _cpu.Registers[5] = 0x00; // H
            _cpu.Registers[6] = 0x00; // L = 0x0000

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 4, 32, 0);
            Assert.AreEqual((ushort)0x0010, _cpu.HL);

            _cpu.Registers[3] = 0x00; // D
            _cpu.Registers[4] = 0x01; // E = 0x0001
            _cpu.Registers[5] = 0x0F; // H
            _cpu.Registers[6] = 0xFF; // L = 0x0FFF
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 5, 40, FlagRegister.HalfCarry);
            Assert.AreEqual((ushort)0x1000, _cpu.HL);

            _cpu.Registers[3] = 0x10; // D
            _cpu.Registers[4] = 0x00; // E = 0x1000
            _cpu.Registers[5] = 0xF0; // H
            _cpu.Registers[6] = 0x00; // L = 0xF000
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 6, 48, FlagRegister.Carry | FlagRegister.Zero);
            Assert.AreEqual((ushort)0x0000, _cpu.HL);

            // ADD HL, HL

            _cpu.Registers[5] = 0x00; // H
            _cpu.Registers[6] = 0x10; // L = 0x0010

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 7, 56, 0);
            Assert.AreEqual((ushort)0x0020, _cpu.HL);

            _cpu.Registers[5] = 0x0F; // H
            _cpu.Registers[6] = 0xFF; // L = 0x0FFF
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 8, 64, FlagRegister.HalfCarry);
            Assert.AreEqual((ushort)0x1FFE, _cpu.HL);

            _cpu.Registers[5] = 0xFF; // H
            _cpu.Registers[6] = 0xFF; // L = 0xFFFF
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 9, 72, FlagRegister.Carry | FlagRegister.HalfCarry);
            Assert.AreEqual((ushort)0xFFFE, _cpu.HL);

            _cpu.SP = 0xC013;
        }

        [TestMethod]
        public void AddA()
        {
            for (int i = 0; i < 8; i++)
            {
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + i] = (byte)(0x80 + i);
            }

            _cpu.Registers[0] = 0x0;
            _cpu.Registers[1] = 0x0;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 1, 4, FlagRegister.Zero);
            Assert.AreEqual((byte)0x0, _cpu.Registers[0]);

            _cpu.Registers[0] = 0x5;
            _cpu.Registers[2] = 0xA;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 2, 8, 0);
            Assert.AreEqual((byte)0xF, _cpu.Registers[0]);

            _cpu.Registers[0] = 0x2;
            _cpu.Registers[3] = 0xFF;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 3, 12, FlagRegister.Carry | FlagRegister.HalfCarry);
            Assert.AreEqual((byte)0x1, _cpu.Registers[0]);

            _cpu.Registers[0] = 0x01;
            _cpu.Registers[4] = 0xFF;
            _cpu.Step();
            // ReSharper disable once RedundantArgumentDefaultValue
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 4, 16, FlagRegister.Carry | FlagRegister.HalfCarry | FlagRegister.Zero);
            Assert.AreEqual((byte)0x0, _cpu.Registers[0]);

            _cpu.Registers[0] = 0x0F;
            _cpu.Registers[5] = 0x01;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 5, 20, FlagRegister.HalfCarry );
            Assert.AreEqual((byte)0x10, _cpu.Registers[0]);

            _cpu.Registers[0] = 0xEF;
            _cpu.Registers[6] = 0x10;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 6, 24, 0);
            Assert.AreEqual((byte)0xFF, _cpu.Registers[0]);

            _cpu.Registers[0] = 0x01;
            _cpu.Registers[5] = 0xC0;
            _cpu.Registers[6] = 0x10; // HL = 0xC010
            _cpu.WriteMemory(0xC010, 0x01);
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 7, 28, 0);
            Assert.AreEqual((byte)0x02, _cpu.Registers[0]);

            _cpu.Registers[0] = 0xFF;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 8, 32, FlagRegister.Carry | FlagRegister.HalfCarry);
            Assert.AreEqual((byte)0xFE, _cpu.Registers[0]);
        }
    }
}
