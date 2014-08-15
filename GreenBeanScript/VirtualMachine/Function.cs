using System;
using System.Collections.Generic;
using System.Text;

namespace GreenBeanScript
{
    public enum FunctionResult : int
    {
        Ok = 0,
        Exception = -1,
        // These are system only
        Sys_Yield = -2,
        Sys_Block = -3,
        Sys_Sleep = -4,
        Sys_Kill = -5,
        Sys_State = -6,
    }
    public delegate FunctionResult NativeFunctionCallback(Thread ScriptThread);

}
