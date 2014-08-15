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
            protected void Iterator(Thread ScriptThread, object Object, ref int IteratorPosition, ref Variable Key, ref Variable Item)
            {
                if (IteratorPosition == -2)
                {
                    IteratorPosition = 0;
                }
                if (IteratorPosition >= ((TableObject)Object).Count)
                {
                    IteratorPosition = -1;
                    return;
                }

                TableNode node = ((TableObject)Object).GetNext(IteratorPosition++);
                Key = node.Key;
                Item = node.Item;
            }

            protected void SetDot(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                //(Operand0.GetTable())[Operand2.GetString()] = Operand1;
               // Operand0.GetTableNoCheck().Set(ref Operand2, ref Operand1);
            }
            protected void GetDot(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                //Operand0 = (Operand0.GetTable())[Operand1.GetString()];
                //Operand0.GetTableNoCheck().Get(ref Operand1, ref Operand0);
            }
            protected void SetInd(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                //(Operand0.GetTable())[Operand1.GetInteger()] = Operand2;
                Operand0.GetTableNoCheck()[Operand1] = Operand2;
                //Operand0.GetTableNoCheck().Set(ref Operand2, ref Operand1);
            }
            protected void GetInd(Thread ScriptThread, ref Variable Operand0, ref Variable Operand1, ref Variable Operand2)
            {
                //Operand0 = (Operand0.GetTable())[Operand1.GetInteger()];
                Operand0 = Operand0.GetTableNoCheck()[Operand1];
                //Operand0 = Operand0.GetTable().Get(ref Operand1);
            }
        }


    }

}
