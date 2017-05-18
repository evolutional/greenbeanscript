using System.Collections.Generic;

namespace GreenBeanScript.VirtualMachine
{
    public class TableObject
    {
        public Variable this[Variable index]
        {
            get
            {
                var i = index;
                return Get(ref i);
            }
            set
            {
                var i = index;
                Set(ref i, ref value);
            }
        }

        public void Set(ref Variable key, ref Variable value)
        {
            if (key.IsInt)
            {
                if (value.IsNull)
                    IndexedItems.Remove(key.GetIntegerNoCheck());
                else
                    IndexedItems[key.GetIntegerNoCheck()] = value;

                return;
            }

            if (value.IsNull)
                HashedItems.Remove(key);
            else
                HashedItems[key] = value;
        }

        public Variable Get(ref Variable key)
        {
            if (key.IsNull)
                return new Variable();

            Variable ret;
            if (key.IsInt)
            {
                if (IndexedItems.TryGetValue(key.GetIntegerNoCheck(), out ret))
                    return ret;
                return new Variable();
            }

            if (HashedItems.TryGetValue(key, out ret))
                return ret;
            return new Variable();
        }

#if ZERO
        public Variable this[Variable Index]
        {
            get
            {
                if (_Items.ContainsKey(Index))
                {
                    return _Items[Index];
                }
                else
                {
                    return new Variable();
                }
            }
            set
            {
                _Items[Index] = value;
            }
        }
#endif

        public TableNode GetFirst()
        {
#if ZERO
            if (_IndexedItems.Count > 0)
            {
                Dictionary<int, Variable>.Enumerator e = _IndexedItems.GetEnumerator();
                e.MoveNext();
                return new TableNode(new Variable(e.Current.Key), e.Current.Value);
            }

            if (_NamedItems.Count > 0)
            {
                Dictionary<string, Variable>.Enumerator e = _NamedItems.GetEnumerator();
                e.MoveNext();
                return new TableNode(new Variable(e.Current.Key), e.Current.Value);
            }
#endif
            return null;
        }

        public TableNode GetNext(int iteratorPos)
        {
            if (iteratorPos == 0)
                return GetFirst();

#if ZERO
            var pos = -1;
            if (IteratorPos < _IndexedItems.Count)
            {
                Dictionary<int, Variable>.Enumerator e = _IndexedItems.GetEnumerator();         
                while (pos != IteratorPos)
                {
                    ++pos; e.MoveNext();
                }
                return new TableNode(new Variable(e.Current.Key), e.Current.Value);
            }
            else 
            {
                pos = _IndexedItems.Count;
                Dictionary<string, Variable>.Enumerator e = _NamedItems.GetEnumerator();
                while (pos != IteratorPos)
                {
                    ++pos; e.MoveNext();
                }
                return new TableNode(new Variable(e.Current.Key), e.Current.Value);
            }
#endif
            return null;
        }

        public int Count
        {
            get { return IndexedItems.Count + HashedItems.Count; }
        }

        //   protected Dictionary<Variable, Variable> _Items = new Dictionary<Variable, Variable>();
        protected Dictionary<int, Variable> IndexedItems = new Dictionary<int, Variable>();
        protected Dictionary<Variable, Variable> HashedItems = new Dictionary<Variable, Variable>();
    }
}