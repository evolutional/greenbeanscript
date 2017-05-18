namespace GreenBeanScript.VirtualMachine.Operations
{
    public delegate Variable OperatorCallback(
        Thread scriptThread, Variable operand0, Variable operand1, Variable operand2);
}