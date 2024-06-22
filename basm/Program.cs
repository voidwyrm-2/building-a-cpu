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
            const int maxRegisters = 8;

            Dictionary<string, byte> instructions = [];
            string?[][] ilist = [
                ["add"],
                ["addi"],
                ["sub"],
                ["subi"],
                ["store"],
                ["load"],
                ["halt", "0xFE"],
                ["noop", "0xFF"]
            ];
            if (ilist.Length > byte.MaxValue) throw new NotImplementedException();

            for (byte i = 0; i < ilist.Length; i++)
            {
                var a = ilist[i];
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

            static AssemblerResult GetRegister(string reg, int ln)
            {
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
                    return new AssemblerResult([], [], NewError($"invalid register '{imm}'", ln));
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
                }
                else
                {
                    string[] s = l.Split(' ');
                    if (instructions.ContainsKey(s[0].ToLower())) bytes.Add(instructions[s[0].ToLower()]);
                    AssemblerResult res;
                    switch (s[0].ToLower())
                    {
                        case "add":
                        case "sub":
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
                                if (s[0].Equals("load", StringComparison.CurrentCultureIgnoreCase)) return new AssemblerResult([], [], NewError($"expected '{s[0].ToLower()} [dstReg] [srcAddress]'", ln));
                                return new AssemblerResult([], [], NewError($"expected '{s[0].ToLower()} [srcReg] [dstAdress]'", ln));
                            }

                            res = GetRegister(s[1], ln);
                            if (res.err != null) return res;
                            bytes.Add(res.bytes[0]);

                            res = GetMemAddr(s[2], ln);
                            if (res.err != null) return res;
                            bytes.Add(res.bytes[0]);
                            bytes.Add(res.bytes[1]);

                            break;

                        case "halt":
                            for (int i = 0; i < 4; i++) { bytes.Add(0xFE); }
                            break;
                        case "noop":
                            for (int i = 0; i < 4; i++) { bytes.Add(0xFF); }
                            break;
                        default:
                            return new AssemblerResult([], [], NewError($"unknown instruction '{s[0]}'", ln));
                    }
                }
            }
            return new AssemblerResult([.. bytes], labels);
        }

        private static string? NewError()
        {
            throw new NotImplementedException();
        }
    }
}