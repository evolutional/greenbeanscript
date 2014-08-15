using System;
using System.Collections.Generic;
using System.Text;

namespace GreenBeanScript
{
    namespace FloatOperators
    {
        internal class OperatorCallbacks : TypeOperators
        {
            protected void Add(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetFloat(Operand0.GetFloat() + Operand1.GetFloat());
            }

            protected void Sub(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetFloat(Operand0.GetFloat() - Operand1.GetFloat());
            }

            protected void Mul(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetFloat(Operand0.GetFloat() * Operand1.GetFloat());
            }

            protected void Div(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetFloat(Operand0.GetFloat() / Operand1.GetFloat());
            }

            protected void Rem(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetFloat(Operand0.GetFloat() % Operand1.GetFloat());
            }

            protected void Neg(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetFloat(-Operand0.GetFloat());
            }

            protected void Not(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                if (Operand0.GetFloat() == 0.0f)
                {
                    Operand0.SetInteger(1);
                }
                else
                {
                    Operand0.SetInteger(0);
                }
            }

            #region Comparisons
            protected void Eq(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetFloat(Operand0.GetFloat() == Operand1.GetFloat() ? 1 : 0);
            }

            protected void Neq(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetFloat(Operand0.GetFloat() != Operand1.GetFloat() ? 1 : 0);
            }

            protected void Lt(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetFloat(Operand0.GetFloat() < Operand1.GetFloat() ? 1 : 0);
            }

            protected void Lte(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetFloat(Operand0.GetFloat() <= Operand1.GetFloat() ? 1 : 0);
            }

            protected void Gt(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetFloat(Operand0.GetFloat() > Operand1.GetFloat() ? 1 : 0);
            }

            protected void Gte(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetFloat(Operand0.GetFloat() >= Operand1.GetFloat() ? 1 : 0);
            }
            #endregion

            public void Initialise(Machine Machine, ScriptType Type)
            {
                Type.SetOperator(Operator.Add, this.Add);
                Type.SetOperator(Operator.Sub, this.Sub);
                Type.SetOperator(Operator.Mul, this.Mul);
                Type.SetOperator(Operator.Div, this.Div);
                Type.SetOperator(Operator.Rem, this.Rem);
                Type.SetOperator(Operator.Neg, this.Rem);
                Type.SetOperator(Operator.Not, this.Rem);

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
