namespace GreenBeanScript.VirtualMachine
{
    public static class ThreadExtensions
    {
        public static void PushNull(this Thread thread)
        {
            thread.Push(Variable.Null);
        }

        public static void PushInteger(this Thread thread, int value)
        {
            thread.Push(value);
        }

        public static void PushFloat(this Thread thread, float value)
        {
            thread.Push(value);
        }

        public static void PushString(this Thread thread, string value)
        {
            thread.Push(value);
        }

        public static void PushFunction(this Thread thread, FunctionObject function)
        {
            thread.Push(function);
        }

        public static void PushTable(this Thread thread, TableObject table)
        {
            thread.Push(table);
        }
    }
}