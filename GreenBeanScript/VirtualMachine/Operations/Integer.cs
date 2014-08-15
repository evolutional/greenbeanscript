using System;
using System.Collections.Generic;
using System.Text;

namespace GreenBeanScript
{
    namespace IntegerOperators
    {
        internal class OpCallbacks : TypeOperators
        {
            protected void Add(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetInteger(Operand0.GetInteger() + Operand1.GetInteger());
            }

            protected void Sub(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetInteger(Operand0.GetInteger() - Operand1.GetInteger());
            }

            protected void Mul(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetInteger(Operand0.GetInteger() * Operand1.GetInteger());
            }

            protected void Div(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetInteger(Operand0.GetInteger() / Operand1.GetInteger());
            }

            protected void Rem(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetInteger(Operand0.GetInteger() % Operand1.GetInteger());
            }

            protected void Neg(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetInteger(-Operand0.GetInteger());
            }


            protected void BitOr(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetInteger(Operand0.GetInteger() | Operand1.GetInteger());
            }

            protected void BitXor(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetInteger(Operand0.GetInteger() ^ Operand1.GetInteger());
            }

            protected void BitAnd(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetInteger(Operand0.GetInteger() & Operand1.GetInteger());
            }


            #region Comparisons
            protected void Eq(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetInteger(Operand0.GetInteger() == Operand1.GetInteger() ? 1 : 0);
            }

            protected void Neq(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetInteger(Operand0.GetInteger() != Operand1.GetInteger() ? 1 : 0);
            }

            protected void Lt(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetInteger(Operand0.GetInteger() < Operand1.GetInteger() ? 1 : 0);
            }

            protected void Lte(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetInteger(Operand0.GetInteger() <= Operand1.GetInteger() ? 1 : 0);
            }

            protected void Gt(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetInteger(Operand0.GetInteger() > Operand1.GetInteger() ? 1 : 0);
            }

            protected void Gte(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetInteger(Operand0.GetInteger() >= Operand1.GetInteger() ? 1 : 0);
            }
#endregion
            public void Initialise(Machine Machine, ScriptType Type)
            {
                Type.SetOperator(Operator.Add, this.Add);
                Type.SetOperator(Operator.Sub, this.Sub);
                Type.SetOperator(Operator.Mul, this.Mul);
                Type.SetOperator(Operator.Div, this.Div);
                Type.SetOperator(Operator.Rem, this.Rem);
                Type.SetOperator(Operator.Neg, this.Neg);
                Type.SetOperator(Operator.BitOr, this.BitOr);
                Type.SetOperator(Operator.BitXor, this.BitXor);
                Type.SetOperator(Operator.BitAnd, this.BitAnd);
                Type.SetOperator(Operator.Eq, this.Eq);
                Type.SetOperator(Operator.Neq, this.Neq);
                Type.SetOperator(Operator.Lt, this.Lt);
                Type.SetOperator(Operator.Lte, this.Lte);
                Type.SetOperator(Operator.Gt, this.Gt);
                Type.SetOperator(Operator.Gte, this.Gte);
            }

        }

    }


}
