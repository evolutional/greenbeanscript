using System;
using System.Collections.Generic;
using System.Text;

namespace GreenBeanScript
{
    namespace StringOperators
    {
        internal class OperatorCallbacks : TypeOperators
        {
            protected void Add(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetString(Operand0.ToString() + Operand1.ToString());
            }

            protected void Eq(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetInteger(Operand0.ToString() == Operand1.ToString() ? 1 : 0);
            }

            protected void Neq(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                Operand0.SetInteger(Operand0.ToString() != Operand1.ToString() ? 1 : 0);
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
