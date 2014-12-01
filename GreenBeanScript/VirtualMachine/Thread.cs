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
            get { return _machine; }
        }

        public ThreadState State
        {
            get { return _State; }
        }

        #endregion

        public Thread(Machine Machine)
        {
            _machine = Machine;
        }

        public Thread(Machine Machine, FunctionObject Function)
        {
            _machine = Machine;
            _Function = Function;
        }

        public void LogException(string Message)
        {
            _machine.Log.LogEntry(Message);
            // TODO: Set state to exception
        }

        #region Public Push Methods
        public void Push(Variable Var)
        {
            _Stack2[_Top++] = Var;
        }
        
        public void PushNull()
        {
            _Stack2[_Top++] = Variable.Null;
        }

        public void PushInteger(int Value)
        {
            _Stack2[_Top++]  = Value;
        }

        public void PushFloat(float Value)
        {
            _Stack2[_Top++] = Value;
        }

        public void PushString(string Value)
        {
            _Stack2[_Top++] = Value;
        }

        public void PushFunction(FunctionObject Function)
        {
            _Stack2[_Top++] = Function;
        }

        public void PushTable(TableObject Table)
        {
            _Stack2[_Top++] = Table;
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
            get { return This.Type; }
        }

        private Variable Top
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
            _Function.GetInstructions(ref _instructionList);
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
                _machine.Log.LogEntry("Attempted to call non-function type");
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
                    _Stack2[_Base - 2] = Variable.Null;
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
                        _machine.SwitchThreadState(this, ThreadState.Blocked);
                        return ThreadState.Blocked;
                    }
                    else if (res == FunctionResult.Sys_Sleep)
                    {
                        // Todo: Sort out return addr
                        _machine.SwitchThreadState(this, ThreadState.Sleeping);
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
            _Function.GetInstructions(ref _instructionList);
            

            return ThreadState.Running;
        }
        #endregion

        ByteCode.Instruction[] _instructionList;

        private Variable PopStack()
        {
            return _Stack2[--_Top];
        }

        private Variable PopStack(int num)
        {
            _Top -= num;
            return GetTopStack();
        }

        private void DupTopStack()
        {
            SetTopStack(_Stack2[_Top - 1]);
            ++_Top;
        }

        private void SetTopStack(Variable var)
        {
            _Stack2[_Top] = var;
        }

        private Variable GetTopStack()
        {
            return _Stack2[_Top];
        }

        public ThreadState Execute()
        {
            if (State != ThreadState.Running)
            {
                return State;
            }

            for (; ; )
            {
                if (_InstructionPtr >= _instructionList.Length)
                    break;

                var inst = _instructionList[_InstructionPtr++];

                switch (inst.OpCode)
                {
                    case ByteCode.Opcode.Pop:
                        {
                            PopStack();
                            break;
                        }

                    case ByteCode.Opcode.Pop2:
                        {
                            PopStack(2);
                            break;
                        }

                    case ByteCode.Opcode.Dup:
                        {
                            DupTopStack();
                            break;
                        }

                    case ByteCode.Opcode.Call:
                        {
                            // pop arg count from stack
                            ThreadState res = PushStackFrame(inst[0].GetInteger(), _InstructionPtr);

                            if (res == ThreadState.Running)
                            {
                                break;
                            }
                            if (res == ThreadState.Sys_Yield) return ThreadState.Running;
                            if (res == ThreadState.Exception)
                            {
                                _machine.SwitchThreadState(this, ThreadState.Killed);
                                return ThreadState.Exception;
                            }
                            if (res == ThreadState.Killed)
                            {
                                _machine.SwitchThreadState(this, ThreadState.Killed);
                            }
                            // if exception then die

                            break;
                        }
                    #region Push Operations
                    case ByteCode.Opcode.PushNull:
                        {
                            PushNull();
                            break;
                        }
                    case ByteCode.Opcode.PushThis:
                        {
                            Push(This);
                            break;
                        }
                    case ByteCode.Opcode.PushInt0:
                    case ByteCode.Opcode.PushInt1:
                    case ByteCode.Opcode.PushInt:
                    case ByteCode.Opcode.PushFp:
                    case ByteCode.Opcode.PushStr:
                    case ByteCode.Opcode.PushFn:
                        {
                            Push(inst[0]);
                            break;
                        }
                    case ByteCode.Opcode.PushTbl:
                        {
                            Push(_machine.CreateTable());
                            break;
                        }
                    #endregion
                    case ByteCode.Opcode.Ret:          // Ret pushes a null
                        {
                            PushNull();
                            ThreadState res = PopStackFrame(_InstructionPtr);
                            if (res == ThreadState.Running)
                            {
                                break;
                            }
                            else if (res == ThreadState.Killed)
                            {
                                // Todo: Sort out return addr
                                _machine.SwitchThreadState(this, ThreadState.Killed);
                                return res;
                            }
                            else if (res == ThreadState.Exception)
                            {
                                _machine.SwitchThreadState(this, ThreadState.Exception);
                                return ThreadState.Exception;
                            }
                            break;
                        }
                    case ByteCode.Opcode.Retv:        
                        {
                            ThreadState res = PopStackFrame(_InstructionPtr);
                            if (res == ThreadState.Killed)
                            {
                                return res;
                            }
                            break;
                        }
//                    case ByteCode.Operator.GetDot:
                    case ByteCode.Opcode.SetGlobal:
                        {
                            Variable v1 = inst[0];
                            if (v1.Type != VariableType.String)
                            {
                                throw new Exception("String required");
                            }
                            
                            Variable v2 = PopStack();
                            string GlobalName = v1.GetString();
                            _machine.Globals[v1] = v2;
                            break;
                        }
                    case ByteCode.Opcode.GetGlobal:
                        {
                            Variable v1 = inst[0];  // global name
                            // TODO: Check for string
                            if (v1.Type != VariableType.String)
                            {
                                throw new Exception("String required");
                            }
                            string GlobalName = v1.GetString();
                            var global = _machine.Globals[v1];
                            Push(global);
                            break;
                        }

                    case ByteCode.Opcode.SetLocal:
                        {
                            int offset = inst[0].GetInteger();
                            _Stack2[_Base + offset] = PopStack();
                            break;
                        }
                    case ByteCode.Opcode.GetLocal:
                        {
                            int offset = inst[0].GetInteger();
                            Push(_Stack2[_Base + offset]);
                            break;
                        }
                    #region Operators
                    case ByteCode.Opcode.OpEq:
                    case ByteCode.Opcode.OpNeq:
                    case ByteCode.Opcode.OpLt:
                    case ByteCode.Opcode.OpLte:
                    case ByteCode.Opcode.OpGt:
                    case ByteCode.Opcode.OpGte:
                    case ByteCode.Opcode.OpRem:
                    case ByteCode.Opcode.OpAdd:
                    case ByteCode.Opcode.OpSub:
                    case ByteCode.Opcode.OpMul:
                    case ByteCode.Opcode.OpDiv:
                        {
                            Operator o = Operator._MAX;
                            switch (inst.OpCode)
                            {
                                #region fold
                                case ByteCode.Opcode.OpAdd:
                                    {
                                        o = Operator.Add;
                                        break;
                                    }
                                case ByteCode.Opcode.OpSub:
                                    {
                                        o = Operator.Sub;
                                        break;
                                    }
                                case ByteCode.Opcode.OpMul:
                                    {
                                        o = Operator.Mul;
                                        break;
                                    }
                                case ByteCode.Opcode.OpDiv:
                                    {
                                        o = Operator.Div;
                                        break;
                                    }
                                case ByteCode.Opcode.GetInd:
                                    {
                                        o = Operator.GetInd;
                                        break;
                                    }
                                case ByteCode.Opcode.OpEq:
                                    {
                                        o = Operator.Eq;
                                        break;
                                    }
                                case ByteCode.Opcode.OpNeq:
                                    {
                                        o = Operator.Neq;
                                        break;
                                    }
                                case ByteCode.Opcode.OpLt:
                                    {
                                        o = Operator.Lt;
                                        break;
                                    }
                                case ByteCode.Opcode.OpLte:
                                    {
                                        o = Operator.Lte;
                                        break;
                                    }
                                case ByteCode.Opcode.OpGt:
                                    {
                                        o = Operator.Gt;
                                        break;
                                    }
                                case ByteCode.Opcode.OpGte:
                                    {
                                        o = Operator.Gte;
                                        break;
                                    }
                                case ByteCode.Opcode.OpRem:
                                    {
                                        o = Operator.Rem;
                                        break;
                                    }
                                #endregion
                            }
                            if (o == Operator._MAX)
                            {
                                throw new NotImplementedException("Operator not mapped or implemented");
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
                                _Stack2[operand - 2] = op(this, _Stack2[operand - 2], _Stack2[operand - 1], _Stack2[operand]);
                              //  --_Top;
                            }
                            else
                            {
                                throw new Exception("Operator not defined");
                            }
                            break;
                        }
                    case ByteCode.Opcode.GetInd:
                        {
                            OperatorCallback op = Machine.GetTypeOperator(_Stack2[_Top - 2].TypeCode, Operator.GetInd);
                            if (op != null)
                            {
                                int operand = _Top--;
                                _Stack2[operand - 2] = op(this, _Stack2[operand - 2], _Stack2[operand - 1], _Stack2[operand]);
                                //  --_Top;
                            }
                            else
                            {
                                throw new Exception("Operator not defined");
                            }
                            break;
                        }
                    case ByteCode.Opcode.SetInd:
                        {
                            OperatorCallback op = Machine.GetTypeOperator(_Stack2[_Top - 3].TypeCode, Operator.SetInd);
                            if (op != null)
                            {
                                _Stack2[_Top - 3] = op(this, _Stack2[_Top - 3], _Stack2[_Top - 2], _Stack2[_Top - 1]);
                                _Top -= 3;
                            }
                            else
                            {
                                throw new Exception("Operator not defined");
                            }
                            break;
                        }
                    case ByteCode.Opcode.ForEach:
                        {
                            TypeIteratorCallback itr = Machine.GetTypeIterator(_Stack2[_Top - 2].TypeCode);
                            if (itr == null)
                            {
                                _machine.Log.LogEntry("Undefined iterator for type");
                                return ThreadState.Exception;
                            }

                            var iteratorPos = _Stack2[_Top - 1].GetInteger();
                            var obj = _Stack2[_Top - 2].GetReference();
                            iteratorPos = itr(this, obj, iteratorPos, _Stack2[_Base + inst[1].GetInteger()], _Stack2[_Base + inst[0].GetInteger()]);
                            if (iteratorPos != -1)
                            {
                                //_Stack2[_Base + Inst[1].GetInteger()] = Key;
                                //_Stack2[_Base + Inst[0].GetInteger()] = Item;
                                SetTopStack(Variable.Zero);
                            }
                            else
                            {
                                SetTopStack(Variable.Zero);
                            }
                            _Stack2[_Top - 1] = iteratorPos;
                            ++_Top;


                            break;
                        }
                    #region Dot Operators
                    case ByteCode.Opcode.GetDot:
                        {
                            OperatorCallback op = Machine.GetTypeOperator(_Stack2[_Top - 1].TypeCode, Operator.GetDot);
                            SetTopStack(inst[0]);
                            if (op != null)
                            {
                                _Stack2[_Top - 1] = op(this, _Stack2[_Top - 1], _Stack2[_Top], _Stack2[_Top]);
                            }
                            else
                            {
                                throw new Exception("Operator not defined");
                            }
                            break;
                        }
                    case ByteCode.Opcode.SetDot:
                        {
                            OperatorCallback op = Machine.GetTypeOperator(_Stack2[_Top - 2].TypeCode, Operator.SetDot);
                            SetTopStack(inst[0]);
                            if (op != null)
                            {
                                _Stack2[_Top - 2] = op(this, _Stack2[_Top - 2], _Stack2[_Top - 1], _Stack2[_Top]);
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
                    case ByteCode.Opcode.Brz:
                        {
                            var v = PopStack();
                            if (v.IsZero)
                            {
                                var newIp = inst[0].GetInteger();

                                if (newIp >= _instructionList.Length)
                                {
                                    throw new Exception("BRZ: Corrupt IP");
                                }

                                _InstructionPtr = newIp;
                            }
                            break;
                        }
                    case ByteCode.Opcode.Brnz:
                    {
                            var v = PopStack();
                            if (!v.IsZero)
                            {
                                var newIp = inst[0].GetInteger();

                                if (newIp >= _instructionList.Length)
                                {
                                    throw new Exception("BRZ: Corrupt IP");
                                }

                                _InstructionPtr = newIp;
                            }
                            break;
                        }
                    case ByteCode.Opcode.Brzk:
                        {
                            var v = _Stack2[_Top - 1];
                            if (v.IsZero)
                            {
                                var newIp = inst[0].GetInteger();

                                if (newIp >= _instructionList.Length)
                                {
                                    throw new Exception("BRZ: Corrupt IP");
                                }

                                _InstructionPtr = newIp;
                            }
                            break;
                        }
                    case ByteCode.Opcode.Brnzk:
                        {
                            var v = _Stack2[_Top - 1];
                            if (!v.IsZero)
                            {
                                var newIp = inst[0].GetInteger();

                                if (newIp >= _instructionList.Length)
                                {
                                    throw new Exception("BRNZK: Corrupt IP");
                                }

                                _InstructionPtr = newIp;
                            }
                            break;
                        }
                    case ByteCode.Opcode.Bra:
                        {
                            var newIp = inst[0].GetInteger();

                            if (newIp >= _instructionList.Length)
                            {
                                throw new Exception("BRA: Corrupt IP");
                            }

                            _InstructionPtr = newIp;
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
        protected Machine _machine;
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
