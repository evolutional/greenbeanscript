using System;
using System.Collections.Generic;
using System.Text;

namespace GreenBeanScript
{
    namespace StringOperators
    {
        internal class OperatorCallbacks : TypeOperators
        {
            protected Variable Add(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.ToString() + Operand1.ToString());
            }

            protected Variable Eq(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.ToString() == Operand1.ToString() ? Variable.One : Variable.Zero);
            }

            protected Variable Neq(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                return (Operand0.ToString() != Operand1.ToString() ? Variable.One : Variable.Zero);
            }

            public void Initialise(Machine Machine, ScriptType Type)
            {
                Type.SetOperator(Operator.Add, this.Add);

                Type.SetOperator(Operator.Eq, this.Eq);
                Type.SetOperator(Operator.Neq, this.Neq);
                /*Type.SetOperator(Operator.Lt, this.Lt);
                Type.SetOperator(Operator.Lte, this.Lte);
                Type.SetOperator(Operator.Gt, this.Gt);
                Type.SetOperator(Operator.Gte, this.Gte);*/
            }

        }

    }


}
