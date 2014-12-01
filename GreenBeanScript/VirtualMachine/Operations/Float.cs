using System;
using System.Collections.Generic;
using System.Text;

namespace GreenBeanScript
{
    namespace FloatOperators
    {
        internal class OperatorCallbacks : TypeOperators
        {
            protected Variable Add(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return Operand0.GetFloat() + Operand1.GetFloat();
            }

            protected Variable Sub(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return Operand0.GetFloat() - Operand1.GetFloat();
            }

            protected Variable Mul(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return Operand0.GetFloat() * Operand1.GetFloat();
            }

            protected Variable Div(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return Operand0.GetFloat() / Operand1.GetFloat();
            }

            protected Variable Rem(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return Operand0.GetFloat() % Operand1.GetFloat();
            }

            protected Variable Neg(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (-Operand0.GetFloat());
            }

            protected Variable Not(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                if (Operand0.GetFloat() == 0.0f)
                {
                    return Variable.One;
                }
                else
                {
                    return Variable.Zero;
                }
            }

            #region Comparisons
            protected Variable Eq(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.GetFloat() == Operand1.GetFloat() ? Variable.One : Variable.Zero);
            }

            protected Variable Neq(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.GetFloat() != Operand1.GetFloat() ? Variable.One : Variable.Zero);
            }

            protected Variable Lt(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.GetFloat() < Operand1.GetFloat() ? Variable.One : Variable.Zero);
            }

            protected Variable Lte(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.GetFloat() <= Operand1.GetFloat() ? Variable.One : Variable.Zero);
            }

            protected Variable Gt(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.GetFloat() > Operand1.GetFloat() ? Variable.One : Variable.Zero);
            }

            protected Variable Gte(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.GetFloat() >= Operand1.GetFloat() ? Variable.One : Variable.Zero);
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
