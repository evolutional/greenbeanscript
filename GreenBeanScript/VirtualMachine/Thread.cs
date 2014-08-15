using System;
using System.Collections.Generic;
using System.Text;

namespace GreenBeanScript
{
    internal class StackFrame
    {
        public int ReturnBase;
        public int InstructionPtr;
        public StackFrame PreviousFrame;
    }

    public enum ThreadState
    {
        Running,
        Sleeping,
        Blocked,
        Killed,
        Exception,

        Sys_Pending,
        Sys_Yield,
        Sys_Exception,
    }

    public class Thread
    {

        #region Properties
        /// <summary>
        /// Returns the number of parameters in a thread callback
        /// </summary>
        public int ParameterCount
        {
            get { return _NumParameters; }
        }
        public int Id
        {
            get { return _ThreadId; }
        }
  
        public Machine Machine
        {
            get { return _Machine; }
        }

        public ThreadState State
        {
            get { return _State; }
        }

        #endregion

        public Thread(Machine Machine)
        {
            _Machine = Machine;
        }

        public Thread(Machine Machine, FunctionObject Function)
        {
            _Machine = Machine;
            _Function = Function;
        }

        public void LogException(string Message)
        {
            _Machine.Log.LogEntry(Message);
            // TODO: Set state to exception
        }

        #region Public Push Methods
        public void Push(Variable Var)
        {
            _Stack2[_Top++] = Var;
        }
        
        public void PushNull()
        {
            _Stack2[_Top++].Nullify();
        }

        public void PushInteger(int Value)
        {
            _Stack2[_Top++].SetInteger(Value);
        }

        public void PushFloat(float Value)
        {
            _Stack2[_Top++].SetFloat(Value);
        }

        public void PushString(string Value)
        {
            _Stack2[_Top++].SetString(Value);
        }

        public void PushFunction(FunctionObject Function)
        {
            _Stack2[_Top++].SetFunction(Function);
        }

        public void PushTable(TableObject Table)
        {
            _Stack2[_Top++].SetTable(Table);
        }
        #endregion

        #region Public Parameter Methods
        public Variable Param(int Index)
        {
            return _Stack2[_Base + Index];
        }

        public Variable This
        {
            get { return _Stack2[_Base - 2]; }
        }

        public VariableType ThisType
        {
            get { return _Stack2[_Base - 2].Type; }
        }

        internal Variable Top
        {
            get { return _Stack2[_Top]; }
        }

        public FunctionObject Function
        {
            get { return _Stack2[_Base - 1].GetFunction(); }
        }

        #endregion

        #region Internal Mathods
        internal void SetId(int Id)
        {
            _ThreadId = Id;
        }

        internal void SetState(ThreadState State)
        {
            _State = State;
        }

        internal ThreadState PopStackFrame(int InstructionPtr)
        {
            if (_Frame == null)
            {
                LogException("Stack undeflow");
                return ThreadState.Exception;
            }

            StackFrame frame = _Frame.PreviousFrame;
            if (frame == null)
            {
                return ThreadState.Killed;
            }

            _InstructionPtr = _Frame.InstructionPtr;
            _Stack2[_Base - 2] = _Stack2[_Top - 1];
            _Top = _Base - 1;
            _Base = _Frame.ReturnBase;
            _Frame = frame;

            _Function = this.Function;
            _Function.GetInstructions(ref _InstructionList);
         
            return ThreadState.Running;
        }

        internal int SetBlocks(ref Variable[] Blocks)
        {
            if (Blocks.Length == 0)
                return 0;

            if (_Blocks == null)
            {
                _Blocks = new List<Variable>();
            }

            foreach (Variable Block in Blocks)
            {
                if (!_Blocks.Contains(Block))
                {
                    _Blocks.Add(Block);
                }
            }


            return 0;
        }

