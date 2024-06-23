﻿namespace SMC4
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

            RunByteCode(bytes, true, 2);
        }

        static void RunByteCode(byte[] code, bool debug = false, byte printmode = 0)
        {
            if (code.Length % 4 != 0)
            {
                Console.WriteLine("error: amount of bytes is a non-four-divisible number");
                return;
            }

            byte[] registers = new byte[16];
            byte[] memory = new byte[65536];

            int pc = 0;
            while (pc < code.Length)
            {
                if (debug) Console.WriteLine($"{pc}: {code[pc]}, {code[pc + 1]}, {code[pc + 2]}, {code[pc + 3]}");
                bool shouldInc = true;
                int jumpAddress = (code[pc + 2] << 8) + (code[pc + 2] << 4) + code[pc + 3];

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

                    // and and andi
                    case 4:
                        registers[code[pc + 3]] = (byte)(registers[code[pc + 1]] & registers[code[pc + 2]]);
                        break;
                    case 5:
                        registers[code[pc + 3]] = (byte)(code[pc + 1] & registers[code[pc + 2]]);
                        break;

                    // or and ori
                    case 6:
                        registers[code[pc + 3]] = (byte)(registers[code[pc + 1]] | registers[code[pc + 2]]);
                        break;
                    case 7:
                        registers[code[pc + 3]] = (byte)(code[pc + 1] | registers[code[pc + 2]]);
                        break;

                    // xor and xori
                    case 8:
                        registers[code[pc + 3]] = (byte)(registers[code[pc + 1]] ^ registers[code[pc + 2]]);
                        break;
                    case 9:
                        registers[code[pc + 3]] = (byte)(code[pc + 1] ^ registers[code[pc + 2]]);
                        break;

                    // store and load
                    case 10:
                        memory[code[pc + 2] << 4 + code[pc + 3]] = registers[code[pc + 1]];
                        break;
                    case 11:
                        registers[code[pc + 1]] = memory[code[pc + 2] << 4 + code[pc + 3]];
                        break;

                    // cmp
                    case 12:
                        if (registers[code[pc + 1]] == registers[code[pc + 2]])
                        {
                            registers[1] = 1;
                        }
                        else if (registers[code[pc + 1]] != registers[code[pc + 2]])
                        {
                            registers[1] = 2;
                        }
                        else if (registers[code[pc + 1]] < registers[code[pc + 2]])
                        {
                            registers[1] = 3;
                        }
                        else if (registers[code[pc + 1]] > registers[code[pc + 2]])
                        {
                            registers[1] = 4;
                        }
                        else
                        {
                            registers[1] = 0;
                        }
                        break;

                    // jmp
                    case 13:
                        pc = jumpAddress;
                        continue;
                    // jeq
                    case 14:
                        if (registers[1] == 1)
                        {
                            pc = jumpAddress;
                            continue;
                        }
                        break;
                    // jne
                    case 15:
                        if (registers[1] == 2)
                        {
                            pc = jumpAddress;
                            continue;
                        }
                        break;
                    // jlt
                    case 16:
                        if (registers[1] == 3)
                        {
                            pc = jumpAddress;
                            continue;
                        }
                        break;
                    // jgt
                    case 17:
                        if (registers[1] == 4)
                        {
                            pc = jumpAddress;
                            continue;
                        }
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

                if (shouldInc) pc += 4;
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