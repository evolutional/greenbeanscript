using System;
using System.Collections.Generic;
using System.Text;

namespace GreenBeanScript
{
    public class TableNode
    {
        public TableNode(Variable Key, Variable Item)
        {
            this.Key = Key; this.Item = Item;
        }
        public Variable Key;
        public Variable Item;
    }

    public class TableObject 
    {

        public Variable this[Variable Index]
        {
            get
            {
                Variable i = Index;
                return Get(ref i);
            }
            set
            {
                Variable i = Index;
                Set(ref i, ref value);
            }
        }

        public void Set(ref Variable Key, ref Variable Value)
        {
            if (Key.IsInt)
            {
                if (Value.IsNull)
                {
                    _IndexedItems.Remove(Key.GetIntegerNoCheck());
                }
                else
                {
                    _IndexedItems[Key.GetIntegerNoCheck()] = Value;
                }

                return;
            }

            if (Value.IsNull)
            {
                _HashedItems.Remove(Key);
            }
            else
            {
                _HashedItems[Key] = Value;
            }

        }

        public Variable Get(ref Variable Key)
        {
            if (Key.IsNull)
                return new Variable();

            Variable ret;
            if (Key.IsInt)
            {                
                if (_IndexedItems.TryGetValue(Key.GetIntegerNoCheck(),out ret))
                {
                    return ret;
                }
                return new Variable();
            }

            if (_HashedItems.TryGetValue(Key, out ret))
            {
                return ret;
            }
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
        public TableNode GetNext(int IteratorPos)
        {
            if (IteratorPos == 0)
                return GetFirst();

            int pos = -1;
#if ZERO
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
            get { return _IndexedItems.Count + _HashedItems.Count; }
        }

     //   protected Dictionary<Variable, Variable> _Items = new Dictionary<Variable, Variable>();
        protected Dictionary<int, Variable> _IndexedItems = new Dictionary<int,Variable>();
        protected Dictionary<Variable, Variable> _HashedItems = new Dictionary<Variable,Variable>();
    }
}
