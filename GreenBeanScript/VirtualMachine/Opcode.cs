namespace GreenBeanScript.VirtualMachine.ByteCode
{
    public enum Opcode
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

        OpInc,
        OpDec,

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
        SetThis
    }
}