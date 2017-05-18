namespace GreenBeanScript.VirtualMachine
{
    public enum ThreadState
    {
        Running,
        Sleeping,
        Blocked,
        Killed,
        Exception,

        SysPending,
        SysYield,
        SysException
    }
}