using System;
using System.Collections.Generic;
using GreenBeanScript.Libs;
using GreenBeanScript.VirtualMachine.Operations;

namespace GreenBeanScript.VirtualMachine
{
    public class Machine
    {
        //protected Dictionary<int, ScriptType> _TypeIdLookups = new Dictionary<int, ScriptType>();
        private readonly ScriptTypeCollection _typeIdLookups = new ScriptTypeCollection();
        protected List<Thread> BlockedThreads = new List<Thread>();
        protected List<Thread> ExceptionThreads = new List<Thread>();
        protected TableObject _Globals;
        protected List<Thread> KilledThreads = new List<Thread>();
        //protected Stack<Variable> _Stack = new Stack<Variable>();
        protected Log _Log;

        protected int NextThreadId = 1;
        protected int NextTypeId;

        protected List<Thread> RunningThreads = new List<Thread>();
        protected List<Thread> SleepingThreads = new List<Thread>();

        protected Dictionary<string, ScriptType> TypeNameLookups = new Dictionary<string, ScriptType>();
        // Add the GreenBean Standard Lib
        private StdLibrary _standardLibary;


        public Machine(StdLibrary stdlibrary)
        {
            NextTypeId = (int) VariableType.UserType;
            _Log = new Log();
            _Globals = new TableObject();
            //
            InitialiseDefaultTypes();


            // Bind standard library to machine
            _standardLibary = stdlibrary;
            stdlibrary.RegisterLibrary(this);
        }

        public Machine() : this(new StdLibrary())
        {
        }

        public TableObject Globals
        {
            get { return _Globals; }
        }

        public Log Log
        {
            get { return _Log; }
        }


        public void SetGlobal(string globalName, Variable value)
        {
            _Globals[new Variable(globalName)] = value;
        }

        public void SetGlobal(int globalId, Variable value)
        {
            _Globals[globalId] = value;
        }

        public int SetBlocks(Thread scriptThread, Variable[] blocks)
        {
            // TODO:

            return 0;
        }

        internal void SwitchThreadState(Thread scriptThread, ThreadState state)
        {
            // Return immediately if they're the same state
            if (scriptThread.State == state)
                return;
            switch (scriptThread.State)
            {
                case ThreadState.Running:
                {
                    // Todo: Remove signals
                    // If this thread is running, remove from running list
                    RunningThreads.Remove(scriptThread);
                    break;
                }
                case ThreadState.Blocked:
                case ThreadState.SysPending:
                {
                    // Todo: Remove blocks
                    BlockedThreads.Remove(scriptThread);
                    break;
                }
                case ThreadState.Sleeping:
                {
                    SleepingThreads.Remove(scriptThread);
                    break;
                }
                case ThreadState.Killed:
                {
                    KilledThreads.Remove(scriptThread);
                    break;
                }
                case ThreadState.Exception:
                {
                    ExceptionThreads.Remove(scriptThread);
                    break;
                }
                default:
                {
                    throw new Exception("TODO: This shouldn't happen");
                }
            }

            switch (state)
            {
                case ThreadState.Running:
                {
                    RunningThreads.Add(scriptThread);
                    break;
                }
                case ThreadState.Blocked:
                {
                    BlockedThreads.Add(scriptThread);
                    break;
                }
                case ThreadState.Sleeping:
                {
                    SleepingThreads.Add(scriptThread);
                    break;
                }
                case ThreadState.Killed:
                {
                    scriptThread.SetState(ThreadState.Killed);
                    // Todo: Machine thread killed callback
                    KilledThreads.Add(scriptThread);
                    return;
                }
                case ThreadState.Exception:
                {
                    ExceptionThreads.Add(scriptThread);
                    break;
                }
                default:
                {
                    throw new Exception("Invalid State");
                }
            }

            // Set the thread's internal state
            scriptThread.SetState(state);
        }

        public ScriptType RegisterType(string typeName)
        {
            if (TypeNameLookups.ContainsKey(typeName))
                throw new Exception("Type already registered");

            var type = new ScriptType(typeName, NextTypeId++);
            TypeNameLookups[typeName] = type;
            return type;
        }

        public OperatorCallback GetTypeOperator(int typeId, Operator op)
        {
            if (!_typeIdLookups.Contains(typeId))
                return null;

            return _typeIdLookups[typeId].GetOperator(op);
        }

        public TypeIteratorCallback GetTypeIterator(int typeId)
        {
            if (!_typeIdLookups.Contains(typeId))
                return null;

            return _typeIdLookups[typeId].GetIterator();
        }

        public void RegisterFunction(string functionName, NativeFunctionCallback function)
        {
            SetGlobal(functionName, new Variable(new FunctionObject(function)));
        }

        public void RegisterType(string typeName, int typeId, ITypeOperators operators)
        {
            var type = new ScriptType(typeName, typeId);
            operators.Initialise(this, type);
            _typeIdLookups.Add(type);
            TypeNameLookups[type.TypeName] = type;
        }

        protected void InitialiseDefaultTypes()
        {
            RegisterType("int", (int) VariableType.Integer, new IntegerOperators());
            RegisterType("float", (int) VariableType.Float, new FloatOperators());
            RegisterType("string", (int) VariableType.String, new StringOperators());
            RegisterType("table", (int) VariableType.Table, new TableOperators());
        }

        #region Create Object Methods

        #region Thread Functions

        /// <summary>
        ///     Creates a basic thread
        /// </summary>
        /// <returns></returns>
        public Thread CreateThread()
        {
            var newThread = new Thread(NextThreadId++, this);
            RunningThreads.Add(newThread);
            return newThread;
        }

        /// <summary>
        ///     Creates a thread with no parameters to the main function
        /// </summary>
        /// <param name="threadFunction"></param>
        /// <param name="This"></param>
        /// <returns></returns>
        public Thread CreateThread(FunctionObject threadFunction, Variable This)
        {
            var newThread = new Thread(NextThreadId++, this, threadFunction);
            // TODO: Notify of thread creation
            RunningThreads.Add(newThread);
            newThread.Push(This); // push this
            newThread.PushFunction(threadFunction);
            newThread.PushStackFrame(0);
            return newThread;
        }

        /// <summary>
        ///     Creates a thread with no parameters to the main function and Null passed as This
        /// </summary>
        /// <param name="threadFunction"></param>
        /// <returns></returns>
        public Thread CreateThread(FunctionObject threadFunction)
        {
            return CreateThread(threadFunction, new Variable());
        }

        #endregion

        #endregion

        #region Execute Methods

        public void ExecuteLibrary(Library lib)
        {
            var thread = CreateThread(lib.MainFunction);
            thread.Execute();
        }

        public int Execute(float delta)
        {
            // Todo: Handle waking up threads

            // Todo: Handle moving pending blocked threads to new threads

            // Todo: Run each running thread
            for (var i = 0; i < RunningThreads.Count; ++i)
            {
                var t = RunningThreads[i];
                t.Execute();
            }

            return 0;
        }

        #endregion
    }
}