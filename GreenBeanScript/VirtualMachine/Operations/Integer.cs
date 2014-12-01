using System;
using System.Collections.Generic;
using System.Text;

namespace GreenBeanScript
{
    namespace IntegerOperators
    {
        internal class OpCallbacks : TypeOperators
        {
            protected Variable Add(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.GetInteger() + Operand1.GetInteger());
            }

            protected Variable Sub(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.GetInteger() - Operand1.GetInteger());
            }

            protected Variable Mul(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.GetInteger() * Operand1.GetInteger());
            }

            protected Variable Div(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.GetInteger() / Operand1.GetInteger());
            }

            protected Variable Rem(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.GetInteger() % Operand1.GetInteger());
            }

            protected Variable Neg(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (-Operand0.GetInteger());
            }


            protected Variable BitOr(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.GetInteger() | Operand1.GetInteger());
            }

            protected Variable BitXor(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.GetInteger() ^ Operand1.GetInteger());
            }

            protected Variable BitAnd(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.GetInteger() & Operand1.GetInteger());
            }


            #region Comparisons
            protected Variable Eq(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.GetInteger() == Operand1.GetInteger() ? Variable.One : Variable.Zero);
            }

            protected Variable Neq(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.GetInteger() != Operand1.GetInteger() ? Variable.One : Variable.Zero);
            }

            protected Variable Lt(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.GetInteger() < Operand1.GetInteger() ? Variable.One : Variable.Zero);
            }

            protected Variable Lte(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.GetInteger() <= Operand1.GetInteger() ? Variable.One : Variable.Zero);
            }

            protected Variable Gt(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.GetInteger() > Operand1.GetInteger() ? Variable.One : Variable.Zero);
            }

            protected Variable Gte(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.GetInteger() >= Operand1.GetInteger() ? Variable.One : Variable.Zero);
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
