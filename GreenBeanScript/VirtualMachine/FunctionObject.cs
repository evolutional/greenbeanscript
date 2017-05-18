using System.Collections.Generic;
using GreenBeanScript.VirtualMachine.ByteCode;

namespace GreenBeanScript.VirtualMachine
{
    public class FunctionObject
    {
        private int _numLocals;

        internal FunctionObject()
        {
        }

        internal FunctionObject(NativeFunctionCallback native)
        {
            this.Native = native;
        }

        public int NumParams { get; private set; }

        public int NumParamsLocals { get; private set; }

        internal List<Instruction> Instructions { get; private set; }

        public NativeFunctionCallback Native { get; }

        internal void Initialise(List<Instruction> instructions, int numLocals, int numParameters)
        {
            this.Instructions = instructions;

            _numLocals = numLocals;
            NumParams = numParameters;
            NumParamsLocals = _numLocals + NumParams;
        }
    }
}