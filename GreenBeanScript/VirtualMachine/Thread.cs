using System;
using System.Collections.Generic;
using System.Text;
using GreenBeanScript.VirtualMachine;

namespace GreenBeanScript
{
    internal class StackFrame
    {
        public int ReturnBase;
        public int InstructionPtr;
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
            get { return _paramCount; }
        }
        public int Id
        {
            get { return _threadId; }
        }
  
        public Machine Machine
        {
            get { return _machine; }
        }

        public ThreadState State
        {
            get { return _state; }
        }

        #endregion

        public Thread(int threadId, Machine machine) : this(threadId, machine, null)
        {
        }

        public Thread(int threadId, Machine machine, FunctionObject function)
        {
            _threadId = threadId;
            _machine = machine;
            _function = function;
            _state = ThreadState.Running;
        }

        public void LogException(string message)
        {
            _machine.Log.LogEntry(message);
            _state = ThreadState.Exception;
        }

        public void Push(Variable value)
        {
            _stack.Push(value);
        }
        

        #region Public Parameter Methods
        public Variable Param(int index)
        {
            return _stack.PeekBase(index);
        }

        public Variable This
        {
            get { return _stack.PeekBase(-2); }
        }

        public FunctionObject Function
        {
            get { return _stack.PeekBase(-1).GetFunction(); }
        }

        #endregion

        #region Internal Mathods


        internal void SetState(ThreadState State)
        {
            _state = State;
        }

        private ThreadState PopStackFrame()
        {
            if (_stackFrames.Count == 0)
            {
                LogException("Stack undeflow");
                return ThreadState.Exception;
            }

            var frame = _stackFrames.Pop();

            if (_stackFrames.Count == 0)
            {
                return ThreadState.Killed;
            }

            _instructionPtr = frame.InstructionPtr;
            _stack.PokeBase(-2, _stack.Peek(-1));

            _stack.StackPointer = _stack.BasePointer - 1;
            _stack.BasePointer = frame.ReturnBase;


            _function = this.Function;
            _instructionList = _function.Instructions;
            return ThreadState.Running;
        }

        internal int SetBlocks(ref Variable[] Blocks)
        {
            if (Blocks.Length == 0)
                return 0;

            if (_blocks == null)
            {
                _blocks = new List<Variable>();
            }

            foreach (Variable Block in Blocks)
            {
                if (!_blocks.Contains(Block))
                {
                    _blocks.Add(Block);
                }
            }


            return 0;
        }

        internal ThreadState PushStackFrame(int parameterCount)
        {
            int Base = _stack.StackPointer - parameterCount;

            if (Base == 2)
            {
                // TODO: New thread callback
                _stack.BasePointer = Base;
            }

            Variable fnVar = _stack.PeekAbs(Base - 1);
            if (fnVar.Type != VariableType.Function)
            {
                _machine.Log.LogEntry("Attempted to call non-function type");
                return ThreadState.Exception;
            }

            var func = fnVar.GetFunction();

            if (func.Native != null)
            {
                var lastBase = _stack.BasePointer;
                var lastTop = _stack.StackPointer;
                _stack.BasePointer = Base;
                _paramCount = parameterCount;

                FunctionResult res = func.Native(this);

                if (lastTop == _stack.StackPointer)
                {
                    _stack.PokeBase(-2, Variable.Null);
                }
                else
                {
                    _stack.PokeBase(-2, _stack.Peek(-1));
                }

                // return stack
                _stack.StackPointer = _stack.BasePointer - 1;
                _stack.BasePointer = lastBase;

                switch (res)
                {
                    case FunctionResult.Ok:
                    {
                        break;
                    }
                    case FunctionResult.Sys_Block:
                    {
                        // Todo: Sort out return addr
                        _machine.SwitchThreadState(this, ThreadState.Blocked);
                        return ThreadState.Blocked;
                    }
                    case FunctionResult.Sys_Yield:
                    {
                        // Todo: Remove signals
                        // Todo: Sort out return addr
                        return ThreadState.Sys_Yield;
                    }
                    case FunctionResult.Sys_Sleep:
                    {
                        // Todo: Sort out return addr
                        _machine.SwitchThreadState(this, ThreadState.Sleeping);
                        return ThreadState.Sleeping;
                    }
                    case FunctionResult.Sys_Kill:
                    {
                        return ThreadState.Killed;
                    }
                    default:
                    {
                        return ThreadState.Exception;
                    }
                }
                
                return _stackFrames.Count == 0 ? ThreadState.Killed : ThreadState.Running;
            }


            // Is a scripted function

            // Null out params
            /*
            for (int p = 0; p < Func.NumParams; ++p)
            {
                _Stack2[_Base + p] = new Variable();
            }*/

            StackFrame frame = new StackFrame {ReturnBase = _stack.BasePointer, InstructionPtr = _instructionPtr};
            _stackFrames.Push(frame);
      

            // Point to fn of new thread
            _instructionPtr = 0;
            _stack.BasePointer = Base;
            _stack.StackPointer = Base + func.NumParamsLocals;
            _function = func;
            _instructionList = _function.Instructions;
            return ThreadState.Running;
        }
        #endregion

