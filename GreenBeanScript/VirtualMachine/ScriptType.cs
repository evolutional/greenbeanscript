using System;
using System.Collections.ObjectModel;
using System.Text;

namespace GreenBeanScript
{
    public delegate void TypeIteratorCallback(Thread ScriptThread, object Object, ref int IteratorPosition, ref Variable Key, ref Variable Item );

    public class ScriptTypeCollection : KeyedCollection<int, ScriptType>
    {
        protected override int GetKeyForItem(ScriptType item)
        {
            return item.TypeCode;
        }
    }

    public class ScriptType
    {
        public ScriptType(string TypeName, int TypeCode)
        {
            _TypeName = TypeName;
            _TypeCode = TypeCode;
            _Operators = new OperatorCallback[(int)Operator._MAX];
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
        public void SetIterator(TypeIteratorCallback Itr)
        {
            _IteratorFunction = Itr;
        }
        public TypeIteratorCallback GetIterator()
        {
            return _IteratorFunction;
        }
        public OperatorCallback GetOperator(Operator op)
        {
            return _Operators[(int)op];
        }
        public void SetOperator(Operator op, OperatorCallback cb)
        {
            _Operators[(int)op] = cb;
        }
        #endregion

        protected OperatorCallback [] _Operators;
        protected TypeIteratorCallback _IteratorFunction;
        protected string _TypeName;
        protected int _TypeCode;
    }
}
