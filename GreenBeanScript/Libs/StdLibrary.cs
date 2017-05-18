using System;
using System.Text;
using GreenBeanScript.VirtualMachine;

namespace GreenBeanScript.Libs
{
    /// <summary>
    ///     The GreenBean Standard library contains the standard base functions available in the default GB environment
    /// </summary>
    public class StdLibrary
    {
        private static DateTime _tickTime = DateTime.Now;

        private readonly Action<string> _printCallback;

        public StdLibrary()
        {
            _printCallback = Console.WriteLine;
        }

        public StdLibrary(Action<string> printcallback)
        {
            _printCallback = printcallback;
        }

        /// <summary>
        ///     Used to return the number of ticks since the last check
        /// </summary>
        /// <param name="scriptThread"></param>
        /// <returns></returns>
        protected FunctionResult Tick(Thread scriptThread)
        {
            var lastTick = _tickTime;
            _tickTime = DateTime.Now;
            var d = _tickTime.Subtract(lastTick);
            scriptThread.PushInteger(d.Minutes*60 + d.Seconds);
            return FunctionResult.Ok;
        }

        /// <summary>
        ///     Prints output to the console (will change)
        /// </summary>
        /// <param name="scriptThread"></param>
        /// <returns></returns>
        protected FunctionResult PrintFunction(Thread scriptThread)
        {
            var sb = new StringBuilder();
            for (var paramid = 0; paramid < scriptThread.ParameterCount; ++paramid)
            {
                var p = scriptThread.Param(paramid);

                sb.Append(p);

                if (paramid < scriptThread.ParameterCount - 1)
                    sb.Append(" ");
            }

            _printCallback(sb.ToString());
            return FunctionResult.Ok;
        }

        protected FunctionResult FormatFunction(Thread scriptThread)
        {
            if (scriptThread.ParameterCount < 1)
                return FunctionResult.Exception;

            var format = scriptThread.Param(0).GetString();
            var sb = new StringBuilder();
            var param = 1;
            var charnum = 0;

            while (charnum < format.Length)
            {
                var c = format[charnum];

                if (c == '%')
                {
                    var c1 = format[charnum + 1];
                    switch (c1)
                    {
                        case 'X':
                        case 'x':
                        {
                            if (!scriptThread.Param(param).IsInt)
                                return FunctionResult.Exception;
                            sb.AppendFormat("{0:X}", scriptThread.Param(param).GetIntegerNoCheck());
                            ++param;
                            break;
                        }
                    }

                    charnum += 2;
                }
                else
                {
                    sb.Append(c);
                    ++charnum;
                }
            }

            scriptThread.PushString(sb.ToString());

            return FunctionResult.Ok;
        }

        /// <summary>
        ///     Registers the library with the VM
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        public bool RegisterLibrary(Machine vm)
        {
            vm.RegisterFunction("print", PrintFunction);
            vm.RegisterFunction("format", FormatFunction);


            vm.RegisterFunction("thread", ThreadFunction);
            vm.RegisterFunction("block", BlockFunction);

            vm.RegisterFunction("TICK", Tick);
            return true;
        }

        #region Thread Library

        /// <summary>
        ///     Registers the "thread" function which creates a VM thread
        /// </summary>
        /// <param name="scriptThread"></param>
        /// <returns></returns>
        protected FunctionResult ThreadFunction(Thread scriptThread)
        {
            if (scriptThread.ParameterCount != 1)
            {
                scriptThread.LogException("Expecting 1 parameter");
                return FunctionResult.Exception;
            }

            var func = scriptThread.Param(0).GetFunction();

            if (func == null)
            {
                scriptThread.LogException("Expecting parameter 1 as function");
                return FunctionResult.Exception;
            }
            // Create a GB VM thread
            var thread = scriptThread.Machine.CreateThread();
            if (thread != null)
            {
                thread.Push(scriptThread.This);
                thread.PushFunction(func);
                for (var i = 0; i < scriptThread.ParameterCount - 1; ++i)
                    thread.Push(scriptThread.Param(i));
                thread.PushStackFrame(scriptThread.ParameterCount - 1);
            }
            else
            {
                scriptThread.LogException("Error creating new thread");
                return FunctionResult.Exception;
            }
            // Return Id to script
            scriptThread.PushInteger(thread.Id);
            return FunctionResult.Ok;
        }

        /// <summary>
        ///     Blocks this thread on the specified variables
        /// </summary>
        /// <param name="scriptThread"></param>
        /// <returns></returns>
        protected FunctionResult BlockFunction(Thread scriptThread)
        {
            if (scriptThread.ParameterCount != 0)
            {
                scriptThread.LogException("Expected 1 or more parameter");
                return FunctionResult.Exception;
            }

            var blockList = new Variable[scriptThread.ParameterCount];
            for (var p = 0; p < scriptThread.ParameterCount; ++p)
                blockList[p] = scriptThread.Param(p);

            var res = scriptThread.Machine.SetBlocks(scriptThread, blockList);

            if (res == -1)
                return FunctionResult.SysBlock;
            if (res == -2)
                return FunctionResult.SysYield;
            // One of the blocks has a corresonding signal, return which one
            scriptThread.Push(blockList[res]);
            return FunctionResult.Ok;
        }

        /// <summary>
        ///     The signal() callback to send a signal to a thread
        /// </summary>
        /// <param name="scriptThread"></param>
        /// <returns></returns>
        protected FunctionResult SignalFunction(Thread scriptThread)
        {
            // TODO: Implement!
            return FunctionResult.Exception;
        }

        #endregion
    }
}