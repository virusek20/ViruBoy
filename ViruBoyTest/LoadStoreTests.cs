using System.Runtime.InteropServices.ComTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ViruBoy;

namespace ViruBoyTest
{
    [TestClass]
    public class LoadStoreTests
    {
        private VirtualCpu _cpu;

        [TestInitialize]
        public void Initialize()
        {
            _cpu = new VirtualCpu(new Cartridge());
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        [DataRow(7)]
        public void LoadWordRegister(int ri)
        {
            byte r = (byte)ri;

            for (int i = 0; i < 8; i++)
            {
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + i] = (byte)(0x40 + i + (r * 8));
            }

            _cpu.WriteMemory(0xC012, 0xAA);

            _cpu.Registers[0] = 0x01;
            _cpu.Registers[1] = 0x02;
            _cpu.Registers[2] = 0x03;
            _cpu.Registers[3] = 0x04;
            _cpu.Registers[4] = 0x05;
            _cpu.Registers[5] = 0x06;
            _cpu.Registers[6] = 0x07;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 1, 4);
            Assert.AreEqual((byte)0x02, _cpu.ReadRTable(r));

            _cpu.Registers[0] = 0x01;
            _cpu.Registers[1] = 0x02;
            _cpu.Registers[2] = 0x03;
            _cpu.Registers[3] = 0x04;
            _cpu.Registers[4] = 0x05;
            _cpu.Registers[5] = 0x06;
            _cpu.Registers[6] = 0x07;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 2, 8);
            Assert.AreEqual((byte)0x03, _cpu.ReadRTable(r));

