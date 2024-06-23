# Architecture
This is the architecture documentation for the SMC-4

The SMC-4 has 65536 8-bit memory addresses(accessed with 0x0000 to 0xFFFF), 2 8-bit special register(accessed with `0`/`zero` and `1`/`comp`), and 14 8-bit general purpose registers(accessed with 2-15)


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
JMP [immediate \| label] | 13 | 0x00-0xFF | 0x00-0xFF | 0x00-0xFF | `pc += [immediate \| label]`
JEQ [immediate \| label] | 14 | 0x00-0xFF | 0x00-0xFF | 0x00-0xFF | `if (registers[1] == 1) pc += [immediate \| label]`
JNE [immediate \| label] | 15 | 0x00-0xFF | 0x00-0xFF | 0x00-0xFF | `if (registers[1] == 2) pc += [immediate \| label]`
JLT [immediate \| label] | 16 | 0x00-0xFF | 0x00-0xFF | 0x00-0xFF | `if (registers[1] == 3) pc += [immediate \| label]`
JGT [immediate \| label] | 17 | 0x00-0xFF | 0x00-0xFF | 0x00-0xFF | `if (registers[1] == 4) pc += [immediate \| label]`
HALT | 0xFE | 0xFE | 0xFE | 254 | `stops the program`
NOOP | 0xFF | 0xFF | 0xFF | 255 | `no operation`


## Conventions
Operations that put something into a register(with the exception of LOAD for symmetry with STORE) use the format `[op] [input1] [input2] [destination]`'

Register 0 is meant to be a 0 constant(but it's not actually hardcoded because I'm lazy) and can be accessed with either `0` or `zero`

Register 1 is the register that the result of the CMP operation is put into, and is the register that the JMP operations check, it can be with either `1` or `comp`