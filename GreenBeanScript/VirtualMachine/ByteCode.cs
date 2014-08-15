using System;
using System.Collections.Generic;
using System.Text;

namespace GreenBeanScript
{
    namespace ByteCode
    {
        internal enum Operator : int
        {
            GetDot = 0,
            SetDot,
            GetInd,
            SetInd,

            OpAdd,
            OpSub,
            OpMul,
            OpDiv,
            OpRem,

            BitOr,
            BitXor,
            BitAnd,
            BitShl,
            BitShr,
            BitInv,

            OpLt,
            OpGt,
            OpLte,
            OpGte,
            OpEq,
            OpNeq,

            OpNeg,
            OpPos,
            OpNot,

            Nop,
            Line,

            Bra,
            Brz,
            Brnz,
            Brzk,
            Brnzk,
            Call,
            Ret,
            Retv,
            ForEach,

            Pop,
            Pop2,
            Dup,
            Dup2,
            Swap,
            PushNull,
            PushInt,
            PushInt0,
            PushInt1,
            PushFp,
            PushStr,
            PushTbl,
            PushFn,
            PushThis,

            GetLocal,
            SetLocal,
            GetGlobal,
            SetGlobal,
            GetThis,
            SetThis,
        }

        internal struct Instruction
        {

            public Instruction(Operator OpCode, int ByteCodeOffset)
            {
                _OpCode = OpCode;
                _Operands = null;
                _ByteCodeOffset = ByteCodeOffset;
            }

            public Instruction(Operator OpCode, int ByteCodeOffset, Variable[] Operands)
            {
                _OpCode = OpCode;
                _Operands = Operands;
                _ByteCodeOffset = ByteCodeOffset;
            }

            public int ByteCodeOffset
            {
                get { return _ByteCodeOffset; }
            }

            public Operator OpCode
            {
                get { return _OpCode; }
            }

            public int OperandCount
            {
                get { return _Operands != null ? _Operands.Length : 0; }
            }

            public Variable this[int index]
            {
                get { return _Operands[index]; }
            }

            public void SetOperand(int Index, Variable Value)
            {
                _Operands[Index] = Value;
            }

            private Operator _OpCode;
            private Variable[] _Operands;
            private int _ByteCodeOffset;
        }
        

    }
}
