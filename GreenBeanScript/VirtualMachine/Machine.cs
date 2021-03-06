﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using GreenBeanScript.Libs;

namespace GreenBeanScript
{
    public class Machine
    {
        // Add the GreenBean Standard Lib
        private StdLibrary StandardLibary;


        public Machine(StdLibrary stdlibrary)
        {
            _NextTypeId = (int)VariableType.UserType;
            _Log = new Log();
            _Globals = new TableObject();
            //
            InitialiseDefaultTypes();


            // Bind standard library to machine
            StandardLibary = stdlibrary;
            stdlibrary.RegisterLibrary(this);

        }
        public Machine() : this(new StdLibrary())
        {
        }

        #region Create Object Methods

        #region Thread Functions
        /// <summary>
        /// Creates a basic thread
        /// </summary>
        /// <returns></returns>
        public Thread CreateThread()
        {
            Thread newThread = new Thread(_NextThreadId++, this);
            _RunningThreads.Add(newThread);
            return newThread;
        }

        /// <summary>
        /// Creates a thread with no parameters to the main function
        /// </summary>
        /// <param name="threadFunction"></param>
        /// <param name="This"></param>
        /// <returns></returns>
        public Thread CreateThread(FunctionObject threadFunction, Variable This)
        {
            Thread newThread = new Thread(_NextThreadId++, this, threadFunction);
            // TODO: Notify of thread creation
            _RunningThreads.Add(newThread);
            newThread.Push(This);// push this
            newThread.PushFunction(threadFunction);
            newThread.PushStackFrame(0);
            return newThread;
        }
        /// <summary>
        /// Creates a thread with no parameters to the main function and Null passed as This
        /// </summary>
        /// <param name="threadFunction"></param>
        /// <returns></returns>
        public Thread CreateThread(FunctionObject threadFunction)
        {
            return CreateThread(threadFunction, new Variable());
        }
        #endregion
        #endregion



        public void SetGlobal(string globalName, Variable value)
        {
            _Globals[new Variable(globalName)] = value;
        }

        public void SetGlobal(int globalId, Variable value)
        {
           _Globals[globalId] = value;
        }

        public TableObject Globals
        {
            get { return _Globals; }
        }

        public Log Log
        {
            get { return _Log; }
        }


        #region Execute Methods
        public void ExecuteLibrary(Library Lib)
        {
            GreenBeanScript.Thread thread = CreateThread(Lib.MainFunction);
            thread.Execute();
        }

        public int Execute(float Delta)
        {
            // Todo: Handle waking up threads

            // Todo: Handle moving pending blocked threads to new threads

            // Todo: Run each running thread
            for (int i = 0; i < _RunningThreads.Count; ++i)
            {
                Thread t = _RunningThreads[i];
                t.Execute();
            }

            return 0;
        }
        #endregion

        public int SetBlocks(Thread ScriptThread, Variable[] Blocks)
        {
            // TODO:

            return 0;
        }

        internal void SwitchThreadState(Thread ScriptThread, ThreadState State)
        {
            // Return immediately if they're the same state
            if (ScriptThread.State == State)
                return;
            switch (ScriptThread.State)
            {
                case ThreadState.Running:
                    {
                        // Todo: Remove signals
                        // If this thread is running, remove from running list
                        _RunningThreads.Remove(ScriptThread);
                        break;
                    }
                case ThreadState.Blocked:
                case ThreadState.Sys_Pending:
                    {
                        // Todo: Remove blocks
                        _BlockedThreads.Remove(ScriptThread);
                        break;
                    }
                case ThreadState.Sleeping:
                    {
                        _SleepingThreads.Remove(ScriptThread);
                        break;
                    }
                case ThreadState.Killed:
                    {
                        _KilledThreads.Remove(ScriptThread);
                        break;
                    }
                case ThreadState.Exception:
                    {
                        _ExceptionThreads.Remove(ScriptThread);
                        break;
                    }
                default:
                    {
                        throw new Exception("TODO: This shouldn't happen");
                    }
                }

            switch (State)
            {
                case ThreadState.Running:
                    {
                        _RunningThreads.Add(ScriptThread);
                        break;
                    }
                case ThreadState.Blocked:
                    {
                        _BlockedThreads.Add(ScriptThread);
                        break;
                    }
                case ThreadState.Sleeping:
                    {
                        _SleepingThreads.Add(ScriptThread);
                        break;
                    }
                case ThreadState.Killed:
                    {
                        ScriptThread.SetState(ThreadState.Killed);
                        // Todo: Machine thread killed callback
                        _KilledThreads.Add(ScriptThread);
                        return;
                    }
                case ThreadState.Exception:
                    {
                        _ExceptionThreads.Add(ScriptThread);
                        break;
                    }
                default:
                    {
                        throw new Exception("Invalid State");
                    }
            }

            // Set the thread's internal state
            ScriptThread.SetState(State);
        }

        public ScriptType RegisterType(string TypeName)
        {
            if (_TypeNameLookups.ContainsKey(TypeName))
            {
                throw new Exception("Type already registered");
            }

            ScriptType type = new ScriptType(TypeName, _NextTypeId++);
            _TypeNameLookups[TypeName] = type;
            return type;
        }

        public OperatorCallback GetTypeOperator(int TypeId, Operator Op)
        {
            if (!_typeIdLookups.Contains(TypeId))
                return null;

            return _typeIdLookups[TypeId].GetOperator(Op);
        }

        public TypeIteratorCallback GetTypeIterator(int TypeId)
        {
            if (!_typeIdLookups.Contains(TypeId))
                return null;

            return _typeIdLookups[TypeId].GetIterator();
        }

        public void RegisterFunction(string FunctionName, NativeFunctionCallback Function)
        {
            SetGlobal(FunctionName, new Variable(new FunctionObject(Function)));
        }

        public void RegisterType(string TypeName, int TypeId, TypeOperators Operators)
        {
            ScriptType Type = new ScriptType(TypeName, TypeId);
            Operators.Initialise(this, Type);
            _typeIdLookups.Add(Type);
            _TypeNameLookups[Type.TypeName] = Type;
        }

        protected void InitialiseDefaultTypes()
        {
            RegisterType("int", (int)VariableType.Integer, new IntegerOperators.OpCallbacks());
            RegisterType("float", (int)VariableType.Float, new FloatOperators.OperatorCallbacks());
            RegisterType("string", (int)VariableType.String, new StringOperators.OperatorCallbacks());
            RegisterType("table", (int)VariableType.Table, new TableOperators.OperatorCallbacks());  
        }

        protected Dictionary<string, ScriptType> _TypeNameLookups = new Dictionary<string, ScriptType>();
        //protected Dictionary<int, ScriptType> _TypeIdLookups = new Dictionary<int, ScriptType>();
        readonly ScriptTypeCollection _typeIdLookups = new ScriptTypeCollection();

        protected List<Thread> _RunningThreads = new List<Thread>();
        protected List<Thread> _KilledThreads = new List<Thread>();
        protected List<Thread> _BlockedThreads = new List<Thread>();
        protected List<Thread> _ExceptionThreads = new List<Thread>();
        protected List<Thread> _SleepingThreads = new List<Thread>();

        protected int _NextThreadId = 1;
        protected int _NextTypeId;
        //protected Stack<Variable> _Stack = new Stack<Variable>();
        protected Log _Log;
        protected TableObject _Globals;
    }
}
