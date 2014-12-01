using System;
using System.Collections.Generic;
using System.Text;

namespace GreenBeanScript
{
    namespace TableOperators
    {
        internal class OperatorCallbacks : TypeOperators
        {
            public void Initialise(Machine Machine, ScriptType Type)
            {
                Type.SetOperator(Operator.GetInd, this.GetInd);
                Type.SetOperator(Operator.SetInd, this.SetInd);
                Type.SetOperator(Operator.GetDot, this.GetDot);
                Type.SetOperator(Operator.SetDot, this.SetDot);
                Type.SetIterator(Iterator);
            }
            protected int Iterator(Thread ScriptThread, object Object, int IteratorPosition, Variable Key, Variable Item)
            {
                if (IteratorPosition == -2)
                {
                    IteratorPosition = 0;
                }
                if (IteratorPosition >= ((TableObject)Object).Count)
                {
                    IteratorPosition = -1;
                    return IteratorPosition;
                }

                var node = ((TableObject)Object).GetNext(IteratorPosition++);
                Key = node.Key;
                Item = node.Item;
                return IteratorPosition;
            }

            protected Variable SetDot(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                //(Operand0.GetTable())[Operand2.GetString()] = Operand1;
               // Operand0.GetTableNoCheck().Set(ref Operand2, ref Operand1);
                return Variable.Null;
            }
            protected Variable GetDot(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                //Operand0 = (Operand0.GetTable())[Operand1.GetString()];
                //Operand0.GetTableNoCheck().Get(ref Operand1, ref Operand0);
                return Variable.Null;
            }
            protected Variable SetInd(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                //(Operand0.GetTable())[Operand1.GetInteger()] = Operand2;
                Operand0.GetTableNoCheck()[Operand1] = Operand2;
                //Operand0.GetTableNoCheck().Set(ref Operand2, ref Operand1);
                return Operand0;
            }
            protected Variable GetInd(Thread ScriptThread, Variable Operand0, Variable Operand1, Variable Operand2)
            {
                //Operand0 = (Operand0.GetTable())[Operand1.GetInteger()];
                return Operand0.GetTableNoCheck()[Operand1];
                //Operand0 = Operand0.GetTable().Get(ref Operand1);
            }
        }


    }

}