            _cpu.Registers[0] = 0x01;
            _cpu.Registers[1] = 0x02;
            _cpu.Registers[2] = 0x03;
            _cpu.Registers[3] = 0x04;
            _cpu.Registers[4] = 0x05;
            _cpu.Registers[5] = 0x06;
            _cpu.Registers[6] = 0x07;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 3, 12);
            Assert.AreEqual((byte)0x04, _cpu.ReadRTable(r));

            _cpu.Registers[0] = 0x01;
            _cpu.Registers[1] = 0x02;
            _cpu.Registers[2] = 0x03;
            _cpu.Registers[3] = 0x04;
            _cpu.Registers[4] = 0x05;
            _cpu.Registers[5] = 0x06;
            _cpu.Registers[6] = 0x07;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 4, 16);
            Assert.AreEqual((byte)0x05, _cpu.ReadRTable(r));

            _cpu.Registers[0] = 0x01;
            _cpu.Registers[1] = 0x02;
            _cpu.Registers[2] = 0x03;
            _cpu.Registers[3] = 0x04;
            _cpu.Registers[4] = 0x05;
            _cpu.Registers[5] = 0x06;
            _cpu.Registers[6] = 0x07;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 5, 20);
            Assert.AreEqual((byte)0x06, _cpu.ReadRTable(r));

            _cpu.Registers[0] = 0x01;
            _cpu.Registers[1] = 0x02;
            _cpu.Registers[2] = 0x03;
            _cpu.Registers[3] = 0x04;
            _cpu.Registers[4] = 0x05;
            _cpu.Registers[5] = 0x06;
            _cpu.Registers[6] = 0x07;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 6, 24);
            Assert.AreEqual((byte)0x07, _cpu.ReadRTable(r));

            _cpu.Registers[0] = 0x01;
            _cpu.Registers[1] = 0x02;
            _cpu.Registers[2] = 0x03;
            _cpu.Registers[3] = 0x04;
            _cpu.Registers[4] = 0x05;
            _cpu.Registers[5] = 0xC0; // H
            _cpu.Registers[6] = 0x12; // L = 0xC012
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 7, 32);
            Assert.AreEqual((byte)0xAA, _cpu.ReadRTable(r));

            _cpu.Registers[0] = 0x01;
            _cpu.Registers[1] = 0x02;
            _cpu.Registers[2] = 0x03;
            _cpu.Registers[3] = 0x04;
            _cpu.Registers[4] = 0x05;
            _cpu.Registers[5] = 0x06;
            _cpu.Registers[6] = 0x07;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 8, 36);
            Assert.AreEqual((byte)0x01, _cpu.ReadRTable(r));
        }

        [TestMethod]
        public void LoadWordIndirectHL()
        {
            for (int i = 0; i < 7; i++)
            {
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + i] = (byte)(0x70 + i);
            }

            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 6] = 0x77;

            _cpu.WriteMemory(0xC012, 0xAA);

            _cpu.Registers[0] = 0x01;
            _cpu.Registers[1] = 0x02;
            _cpu.Registers[2] = 0x03;
            _cpu.Registers[3] = 0x04;
            _cpu.Registers[4] = 0x05;
            _cpu.Registers[5] = 0xC0; // H
            _cpu.Registers[6] = 0x12; // L = 0xC012
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 1, 8);
            Assert.AreEqual((byte)0x02, _cpu.ReadRTable(6));

            _cpu.Registers[0] = 0x01;
            _cpu.Registers[1] = 0x02;
            _cpu.Registers[2] = 0x03;
            _cpu.Registers[3] = 0x04;
            _cpu.Registers[4] = 0x05;
            _cpu.Registers[5] = 0xC0; // H
            _cpu.Registers[6] = 0x12; // L = 0xC012
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 2, 16);
            Assert.AreEqual((byte)0x03, _cpu.ReadRTable(6));

            _cpu.Registers[0] = 0x01;
            _cpu.Registers[1] = 0x02;
            _cpu.Registers[2] = 0x03;
            _cpu.Registers[3] = 0x04;
            _cpu.Registers[4] = 0x05;
            _cpu.Registers[5] = 0xC0; // H
            _cpu.Registers[6] = 0x12; // L = 0xC012
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 3, 24);
            Assert.AreEqual((byte)0x04, _cpu.ReadRTable(6));

            _cpu.Registers[0] = 0x01;
            _cpu.Registers[1] = 0x02;
            _cpu.Registers[2] = 0x03;
            _cpu.Registers[3] = 0x04;
            _cpu.Registers[4] = 0x05;
            _cpu.Registers[5] = 0xC0; // H
            _cpu.Registers[6] = 0x12; // L = 0xC012
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 4, 32);
            Assert.AreEqual((byte)0x05, _cpu.ReadRTable(6));

            _cpu.Registers[0] = 0x01;
            _cpu.Registers[1] = 0x02;
            _cpu.Registers[2] = 0x03;
            _cpu.Registers[3] = 0x04;
            _cpu.Registers[4] = 0x05;
            _cpu.Registers[5] = 0xC0; // H
            _cpu.Registers[6] = 0x12; // L = 0xC012
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 5, 40);
            Assert.AreEqual((byte)0xC0, _cpu.ReadRTable(6));

            _cpu.Registers[0] = 0x01;
            _cpu.Registers[1] = 0x02;
            _cpu.Registers[2] = 0x03;
            _cpu.Registers[3] = 0x04;
            _cpu.Registers[4] = 0x05;
            _cpu.Registers[5] = 0xC0; // H
            _cpu.Registers[6] = 0x12; // L = 0xC012
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 6, 48);
            Assert.AreEqual((byte)0x12, _cpu.ReadRTable(6));

            _cpu.Registers[0] = 0x01;
            _cpu.Registers[1] = 0x02;
            _cpu.Registers[2] = 0x03;
            _cpu.Registers[3] = 0x04;
            _cpu.Registers[4] = 0x05;
            _cpu.Registers[5] = 0xC0; // H
            _cpu.Registers[6] = 0x12; // L = 0xC012
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 7, 56);
            Assert.AreEqual((byte)0x01, _cpu.ReadRTable(6));
        }

        [TestMethod]
        public void LoadDWord()
        {
            for (int i = 0; i < 4; i++)
            {
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + i * 3] = (byte)(0x01 + i * 0x10);
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 1 + i * 3] = 0xAA;
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 2 + i * 3] = 0xBB;
            }

            for (uint i = 0; i < 4; i++)
            {
                _cpu.Step();
                Assert.That.CpuIsInState(_cpu, (ushort)(Cartridge.CodeStartAddress + 3 * (i + 1)), 12UL * (i + 1));

                if (i != 3) Assert.AreEqual((ushort)0xBBAA, (ushort)((_cpu.Registers[1 + 2 * i] << 8) | _cpu.Registers[2 + 2 * i]));
                else Assert.AreEqual((ushort)0xBBAA, _cpu.SP);
            }
        }

        [TestMethod]
        public void LoadWord()
        {
            for (int i = 0; i < 3; i++)
            {
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + i * 2] = (byte)(0x06 + i * 0x10);
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + i * 2 + 1] = 0xAA;
            }

            for (int i = 0; i < 4; i++)
            {
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 6 + i * 2] = (byte)(0x0E + i * 0x10);
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 7 + i * 2] = 0xBB;
            }

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 2, 8UL);
            Assert.AreEqual(0xAA, _cpu.Registers[1]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 4, 16UL);
            Assert.AreEqual(0xAA, _cpu.Registers[3]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 6, 24UL);
            Assert.AreEqual(0xAA, _cpu.Registers[5]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 8, 32UL);
            Assert.AreEqual(0xBB, _cpu.Registers[2]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 10, 40UL);
            Assert.AreEqual(0xBB, _cpu.Registers[4]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 12, 48UL);
            Assert.AreEqual(0xBB, _cpu.Registers[6]);

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 14, 56UL);
            Assert.AreEqual(0xBB, _cpu.Registers[0]);
        }

        [TestMethod]
        public void StoreWordIndirectHL()
        {
            _cpu.Registers[5] = 0xC0;
            _cpu.Registers[6] = 0x11;

            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress] = 0x36;
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 1] = 0xCC;

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 2, 12UL);
            Assert.AreEqual(0xCC, _cpu.ResolveMemory(0xC011));
        }

        [TestMethod]
        public void StoreWordIndirectMisc()
        {
            _cpu.Registers[1] = 0xC0; // B
            _cpu.Registers[2] = 0x10; // C = 0x0110

            _cpu.Registers[3] = 0xC0; // D
            _cpu.Registers[4] = 0x11; // E = 0x0111

            _cpu.Registers[5] = 0xC0; // H
            _cpu.Registers[6] = 0x12; // L = 0xC012


            for (int i = 0; i < 4; i++)
            {
                _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + i] = (byte)(0x02 + i * 0x10);
            }

            _cpu.Registers[0] = 0xAA;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 1, 8UL);
            Assert.AreEqual(0xAA, _cpu.ResolveMemory(0xC010));

            _cpu.Registers[0] = 0xBB;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 2, 16UL);
            Assert.AreEqual(0xBB, _cpu.ResolveMemory(0xC011));

            _cpu.Registers[0] = 0xCC;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 3, 24UL);
            Assert.AreEqual(0xCC, _cpu.ResolveMemory(0xC012));
            Assert.AreEqual(0xC0, _cpu.Registers[5]);
            Assert.AreEqual(0x13, _cpu.Registers[6]);

            _cpu.Registers[0] = 0xDD;
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 4, 32UL);
            Assert.AreEqual(0xDD, _cpu.ResolveMemory(0xC013));
            Assert.AreEqual(0xC0, _cpu.Registers[5]);
            Assert.AreEqual(0x12, _cpu.Registers[6]);
        }

        [TestMethod]
        public void LoadRegisterAIndirect()
        {
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 0] = 0x0A;
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 1] = 0x1A;
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 2] = 0x2A;
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 3] = 0x3A;

            _cpu.WriteMemory(0xC012, 0xAA);
            _cpu.WriteMemory(0xC013, 0x0F);
            _cpu.WriteMemory(0xC014, 0xFF);

            _cpu.Registers[1] = 0xC0; // B
            _cpu.Registers[2] = 0x12; // C = 0xC012
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 1, 8);
            Assert.AreEqual(0xAA, _cpu.Registers[0]);

            _cpu.Registers[3] = 0xC0; // D
            _cpu.Registers[4] = 0x13; // E = 0xC013
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 2, 16);
            Assert.AreEqual(0x0F, _cpu.Registers[0]);

            _cpu.Registers[5] = 0xC0; // H
            _cpu.Registers[6] = 0x14; // L = 0xC014
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 3, 24);
            Assert.AreEqual(0xFF, _cpu.Registers[0]);
            Assert.AreEqual(0xC0, _cpu.Registers[5]);
            Assert.AreEqual(0x15, _cpu.Registers[6]);

            _cpu.Registers[5] = 0xC0; // H
            _cpu.Registers[6] = 0x14; // L = 0xC014
            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 4, 32);
            Assert.AreEqual(0xFF, _cpu.Registers[0]);
            Assert.AreEqual(0xC0, _cpu.Registers[5]);
            Assert.AreEqual(0x13, _cpu.Registers[6]);
        }

        [TestMethod]
        public void StoreStackIndirect()
        {
            _cpu.SP = 0xABCD;

            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 0] = 0x08;
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 1] = 0xC0;
            _cpu.LoadedCartridge.Bank0[Cartridge.CodeStartAddress + 2] = 0xAA;

            _cpu.Step();
            Assert.That.CpuIsInState(_cpu, Cartridge.CodeStartAddress + 3, 20);
            Assert.AreEqual((byte)0xCD, _cpu.ResolveMemory(0xC0AA));
            Assert.AreEqual((byte)0xAB, _cpu.ResolveMemory(0xC0AB));
        }
    }
}