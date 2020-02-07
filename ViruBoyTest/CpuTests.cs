
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ViruBoy;

namespace ViruBoyTest
{
    [TestClass]
    public class CpuTests
    {
        private VirtualCpu _cpu;

        [TestInitialize]
        public void Initialize()
        {
            _cpu = new VirtualCpu(new Cartridge());
        }

        [TestMethod]
        public void Nop()
        {
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress] = 0x00;

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 1, 4UL);
        }

        [TestMethod]
        public void Halt()
        {
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress] = 0x76;

            try
            {
                _cpu.Step();
            }
            catch (NotImplementedException)
            {
                // TODO: Uhhh
                return;
            }

            Assert.Fail();
        }

        [TestMethod]
        public void Complement()
        {
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 0] = 0x2F;
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 1] = 0x2F;
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 2] = 0x2F;

            _cpu.Registers[0] = 0x00;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 1, 4UL, FlagRegister.NSubstract | FlagRegister.HalfCarry | FlagRegister.Carry | FlagRegister.Zero);
            Assert.AreEqual(0xFF, _cpu.Registers[0]);

            _cpu.Registers[0] = 0xFF;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 2, 8UL, FlagRegister.NSubstract | FlagRegister.HalfCarry | FlagRegister.Carry | FlagRegister.Zero);
            Assert.AreEqual(0x00, _cpu.Registers[0]);

            _cpu.Registers[0] = 0xAA;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 3, 12UL, FlagRegister.NSubstract | FlagRegister.HalfCarry | FlagRegister.Carry | FlagRegister.Zero);
            Assert.AreEqual(0x55, _cpu.Registers[0]);
        }

        

        [TestMethod]
        public void SetCarryFlag()
        {
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 0] = 0x37; // SCF
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 1] = 0x37; // SCF

            _cpu.F |= FlagRegister.Carry;
            _cpu.F |= FlagRegister.NSubstract;
            _cpu.F &= ~FlagRegister.Zero;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 1, 4, FlagRegister.Carry);

            _cpu.F |= FlagRegister.Zero;
            _cpu.F &= ~FlagRegister.Carry;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 2, 8, FlagRegister.Zero | FlagRegister.Carry);
        }
    }
}