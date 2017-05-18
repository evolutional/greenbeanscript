namespace GreenBeanScript.VirtualMachine.Operations
{
    internal class FloatOperators : ITypeOperators
    {
        public void Initialise(Machine machine, ScriptType type)
        {
            type.SetOperator(Operator.Add, Add);
            type.SetOperator(Operator.Sub, Sub);
            type.SetOperator(Operator.Mul, Mul);
            type.SetOperator(Operator.Div, Div);
            type.SetOperator(Operator.Rem, Rem);
            type.SetOperator(Operator.Neg, Rem);
            type.SetOperator(Operator.Not, Rem);

            type.SetOperator(Operator.Eq, Eq);
            type.SetOperator(Operator.Neq, Neq);
            type.SetOperator(Operator.Lt, Lt);
            type.SetOperator(Operator.Lte, Lte);
            type.SetOperator(Operator.Gt, Gt);
            type.SetOperator(Operator.Gte, Gte);
        }

        protected Variable Add(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetFloat() + operand1.GetFloat();
        }

        protected Variable Sub(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetFloat() - operand1.GetFloat();
        }

        protected Variable Mul(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetFloat()*operand1.GetFloat();
        }

        protected Variable Div(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetFloat()/operand1.GetFloat();
        }

        protected Variable Rem(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetFloat()%operand1.GetFloat();
        }

        protected Variable Neg(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return -operand0.GetFloat();
        }

        protected Variable Not(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            if (operand0.GetFloat() == 0.0f)
                return Variable.One;
            return Variable.Zero;
        }

        #region Comparisons

        protected Variable Eq(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetFloat() == operand1.GetFloat() ? Variable.One : Variable.Zero;
        }

        protected Variable Neq(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetFloat() != operand1.GetFloat() ? Variable.One : Variable.Zero;
        }

        protected Variable Lt(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetFloat() < operand1.GetFloat() ? Variable.One : Variable.Zero;
        }

        protected Variable Lte(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetFloat() <= operand1.GetFloat() ? Variable.One : Variable.Zero;
        }

        protected Variable Gt(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetFloat() > operand1.GetFloat() ? Variable.One : Variable.Zero;
        }

        protected Variable Gte(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetFloat() >= operand1.GetFloat() ? Variable.One : Variable.Zero;
        }

        #endregion
    }
}