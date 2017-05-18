namespace GreenBeanScript.VirtualMachine.Operations
{
    internal class TableOperators : ITypeOperators
    {
        public void Initialise(Machine machine, ScriptType type)
        {
            type.SetOperator(Operator.GetInd, GetInd);
            type.SetOperator(Operator.SetInd, SetInd);
            type.SetOperator(Operator.GetDot, GetDot);
            type.SetOperator(Operator.SetDot, SetDot);
            type.SetIterator(Iterator);
        }

        protected int Iterator(Thread scriptThread, object Object, int iteratorPosition, Variable key, Variable item)
        {
            if (iteratorPosition == -2)
                iteratorPosition = 0;
            if (iteratorPosition >= ((TableObject) Object).Count)
            {
                iteratorPosition = -1;
                return iteratorPosition;
            }

            var node = ((TableObject) Object).GetNext(iteratorPosition++);
            key = node.Key;
            item = node.Item;
            return iteratorPosition;
        }

        protected Variable SetDot(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            //(Operand0.GetTable())[Operand2.GetString()] = Operand1;
            // Operand0.GetTableNoCheck().Set(ref Operand2, ref Operand1);
            return Variable.Null;
        }

        protected Variable GetDot(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            //Operand0 = (Operand0.GetTable())[Operand1.GetString()];
            //Operand0.GetTableNoCheck().Get(ref Operand1, ref Operand0);
            return Variable.Null;
        }

        protected Variable SetInd(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            //(Operand0.GetTable())[Operand1.GetInteger()] = Operand2;
            operand0.GetTableNoCheck()[operand1] = operand2;
            //Operand0.GetTableNoCheck().Set(ref Operand2, ref Operand1);
            return operand0;
        }

        protected Variable GetInd(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            //Operand0 = (Operand0.GetTable())[Operand1.GetInteger()];
            return operand0.GetTableNoCheck()[operand1];
            //Operand0 = Operand0.GetTable().Get(ref Operand1);
        }
    }
}