        internal ThreadState PushStackFrame(int ParameterCount, int InstructionPtr)
        {
            int Base = _Top - ParameterCount;

            if (Base == 2)
            {
                // TODO: New thread callback
                _Base = Base;
            }

            Variable fnVar = _Stack2[Base - 1];
            if (fnVar.Type != VariableType.Function)
            {
                _Machine.Log.LogEntry("Attempted to call non-function type");
                return ThreadState.Exception;
            }

            FunctionObject Func = fnVar.GetFunction();

            if (Func.Native != null)
            {
                int LastBase = _Base;
                int LastTop = _Top;
                _Base = Base;
                _NumParameters = ParameterCount;

                FunctionResult res = Func.Native(this);

                if (LastTop == _Top)
                {
                    _Stack2[_Base - 2].Nullify();    // Push a null
                }
                else
                {
                    _Stack2[_Base - 2] = _Stack2[_Top - 1];
                }

                // return stack
                _Top = _Base - 1;
                _Base = LastBase;

                // Sort out the call result
                if (res != FunctionResult.Ok)
                {
                    if (res == FunctionResult.Sys_Yield)
                    {
                        // Todo: Remove signals
                        // Todo: Sort out return addr
                        return ThreadState.Sys_Yield;
                    }
                    else if (res == FunctionResult.Sys_Block)
                    {
                        // Todo: Sort out return addr
                        _Machine.SwitchThreadState(this, ThreadState.Blocked);
                        return ThreadState.Blocked;
                    }
                    else if (res == FunctionResult.Sys_Sleep)
                    {
                        // Todo: Sort out return addr
                        _Machine.SwitchThreadState(this, ThreadState.Sleeping);
                        return ThreadState.Sleeping;
                    }
                    else if (res == FunctionResult.Sys_Kill)
                    {
                        return ThreadState.Killed;
                    }
                    return ThreadState.Exception;
                }

                if (_Frame == null)
                {
                    return ThreadState.Killed;
                }

                return ThreadState.Running;
            }


            // Is a scripted function

            // Null out params
            /*
            for (int p = 0; p < Func.NumParams; ++p)
            {
                _Stack2[_Base + p] = new Variable();
            }*/

            StackFrame frame = new StackFrame();
            frame.ReturnBase = _Base;
            frame.InstructionPtr = _InstructionPtr;
            frame.PreviousFrame = _Frame;
            _Frame = frame;

            // Point to fn of new thread
            _InstructionPtr = 0;
            _Base = Base;
            _Top = Base + Func.NumParamsLocals;
            _Function = Func;
            _Function.GetInstructions(ref _InstructionList);
            

            return ThreadState.Running;
        }
        #endregion

        ByteCode.Instruction[] _InstructionList;

