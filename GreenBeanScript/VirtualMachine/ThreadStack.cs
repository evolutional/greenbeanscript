namespace GreenBeanScript.VirtualMachine
{
    public class ThreadStack
    {
        private readonly Variable[] _stack = new Variable[16384]; // Stack


        public int StackPointer { get; set; }

        public int BasePointer { get; set; }

        public void Push(Variable var)
        {
            _stack[StackPointer++] = var;
        }

        public Variable Pop()
        {
            return _stack[--StackPointer];
        }

        public Variable Pop(int num)
        {
            StackPointer -= num;
            return Peek();
        }

        public void Poke(Variable var)
        {
            _stack[StackPointer] = var;
        }

        public Variable Peek()
        {
            return _stack[StackPointer];
        }

        public void PokeAbs(int offset, Variable val)
        {
            _stack[offset] = val;
        }

        public Variable PeekAbs(int offset)
        {
            return _stack[offset];
        }

        public void Poke(int offsetFromTop, Variable val)
        {
            _stack[StackPointer + offsetFromTop] = val;
        }

        public Variable Peek(int offsetFromTop)
        {
            return _stack[StackPointer + offsetFromTop];
        }

        public Variable PeekBase(int offsetFromBase)
        {
            return _stack[BasePointer + offsetFromBase];
        }

        public void PokeBase(int offsetFromBase, Variable val)
        {
            _stack[BasePointer + offsetFromBase] = val;
        }
    }
}