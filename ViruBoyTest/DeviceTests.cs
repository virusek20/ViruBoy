using Microsoft.VisualStudio.TestTools.UnitTesting;
using ViruBoy;

namespace ViruBoyTest
{
    [TestClass]
    public class DeviceTests
    {
        private VirtualCpu _cpu;

        [TestInitialize]
        public void Initialize()
        {
            _cpu = new VirtualCpu(new Cartridge());
        }

        [TestMethod]
        public void InitialSettings()
        {
            Assert.AreEqual(_cpu.F, (FlagRegister)0xB0);
            Assert.AreEqual(_cpu.PC, Cartridge.CodeStartAddress);
            Assert.AreEqual(_cpu.SP, (ushort)0xFFFE);

            Assert.AreEqual(_cpu.Registers[0], (byte)0x01);
            Assert.AreEqual(_cpu.Registers[2], (byte)0x13);
            Assert.AreEqual(_cpu.Registers[4], (byte)0xD8);
            Assert.AreEqual(_cpu.Registers[5], (byte)0x01);
            Assert.AreEqual(_cpu.Registers[6], (byte)0x4D);

            /*
            Assert.AreEqual(_cpu.ResolveMemory(0xFF05), (byte)0x00); // TIMA
            Assert.AreEqual(_cpu.ResolveMemory(0xFF06), (byte)0x00); // TMA
            Assert.AreEqual(_cpu.ResolveMemory(0xFF07), (byte)0x00); // TAC
            Assert.AreEqual(_cpu.ResolveMemory(0xFF10), (byte)0x80); // NR10
            Assert.AreEqual(_cpu.ResolveMemory(0xFF11), (byte)0xBF); // NR11
            Assert.AreEqual(_cpu.ResolveMemory(0xFF12), (byte)0xF3); // NR12
            Assert.AreEqual(_cpu.ResolveMemory(0xFF14), (byte)0xBF); // NR14
            Assert.AreEqual(_cpu.ResolveMemory(0xFF16), (byte)0x3F); // NR21
            Assert.AreEqual(_cpu.ResolveMemory(0xFF17), (byte)0x00); // NR22
            Assert.AreEqual(_cpu.ResolveMemory(0xFF19), (byte)0xBF); // NR24
            Assert.AreEqual(_cpu.ResolveMemory(0xFF1A), (byte)0x7F); // NR30
            Assert.AreEqual(_cpu.ResolveMemory(0xFF1B), (byte)0xFF); // NR31
            Assert.AreEqual(_cpu.ResolveMemory(0xFF1C), (byte)0x9F); // NR32
            Assert.AreEqual(_cpu.ResolveMemory(0xFF1E), (byte)0xBF); // NR33
            Assert.AreEqual(_cpu.ResolveMemory(0xFF20), (byte)0xFF); // NR41
            Assert.AreEqual(_cpu.ResolveMemory(0xFF21), (byte)0x00); // NR42
            Assert.AreEqual(_cpu.ResolveMemory(0xFF22), (byte)0x00); // NR43
            Assert.AreEqual(_cpu.ResolveMemory(0xFF23), (byte)0xBF); // NR30
            Assert.AreEqual(_cpu.ResolveMemory(0xFF24), (byte)0x77); // NR50
            Assert.AreEqual(_cpu.ResolveMemory(0xFF25), (byte)0xF3); // NR51
            Assert.AreEqual(_cpu.ResolveMemory(0xFF26), (byte)0xF1); // NR52
            Assert.AreEqual(_cpu.ResolveMemory(0xFF40), (byte)0x91); // LCDC
            Assert.AreEqual(_cpu.ResolveMemory(0xFF42), (byte)0x00); // SCY
            Assert.AreEqual(_cpu.ResolveMemory(0xFF43), (byte)0x00); // SCX
            Assert.AreEqual(_cpu.ResolveMemory(0xFF45), (byte)0x00); // LYC
            Assert.AreEqual(_cpu.ResolveMemory(0xFF47), (byte)0xFC); // BGP
            Assert.AreEqual(_cpu.ResolveMemory(0xFF48), (byte)0xFF); // OBP0
            Assert.AreEqual(_cpu.ResolveMemory(0xFF49), (byte)0xFF); // OBP1
            Assert.AreEqual(_cpu.ResolveMemory(0xFF4A), (byte)0x00); // WY
            Assert.AreEqual(_cpu.ResolveMemory(0xFF4B), (byte)0x00); // WX
            Assert.AreEqual(_cpu.ResolveMemory(0xFFFF), (byte)0x00); // IE
            */
        }
    }
}
