using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GreenBeanScript
{
    public enum VariableType
    {
        Null,
        Integer,
        Float,
        String,
        Table,
        Function,

        UserType,
    }

    public class Variable : IEquatable<Variable>
    {
        public static Variable Null = new Variable();
        public static Variable Zero = new Variable(0);
        public static Variable One = new Variable(1);

        #region Constructors
        public Variable()
        {
            _type = VariableType.Null;
            _float = 0;
            _refValue = null;
            _string = null;

            _int = 0;
        }

        public Variable(int Value)
        {
            _type = VariableType.Integer;
            _float = 0;
            _refValue = null;
            _string = null;

            _int = Value;
        }

        public Variable(float Value)
        {
            _type = VariableType.Float;
            _int = 0;
            _refValue = null;
            _string = null;

            _float = Value;
        }

        public Variable(string Value)
        {
            _type = VariableType.String;
            _int = 0;
            _float = 0;
            _refValue = null;

            _string = Value;
        }

        public Variable(FunctionObject Value)
        {
            _type = VariableType.Function;
            _int = 0;
            _float = 0;
            _string = null;

            _refValue = Value;
        }

        public Variable(TableObject Value)
        {
            _type = VariableType.Table;
            _int = 0;
            _float = 0;
            _string = null;

            _refValue = Value;
        }
        #endregion

        #region Get Methods (No type checking)
        public TableObject GetTableNoCheck()
        {
            return (TableObject)_refValue;
        }
        public FunctionObject GetFunctionNoCheck()
        {
            return (FunctionObject)_refValue;
        }
        public string GetStringNoCheck()
        {
            return _string;
        }
        public float GetFloatNoCheck()
        {
            return _float;
        }
        public int GetIntegerNoCheck()
        {
            return _int;
        }
        #endregion

        #region Get Methods
        public string GetString()
        {
            if (!IsString)
            {
                return null;
            }

            return _string;
        }

        public TableObject GetTable()
        {
            if (_type != VariableType.Table)
            {
                return null;
            }

            return (TableObject)_refValue;
        }

        public FunctionObject GetFunction()
        {
            if (_type != VariableType.Function)
            {
                return null;
            }

            return (FunctionObject)_refValue;
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

            return _int;
        }

        public float GetFloat()
        {
            if (IsNull)
            {
                return 0;
            }
            if (IsFloat)
            {
                return _float;
            }
            if (IsInt)
            {
                return Convert.ToSingle(_int);
            }

            throw new Exception("Invalid cast");
        }

        public object GetReference()
        {
            if (!IsReference)
            {
                return null;
            }
            return _refValue;
        }

        public bool IsNull
        {
            get { return _type == VariableType.Null; }
        }

        public bool IsReference
        {
            get { return (TypeCode > (int)VariableType.Float); }
        }

        public bool IsEqual(Variable otherVar)
        {
            if (otherVar.Type != _type)
            {
                return false;
            }

            if (IsInt)
            {
                return GetIntegerNoCheck() == otherVar.GetIntegerNoCheck();
            }

            if (IsFloat)
            {
                return GetFloatNoCheck() == otherVar.GetFloatNoCheck();
            }

            if (IsString)
            {
                return GetStringNoCheck().Equals(otherVar.GetStringNoCheck());
            }

            return otherVar.GetReference().Equals(_refValue);
        }

        public bool Equals(Variable obj)
        {
            return IsEqual(obj);
        }

        public int GetHash
        {
            get
            {
                return GetHashCode();
            }
        }

        #endregion
        

        #region Overrides
        public override int GetHashCode()
        {
            if (IsInt)
            {
                return _int.GetHashCode();
            }

            if (IsFloat)
            {
                return _float.GetHashCode();
            }

            if (IsString)
            {
                return _string.GetHashCode();
            }

            return _refValue.GetHashCode();
        }
        public override string ToString()
        {
            switch (_type)
            {
                case VariableType.Null:
                    return "null";
                case VariableType.Integer:
                    return _int.ToString();
                case VariableType.Float:
                    return _float.ToString();
                case VariableType.String:
                    return _string;
                default:
                    return _refValue.ToString();
            }
        }
        #endregion

        public int TypeCode
        {
            get { return (int)_type; }
        }

        public VariableType Type
        {
            get { return _type; }
        }

        public bool IsString { get { return _type == VariableType.String; } }
        public bool IsInt { get { return _type == VariableType.Integer; } }
        public bool IsFloat { get { return _type == VariableType.Float; } }
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

        public static implicit operator Variable(int value)
        {
            return new Variable(value);
        }

        public static implicit operator Variable(float value)
        {
            return new Variable(value);
        }

        public static implicit operator Variable(string value)
        {
            return new Variable(value);
        }

        public static implicit operator Variable(FunctionObject value)
        {
            return new Variable(value);
        }

        public static implicit operator Variable(TableObject value)
        {
            return new Variable(value);
        }

        
        private readonly int _int;
        private readonly float _float;
        private readonly Object _refValue;
        private readonly string _string;
        private readonly VariableType _type;
    }
}