        public ThreadState Execute()
        {
            if (State != ThreadState.Running)
            {
                return State;
            }

            for (; ; )
            {
                if (_InstructionPtr >= _InstructionList.Length)
                    break;

                ByteCode.Instruction Inst = _InstructionList[_InstructionPtr++];
                switch (Inst.OpCode)
                {
                    case ByteCode.Operator.Pop:
                        {
                            --_Top;
                            break;
                        }

                    case ByteCode.Operator.Pop2:
                        {
                            _Top -= 2;
                            break;
                        }

                    case ByteCode.Operator.Dup:
                        {
                            _Stack2[_Top] = _Stack2[_Top - 1];
                            ++_Top;
                            break;
                        }

                    case ByteCode.Operator.Call:
                        {
                            // pop arg count from stack
                            ThreadState res = PushStackFrame(Inst[0].GetInteger(), _InstructionPtr);

                            if (res == ThreadState.Running)
                            {
                                break;
                            }
                            if (res == ThreadState.Sys_Yield) return ThreadState.Running;
                            if (res == ThreadState.Exception)
                            {
                                _Machine.SwitchThreadState(this, ThreadState.Killed);
                                return ThreadState.Exception;
                            }
                            if (res == ThreadState.Killed)
                            {
                                _Machine.SwitchThreadState(this, ThreadState.Killed);
                            }
                            // if exception then die

                            break;
                        }
                    #region Push Operations
                    case ByteCode.Operator.PushNull:
                        {
                            _Stack2[_Top++].Nullify();
                            break;
                        }
                    case ByteCode.Operator.PushThis:
                        {
                            _Stack2[_Top++] = This;
                            break;
                        }
                    case ByteCode.Operator.PushInt0:
                    case ByteCode.Operator.PushInt1:
                    case ByteCode.Operator.PushInt:
                    case ByteCode.Operator.PushFp:
                    case ByteCode.Operator.PushStr:
                    case ByteCode.Operator.PushFn:
                        {
                            _Stack2[_Top++] = Inst[0];
                            break;
                        }
                    case ByteCode.Operator.PushTbl:
                        {
                            _Stack2[_Top++].SetTable(_Machine.CreateTable());
                            break;
                        }
                    #endregion
                    case ByteCode.Operator.Ret:          // Ret pushes a null
                        {
                            _Stack2[_Top++].Nullify();
                            ThreadState res = PopStackFrame(_InstructionPtr);
                            if (res == ThreadState.Running)
                            {
                                break;
                            }
                            else if (res == ThreadState.Killed)
                            {
                                // Todo: Sort out return addr
                                _Machine.SwitchThreadState(this, ThreadState.Killed);
                                return res;
                            }
                            else if (res == ThreadState.Exception)
                            {
                                _Machine.SwitchThreadState(this, ThreadState.Exception);
                                return ThreadState.Exception;
                            }
                            break;
                        }
                    case ByteCode.Operator.Retv:        
                        {
                            ThreadState res = PopStackFrame(_InstructionPtr);
                            if (res == ThreadState.Killed)
                            {
                                return res;
                            }
                            break;
                        }
//                    case ByteCode.Operator.GetDot:
                    case ByteCode.Operator.SetGlobal:
                        {
                            Variable v1 = Inst[0];
                            // TODO: Check for string
                            if (v1.Type != VariableType.String)
                            {
                                throw new Exception("String required");
                            }
                            
                            Variable v2 = _Stack2[--_Top];
                            string GlobalName = v1.GetString();
                            _Machine.Globals[v1] = v2;
                            break;
                        }
                    case ByteCode.Operator.GetGlobal:
                        {
                            Variable v1 = Inst[0];  // global name
                            // TODO: Check for string
                            if (v1.Type != VariableType.String)
                            {
                                throw new Exception("String required");
                            }
                            string GlobalName = v1.GetString();
                            _Stack2[_Top++] = _Machine.Globals[v1];
                            break;
                        }

                    case ByteCode.Operator.SetLocal:
                        {
                            int offset = Inst[0].GetInteger();
                            _Stack2[_Base + offset] = _Stack2[--_Top];
                            break;
                        }
                    case ByteCode.Operator.GetLocal:
                        {
                            int offset = Inst[0].GetInteger();
                            _Stack2[_Top++] = _Stack2[_Base + offset];
                            break;
                        }
                    #region Operators
                    case ByteCode.Operator.OpEq:
                    case ByteCode.Operator.OpNeq:
                    case ByteCode.Operator.OpLt:
                    case ByteCode.Operator.OpLte:
                    case ByteCode.Operator.OpGt:
                    case ByteCode.Operator.OpGte:
                    case ByteCode.Operator.OpAdd:
                    case ByteCode.Operator.OpSub:
                    case ByteCode.Operator.OpMul:
                    case ByteCode.Operator.OpDiv:
                        {
                            Operator o = Operator._MAX;
                            switch (Inst.OpCode)
                            {
                                #region fold
                                case ByteCode.Operator.OpAdd:
                                    {
                                        o = Operator.Add;
                                        break;
                                    }
                                case ByteCode.Operator.OpSub:
                                    {
                                        o = Operator.Sub;
                                        break;
                                    }
                                case ByteCode.Operator.OpMul:
                                    {
                                        o = Operator.Mul;
                                        break;
                                    }
                                case ByteCode.Operator.OpDiv:
                                    {
                                        o = Operator.Div;
                                        break;
                                    }
                                case ByteCode.Operator.GetInd:
                                    {
                                        o = Operator.GetInd;
                                        break;
                                    }
                                case ByteCode.Operator.OpEq:
                                    {
                                        o = Operator.Eq;
                                        break;
                                    }
                                case ByteCode.Operator.OpNeq:
                                    {
                                        o = Operator.Neq;
                                        break;
                                    }
                                case ByteCode.Operator.OpLt:
                                    {
                                        o = Operator.Lt;
                                        break;
                                    }
                                case ByteCode.Operator.OpLte:
                                    {
                                        o = Operator.Lte;
                                        break;
                                    }
                                case ByteCode.Operator.OpGt:
                                    {
                                        o = Operator.Gt;
                                        break;
                                    }
                                case ByteCode.Operator.OpGte:
                                    {
                                        o = Operator.Gte;
                                        break;
                                    }
                                #endregion
                            }
                            int t = _Stack2[_Top - 1].TypeCode;
                            if (_Stack2[_Top - 2].TypeCode > t)
                            {
                                t = _Stack2[_Top - 2].TypeCode;
                            }

                            OperatorCallback op = Machine.GetTypeOperator(t, o);
                            if (op != null)
                            {
                                int operand = _Top--;
                                op(this, ref _Stack2[operand - 2], ref _Stack2[operand - 1], ref _Stack2[operand]);
                              //  --_Top;
                            }
                            else
                            {
                                throw new Exception("Operator not defined");
                            }
                            break;
                        }
                    case ByteCode.Operator.GetInd:
                        {
                            OperatorCallback op = Machine.GetTypeOperator(_Stack2[_Top - 2].TypeCode, Operator.GetInd);
                            if (op != null)
                            {
                                int operand = _Top--;
                                op(this, ref _Stack2[operand - 2], ref _Stack2[operand - 1], ref _Stack2[operand]);
                                //  --_Top;
                            }
                            else
                            {
                                throw new Exception("Operator not defined");
                            }
                            break;
                        }
                    case ByteCode.Operator.SetInd:
                        {
                            OperatorCallback op = Machine.GetTypeOperator(_Stack2[_Top - 3].TypeCode, Operator.SetInd);
                            if (op != null)
                            {
                                op(this, ref _Stack2[_Top - 3], ref _Stack2[_Top - 2], ref _Stack2[_Top - 1]);
                                _Top -= 3;
                            }
                            else
                            {
                                throw new Exception("Operator not defined");
                            }
                            break;
                        }
                    case ByteCode.Operator.ForEach:
                        {
                            TypeIteratorCallback itr = Machine.GetTypeIterator(_Stack2[_Top - 2].TypeCode);
                            if (itr == null)
                            {
                                _Machine.Log.LogEntry("Undefined iterator for type");
                                return ThreadState.Exception;
                            }

                            int IteratorPos = _Stack2[_Top - 1].GetInteger();
                            object obj = _Stack2[_Top - 2].GetReference();
                            itr(this, obj, ref IteratorPos, ref _Stack2[_Base + Inst[1].GetInteger()], ref _Stack2[_Base + Inst[0].GetInteger()]);
                            if (IteratorPos != -1)
                            {
                                //_Stack2[_Base + Inst[1].GetInteger()] = Key;
                                //_Stack2[_Base + Inst[0].GetInteger()] = Item;
                                _Stack2[_Top].SetInteger(0);
                            }
                            else
                            {
                                _Stack2[_Top].SetInteger(0);
                            }
                            _Stack2[_Top - 1].SetInteger(IteratorPos);
                            ++_Top;


                            break;
                        }
                    #region Dot Operators
                    case ByteCode.Operator.GetDot:
                        {
                            OperatorCallback op = Machine.GetTypeOperator(_Stack2[_Top - 1].TypeCode, Operator.GetDot);
                            _Stack2[_Top] = Inst[0];
                            if (op != null)
                            {
                                op(this, ref _Stack2[_Top - 1], ref _Stack2[_Top], ref _Stack2[_Top]);
                            }
                            else
                            {
                                throw new Exception("Operator not defined");
                            }
                            break;
                        }
                    case ByteCode.Operator.SetDot:
                        {
                            OperatorCallback op = Machine.GetTypeOperator(_Stack2[_Top - 2].TypeCode, Operator.SetDot);
                            _Stack2[_Top] = Inst[0];
                            if (op != null)
                            {
                                op(this, ref _Stack2[_Top - 2], ref _Stack2[_Top - 1], ref _Stack2[_Top]);
                                _Top -= 2;
                            }
                            else
                            {
                                throw new Exception("Operator not defined");
                            }
                            break;
                        }
                    #endregion       
                    #endregion
                    #region Branch
                    case ByteCode.Operator.Brz:
                        {
                            if (_Stack2[--_Top].GetInteger() == 0)
                            {
                                _InstructionPtr = Inst[0].GetInteger();
                            }
                            break;
                        }
                    case ByteCode.Operator.Brnz:
                        {
                            if (_Stack2[--_Top].GetInteger() != 0)
                            {
                                _InstructionPtr = Inst[0].GetInteger();
                            }
                            break;
                        }
                    case ByteCode.Operator.Brzk:
                        {
                            if (_Stack2[_Top - 1].GetInteger() == 0)
                            {
                                _InstructionPtr = Inst[0].GetInteger();
                            }
                            break;
                        }
                    case ByteCode.Operator.Brnzk:
                        {
                            if (_Stack2[_Top - 1].GetInteger() != 0)
                            {
                                _InstructionPtr = Inst[0].GetInteger();
                            }
                            break;
                        }
                    case ByteCode.Operator.Bra:
                        {
                            _InstructionPtr = Inst[0].GetInteger();
                            break;
                        }
                    #endregion
                    default:
                        {
                            throw new Exception("Unknown instr");
                        }
                } // end switch instruction
            } // end loop

            return ThreadState.Exception;
        }


        protected FunctionObject _Function;
        protected Machine _Machine;
        internal StackFrame _Frame;
        protected int _ThreadId = 0;
        protected ThreadState _State;
        protected int _Top = 0;
        protected int _Base = 0;
        protected int _InstructionPtr = 0;
        protected int _NumParameters = 0;
        protected List<Variable> _Blocks;
        protected Variable[] _Stack2 = new Variable[16384];   // Stack
    }
}
