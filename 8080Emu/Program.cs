using System;
using System.IO;

namespace _8080Emu
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            i8080Cpu cpu = new i8080Cpu();

            loadMemory(cpu);

            displayRegs(cpu);

            Console.WriteLine("***BEGIN***");

            while (cpu.running)
            {
                cpu.execute();

                displayRegs(cpu);
                displayFlags(cpu);
                Console.WriteLine();
            }

            Console.WriteLine("***END***");
            displayRegs(cpu);

            displayFlags(cpu);
            Console.Read();
            //dumpMemory(cpu);
        }

        public static void displayRegs(i8080Cpu cpu)
        {
            Console.WriteLine("A:0x{0}    B:0x{1}", cpu.reg_a.ToString("X2"), cpu.reg_b.ToString("X2"));
            Console.WriteLine("C:0x{0:X2}    D:0x{1}", cpu.reg_c, cpu.reg_d.ToString("X2"));
            Console.WriteLine("E:0x{0}    H:0x{1}", cpu.reg_e.ToString("X2"), cpu.reg_h.ToString("X2"));
            Console.WriteLine("L:0x{0}    FLAGS:0x{1}", cpu.reg_l.ToString("X2"), cpu.reg_flags.ToString("X2"));
        }

        public static void displayFlags(i8080Cpu cpu)
        {
            Console.WriteLine("CF: {0} PF: {1} ZF: {2} SF: {3}", cpu.getCF(), cpu.getPF(), cpu.getZF(), cpu.getSF());
        }


        public static void dumpMemory(i8080Cpu cpu)
        {
            StreamWriter dumpfile = new StreamWriter("mem.txt");
            BinaryWriter binDumpFile = new BinaryWriter(File.Open("mem.bin", FileMode.Create));
            for (int i = 0x0; i <= 0xFFFF; i++)
            {
                dumpfile.WriteLine("0x{0} 0x{1}", i.ToString("X4"), cpu.memory[i].ToString("X2"));
                binDumpFile.Write(cpu.memory[i]);
            }

            dumpfile.Close();
            binDumpFile.Close();
        }

        public static void loadMemory(i8080Cpu cpu)
        {
            BinaryReader binReader = new BinaryReader(File.OpenRead("mem.bin"));

            for (int i = 0x0; i <= 0xffff; i++)
            {
                cpu.memory[i] = binReader.ReadByte();
            }
            binReader.Close();
        }
    }
}