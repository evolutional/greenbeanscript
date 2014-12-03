using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace GreenBeanScript
{

    internal class gmlHeader
    {
        public Int32 id;
        public Int32 flags;
        public Int32 stOffset;
        public Int32 scOffset;
        public Int32 fnOffset;
    };


    internal class gmlFunction
    {
        public UInt32 func;
        public UInt32 id;
        public UInt32 flags;
        public UInt32 numParams;
        public UInt32 numLocals;
        public UInt32 maxStackSize;
        public UInt32 byteCodeLen;
    };

    internal class gmlLineInfo
    {
        public UInt32 byteCodeAddress;
        public UInt32 lineNumber;
    };


    internal class StringTableIem
    {
        public int Id { get; set; }
        public int Offset { get; set; }
        public string Value { get; set; }
    }

    internal class StringTable
    {
        private readonly Dictionary<int, StringTableIem> _offsets;
        private readonly Dictionary<int, StringTableIem> _strings;
        private int _dataSize;
        public StringTable()
        {
            _offsets = new Dictionary<int, StringTableIem>();
            _strings = new Dictionary<int, StringTableIem>();
        }

        public int Count { get { return _strings.Count; } }
        public int Size { get { return _dataSize; } }
        public void Add(StringTableIem stringTableIem)
        {
            _strings.Add(stringTableIem.Id, stringTableIem);
            _offsets.Add(stringTableIem.Offset, stringTableIem);
            _dataSize += stringTableIem.Value.Length + 1;
        }

        public StringTableIem GetByOffset(int offset)
        {
            return _offsets[offset];
        }
        
        public StringTableIem GetById(int id)
        {
            return _strings[id];
        }
    }

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

        public bool LoadFromFile(Machine Machine, string FileName)
        {
            return Load(Machine, File.OpenRead(FileName));
        }

        public void ListLibraryFromFile(StringBuilder log, string libFileName)
        {
            List(libFileName, File.OpenRead(libFileName), log);
        }

        public void List(string libFileName, Stream fileStream, StringBuilder log)
        {
            var data = new BinaryReader(fileStream);

            // Load Header
            var header = ReadHeader(data);

            var debug = (header.flags & 1);
            log.AppendFormat("GM LIBRARY {0}, DEBUG={1}\r\n\r\n", Path.GetFullPath(libFileName), debug);

            // Load string table
            var stringTable = ReadStringTable(data);

            log.AppendFormat("STRING TABLE, SIZE={0}\r\n" +
                             "===============================================================================\r\n", stringTable.Size.ToString().PadLeft(5));


            for (var i = 0; i < stringTable.Count; ++i)
            {
                var str = stringTable.GetById(i);
                // "%05d:%s\n", i+1,&stringTable[i+1]
                log.AppendFormat("{0:D5}:{1}\r\n", str.Offset, str.Value);
            }

            log.Append("\r\nNO SOURCE CODE IN LIB\r\n");

            // Jump to offset of functions
            data.BaseStream.Seek(header.fnOffset, SeekOrigin.Begin);
            // read number of functions
            var numFunctions = data.ReadUInt32();

            log.AppendFormat("FUNCTIONS, COUNT={0}\r\n" +
                             "===============================================================================\r\n\r\n", numFunctions);

            for (var i = 0; i < numFunctions; ++i)
            {
                ListFunction(data, log);
            }

            log.Append("END LIB\r\n");
        }

        private void ListFunction(BinaryReader data, StringBuilder log)
        {
            log.Append("===============================================================================\r\n");

            var function = ReadFunction(data);

            log.AppendFormat("FUNCTION ID {0}\r\n", function.id);
            log.AppendFormat("FLAGS {0}\r\n", function.flags);
            log.AppendFormat("NUM PARAMS {0}\r\n", function.numParams);
            log.AppendFormat("NUM LOCALS {0}\r\n", function.numLocals);
            log.AppendFormat("MAX STACK {0}\r\n", function.maxStackSize);
            log.AppendFormat("BYTE CODE SIZE {0}\r\n", function.byteCodeLen);

            var byteCode = ReadByteCode(data, (int)function.byteCodeLen);

            ListBytecode(byteCode, log);

            log.Append("\r\n\r\n");
        }

        private void ListBytecode(byte[] byteCode, StringBuilder log)
        {
            var instruction = 0;
            var start = 0;
            var end = byteCode.Length;

            while (instruction < end)
            {
                var opiptr = false;
                var opf32 = false;
                var opisymbol = false;
                var cp = "";

                int addr = instruction - start;

                var bc = BitConverter.ToUInt32(byteCode, instruction);

                switch ((ByteCode.Opcode)bc)
                {
                  case ByteCode.Opcode.Nop : cp = "nop"; break;
                  case ByteCode.Opcode.Line : cp = "line"; break;

                  case ByteCode.Opcode.GetDot : cp = "get dot"; opiptr = true; break;
                  case ByteCode.Opcode.SetDot : cp = "set dot"; opiptr = true; break;
                  case ByteCode.Opcode.GetInd : cp = "get index"; break;
                  case ByteCode.Opcode.SetInd : cp = "set index"; break;

                  case ByteCode.Opcode.Bra : cp = "bra"; opiptr = true; break;
                  case ByteCode.Opcode.Brz : cp = "brz"; opiptr = true; break;
                  case ByteCode.Opcode.Brnz : cp = "brnz"; opiptr = true; break;
                  case ByteCode.Opcode.Brzk : cp = "brzk"; opiptr = true; break;
                  case ByteCode.Opcode.Brnzk : cp = "brnzk"; opiptr = true; break;
                  case ByteCode.Opcode.Call : cp = "call"; opiptr = true; break;
                  case ByteCode.Opcode.Ret : cp = "ret"; break;
                  case ByteCode.Opcode.Retv : cp = "retv"; break;
                  case ByteCode.Opcode.ForEach : cp = "foreach"; opiptr = true; break;
      
                  case ByteCode.Opcode.Pop : cp = "pop"; break;
                  case ByteCode.Opcode.Pop2 : cp = "pop2"; break;
                  case ByteCode.Opcode.Dup : cp = "dup"; break;
                  case ByteCode.Opcode.Dup2 : cp = "dup2"; break;
                  case ByteCode.Opcode.Swap : cp = "swap"; break;
                  case ByteCode.Opcode.PushNull : cp = "push null"; break;
                  case ByteCode.Opcode.PushInt : cp = "push int"; opiptr = true; break;
                  case ByteCode.Opcode.PushInt0 : cp = "push int 0"; break;
                  case ByteCode.Opcode.PushInt1 : cp = "push int 1"; break;
                  case ByteCode.Opcode.PushFp : cp = "push fp"; opf32 = true; break;
                  case ByteCode.Opcode.PushStr : cp = "push str"; opiptr = true; break;
                  case ByteCode.Opcode.PushTbl : cp = "push tbl"; break;
                  case ByteCode.Opcode.PushFn : cp = "push fn"; opiptr = true; break;
                  case ByteCode.Opcode.PushThis : cp = "push this"; break;
      
                  case ByteCode.Opcode.GetLocal : cp = "get local"; opiptr = true; break;
                  case ByteCode.Opcode.SetLocal : cp = "set local"; opiptr = true; break;
                  case ByteCode.Opcode.GetGlobal : cp = "get global"; opiptr = true; break;
                  case ByteCode.Opcode.SetGlobal : cp = "set global"; opiptr = true; break;
                  case ByteCode.Opcode.GetThis : cp = "get this"; opiptr = true; break;
                  case ByteCode.Opcode.SetThis : cp = "set this"; opiptr = true; break;
      
                  case ByteCode.Opcode.OpAdd : cp = "add"; break;
                  case ByteCode.Opcode.OpSub : cp = "sub"; break;
                  case ByteCode.Opcode.OpMul : cp = "mul"; break;
                  case ByteCode.Opcode.OpDiv : cp = "div"; break;
                  case ByteCode.Opcode.OpRem : cp = "rem"; break;
         
                  case ByteCode.Opcode.OpInc : cp = "inc"; break;
                  case ByteCode.Opcode.OpDec : cp = "dec"; break;
  

                  case ByteCode.Opcode.BitOr : cp = "bor"; break;
                  case ByteCode.Opcode.BitXor : cp = "bxor"; break;
                  case ByteCode.Opcode.BitAnd : cp = "band"; break;
                  case ByteCode.Opcode.BitInv : cp = "binv"; break;
                  case ByteCode.Opcode.BitShl : cp = "bshl"; break;
                  case ByteCode.Opcode.BitShr : cp = "bshr"; break;
      
                  case ByteCode.Opcode.OpNeg : cp = "neg"; break;
                  case ByteCode.Opcode.OpPos : cp = "pos"; break;
                  case ByteCode.Opcode.OpNot : cp = "not"; break;
      
                  case ByteCode.Opcode.OpLt : cp = "lt"; break;
                  case ByteCode.Opcode.OpGt : cp = "gt"; break;
                  case ByteCode.Opcode.OpLte : cp = "lte"; break;
                  case ByteCode.Opcode.OpGte : cp = "gte"; break;
                  case ByteCode.Opcode.OpEq : cp = "eq"; break;
                  case ByteCode.Opcode.OpNeq : cp = "neq"; break;

                  //case ByteCode.Operator.Fork: cp = "fork"; opiptr = true; break;
                  default : cp = "ERROR"; break;
                }

                instruction += 4;

                if(opf32)
                {
                    var val = BitConverter.ToSingle(byteCode, instruction);
                    instruction += 4;
                    log.AppendFormat("  {0:D4} {1} {2:F6}\r\r\n", addr, cp, val);
                }
                else if (opiptr)
                {
                    var val = BitConverter.ToInt32(byteCode, instruction);
                    instruction += 4;
                    log.AppendFormat("  {0:D4} {1} {2}\r\r\n", addr, cp, val);
                }
                else
                {
                    log.AppendFormat("  {0:D4} {1}\r\r\n", addr, cp);
                }

            }
        }

        private byte[] ReadByteCode(BinaryReader data, int bytecodeLength)
        {
            return data.ReadBytes(bytecodeLength);
        }

        private gmlHeader ReadHeader(BinaryReader data)
        {
            var header = new gmlHeader();
            header.id = data.ReadInt32();
            header.flags = data.ReadInt32();
            header.stOffset = data.ReadInt32();
            header.scOffset = data.ReadInt32();
            header.fnOffset = data.ReadInt32();
            return header;
        }

        private StringTable ReadStringTable(BinaryReader data)
        {
            var numStrings = data.ReadUInt32();
            var stringTab = new StringTable();

            char[] stringTable = data.ReadChars((int)numStrings);
            StringBuilder sb = new StringBuilder();
            uint stringid = 0;
            uint li = 0;
            for (uint i = 0; i < numStrings - 1; ++i)
            {
                if (stringTable[i] == 0)
                {
                    stringTab.Add(new StringTableIem { Id = (int)stringid, Offset = (int)li, Value = sb.ToString() });
                    sb = new StringBuilder();
                    ++stringid;
                    li = i + 1;
                }
                else
                {
                    sb.Append(stringTable[i]);
                }
            }

            stringTab.Add(new StringTableIem {Id = (int)stringid, Offset = (int)li, Value = sb.ToString()});

            return stringTab;
        }

        private gmlFunction ReadFunction(BinaryReader data)
        {
            // Read out function entry
            var function = new gmlFunction();
            function.func = data.ReadUInt32();
            function.id = data.ReadUInt32();
            function.flags = data.ReadUInt32();
            function.numParams = data.ReadUInt32();
            function.numLocals = data.ReadUInt32();
            function.maxStackSize = data.ReadUInt32();
            function.byteCodeLen = data.ReadUInt32();
            return function;
        }

        public bool Load(Machine Machine, Stream fileStream)
        {
            var data = new BinaryReader(fileStream);

            List<FunctionObject> functionObjects = new List<FunctionObject>();

            UInt32 numFunctions = 0;

            // Load Header
            var header = ReadHeader(data);

            // Load string table
            var stringTable = ReadStringTable(data);

            Debug.Assert(header.scOffset == 0); // Can't handle included sourcecode for now
            
            // Jump to offset of functions
            data.BaseStream.Seek(header.fnOffset, SeekOrigin.Begin);
            // read number of functions
            numFunctions = data.ReadUInt32();

            for (int i = 0; i < numFunctions; ++i)
            {
                // Read out function entry
                var function = ReadFunction(data);

                if (function.flags == 1)
                    _MainFunctionId = i;

                byte[] bytecode = data.ReadBytes((int)function.byteCodeLen);
                uint end = function.byteCodeLen;
                List<ByteCode.Instruction> InstructionList = new List<ByteCode.Instruction>();

                for (int instruction = 0; instruction < end; )
                {
                    ByteCode.Opcode instr = (ByteCode.Opcode)BitConverter.ToUInt32(bytecode, instruction);
                    int bytecodeoffset = instruction;
                    instruction += 4;
                    switch (instr)
                    {
                        #region Operators
                        case ByteCode.Opcode.OpRem:
                        case ByteCode.Opcode.OpAdd:
                        case ByteCode.Opcode.OpDiv:
                        case ByteCode.Opcode.OpEq:
                        case ByteCode.Opcode.OpGt:
                        case ByteCode.Opcode.OpGte:
                        case ByteCode.Opcode.OpLt:
                        case ByteCode.Opcode.OpLte:
                        case ByteCode.Opcode.OpMul:
                        case ByteCode.Opcode.OpNeg:
                        case ByteCode.Opcode.OpSub:
                        case ByteCode.Opcode.SetInd:
                        case ByteCode.Opcode.GetInd:
                            {
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset));
                                break;
                            }
                        case ByteCode.Opcode.SetDot:
                        case ByteCode.Opcode.GetDot:
                            {
                                uint reference = System.BitConverter.ToUInt32(bytecode, instruction);
                                instruction += sizeof(uint);
                                var s = stringTable.GetByOffset((int)reference);
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable(s.Value) }));
                                break;
                            }
                        #endregion
                        case ByteCode.Opcode.Ret:
                        case ByteCode.Opcode.Retv:
                        case ByteCode.Opcode.Dup:
                        case ByteCode.Opcode.Pop:
                        case ByteCode.Opcode.Pop2:                        
                            {
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset));
                                break;
                            }

                        case ByteCode.Opcode.ForEach:
                            {
                                Int16 localref = System.BitConverter.ToInt16(bytecode, instruction);
                                instruction += sizeof(Int16);
                                Int16 localkey = System.BitConverter.ToInt16(bytecode, instruction);
                                instruction += sizeof(Int16);
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable((int)localref), new Variable((int)localkey) }));
                                break;
                            }
                        
                        case ByteCode.Opcode.Call:
                            {
                                int numParams = BitConverter.ToInt32(bytecode, instruction);
                                instruction += sizeof(int);

                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable(numParams) }));
                                break;
                            }
                        case ByteCode.Opcode.GetLocal:
                        case ByteCode.Opcode.SetLocal:
                            {
                                uint reference = System.BitConverter.ToUInt32(bytecode, instruction);
                                instruction += sizeof(uint);
                                uint offset = reference;
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable((int)offset) }));
                                break;
                            }

                        case ByteCode.Opcode.GetGlobal:
                        case ByteCode.Opcode.SetGlobal:
                            {
                                uint reference = BitConverter.ToUInt32(bytecode, instruction);
                                instruction += sizeof(int);
                                var s = stringTable.GetByOffset((int)reference);
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable(s.Value) }));
                                break;
                            }
                        #region Branch
                        case ByteCode.Opcode.Brzk:
                        case ByteCode.Opcode.Brz:
                        case ByteCode.Opcode.Bra:
                            {
                                int offset = BitConverter.ToInt32(bytecode, instruction);
                                instruction += sizeof(int);
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable(offset) }));
                                break;
                            }
                        #endregion
                        #region Push Operators
                        case ByteCode.Opcode.PushThis:
                        case ByteCode.Opcode.PushNull:
                            {
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset));
                                break;
                            }
                        case ByteCode.Opcode.PushInt0:
                            {
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable(0) }));
                                break;
                            }
                        case ByteCode.Opcode.PushInt1:
                            {
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable(1) }));
                                break;
                            }
                        case ByteCode.Opcode.PushInt:
                            {
                                int val = BitConverter.ToInt32(bytecode, instruction);
                                instruction += sizeof(int);

                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable(val) }));
                                break;
                            }
                        case ByteCode.Opcode.PushFp:
                            {
                                var i32 = BitConverter.ToInt32(bytecode, instruction);
                                float f = BitConverter.ToSingle(bytecode, instruction);
                                instruction += sizeof(float);

                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable(f) }));
                                break;
                            }
                        case ByteCode.Opcode.PushTbl:
                            {
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset));//, new Variable[] { new Variable(Machine.CreateTable()) }));
                                break;
                            }
                        case ByteCode.Opcode.PushStr:
                            {
                                uint reference = System.BitConverter.ToUInt32(bytecode, instruction);
                                instruction += sizeof(uint);
                                var s = stringTable.GetByOffset((int)reference);
                                InstructionList.Add(new ByteCode.Instruction(instr, bytecodeoffset, new Variable[] { new Variable(s.Value) }));
                                break;
                            }

                        case ByteCode.Opcode.PushFn:
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
                                throw new Exception("Invalid opcode " +  instr.ToString());
                            }

                    };  // end of switch

                }   // end of function loop


                functionObjects.Add( new FunctionObject() );
                functionObjects[i].Initialise(InstructionList, (int)function.numLocals, (int)function.numParams);
            }


            // Loop back over the newly loaded functions and set the functionobjects for each reference
            for (int funcid = 0; funcid < numFunctions; ++funcid)
            {
                var InstructionList = functionObjects[funcid].Instructions;
                int InstructionId = 0;
                foreach (ByteCode.Instruction instruction in InstructionList)
                {
                    if (instruction.OpCode == ByteCode.Opcode.PushFn)
                    {
                        InstructionList[InstructionId].SetOperand(0, new Variable(functionObjects[instruction[0].GetInteger()]));
                    }
                    else if ((instruction.OpCode == ByteCode.Opcode.Bra) 
                        || (instruction.OpCode == ByteCode.Opcode.Brz)
                        || (instruction.OpCode == ByteCode.Opcode.Brzk)
                        )
                    {
                        // Loop through and find the 
                        for (int INum = 0; INum < InstructionList.Count; ++INum )
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
            data.Close();
            return true;
        }

        
    }
}
