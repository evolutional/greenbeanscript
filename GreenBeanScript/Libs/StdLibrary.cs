﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GreenBeanScript.Libs
{
    /// <summary>
    /// The GreenBean Standard library contains the standard base functions available in the default GB environment
    /// </summary>
    public class StdLibrary
    {
        static DateTime TickTime = DateTime.Now;

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
        /// Used to return the number of ticks since the last check
        /// </summary>
        /// <param name="ScriptThread"></param>
        /// <returns></returns>
        protected FunctionResult Tick(Thread ScriptThread)
        {
            DateTime lastTick = TickTime;
            TickTime = System.DateTime.Now;
            TimeSpan d = TickTime.Subtract(lastTick);
            ScriptThread.PushInteger((d.Minutes*60)+d.Seconds);
            return FunctionResult.Ok;
        }

        /// <summary>
        /// Prints output to the console (will change)
        /// </summary>
        /// <param name="ScriptThread"></param>
        /// <returns></returns>
        protected FunctionResult PrintFunction(Thread ScriptThread)
        {
            var sb = new StringBuilder();
            for (int paramid = 0; paramid < ScriptThread.ParameterCount; ++paramid)
            {
                Variable p = ScriptThread.Param(paramid);

                sb.Append(p.ToString());

                if (paramid < ScriptThread.ParameterCount - 1)
                    sb.Append(" ");
            }

            _printCallback(sb.ToString());
            return FunctionResult.Ok;
        }

        protected FunctionResult FormatFunction(Thread ScriptThread)
        {
            if (ScriptThread.ParameterCount < 1)
            {
                return FunctionResult.Exception;
            }

            var format = ScriptThread.Param(0).GetString();
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
                            if (!ScriptThread.Param(param).IsInt)
                            {
                                return FunctionResult.Exception;
                            }
                            sb.AppendFormat("{0:X}", ScriptThread.Param(param).GetIntegerNoCheck());
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
        
            ScriptThread.PushString(sb.ToString());

            return FunctionResult.Ok;
        }

        #region Thread Library
        /// <summary>
        /// Registers the "thread" function which creates a VM thread
        /// </summary>
        /// <param name="ScriptThread"></param>
        /// <returns></returns>
        protected FunctionResult ThreadFunction(Thread ScriptThread)
        {
            if (ScriptThread.ParameterCount != 1)
            {
                ScriptThread.LogException("Expecting 1 parameter");
                return FunctionResult.Exception;
            }

            FunctionObject Func = ScriptThread.Param(0).GetFunction();

            if (Func == null)
            {
                ScriptThread.LogException("Expecting parameter 1 as function");
                return FunctionResult.Exception;
            }
            // Create a GB VM thread
            Thread thread = ScriptThread.Machine.CreateThread();
            if (thread != null)
            {
                thread.Push(ScriptThread.This);
                thread.PushFunction(Func);
                for (int i = 0; i < ScriptThread.ParameterCount - 1; ++i)
                {
                    thread.Push(ScriptThread.Param(i));
                }
                thread.PushStackFrame(ScriptThread.ParameterCount - 1);
            }
            else
            {
                ScriptThread.LogException("Error creating new thread");
                return FunctionResult.Exception;
            }
            // Return Id to script
            ScriptThread.PushInteger(thread.Id);
            return FunctionResult.Ok;
        }

        /// <summary>
        /// Blocks this thread on the specified variables
        /// </summary>
        /// <param name="ScriptThread"></param>
        /// <returns></returns>
        protected FunctionResult BlockFunction(Thread ScriptThread)
        {
            if (ScriptThread.ParameterCount != 0)
            {
                ScriptThread.LogException("Expected 1 or more parameter");
                return FunctionResult.Exception;
            }

            Variable[] BlockList = new Variable[ScriptThread.ParameterCount];
            for (int p = 0; p < ScriptThread.ParameterCount; ++p)
            {
                BlockList[p] = ScriptThread.Param(p);
            }

            int res = ScriptThread.Machine.SetBlocks(ScriptThread, BlockList);

            if (res == -1)
            {
                return FunctionResult.Sys_Block;
            }
            else if (res == -2)
            {
                return FunctionResult.Sys_Yield;
            }
            // One of the blocks has a corresonding signal, return which one
            ScriptThread.Push(BlockList[res]);
            return FunctionResult.Ok;
        }

        /// <summary>
        /// The signal() callback to send a signal to a thread
        /// </summary>
        /// <param name="ScriptThread"></param>
        /// <returns></returns>
        protected FunctionResult SignalFunction(Thread ScriptThread)
        {
            // TODO: Implement!
            return FunctionResult.Exception;
        }
        #endregion

        /// <summary>
        /// Registers the library with the VM
        /// </summary>
        /// <param name="Vm"></param>
        /// <returns></returns>
        public bool RegisterLibrary(Machine Vm)
        {
            Vm.RegisterFunction("print", PrintFunction);
            Vm.RegisterFunction("format", FormatFunction);


            Vm.RegisterFunction("thread", ThreadFunction);
            Vm.RegisterFunction("block", BlockFunction);

            Vm.RegisterFunction("TICK", Tick);
            return true;
        }

    }
}
