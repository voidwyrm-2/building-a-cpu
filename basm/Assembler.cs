namespace Basm
{
    class Program
    {
        private static readonly string usage = "basm [input path]]";

        static void Main(string[] args)
        {
            string outpath;

            if (args.Length != 1)
            {
                Console.WriteLine("expected '" + usage + "'");
                return;
            }

            string inpath = args[0];

            string t = "";
            string[] p = Path.GetFullPath(inpath).Split(Path.DirectorySeparatorChar);
            for (int i = 0; i < p.Length - 1; i++)
            {
                t += Path.DirectorySeparatorChar.ToString() + p[i];
            }
            outpath = t[1..] + "/" + Path.GetFileNameWithoutExtension(inpath) + ".bin";

            string content;
            try
            {
                content = File.ReadAllText(inpath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            AssemblerResult asmRes = Assemble(content);
            if (asmRes.err != null)
            {
                Console.WriteLine(asmRes.err);
                return;
            }

            try
            {
                File.WriteAllBytes(outpath, asmRes.bytes);
                if (Path.Exists(outpath)) Console.WriteLine($"file '{Path.GetFileName(outpath)}' already existed and was overwritten");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }

        private readonly struct AssemblerResult(byte[] bytes, Dictionary<string, int> labels, string? err = null)
        {
            public readonly byte[] bytes = bytes;

            public readonly Dictionary<string, int> labels = labels;

            public readonly string? err = err;
        }

        private static AssemblerResult Assemble(string text)
        {
            const int maxRegisters = 16;

            Dictionary<string, byte> instructions = [];
            string?[][] ilist = [
                ["add"],
                ["addi"],
                ["sub"],
                ["subi"],
                ["and"],
                ["andi"],
                ["or"],
                ["ori"],
                ["xor"],
                ["xori"],
                ["store"],
                ["load"],
                ["cmp"],
                ["jmp"],
                ["jeq"],
                ["jne"],
                ["jlt"],
                ["jgt"],
                ["halt", "0xFE"],
                ["noop", "0xFF"]
            ];
            if (ilist.Length > byte.MaxValue) throw new NotImplementedException();

            for (byte i = 0; i < ilist.Length; i++)
            {
                string?[] a = ilist[i];
                if (a.Length > 1)
                {
#pragma warning disable CS8604
                    instructions[a[0]] = Convert.ToByte(a[1], 16);
#pragma warning restore CS8604
                }
                else
                {
#pragma warning disable CS8604
                    instructions[a[0]] = i;
#pragma warning restore CS8604
                }
            }

            static string NewError(string msg, int ln)
            {
                return $"error on line {ln + 1}: {msg}";
            }

            string[] lines = text.Trim().Split('\n');

            List<byte> bytes = [];

            Dictionary<string, int> labels = [];

            int[] ignoredLines = new int[lines.Length];

            static AssemblerResult GetRegister(string reg, int ln)
            {
                if (reg.Equals("zero", StringComparison.CurrentCultureIgnoreCase))
                {
                    return new AssemblerResult([0], []);
                }
                else if (reg.Equals("comp", StringComparison.CurrentCultureIgnoreCase))
                {
                    return new AssemblerResult([1], []);
                }

                byte convertedReg;
                try
                {
                    convertedReg = Convert.ToByte(reg);
                }
                catch (Exception)
                {
                    return new AssemblerResult([], [], NewError($"invalid register '{reg}'", ln));
                }

                if (convertedReg > maxRegisters - 1) return new AssemblerResult([], [], NewError($"invalid register '{reg}'", ln));

                return new AssemblerResult([convertedReg], []);
            }

            static AssemblerResult GetImmediate(string imm, int ln)
            {
                byte convertedImm;
                try
                {
                    convertedImm = Convert.ToByte(imm);
                }
                catch (Exception)
                {
                    return new AssemblerResult([], [], NewError($"invalid immediate '{imm}'", ln));
                }

                return new AssemblerResult([convertedImm], []);
            }

            static AssemblerResult GetMemAddr(string addr, int ln)
            {
                short convertedAddr;
                try
                {
                    convertedAddr = Convert.ToInt16(addr, 16);
                }
                catch (Exception)
                {
                    return new AssemblerResult([], [], NewError($"invalid register '{addr}'", ln));
                }

                return new AssemblerResult([(byte)(convertedAddr >> 8), (byte)(convertedAddr & 0xFF)], []);
            }

            for (int ln = 0; ln < lines.Length; ln++)
            {
                string l = lines[ln].Split(';')[0].Trim();

                if (l.Length == 0) continue;

                if (l[0] == '.')
                {
                    string labelName = l[1..].Trim();
                    if (labelName == "") return new AssemblerResult([], [], NewError($"label names cannot be empty", ln));
                    if (labels.ContainsKey(labelName)) return new AssemblerResult([], [], NewError($"label '{labelName}' already exists", ln));
                    labels.Add(labelName, ln * 4);
                    ignoredLines[ln] = 2;
                }
            }

            for (int ln = 0; ln < lines.Length; ln++)
            {
                string l = lines[ln].Split(';')[0].Trim();

                if (l.Length == 0 || ignoredLines[ln] > 0)
                {
                    if (ignoredLines[ln] == 2)
                    {
                        for (int i = 0; i < 4; i++) { bytes.Add(0xFF); }
                    }
                    continue;
                }

                string[] s = l.Split(' ');
                if (instructions.ContainsKey(s[0].ToLower())) bytes.Add(instructions[s[0].ToLower()]);
                AssemblerResult res;
                switch (s[0].ToLower())
                {
                    case "add":
                    case "sub":
                    case "and":
                    case "or":
                    case "xor":
                        if (s.Length != 4) return new AssemblerResult([], [], NewError($"expected '{s[0].ToLower()} [srcReg1] [srcReg2] [dstReg]'", ln));

                        res = GetRegister(s[1], ln);
                        if (res.err != null) return res;
                        bytes.Add(res.bytes[0]);

                        res = GetRegister(s[2], ln);
                        if (res.err != null) return res;
                        bytes.Add(res.bytes[0]);

                        res = GetRegister(s[3], ln);
                        if (res.err != null) return res;
                        bytes.Add(res.bytes[0]);

                        break;

                    case "addi":
                    case "subi":
                    case "andi":
                    case "ori":
                    case "xori":
                        if (s.Length != 4) return new AssemblerResult([], [], NewError($"expected '{s[0].ToLower()} [immediate] [srcReg] [dstReg]'", ln));

                        res = GetImmediate(s[1], ln);
                        if (res.err != null) return res;
                        bytes.Add(res.bytes[0]);

                        res = GetRegister(s[2], ln);
                        if (res.err != null) return res;
                        bytes.Add(res.bytes[0]);

                        res = GetRegister(s[3], ln);
                        if (res.err != null) return res;
                        bytes.Add(res.bytes[0]);

                        break;

                    case "load":
                    case "store":
                        if (s.Length != 3)
                        {
                            return new AssemblerResult([], [], NewError(s[0].Equals("load", StringComparison.CurrentCultureIgnoreCase) ? $"expected '{s[0].ToLower()} [dstReg] [srcAddress]'" : $"expected '{s[0].ToLower()} [srcReg] [dstAdress]'", ln));
                        }

                        res = GetRegister(s[1], ln);
                        if (res.err != null) return res;
                        bytes.Add(res.bytes[0]);

                        res = GetMemAddr(s[2], ln);
                        if (res.err != null) return res;
                        bytes.Add(res.bytes[0]);
                        bytes.Add(res.bytes[1]);

                        break;

                    /*
                    case "mov":
                        if (s.Length != 3) return new AssemblerResult([], [], NewError($"expected '{s[0].ToLower()} [srcReg] [dstReg]'", ln));

                        res = GetRegister(s[1], ln);
                        if (res.err != null) return res;
                        bytes.Add(res.bytes[0]);

                        res = GetRegister(s[2], ln);
                        if (res.err != null) return res;
                        bytes.Add(res.bytes[0]);

                        break;
                    */

                    case "cmp":
                        if (s.Length != 3) return new AssemblerResult([], [], NewError($"expected '{s[0].ToLower()} [srcReg] [dstReg]'", ln));

                        res = GetRegister(s[1], ln);
                        if (res.err != null) return res;
                        bytes.Add(res.bytes[0]);

                        res = GetRegister(s[2], ln);
                        if (res.err != null) return res;
                        bytes.Add(res.bytes[0]);

                        bytes.Add(0);

                        break;

                    case "jmp":
                    case "jeq":
                    case "jne":
                    case "jlt":
                    case "jgt":
                        if (s.Length != 2) return new AssemblerResult([], [], NewError($"expected '{s[0].ToLower()} [immediate | label]'", ln));

                        uint convertedImm;
                        try
                        {
                            convertedImm = Convert.ToUInt32(s[1]);
                        }
                        catch (Exception)
                        {
                            if (labels.TryGetValue(s[1], out int labelVal))
                            {
                                bytes.Add((byte)(labelVal >> 16));
                                bytes.Add((byte)((labelVal >> 8) & 0xFF));
                                bytes.Add((byte)(labelVal & 0xFF));
                                break;
                            }
                            return new AssemblerResult([], [], NewError($"invalid immediate '{s[1]}'", ln));
                        }

                        // I have 3 bytes(24-bits) to hold the address of the jump, but a uint32 is 32-bits(duh),
                        // not 24, so this prevents any problems that would be caused by that
                        if (convertedImm > byte.MaxValue * 3) return new AssemblerResult([], [], NewError($"invalid immediate '{s[1]}'", ln));

                        bytes.Add((byte)(convertedImm >> 16));
                        bytes.Add((byte)((convertedImm >> 8) & 0xFF));
                        bytes.Add((byte)(convertedImm & 0xFF));
                        break;

                    case "halt":
                        if (s.Length != 1) return new AssemblerResult([], [], NewError($"expected '{s[0].ToLower()}'", ln));
                        for (int i = 0; i < 3; i++) { bytes.Add(0xFE); }
                        break;
                    case "noop":
                        if (s.Length != 1) return new AssemblerResult([], [], NewError($"expected '{s[0].ToLower()}'", ln));
                        for (int i = 0; i < 3; i++) { bytes.Add(0xFF); }
                        break;
                    default:
                        return new AssemblerResult([], [], NewError($"unknown instruction '{s[0]}'", ln));
                }
            }

            /*
            string temp = "";
            foreach (byte b in bytes)
            {
                temp += b.ToString() + " ";
            }
            Console.WriteLine(temp.Trim());
            */
            return new AssemblerResult([.. bytes], labels);
        }
    }
}