        private List<ByteCode.Instruction> _instructionList;

       


        public ThreadState Execute()
        {
            if (State != ThreadState.Running)
            {
                return State;
            }

            for (; ; )
            {
                if (_instructionPtr >= _instructionList.Count)
                    break;

                var inst = _instructionList[_instructionPtr++];

                switch (inst.OpCode)
                {
                    case ByteCode.Opcode.Pop:
                        {
                            _stack.Pop();
                            break;
                        }
                    case ByteCode.Opcode.Pop2:
                        {
                            _stack.Pop(2);
                            break;
                        }
                    case ByteCode.Opcode.Dup:
                        {
                            _stack.Push(_stack.Peek());
                            break;
                        }
                    case ByteCode.Opcode.Call:
                        {
                            // pop arg count from stack
                            ThreadState res = PushStackFrame(inst[0].GetInteger());

                            switch (res)
                            {
                                case ThreadState.Running:
                                {
                                    break;
                                }
                                case ThreadState.Sys_Yield:
                                {
                                    return ThreadState.Running;
                                }
                                case ThreadState.Exception:
                                {
                                    _machine.SwitchThreadState(this, ThreadState.Killed);
                                    return ThreadState.Exception;
                                }
                                case ThreadState.Killed:
                                {
                                    _machine.SwitchThreadState(this, ThreadState.Killed);
                                    break;
                                }
                                default:
                                {
                                    break;
                                }
                            }


                            break;
                        }
                    #region Push Operations
                    case ByteCode.Opcode.PushNull:
                        {
                            Push(Variable.Null);
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
                            Push(new TableObject());
                            break;
                        }
                    #endregion
                    case ByteCode.Opcode.Ret:          // Ret pushes a null
                        {
                            Push(Variable.Null);
                            ThreadState res = PopStackFrame();
                            switch (res)
                            {
                                case ThreadState.Exception:
                                case ThreadState.Killed:
                                {
                                    _machine.SwitchThreadState(this, res);
                                    return res;
                                }
                                default:
                                {
                                    break;
                                }
                            }
                            break;
                        }
                    case ByteCode.Opcode.Retv:        
                        {
                            ThreadState res = PopStackFrame();
                            if (res == ThreadState.Killed)
                            {
                                return res;
                            }
                            break;
                        }
                    case ByteCode.Opcode.SetGlobal:
                        {
                            Variable v1 = inst[0];
                            if (v1.Type != VariableType.String)
                            {
                                throw new Exception("String required");
                            }

                            Variable v2 = _stack.Pop();
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
                            var global = _machine.Globals[v1];
                            Push(global);
                            break;
                        }

                    case ByteCode.Opcode.SetLocal:
                        {
                            int offset = inst[0].GetInteger();
                            _stack.PokeBase(offset, _stack.Pop());
                            break;
                        }
                    case ByteCode.Opcode.GetLocal:
                        {
                            int offset = inst[0].GetInteger();
                            Push(_stack.PeekBase(offset));
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
                            var t1 = _stack.Peek(-1).TypeCode;
                            var t2 = _stack.Peek(-2).TypeCode;
                            if (t2 > t1)
                            {
                                t1 = t2;
                            }

                            OperatorCallback op = Machine.GetTypeOperator(t1, o);
                            if (op != null)
                            {
                                var operand = _stack.StackPointer--;
                                _stack.PokeAbs(operand - 2, op(this, _stack.PeekAbs(operand - 2), _stack.PeekAbs(operand - 1), _stack.PeekAbs(operand)));
                            }
                            else
                            {
                                throw new Exception("Operator not defined");
                            }
                            break;
                        }
                    case ByteCode.Opcode.GetInd:
                    {
                            var type = _stack.Peek(-2).TypeCode;
                            var op = Machine.GetTypeOperator(type, Operator.GetInd);
                            if (op != null)
                            {
                                var operand = _stack.StackPointer--;
                                _stack.PokeAbs(operand - 2, op(this, _stack.PeekAbs(operand - 2), _stack.PeekAbs(operand - 1), _stack.PeekAbs(operand)));
                            }
                            else
                            {
                                throw new Exception("Operator not defined");
                            }
                            break;
                        }
                    case ByteCode.Opcode.SetInd:
                    {
                        var topMin3 = _stack.Peek(-3);
                            OperatorCallback op = Machine.GetTypeOperator(topMin3.TypeCode, Operator.SetInd);
                            if (op != null)
                            {
                                _stack.Poke(-3, op(this, topMin3, _stack.Peek(-2), _stack.Peek(-1)));
                                _stack.StackPointer -= 3;
                            }
                            else
                            {
                                throw new Exception("Operator not defined");
                            }
                            break;
                        }
                    case ByteCode.Opcode.ForEach:
                        {
                            var topMin2 = _stack.Peek(-2);
                            TypeIteratorCallback itr = Machine.GetTypeIterator(topMin2.TypeCode);
                            if (itr == null)
                            {
                                _machine.Log.LogEntry("Undefined iterator for type");
                                return ThreadState.Exception;
                            }

                            var iteratorPos = _stack.Peek(-1).GetInteger();
                            var obj = _stack.Peek(-2).GetReference();
                            iteratorPos = itr(this, obj, iteratorPos, _stack.PeekBase(inst[1].GetInteger()), _stack.PeekBase(inst[0].GetInteger()));
                            if (iteratorPos != -1)
                            {
                                //_Stack2[_Base + Inst[1].GetInteger()] = Key;
                                //_Stack2[_Base + Inst[0].GetInteger()] = Item;
                                _stack.Poke(Variable.Zero);
                            }
                            else
                            {
                                _stack.Poke(Variable.Zero);
                            }
                            _stack.Poke(-1, iteratorPos);
                            ++_stack.StackPointer;


                            break;
                        }
                    #region Dot Operators
                    case ByteCode.Opcode.GetDot:
                    {
                            var v1 = _stack.Peek(-1);
                            OperatorCallback op = Machine.GetTypeOperator(v1.TypeCode, Operator.GetDot);
                            _stack.Poke(inst[0]);
                            if (op != null)
                            {
                                _stack.Poke(-1, op(this, v1, _stack.Peek(), _stack.Peek()));
                            }
                            else
                            {
                                throw new Exception("Operator not defined");
                            }
                            break;
                        }
                    case ByteCode.Opcode.SetDot:
                        {
                            var v1 = _stack.Peek(-2);
                            OperatorCallback op = Machine.GetTypeOperator(v1.TypeCode, Operator.SetDot);
                            _stack.Poke(inst[0]);
                            if (op != null)
                            {
                                _stack.Poke(-2, op(this, _stack.Peek(-2), _stack.Peek(-1), _stack.Peek()));
                                _stack.StackPointer -= 2;
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
                            var v = _stack.Pop();
                            if (v.IsZero)
                            {
                                var newIp = inst[0].GetInteger();

                                if (newIp >= _instructionList.Count)
                                {
                                    throw new Exception("BRZ: Corrupt IP");
                                }

                                _instructionPtr = newIp;
                            }
                            break;
                        }
                    case ByteCode.Opcode.Brnz:
                    {
                            var v = _stack.Pop();
                            if (!v.IsZero)
                            {
                                var newIp = inst[0].GetInteger();

                                if (newIp >= _instructionList.Count)
                                {
                                    throw new Exception("BRZ: Corrupt IP");
                                }

                                _instructionPtr = newIp;
                            }
                            break;
                        }
                    case ByteCode.Opcode.Brzk:
                        {
                            var v = _stack.Peek(-1);
                            if (v.IsZero)
                            {
                                var newIp = inst[0].GetInteger();

                                if (newIp >= _instructionList.Count)
                                {
                                    throw new Exception("BRZ: Corrupt IP");
                                }

                                _instructionPtr = newIp;
                            }
                            break;
                        }
                    case ByteCode.Opcode.Brnzk:
                        {
                            var v = _stack.Peek(-1);
                            if (!v.IsZero)
                            {
                                var newIp = inst[0].GetInteger();

                                if (newIp >= _instructionList.Count)
                                {
                                    throw new Exception("BRNZK: Corrupt IP");
                                }

                                _instructionPtr = newIp;
                            }
                            break;
                        }
                    case ByteCode.Opcode.Bra:
                        {
                            var newIp = inst[0].GetInteger();

                            if (newIp >= _instructionList.Count)
                            {
                                throw new Exception("BRA: Corrupt IP");
                            }

                            _instructionPtr = newIp;
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


        private FunctionObject _function;
        private readonly Machine _machine;
        private readonly Stack<StackFrame> _stackFrames = new Stack<StackFrame>();
        private readonly int _threadId;
        private ThreadState _state;

        private int _instructionPtr;
        private int _paramCount;
        private List<Variable> _blocks;
        private readonly ThreadStack _stack = new ThreadStack(); 
    }
}
