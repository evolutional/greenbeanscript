using GreenBeanScript.VirtualMachine.Operations;

namespace GreenBeanScript.VirtualMachine
{
    public class ScriptType
    {
        protected TypeIteratorCallback IteratorFunction;

        protected OperatorCallback[] Operators;
        protected int _TypeCode;
        protected string _TypeName;

        public ScriptType(string typeName, int typeCode)
        {
            _TypeName = typeName;
            _TypeCode = typeCode;
            Operators = new OperatorCallback[(int) Operator.Max];
        }

        #region Properties

        public int TypeCode
        {
            get { return _TypeCode; }
        }

        public string TypeName
        {
            get { return _TypeName; }
        }

        #endregion

        #region Methods

        public void SetIterator(TypeIteratorCallback itr)
        {
            IteratorFunction = itr;
        }

        public TypeIteratorCallback GetIterator()
        {
            return IteratorFunction;
        }

        public OperatorCallback GetOperator(Operator op)
        {
            return Operators[(int) op];
        }

        public void SetOperator(Operator op, OperatorCallback cb)
        {
            Operators[(int) op] = cb;
        }

        #endregion
    }
}