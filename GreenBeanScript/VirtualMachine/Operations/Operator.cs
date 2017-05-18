namespace GreenBeanScript.VirtualMachine.Operations
{
    public enum Operator
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

        Max
    }
}