namespace GreenBeanScript.VirtualMachine
{
    public delegate int TypeIteratorCallback(
        Thread scriptThread, object Object, int iteratorPosition, Variable key, Variable item);
}