namespace GreenBeanScript.VirtualMachine.Operations
{
    internal class StringOperators : ITypeOperators
    {
        public void Initialise(Machine machine, ScriptType type)
        {
            type.SetOperator(Operator.Add, Add);

            type.SetOperator(Operator.Eq, Eq);
            type.SetOperator(Operator.Neq, Neq);
            /*Type.SetOperator(Operator.Lt, this.Lt);
            Type.SetOperator(Operator.Lte, this.Lte);
            Type.SetOperator(Operator.Gt, this.Gt);
            Type.SetOperator(Operator.Gte, this.Gte);*/
        }

        protected Variable Add(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0 + operand1.ToString();
        }

        protected Variable Eq(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.ToString() == operand1.ToString() ? Variable.One : Variable.Zero;
        }

        protected Variable Neq(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.ToString() != operand1.ToString() ? Variable.One : Variable.Zero;
        }
    }
}