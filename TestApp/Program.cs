using System;
using System.Diagnostics;
using ViruBoy;

namespace TestApp
{
    public class Program
    {
        private static Cartridge _cart;
        private static VirtualCpu _cpu;
        private static bool _step = true;
        private static ushort _breakpoint;
        private static bool _running = true;

        public static void Main(string[] args)
        {
            _cart = new Cartridge("Tetris");

            for (int i = 0; i < 100; i++)
            {
                _cpu = new VirtualCpu(_cart);
                TestLength(1000000);
            }

            Console.ReadLine();
            

            while (_running)
            {
                _cpu.Step();
                UpdateConsole();
            }
        }

        private static void TestLength(ulong instructionCount)
        {
            Stopwatch watch = Stopwatch.StartNew();

            for (ulong i = 0; i < instructionCount; i++) _cpu.Step();

            watch.Stop();
            Console.WriteLine($"{instructionCount} instructions: {watch.Elapsed.TotalMilliseconds}ms");
        }

        private static void UpdateConsole()
        {
            if (_cpu.PC == _breakpoint) _step = true;

            if (_step)
            {
                Console.Clear();
                Console.WriteLine("AF= {0:X2}{1:X2}", _cpu.Registers[0], (byte)_cpu.F);
                Console.WriteLine("BC= {0:X2}{1:X2}", _cpu.Registers[1], _cpu.Registers[2]);
                Console.WriteLine("DE= {0:X2}{1:X2}", _cpu.Registers[3], _cpu.Registers[4]);
                Console.WriteLine("HL= {0:X2}{1:X2}", _cpu.Registers[5], _cpu.Registers[6]);
                Console.WriteLine("SP= {0:X4}", _cpu.SP);
                Console.WriteLine("PC= {0:X4}", _cpu.PC);

                if (_cpu.PC == _breakpoint) Console.WriteLine("Breakpoint hit!");

                var info = Console.ReadKey(true);

                switch (info.Key)
                {
                    case ConsoleKey.M: //[M]emory
                    {
                        Console.Write("Memory address: ");
                        var mem = Console.ReadLine();
                        var addr = Convert.ToUInt16(mem, 16);
                        Console.WriteLine(_cpu.ResolveMemory(addr));
                        Console.ReadKey(true);
                        break;
                    }
                    case ConsoleKey.B: // [B]reakpoint
                    {
                        Console.Write("Breakpoint address: ");
                        var mem = Console.ReadLine();
                        _breakpoint = Convert.ToUInt16(mem, 16);
                        _step = false;
                        break;
                    }
                    case ConsoleKey.R: // [R]un
                        _step = false;
                        break;
                    case ConsoleKey.T: // [T]ime
                        Console.Write("Benchmark: ");
                        var ben = Console.ReadLine();
                        TestLength(Convert.ToUInt64(ben));
                        Console.WriteLine("Press any key to resume debugger...");
                        Console.ReadKey(true);
                        break;
                    case ConsoleKey.E: // [E]xit
                        _running = false;
                        break;
                }
            }
        }
    }
}
