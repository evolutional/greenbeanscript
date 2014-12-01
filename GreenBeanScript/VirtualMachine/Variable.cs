using System;
using System.Collections.Generic;
using System.Text;

namespace GreenBeanScript
{
    public enum VariableType : int
    {
        Null,
        Integer,
        Float,
        String,
        Table,
        Function,

        UserType,
    }

    public struct Variable : IEquatable<Variable>
    {
        #region Constructors
        public Variable(int Value)
        {
            _Type = VariableType.Integer;
            _RefValue = Value;
        }

        public Variable(float Value)
        {
            _Type = VariableType.Float;
            _RefValue = Value;
        }

        public Variable(string Value)
        {
            _Type = VariableType.String;
            _RefValue = Value;
        }

        public Variable(FunctionObject Value)
        {
            _Type = VariableType.Function;
            _RefValue = Value;
        }

        public Variable(TableObject Value)
        {
            _Type = VariableType.Table;
            _RefValue = Value;
        }

        public Variable(VariableType Type, Object Value)
        {
            _Type = Type;
            _RefValue = Value;
         }
        #endregion

        #region Get Methods (No type checking)
        public TableObject GetTableNoCheck()
        {
            return (TableObject)_RefValue;
        }
        public FunctionObject GetFunctionNoCheck()
        {
            return (FunctionObject)_RefValue;
        }
        public string GetStringNoCheck()
        {
            return (string)_RefValue;
        }
        public float GetFloatNoCheck()
        {
            return Convert.ToSingle(_RefValue);
        }
        public int GetIntegerNoCheck()
        {
            return Convert.ToInt32(_RefValue);
        }
        #endregion

        #region Get Methods
        public string GetString()
        {
            if (_Type != VariableType.String)
            {
                return null;
            }

            return (string)_RefValue;
        }

        public TableObject GetTable()
        {
            if (_Type != VariableType.Table)
            {
                return null;
            }

            return (TableObject)_RefValue;
        }

        public FunctionObject GetFunction()
        {
            if (_Type != VariableType.Function)
            {
                return null;
            }

            return (FunctionObject)_RefValue;
        }

        public int GetInteger()
        {
            if (IsNull)
            {
                return 0;
            }

            if (!IsInt)
            {
                throw new Exception("Invalid cast");
            }

            return (int)_RefValue;
        }

        public float GetFloat()
        {
            if (IsNull)
            {
                return 0;
            }
            if (IsFloat)
            {
                return (float)_RefValue;
            }
            if (IsInt)
            {
                return Convert.ToSingle(_RefValue);
            }

            throw new Exception("Invalid cast");
        }

        public object GetReference()
        {
            if (!IsReference)
            {
                return null;
            }
            return _RefValue;
        }

        public bool IsNull
        {
            get { return _Type == VariableType.Null; }
        }

        public bool IsReference
        {
            get { return (TypeCode > (int)VariableType.Float); }
        }

        public bool IsEqual(ref Variable otherVar)
        {
            if (otherVar.Type != _Type)
                return false;


            if (otherVar.Type == VariableType.Integer)
            {
                return GetIntegerNoCheck() == otherVar.GetIntegerNoCheck();
            }

            if (otherVar.Type == VariableType.Float)
            {
                return GetFloatNoCheck() == otherVar.GetFloatNoCheck();
            }

            return otherVar.GetReference().Equals(_RefValue);
        }

        public bool Equals(Variable obj)
        {
            return IsEqual(ref obj);
        }

        public int GetHash
        {
            get
            {
                return _RefValue.GetHashCode();
            }

        }

        #endregion

        #region Set Methods
        public void Nullify()
        {
            _Type = VariableType.Null;
        }

        public void SetInteger(int Value)
        {
            _Type = VariableType.Integer;
            _RefValue = Value;
        }
        public void SetFloat(float Value)
        {
            _Type = VariableType.Float;
            _RefValue = Value;
        }
        public void SetString(string Value)
        {
            _Type = VariableType.String;
            _RefValue = Value;
        }
        public void SetFunction(FunctionObject Value)
        {
            _Type = VariableType.Function;
            _RefValue = Value;
        }
        public void SetTable(TableObject Value)
        {
            _Type = VariableType.Table;
            _RefValue = Value;
        }
        #endregion

        #region Overrides
        public override int GetHashCode()
        {
            return _RefValue.GetHashCode();
        }
        public override string ToString()
        {
            switch (_Type)
            {
                case VariableType.Null:
                    return "null";
                case VariableType.Integer:
                    return ((int)_RefValue).ToString();
                case VariableType.Float:
                    return ((float)_RefValue).ToString();
                case VariableType.String:
                    return ((string)_RefValue);
                default:
                    return _RefValue.ToString();
            }
        }
        #endregion

        public int TypeCode
        {
            get { return (int)_Type; }
        }

        public VariableType Type
        {
            get { return _Type; }
        }

        public bool IsInt { get { return _Type == VariableType.Integer; } }
        public bool IsFloat { get { return _Type == VariableType.Float; } }
        public bool IsNumber { get { return IsInt || IsFloat; }}

        public bool IsZero
        {
            get
            {
                if (IsNull)
                {
                    return true;
                }
                else if (IsInt)
                {
                    return GetIntegerNoCheck() == 0;
                }
                else if (IsFloat)
                {
                    return GetFloatNoCheck() == 0;
                }
                return false;
            }
        }

        internal VariableType _Type;
        internal Object _RefValue;
    }
}
