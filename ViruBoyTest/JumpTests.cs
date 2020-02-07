using Microsoft.VisualStudio.TestTools.UnitTesting;
using ViruBoy;

namespace ViruBoyTest
{
    [TestClass]
    public class JumpTests
    {
        private VirtualCpu _cpu;

        [TestInitialize]
        public void Initialize()
        {
            _cpu = new VirtualCpu(new Cartridge());
        }

        [TestMethod]
        public void JumpRelative()
        {
            unchecked
            {
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 0] = 0x18;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 1] = 10;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 2] = 0x18;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 3] = 0;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 4] = 0x18;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 5] = (byte)-8;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 6] = 0x18;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 7] = (byte)-2;
            }

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 12, 12);

            _cpu.PC = Cartridge.CodeStartAddress + 2;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 4, 24);

            _cpu.PC = Cartridge.CodeStartAddress + 4;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress - 2, 36);

            _cpu.PC = Cartridge.CodeStartAddress + 6;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 6, 48);
        }

        [TestMethod]
        public void JumpRelativeZeroFlag()
        {
            unchecked
            {
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 0] = 0x28;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 1] = 10;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 2] = 0x28;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 3] = 0;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 4] = 0x28;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 5] = (byte)-8;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 6] = 0x28;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 7] = (byte)-2;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 8] = 0x28;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 9] = 10;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 10] = 0x28;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 11] = 0;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 12] = 0x28;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 13] = (byte)-8;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 14] = 0x28;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 15] = (byte)-2;
            }

            // Flag set
            _cpu.F |= FlagRegister.Zero;

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 12, 12);

            _cpu.PC = Cartridge.CodeStartAddress + 2;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 4, 24);

            _cpu.PC = Cartridge.CodeStartAddress + 4;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress - 2, 36);

            _cpu.PC = Cartridge.CodeStartAddress + 6;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 6, 48);

            // Flag reset
            _cpu.F &= ~FlagRegister.Zero;

            _cpu.PC = Cartridge.CodeStartAddress + 8;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 10, 56, FlagRegister.Carry | FlagRegister.HalfCarry);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 12, 64, FlagRegister.Carry | FlagRegister.HalfCarry);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 14, 72, FlagRegister.Carry | FlagRegister.HalfCarry);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 16, 80, FlagRegister.Carry | FlagRegister.HalfCarry);
        }

        [TestMethod]
        public void JumpRelativeNotZeroFlag()
        {
            unchecked
            {
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 0] = 0x20;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 1] = 10;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 2] = 0x20;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 3] = 0;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 4] = 0x20;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 5] = (byte)-8;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 6] = 0x20;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 7] = (byte)-2;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 8] = 0x20;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 9] = 10;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 10] = 0x20;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 11] = 0;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 12] = 0x20;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 13] = (byte)-8;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 14] = 0x20;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 15] = (byte)-2;
            }

            // Flag reset
            _cpu.F &= ~FlagRegister.Zero;

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 12, 12, FlagRegister.Carry | FlagRegister.HalfCarry);

            _cpu.PC = Cartridge.CodeStartAddress + 2;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 4, 24, FlagRegister.Carry | FlagRegister.HalfCarry);

            _cpu.PC = Cartridge.CodeStartAddress + 4;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress - 2, 36, FlagRegister.Carry | FlagRegister.HalfCarry);

            _cpu.PC = Cartridge.CodeStartAddress + 6;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 6, 48, FlagRegister.Carry | FlagRegister.HalfCarry);

            // Flag set
            _cpu.F |= FlagRegister.Zero;

            _cpu.PC = Cartridge.CodeStartAddress + 8;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 10, 56);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 12, 64);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 14, 72);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 16, 80);
        }

        [TestMethod]
        public void JumpRelativeCarryFlag()
        {
            unchecked
            {
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 0] = 0x38;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 1] = 10;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 2] = 0x38;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 3] = 0;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 4] = 0x38;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 5] = (byte)-8;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 6] = 0x38;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 7] = (byte)-2;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 8] = 0x38;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 9] = 10;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 10] = 0x38;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 11] = 0;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 12] = 0x38;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 13] = (byte)-8;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 14] = 0x38;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 15] = (byte)-2;
            }

            // Flag set
            _cpu.F |= FlagRegister.Carry;

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 12, 12);

            _cpu.PC = Cartridge.CodeStartAddress + 2;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 4, 24);

            _cpu.PC = Cartridge.CodeStartAddress + 4;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress - 2, 36);

            _cpu.PC = Cartridge.CodeStartAddress + 6;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 6, 48);

            // Flag reset
            _cpu.F &= ~FlagRegister.Carry;

            _cpu.PC = Cartridge.CodeStartAddress + 8;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 10, 56, FlagRegister.Zero | FlagRegister.HalfCarry);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 12, 64, FlagRegister.Zero | FlagRegister.HalfCarry);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 14, 72, FlagRegister.Zero | FlagRegister.HalfCarry);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 16, 80, FlagRegister.Zero | FlagRegister.HalfCarry);
        }

        [TestMethod]
        public void JumpRelativeNotCarryFlag()
        {
            unchecked
            {
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 0] = 0x30;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 1] = 10;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 2] = 0x30;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 3] = 0;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 4] = 0x30;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 5] = (byte)-8;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 6] = 0x30;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 7] = (byte)-2;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 8] = 0x30;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 9] = 10;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 10] = 0x30;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 11] = 0;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 12] = 0x30;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 13] = (byte)-8;

                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 14] = 0x30;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 15] = (byte)-2;
            }

            // Flag reset
            _cpu.F &= ~FlagRegister.Carry;

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 12, 12, FlagRegister.Zero | FlagRegister.HalfCarry);

            _cpu.PC = Cartridge.CodeStartAddress + 2;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 4, 24, FlagRegister.Zero | FlagRegister.HalfCarry);

            _cpu.PC = Cartridge.CodeStartAddress + 4;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress - 2, 36, FlagRegister.Zero | FlagRegister.HalfCarry);

            _cpu.PC = Cartridge.CodeStartAddress + 6;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 6, 48, FlagRegister.Zero | FlagRegister.HalfCarry);

            // Flag set
            _cpu.F |= FlagRegister.Carry;

            _cpu.PC = Cartridge.CodeStartAddress + 8;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 10, 56);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 12, 64);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 14, 72);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 16, 80);
        }
    }
}
