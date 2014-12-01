using System;
using System.Collections.Generic;
using System.Text;

namespace GreenBeanScript
{
    public delegate Variable OperatorCallback(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2);
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
