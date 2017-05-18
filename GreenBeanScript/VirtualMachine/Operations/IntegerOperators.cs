namespace GreenBeanScript.VirtualMachine.Operations
{
    internal class IntegerOperators : ITypeOperators
    {
        public void Initialise(Machine machine, ScriptType type)
        {
            type.SetOperator(Operator.Add, Add);
            type.SetOperator(Operator.Sub, Sub);
            type.SetOperator(Operator.Mul, Mul);
            type.SetOperator(Operator.Div, Div);
            type.SetOperator(Operator.Rem, Rem);
            type.SetOperator(Operator.Neg, Neg);
            type.SetOperator(Operator.BitOr, BitOr);
            type.SetOperator(Operator.BitXor, BitXor);
            type.SetOperator(Operator.BitAnd, BitAnd);
            type.SetOperator(Operator.Eq, Eq);
            type.SetOperator(Operator.Neq, Neq);
            type.SetOperator(Operator.Lt, Lt);
            type.SetOperator(Operator.Lte, Lte);
            type.SetOperator(Operator.Gt, Gt);
            type.SetOperator(Operator.Gte, Gte);
        }

        protected Variable Add(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetInteger() + operand1.GetInteger();
        }

        protected Variable Sub(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetInteger() - operand1.GetInteger();
        }

        protected Variable Mul(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetInteger()*operand1.GetInteger();
        }

        protected Variable Div(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetInteger()/operand1.GetInteger();
        }

        protected Variable Rem(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetInteger()%operand1.GetInteger();
        }

        protected Variable Neg(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return -operand0.GetInteger();
        }


        protected Variable BitOr(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetInteger() | operand1.GetInteger();
        }

        protected Variable BitXor(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetInteger() ^ operand1.GetInteger();
        }

        protected Variable BitAnd(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetInteger() & operand1.GetInteger();
        }

        #region Comparisons

        protected Variable Eq(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetInteger() == operand1.GetInteger() ? Variable.One : Variable.Zero;
        }

        protected Variable Neq(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetInteger() != operand1.GetInteger() ? Variable.One : Variable.Zero;
        }

        protected Variable Lt(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetInteger() < operand1.GetInteger() ? Variable.One : Variable.Zero;
        }

        protected Variable Lte(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetInteger() <= operand1.GetInteger() ? Variable.One : Variable.Zero;
        }

        protected Variable Gt(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetInteger() > operand1.GetInteger() ? Variable.One : Variable.Zero;
        }

        protected Variable Gte(Thread scriptThread, Variable operand0, Variable operand1, Variable operand2)
        {
            return operand0.GetInteger() >= operand1.GetInteger() ? Variable.One : Variable.Zero;
        }

        #endregion
    }
}