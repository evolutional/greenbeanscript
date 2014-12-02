namespace GreenBeanScript
{
    public static class ThreadExtensions
    {
        public static void PushNull(this Thread thread)
        {
            thread.Push(Variable.Null);
        }

        public static void PushInteger(this Thread thread, int Value)
        {
            thread.Push(Value);
        }

        public static void PushFloat(this Thread thread, float Value)
        {
            thread.Push(Value);
        }

        public static void PushString(this Thread thread, string Value)
        {
            thread.Push(Value);
        }

        public static void PushFunction(this Thread thread, FunctionObject Function)
        {
            thread.Push(Function);
        }

        public static void PushTable(this Thread thread, TableObject Table)
        {
            thread.Push(Table);
        }
    }
}