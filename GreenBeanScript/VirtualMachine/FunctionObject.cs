using System;
using System.Collections.Generic;
using System.Text;



namespace GreenBeanScript
{
    public class FunctionObject
    {
        private int _NumLocals = 0;
        private int _NumParams = 0;
        private int _NumParamsLocals = 0;

        private List<ByteCode.Instruction> _Instructions;
        private NativeFunctionCallback _NativeCallback;

        public int NumParams
        {
            get { return _NumParams; }
        }

        public int NumParamsLocals
        {
            get { return _NumParamsLocals; }
        }

        internal List<ByteCode.Instruction> Instructions
        {
            get { return _Instructions; }
        }

        public NativeFunctionCallback Native
        {
            get { return _NativeCallback; }
        }

        internal FunctionObject()
        {
        }

        internal FunctionObject(NativeFunctionCallback Native)
        {
            _NativeCallback = Native;
        }

        internal void Initialise(List<ByteCode.Instruction> Instructions, int NumLocals, int NumParameters)
        {
            _Instructions = Instructions;

            _NumLocals = NumLocals;
            _NumParams = NumParameters;
            _NumParamsLocals = _NumLocals + _NumParams;

        }



    }
}
