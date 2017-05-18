namespace GreenBeanScript.VirtualMachine
{
    public enum FunctionResult
    {
        Ok = 0,
        Exception = -1,
        // These are system only
        SysYield = -2,
        SysBlock = -3,
        SysSleep = -4,
        SysKill = -5,
        SysState = -6
    }
}