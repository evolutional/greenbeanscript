using System;
using System.Collections.Generic;
using GreenBeanScript.VirtualMachine.ByteCode;
using GreenBeanScript.VirtualMachine.Operations;

namespace GreenBeanScript.VirtualMachine
{
    public class Thread
    {
        private readonly ThreadStack _stack = new ThreadStack();
        private readonly Stack<StackFrame> _stackFrames = new Stack<StackFrame>();
        
        private FunctionObject _function;

        private List<Instruction> _instructionList;

        private int _instructionPtr;

        public Thread(int threadId, Machine machine) : this(threadId, machine, null)
        {
        }

        public Thread(int threadId, Machine machine, FunctionObject function)
        {
            Id = threadId;
            Machine = machine;
            _function = function;
            State = ThreadState.Running;
        }

        public void LogException(string message)
        {
            Machine.Log.LogEntry(message);
            State = ThreadState.Exception;
        }

        public void Push(Variable value)
        {
            _stack.Push(value);
        }


        public ThreadState Execute()
        {
            if (State != ThreadState.Running)
                return State;

            for (;;)
            {
                if (_instructionPtr >= _instructionList.Count)
                    break;

                var inst = _instructionList[_instructionPtr++];

                switch (inst.OpCode)
                {
                    case Opcode.Pop:
                    {
                        _stack.Pop();
                        break;
                    }
                    case Opcode.Pop2:
                    {
                        _stack.Pop(2);
                        break;
                    }
                    case Opcode.Dup:
                    {
                        _stack.Push(_stack.Peek());
                        break;
                    }
                    case Opcode.Call:
                    {
                        // pop arg count from stack
                        var res = PushStackFrame(inst[0].GetInteger());

                        switch (res)
                        {
                            case ThreadState.Running:
                            {
                                break;
                            }
                            case ThreadState.SysYield:
                            {
                                return ThreadState.Running;
                            }
                            case ThreadState.Exception:
                            {
                                Machine.SwitchThreadState(this, ThreadState.Killed);
                                return ThreadState.Exception;
                            }
                            case ThreadState.Killed:
                            {
                                Machine.SwitchThreadState(this, ThreadState.Killed);
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

                    case Opcode.PushNull:
                    {
                        Push(Variable.Null);
                        break;
                    }
                    case Opcode.PushThis:
                    {
                        Push(This);
                        break;
                    }
                    case Opcode.PushInt0:
                    case Opcode.PushInt1:
                    case Opcode.PushInt:
                    case Opcode.PushFp:
                    case Opcode.PushStr:
                    case Opcode.PushFn:
                    {
                        Push(inst[0]);
                        break;
                    }
                    case Opcode.PushTbl:
                    {
                        Push(new TableObject());
                        break;
                    }

                        #endregion

                    case Opcode.Ret: // Ret pushes a null
                    {
                        Push(Variable.Null);
                        var res = PopStackFrame();
                        switch (res)
                        {
                            case ThreadState.Exception:
                            case ThreadState.Killed:
                            {
                                Machine.SwitchThreadState(this, res);
                                return res;
                            }
                            default:
                            {
                                break;
                            }
                        }
                        break;
                    }
                    case Opcode.Retv:
                    {
                        var res = PopStackFrame();
                        if (res == ThreadState.Killed)
                            return res;
                        break;
                    }
                    case Opcode.SetGlobal:
                    {
                        var v1 = inst[0];
                        if (v1.Type != VariableType.String)
                            throw new Exception("String required");

                        var v2 = _stack.Pop();
                        Machine.Globals[v1] = v2;
                        break;
                    }
                    case Opcode.GetGlobal:
                    {
                        var v1 = inst[0]; // global name
                        // TODO: Check for string
                        if (v1.Type != VariableType.String)
                            throw new Exception("String required");
                        var global = Machine.Globals[v1];
                        Push(global);
                        break;
                    }

                    case Opcode.SetLocal:
                    {
                        var offset = inst[0].GetInteger();
                        _stack.PokeBase(offset, _stack.Pop());
                        break;
                    }
                    case Opcode.GetLocal:
                    {
                        var offset = inst[0].GetInteger();
                        Push(_stack.PeekBase(offset));
                        break;
                    }

                        #region Operators

                    case Opcode.OpEq:
                    case Opcode.OpNeq:
                    case Opcode.OpLt:
                    case Opcode.OpLte:
                    case Opcode.OpGt:
                    case Opcode.OpGte:
                    case Opcode.OpRem:
                    case Opcode.OpAdd:
                    case Opcode.OpSub:
                    case Opcode.OpMul:
                    case Opcode.OpDiv:
                    {
                        var o = Operator.Max;
                        switch (inst.OpCode)
                        {
                                #region fold

                            case Opcode.OpAdd:
                            {
                                o = Operator.Add;
                                break;
                            }
                            case Opcode.OpSub:
                            {
                                o = Operator.Sub;
                                break;
                            }
                            case Opcode.OpMul:
                            {
                                o = Operator.Mul;
                                break;
                            }
                            case Opcode.OpDiv:
                            {
                                o = Operator.Div;
                                break;
                            }
                            case Opcode.GetInd:
                            {
                                o = Operator.GetInd;
                                break;
                            }
                            case Opcode.OpEq:
                            {
                                o = Operator.Eq;
                                break;
                            }
                            case Opcode.OpNeq:
                            {
                                o = Operator.Neq;
                                break;
                            }
                            case Opcode.OpLt:
                            {
                                o = Operator.Lt;
                                break;
                            }
                            case Opcode.OpLte:
                            {
                                o = Operator.Lte;
                                break;
                            }
                            case Opcode.OpGt:
                            {
                                o = Operator.Gt;
                                break;
                            }
                            case Opcode.OpGte:
                            {
                                o = Operator.Gte;
                                break;
                            }
                            case Opcode.OpRem:
                            {
                                o = Operator.Rem;
                                break;
                            }

                                #endregion
                        }
                        if (o == Operator.Max)
                            throw new NotImplementedException("Operator not mapped or implemented");
                        var t1 = _stack.Peek(-1).TypeCode;
                        var t2 = _stack.Peek(-2).TypeCode;
                        if (t2 > t1)
                            t1 = t2;

                        var op = Machine.GetTypeOperator(t1, o);
                        if (op != null)
                        {
                            var operand = _stack.StackPointer--;
                            _stack.PokeAbs(operand - 2,
                                op(this, _stack.PeekAbs(operand - 2), _stack.PeekAbs(operand - 1),
                                    _stack.PeekAbs(operand)));
                        }
                        else
                        {
                            throw new Exception("Operator not defined");
                        }
                        break;
                    }
                    case Opcode.GetInd:
                    {
                        var type = _stack.Peek(-2).TypeCode;
                        var op = Machine.GetTypeOperator(type, Operator.GetInd);
                        if (op != null)
                        {
                            var operand = _stack.StackPointer--;
                            _stack.PokeAbs(operand - 2,
                                op(this, _stack.PeekAbs(operand - 2), _stack.PeekAbs(operand - 1),
                                    _stack.PeekAbs(operand)));
                        }
                        else
                        {
                            throw new Exception("Operator not defined");
                        }
                        break;
                    }
                    case Opcode.SetInd:
                    {
                        var topMin3 = _stack.Peek(-3);
                        var op = Machine.GetTypeOperator(topMin3.TypeCode, Operator.SetInd);
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
                    case Opcode.ForEach:
                    {
                        var topMin2 = _stack.Peek(-2);
                        var itr = Machine.GetTypeIterator(topMin2.TypeCode);
                        if (itr == null)
                        {
                            Machine.Log.LogEntry("Undefined iterator for type");
                            return ThreadState.Exception;
                        }

                        var iteratorPos = _stack.Peek(-1).GetInteger();
                        var obj = _stack.Peek(-2).GetReference();
                        iteratorPos = itr(this, obj, iteratorPos, _stack.PeekBase(inst[1].GetInteger()),
                            _stack.PeekBase(inst[0].GetInteger()));
                        if (iteratorPos != -1)
                            _stack.Poke(Variable.Zero);
                        else
                            _stack.Poke(Variable.Zero);
                        _stack.Poke(-1, iteratorPos);
                        ++_stack.StackPointer;


                        break;
                    }

                        #region Dot Operators

                    case Opcode.GetDot:
                    {
                        var v1 = _stack.Peek(-1);
                        var op = Machine.GetTypeOperator(v1.TypeCode, Operator.GetDot);
                        _stack.Poke(inst[0]);
                        if (op != null)
                            _stack.Poke(-1, op(this, v1, _stack.Peek(), _stack.Peek()));
                        else
                            throw new Exception("Operator not defined");
                        break;
                    }
                    case Opcode.SetDot:
                    {
                        var v1 = _stack.Peek(-2);
                        var op = Machine.GetTypeOperator(v1.TypeCode, Operator.SetDot);
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

                    case Opcode.Brz:
                    {
                        var v = _stack.Pop();
                        if (v.IsZero)
                        {
                            var newIp = inst[0].GetInteger();

                            if (newIp >= _instructionList.Count)
                                throw new Exception("BRZ: Corrupt IP");

                            _instructionPtr = newIp;
                        }
                        break;
                    }
                    case Opcode.Brnz:
                    {
                        var v = _stack.Pop();
                        if (!v.IsZero)
                        {
                            var newIp = inst[0].GetInteger();

                            if (newIp >= _instructionList.Count)
                                throw new Exception("BRZ: Corrupt IP");

                            _instructionPtr = newIp;
                        }
                        break;
                    }
                    case Opcode.Brzk:
                    {
                        var v = _stack.Peek(-1);
                        if (v.IsZero)
                        {
                            var newIp = inst[0].GetInteger();

                            if (newIp >= _instructionList.Count)
                                throw new Exception("BRZ: Corrupt IP");

                            _instructionPtr = newIp;
                        }
                        break;
                    }
                    case Opcode.Brnzk:
                    {
                        var v = _stack.Peek(-1);
                        if (!v.IsZero)
                        {
                            var newIp = inst[0].GetInteger();

                            if (newIp >= _instructionList.Count)
                                throw new Exception("BRNZK: Corrupt IP");

                            _instructionPtr = newIp;
                        }
                        break;
                    }
                    case Opcode.Bra:
                    {
                        var newIp = inst[0].GetInteger();

                        if (newIp >= _instructionList.Count)
                            throw new Exception("BRA: Corrupt IP");

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

        #region Properties

        /// <summary>
        ///     Returns the number of parameters in a thread callback
        /// </summary>
        public int ParameterCount { get; private set; }

        public int Id { get; }

        public Machine Machine { get; }

        public ThreadState State { get; private set; }

        #endregion

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

        internal void SetState(ThreadState state)
        {
            this.State = state;
        }

        private ThreadState PopStackFrame()
        {
            if (_stackFrames.Count == 0)
            {
                LogException("Stack underflow");
                return ThreadState.Exception;
            }

            var frame = _stackFrames.Pop();

            if (_stackFrames.Count == 0)
                return ThreadState.Killed;

            _instructionPtr = frame.InstructionPtr;
            _stack.PokeBase(-2, _stack.Peek(-1));

            _stack.StackPointer = _stack.BasePointer - 1;
            _stack.BasePointer = frame.ReturnBase;


            _function = Function;
            _instructionList = _function.Instructions;
            return ThreadState.Running;
        }

        internal ThreadState PushStackFrame(int parameterCount)
        {
            var Base = _stack.StackPointer - parameterCount;

            if (Base == 2)
                _stack.BasePointer = Base;

            var fnVar = _stack.PeekAbs(Base - 1);
            if (fnVar.Type != VariableType.Function)
            {
                Machine.Log.LogEntry("Attempted to call non-function type");
                return ThreadState.Exception;
            }

            var func = fnVar.GetFunction();

            if (func.Native != null)
            {
                var lastBase = _stack.BasePointer;
                var lastTop = _stack.StackPointer;
                _stack.BasePointer = Base;
                ParameterCount = parameterCount;

                var res = func.Native(this);

                if (lastTop == _stack.StackPointer)
                    _stack.PokeBase(-2, Variable.Null);
                else
                    _stack.PokeBase(-2, _stack.Peek(-1));

                // return stack
                _stack.StackPointer = _stack.BasePointer - 1;
                _stack.BasePointer = lastBase;

                switch (res)
                {
                    case FunctionResult.Ok:
                    {
                        break;
                    }
                    case FunctionResult.SysBlock:
                    {
                        // Todo: Sort out return addr
                        Machine.SwitchThreadState(this, ThreadState.Blocked);
                        return ThreadState.Blocked;
                    }
                    case FunctionResult.SysYield:
                    {
                        // Todo: Remove signals
                        // Todo: Sort out return addr
                        return ThreadState.SysYield;
                    }
                    case FunctionResult.SysSleep:
                    {
                        // Todo: Sort out return addr
                        Machine.SwitchThreadState(this, ThreadState.Sleeping);
                        return ThreadState.Sleeping;
                    }
                    case FunctionResult.SysKill:
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

            var frame = new StackFrame {ReturnBase = _stack.BasePointer, InstructionPtr = _instructionPtr};
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
    }
}