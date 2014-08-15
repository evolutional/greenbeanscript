using System;
using System.Collections.Generic;
using System.Text;

namespace GreenBeanScript
{
    public delegate void OperatorCallback(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2 );
    public interface TypeOperators
    {
        void Initialise(Machine Machine, ScriptType Type);
    }

    public enum Operator : int
    {
        GetDot,
        SetDot,
        GetInd,
        SetInd,

        Add,
        Sub,
        Mul,
        Div,
        Rem,

        BitOr,
        BitXor,
        BitAnd,
        BitShiftLeft,
        BitShiftRight,

        Inv,
        Lt,
        Gt,
        Lte,
        Gte,
        Eq,
        Neq,

        Neg,
        Pos,
        Not,

        _MAX,
    }

}
