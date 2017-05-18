namespace GreenBeanScript.VirtualMachine
{
    namespace ByteCode
    {
        internal struct Instruction
        {
            public Instruction(Opcode opCode, int byteCodeOffset)
            {
                this.OpCode = opCode;
                _operands = null;
                this.ByteCodeOffset = byteCodeOffset;
            }

            public Instruction(Opcode opCode, int byteCodeOffset, Variable[] operands)
            {
                this.OpCode = opCode;
                _operands = operands;
                this.ByteCodeOffset = byteCodeOffset;
            }

            public int ByteCodeOffset { get; }

            public Opcode OpCode { get; }

            public int OperandCount
            {
                get { return _operands != null ? _operands.Length : 0; }
            }

            public Variable this[int index]
            {
                get { return _operands[index]; }
            }

            public void SetOperand(int index, Variable value)
            {
                _operands[index] = value;
            }

            private readonly Variable[] _operands;
        }
    }
}