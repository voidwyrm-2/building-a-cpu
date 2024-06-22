namespace CPU
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("expected 'smc4 [binary file path]'");
                return;
            }

            byte[] bytes;
            try
            {
                bytes = File.ReadAllBytes(args[0]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

            RunByteCode(bytes, 1);
        }

        static void RunByteCode(byte[] code, byte printmode = 0)
        {
            if (code.Length % 2 != 0)
            {
                Console.WriteLine("error: amount of bytes is a non-eight-divisible number");
                return;
            }

            byte[] registers = new byte[8];
            byte[] memory = new byte[0xFFFF];

            for (int pc = 0; pc < code.Length; pc += 4)
            {
                switch (code[pc])
                {
                    // add and addi
                    case 0:
                        registers[code[pc + 3]] = (byte)(registers[code[pc + 1]] + registers[code[pc + 2]]);
                        break;
                    case 1:
                        registers[code[pc + 3]] = (byte)(code[pc + 1] + registers[code[pc + 2]]);
                        break;

                    // sub and subi
                    case 2:
                        registers[code[pc + 3]] = (byte)(registers[code[pc + 1]] - registers[code[pc + 2]]);
                        break;
                    case 3:
                        registers[code[pc + 3]] = (byte)(code[pc + 1] - registers[code[pc + 2]]);
                        break;

                    // store and load
                    case 4:
                        memory[code[pc + 2] << 4 + code[pc + 3]] = registers[code[pc + 1]];
                        break;
                    case 5:
                        registers[code[pc + 1]] = memory[code[pc + 2] << 4 + code[pc + 3]];
                        break;

                    // halt and noop
                    case 0xFE:
                        if (code[pc + 1] == 0xFE && code[pc + 2] == 0xFE && code[pc + 3] == 0xFE) return;
                        Console.WriteLine($"error: unknown opcode '{code[pc]}'");
                        return;
                    case 0xFF:
                        if (code[pc + 1] != 0xFF || code[pc + 2] != 0xFF || code[pc + 3] != 0xFF)
                        {
                            Console.WriteLine($"error: unknown opcode '{code[pc]}'");
                            return;
                        }
                        break;

                    default:
                        Console.WriteLine($"error: unknown opcode '{code[pc]}'");
                        return;
                }

                if (printmode == 2)
                {
                    string registers_str = "";
                    foreach (var i in registers) registers_str += i.ToString() + " ";
                    Console.WriteLine("registers: " + registers_str.Trim());
                }
            }

            if (printmode == 1)
            {
                string registers_str = "";
                foreach (var i in registers) registers_str += i.ToString() + " ";
                Console.WriteLine("registers: " + registers_str.Trim());

                string memory_str = "";
                foreach (var i in memory) memory_str += i.ToString() + " ";
                Console.WriteLine("memory: " + memory_str.Trim());
            }
        }
    }
}