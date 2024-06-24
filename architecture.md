# Architecture
This is the architecture documentation for the SMC-4

The SMC-4 has 65536 signed 32-bit memory addresses(accessed with 0x0000 to 0xFFFF), and 32 signed 32-bit registers:<br>
3 special registers(accessed with `0`/`zero`, `1`/`comp`, `2`/`cc`, respectively), 4 argument registers(accessed with 3-6), and 24 general purpose registers(accessed with 7-31)


## Instruction Set

Instruction | Opcode | Register | Address/Data | Address/Register/Data | Operation
------- | ------- | ------- | ------- | ------- | -------
ADD [srcReg1] [srcReg2] [dstReg] | 0 | 0-16 | 0-16 | 0-16 | `dstReg = srcReg + srcReg2`
ADDI [imm] [srcReg] [dstReg] | 1 | immediate(0-255) | 0-16 | 0-16 | `dstReg = srcReg + imm`
SUB [srcReg1] [srcReg2] [dstReg] | 2 | 0-16 | 0-16 | 0-16 | `dstReg = srcReg1 - srcReg2`
SUBI [imm] [srcReg] [dstReg] | 3 | immediate(0-255) | 0-16 | 0-16 | `dstReg = srcReg - imm`
AND [srcReg1] [srcReg2] [dstReg] | 4 | 0-16 | 0-16 | 0-16 | `dstReg = srcReg1 & srcReg2`
ANDI [imm] [srcReg] [dstReg] | 5 | immediate(0-255) | 0-16 | 0-16 | `dstReg = srcReg & imm`
OR [srcReg1] [srcReg2] [dstReg] | 6 | 0-16 | 0-16 | 0-16 | `dstReg = srcReg1 \| srcReg2`
ORI [imm] [srcReg] [dstReg] | 7 | immediate(0-255) | 0-16 | 0-16 | `dstReg = srcReg \| imm`
XOR [srcReg1] [srcReg2] [dstReg] | 8 | 0-16 | 0-16 | 0-16 | `dstReg = srcReg1 ^ srcReg2`
XORI [imm] [srcReg] [dstReg] | 9 | immediate(0-255) | 0-16 | 0-16 | `dstReg = srcReg ^ imm`
STORE [srcReg] [memoryAddress] | 10 | 0-16 | 0x00-0xFF | 0x00-0xFF | `memoryAddress = srcReg`
LOAD [dstReg] [memoryAddress] | 11 | 0-16 | 0x00-0xFF | 0x00-0xFF | `dstReg = memoryAddress`
CMP [reg1] [reg2] | 12 | 0-16 | 0-16 | 0x00 | `registers[1] = reg1 == reg2 ? 1 : reg1 != reg2 ? 2 : reg1 < reg2 ? 3 : reg1 > reg2 ? 4 : 0`
JMP [immediate \| label] | 13 | 0x00-0xFF | 0x00-0xFF | 0x00-0xFF | `pc = [immediate \| label]`
JEQ [immediate \| label] | 14 | 0x00-0xFF | 0x00-0xFF | 0x00-0xFF | `if (registers[1] == 1) pc = [immediate \| label]`
JNE [immediate \| label] | 15 | 0x00-0xFF | 0x00-0xFF | 0x00-0xFF | `if (registers[1] == 2) pc = [immediate \| label]`
JLT [immediate \| label] | 16 | 0x00-0xFF | 0x00-0xFF | 0x00-0xFF | `if (registers[1] == 3) pc = [immediate \| label]`
JGT [immediate \| label] | 17 | 0x00-0xFF | 0x00-0xFF | 0x00-0xFF | `if (registers[1] == 4) pc = [immediate \| label]`
JAL [immediate \| label] | 18 | 0x00-0xFF | 0x00-0xFF | 0x00-0xFF | `ra = pc; pc = [immediate \| label]`
LABEL | 252 | 252 | 252 | 252 |
RET | 253 | 253 | 253 | 253 | `pc = ra; ra = -1`
HALT | 254 | 254 | 254 | 254 | `stops the program`
NOOP | 255 | 255 | 255 | 255 | `no operation`


## Conventions
Operations that put something into a register(with the exception of LOAD for symmetry with STORE) use the format `[op] [input1] [input2] [destination]`

Register 0 is meant to be a 0 constant(but it's not actually hardcoded because I'm lazy) and can be accessed with either `0` or `zero`

Register 1 is the register that the result of the CMP operation is put into, and is the register that the JMP operations check, it can be accessed with either `1` or `comp`

Register 2 is the call code register, it's where you put the system call codes, it can be accessed with either `2` or `cc`