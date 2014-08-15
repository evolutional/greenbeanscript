using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace GreenBeanScript
{

    internal struct gmlHeader
    {
        public Int32 id;
        public Int32 flags;
        public Int32 stOffset;
        public Int32 scOffset;
        public Int32 fnOffset;
    };

    internal struct gmlStrings
    {
        public UInt32 size;
    };

    internal struct gmlSource
    {
        public UInt32 size;
        public UInt32 flags;
    };

    internal struct gmlFunction
    {
        public UInt32 func;
        public UInt32 id;
        public UInt32 flags;
        public UInt32 numParams;
        public UInt32 numLocals;
        public UInt32 maxStackSize;
        public UInt32 byteCodeLen;
    };

    internal struct gmlLineInfo
    {
        public UInt32 byteCoderAddress;
        public UInt32 lineNumber;
    };

    public class Library
    {
        protected int _MainFunctionId;
        protected List<FunctionObject> _Functions;

        public List<FunctionObject> Functions
        {
            get { return _Functions; }
        }

        public FunctionObject MainFunction
        {
            get { return _Functions[_MainFunctionId]; }
        }

        public int MainFunctionId
        {
            get { return _MainFunctionId; }
        }

        public bool Load(Machine Machine, string FileName)
        {
            BinaryReader GmLibData = new BinaryReader(File.OpenRead(FileName));
            gmlHeader header = new gmlHeader();
            gmlStrings strings = new gmlStrings();
            gmlSource source = new gmlSource();
            gmlFunction function;

            List<FunctionObject> functionObjects = new List<FunctionObject>();

            UInt32 numFunctions = 0;

            /// Load Header
            header.id = GmLibData.ReadInt32();
            header.flags = GmLibData.ReadInt32();
            header.stOffset = GmLibData.ReadInt32();
            header.scOffset = GmLibData.ReadInt32();
            header.fnOffset = GmLibData.ReadInt32();

            /// Load string table
            strings.size = GmLibData.ReadUInt32();
            char[] stringTable = GmLibData.ReadChars((int)strings.size);
            string[] st = new string[strings.size];
            Dictionary<uint, uint> stringTabOffset = new Dictionary<uint, uint>();
            StringBuilder sb = new StringBuilder();
            uint stringid = 0;
            uint li = 0;
            for (uint i = 0; i < strings.size - 1; ++i)
            {
                if (stringTable[i] == 0)
                {
                    st[stringid] = sb.ToString();
                    stringTabOffset.Add(li, stringid); li = i+1;
                    sb = new StringBuilder();
                    ++stringid;
                }
                else
                {
                    sb.Append(stringTable[i]);
                }
            }
            st[stringid] = sb.ToString();
            stringTabOffset.Add(li, stringid);
            Debug.Assert(header.scOffset == 0); // Can't handle included sourcecode for now
            
            // Jump to offset of functions
            GmLibData.BaseStream.Seek(header.fnOffset, SeekOrigin.Begin);
            // read number of functions
            numFunctions = GmLibData.ReadUInt32();

            for (int i = 0; i < numFunctions; ++i)
            {
                // Read out function entry
                function = new gmlFunction();
                function.func = GmLibData.ReadUInt32();
                function.id = GmLibData.ReadUInt32();
                function.flags = GmLibData.ReadUInt32();
                function.numParams = GmLibData.ReadUInt32();
                function.numLocals = GmLibData.ReadUInt32();
                function.maxStackSize = GmLibData.ReadUInt32();
                function.byteCodeLen = GmLibData.ReadUInt32();

                if (function.flags == 1)
                    _MainFunctionId = i;

                byte[] bytecode = GmLibData.ReadBytes((int)function.byteCodeLen);
                uint end = function.byteCodeLen;
                List<ByteCode.Instruction> InstructionList = new List<ByteCode.Instruction>();

                for (int instruction = 0; instruction < end; )
                {
                    ByteCode.Operator instr = (ByteCode.Operator)bytecode[instruction];
                    int bytecodeoffset = instruction;
                    instruction += 4;
                    switch (instr)
                    {
                        #region Operators
                        case ByteCode.Operator.OpAdd:
                        case ByteCode.Operator.OpDiv:
                        case ByteCode.Operator.OpEq:
                        case ByteCode.Operator.OpGt:
                        case ByteCode.Operator.OpGte:
                        case ByteCode.Operator.OpLt:
                        case ByteCode.Operator.OpLte:
                        case ByteCode.Operator.OpMul:
                        case ByteCode.Operator.OpNeg:
                        case ByteCode.Operator.OpSub:
                        case ByteCode.Operator.SetInd:
                        case ByteCode.Operator.GetInd:
                            {
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset));
                                break;
                            }
                        case ByteCode.Operator.SetDot:
                        case ByteCode.Operator.GetDot:
                            {
                                uint reference = System.BitConverter.ToUInt32(bytecode, instruction);
                                instruction += sizeof(uint);
                                uint offset = stringTabOffset[reference];
                                string s = st[offset];
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable(s) }));
                                break;
                            }
                        #endregion
                        case ByteCode.Operator.Ret:
                        case ByteCode.Operator.Retv:
                        case ByteCode.Operator.Dup:
                        case ByteCode.Operator.Pop:
                        case ByteCode.Operator.Pop2:
                            {
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset));
                                break;
                            }

                        case ByteCode.Operator.ForEach:
                            {
                                Int16 localref = System.BitConverter.ToInt16(bytecode, instruction);
                                instruction += sizeof(Int16);
                                Int16 localkey = System.BitConverter.ToInt16(bytecode, instruction);
                                instruction += sizeof(Int16);
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable((int)localref), new Variable((int)localkey) }));
                                break;
                            }
                        
                        case ByteCode.Operator.Call:
                            {
                                int numParams = BitConverter.ToInt32(bytecode, instruction);
                                instruction += sizeof(int);

                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable(numParams) }));
                                break;
                            }
                        case ByteCode.Operator.GetLocal:
                        case ByteCode.Operator.SetLocal:
                            {
                                uint reference = System.BitConverter.ToUInt32(bytecode, instruction);
                                instruction += sizeof(uint);
                                uint offset = reference;
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable((int)offset) }));
                                break;
                            }

                        case ByteCode.Operator.GetGlobal:
                        case ByteCode.Operator.SetGlobal:
                            {
                                uint reference = BitConverter.ToUInt32(bytecode, instruction);
                                instruction += sizeof(int);
                                uint offset = stringTabOffset[reference];
                                string s = st[offset];
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable(s) }));
                                break;
                            }
                        #region Branch
                        case ByteCode.Operator.Brz:
                        case ByteCode.Operator.Bra:
                            {
                                int offset = BitConverter.ToInt32(bytecode, instruction);
                                instruction += sizeof(int);
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable(offset) }));
                                break;
                            }
                        #endregion
                        #region Push Operators
                        case ByteCode.Operator.PushThis:
                        case ByteCode.Operator.PushNull:
                            {
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset));
                                break;
                            }
                        case ByteCode.Operator.PushInt0:
                            {
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable(0) }));
                                break;
                            }
                        case ByteCode.Operator.PushInt1:
                            {
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable(1) }));
                                break;
                            }
                        case ByteCode.Operator.PushInt:
                            {
                                int val = BitConverter.ToInt32(bytecode, instruction);
                                instruction += sizeof(int);

                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable(val) }));
                                break;
                            }
                        case ByteCode.Operator.PushFp:
                            {
                                float f = BitConverter.ToSingle(bytecode, instruction);
                                instruction += sizeof(float);

                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable(f) }));
                                break;
                            }
                        case ByteCode.Operator.PushTbl:
                            {
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset));//, new Variable[] { new Variable(Machine.CreateTable()) }));
                                break;
                            }
                        case ByteCode.Operator.PushStr:
                            {
                                uint reference = System.BitConverter.ToUInt32(bytecode, instruction);
                                instruction += sizeof(uint);
                                uint offset = stringTabOffset[reference];
                                string s = st[offset];
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable(s) }));
                                break;
                            }

                        case ByteCode.Operator.PushFn:
                            {
                                uint reference = System.BitConverter.ToUInt32(bytecode, instruction);
                                instruction += sizeof(uint);
                                uint offset = reference;
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable((int)offset) }));
                                break;
                            }
                        #endregion
                        default:
                            {
                                throw new Exception("Invalid function");
                            }

                    };  // end of switch

                }   // end of function loop


                functionObjects.Add( new FunctionObject() );
                functionObjects[i].Initialise(InstructionList, (int)function.numLocals, (int)function.numParams);
            }


            // Loop back over the newly loaded functions and set the functionobjects for each reference
            for (int funcid = 0; funcid < numFunctions; ++funcid)
            {
                ByteCode.Instruction[] InstructionList = functionObjects[funcid].Instructions;
                int InstructionId = 0;
                foreach (ByteCode.Instruction instruction in InstructionList)
                {
                    if (instruction.OpCode == ByteCode.Operator.PushFn)
                    {
                        InstructionList[InstructionId].SetOperand(0, new Variable(functionObjects[instruction[0].GetInteger()]));
                    }
                    else if ((instruction.OpCode == ByteCode.Operator.Bra) || (instruction.OpCode == ByteCode.Operator.Brz))
                    {
                        // Loop through and find the 
                        for (int INum = 0; INum < InstructionList.Length; ++INum )
                        {
                            if (InstructionList[INum].ByteCodeOffset == instruction[0].GetInteger())
                            {
                                InstructionList[InstructionId].SetOperand(0, new Variable(INum));
                                break;
                            }
                        }
                    }

                    ++InstructionId;
                }

            }

            _Functions = functionObjects;
            GmLibData.Close();
            return true;
        }

    }
}
