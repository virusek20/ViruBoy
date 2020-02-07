using Microsoft.VisualStudio.TestTools.UnitTesting;
using ViruBoy;

namespace ViruBoyTest
{
    public static class AssertExtensions
    {
        private const FlagRegister DefaultRegister = (FlagRegister)0xB0;

        public static void CpuIsInState(this Assert assert, VirtualCpu cpu, ushort expectedPC, ulong expectedCycles, FlagRegister expectedFlagRegister = DefaultRegister)
        {
            Assert.AreEqual(expectedPC, cpu.PC);
            Assert.AreEqual(expectedCycles, cpu.MachineCycles);
            Assert.AreEqual(expectedFlagRegister, cpu.F);
        }
    }
}
