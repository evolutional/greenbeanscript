using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using GreenBeanScript.VirtualMachine.ByteCode;
using GreenBeanScript.VirtualMachine.FileFormat;

namespace GreenBeanScript.VirtualMachine
{
    public class Library
    {
        protected List<FunctionObject> _Functions;
        protected int _MainFunctionId;

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

        public bool LoadFromFile(Machine machine, string fileName)
        {
            return Load(machine, File.OpenRead(fileName));
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

            var debug = header.Flags & 1;
            log.AppendFormat("GM LIBRARY {0}, DEBUG={1}\r\n\r\n", Path.GetFullPath(libFileName), debug);

            // Load string table
            var stringTable = ReadStringTable(data);

            log.AppendFormat("STRING TABLE, SIZE={0}\r\n" +
                             "===============================================================================\r\n",
                stringTable.Size.ToString().PadLeft(5));


            for (var i = 0; i < stringTable.Count; ++i)
            {
                var str = stringTable.GetById(i);
                // "%05d:%s\n", i+1,&stringTable[i+1]
                log.AppendFormat("{0:D5}:{1}\r\n", str.Offset, str.Value);
            }

            log.Append("\r\nNO SOURCE CODE IN LIB\r\n");

            // Jump to offset of functions
            data.BaseStream.Seek(header.FnOffset, SeekOrigin.Begin);
            // read number of functions
            var numFunctions = data.ReadUInt32();

            log.AppendFormat("FUNCTIONS, COUNT={0}\r\n" +
                             "===============================================================================\r\n\r\n",
                numFunctions);

            for (var i = 0; i < numFunctions; ++i)
                ListFunction(data, log);

            log.Append("END LIB\r\n");
        }

        private void ListFunction(BinaryReader data, StringBuilder log)
        {
            log.Append("===============================================================================\r\n");

            var function = ReadFunction(data);

            log.AppendFormat("FUNCTION ID {0}\r\n", function.Id);
            log.AppendFormat("FLAGS {0}\r\n", function.Flags);
            log.AppendFormat("NUM PARAMS {0}\r\n", function.NumParams);
            log.AppendFormat("NUM LOCALS {0}\r\n", function.NumLocals);
            log.AppendFormat("MAX STACK {0}\r\n", function.MaxStackSize);
            log.AppendFormat("BYTE CODE SIZE {0}\r\n", function.ByteCodeLen);

            var byteCode = ReadByteCode(data, (int) function.ByteCodeLen);

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
                var cp = "";

                var addr = instruction - start;

                var bc = BitConverter.ToUInt32(byteCode, instruction);

                switch ((Opcode) bc)
                {
                    case Opcode.Nop:
                        cp = "nop";
                        break;
                    case Opcode.Line:
                        cp = "line";
                        break;

                    case Opcode.GetDot:
                        cp = "get dot";
                        opiptr = true;
                        break;
                    case Opcode.SetDot:
                        cp = "set dot";
                        opiptr = true;
                        break;
                    case Opcode.GetInd:
                        cp = "get index";
                        break;
                    case Opcode.SetInd:
                        cp = "set index";
                        break;

                    case Opcode.Bra:
                        cp = "bra";
                        opiptr = true;
                        break;
                    case Opcode.Brz:
                        cp = "brz";
                        opiptr = true;
                        break;
                    case Opcode.Brnz:
                        cp = "brnz";
                        opiptr = true;
                        break;
                    case Opcode.Brzk:
                        cp = "brzk";
                        opiptr = true;
                        break;
                    case Opcode.Brnzk:
                        cp = "brnzk";
                        opiptr = true;
                        break;
                    case Opcode.Call:
                        cp = "call";
                        opiptr = true;
                        break;
                    case Opcode.Ret:
                        cp = "ret";
                        break;
                    case Opcode.Retv:
                        cp = "retv";
                        break;
                    case Opcode.ForEach:
                        cp = "foreach";
                        opiptr = true;
                        break;

                    case Opcode.Pop:
                        cp = "pop";
                        break;
                    case Opcode.Pop2:
                        cp = "pop2";
                        break;
                    case Opcode.Dup:
                        cp = "dup";
                        break;
                    case Opcode.Dup2:
                        cp = "dup2";
                        break;
                    case Opcode.Swap:
                        cp = "swap";
                        break;
                    case Opcode.PushNull:
                        cp = "push null";
                        break;
                    case Opcode.PushInt:
                        cp = "push int";
                        opiptr = true;
                        break;
                    case Opcode.PushInt0:
                        cp = "push int 0";
                        break;
                    case Opcode.PushInt1:
                        cp = "push int 1";
                        break;
                    case Opcode.PushFp:
                        cp = "push fp";
                        opf32 = true;
                        break;
                    case Opcode.PushStr:
                        cp = "push str";
                        opiptr = true;
                        break;
                    case Opcode.PushTbl:
                        cp = "push tbl";
                        break;
                    case Opcode.PushFn:
                        cp = "push fn";
                        opiptr = true;
                        break;
                    case Opcode.PushThis:
                        cp = "push this";
                        break;

                    case Opcode.GetLocal:
                        cp = "get local";
                        opiptr = true;
                        break;
                    case Opcode.SetLocal:
                        cp = "set local";
                        opiptr = true;
                        break;
                    case Opcode.GetGlobal:
                        cp = "get global";
                        opiptr = true;
                        break;
                    case Opcode.SetGlobal:
                        cp = "set global";
                        opiptr = true;
                        break;
                    case Opcode.GetThis:
                        cp = "get this";
                        opiptr = true;
                        break;
                    case Opcode.SetThis:
                        cp = "set this";
                        opiptr = true;
                        break;

                    case Opcode.OpAdd:
                        cp = "add";
                        break;
                    case Opcode.OpSub:
                        cp = "sub";
                        break;
                    case Opcode.OpMul:
                        cp = "mul";
                        break;
                    case Opcode.OpDiv:
                        cp = "div";
                        break;
                    case Opcode.OpRem:
                        cp = "rem";
                        break;

                    case Opcode.OpInc:
                        cp = "inc";
                        break;
                    case Opcode.OpDec:
                        cp = "dec";
                        break;


                    case Opcode.BitOr:
                        cp = "bor";
                        break;
                    case Opcode.BitXor:
                        cp = "bxor";
                        break;
                    case Opcode.BitAnd:
                        cp = "band";
                        break;
                    case Opcode.BitInv:
                        cp = "binv";
                        break;
                    case Opcode.BitShl:
                        cp = "bshl";
                        break;
                    case Opcode.BitShr:
                        cp = "bshr";
                        break;

                    case Opcode.OpNeg:
                        cp = "neg";
                        break;
                    case Opcode.OpPos:
                        cp = "pos";
                        break;
                    case Opcode.OpNot:
                        cp = "not";
                        break;

                    case Opcode.OpLt:
                        cp = "lt";
                        break;
                    case Opcode.OpGt:
                        cp = "gt";
                        break;
                    case Opcode.OpLte:
                        cp = "lte";
                        break;
                    case Opcode.OpGte:
                        cp = "gte";
                        break;
                    case Opcode.OpEq:
                        cp = "eq";
                        break;
                    case Opcode.OpNeq:
                        cp = "neq";
                        break;

                    //case ByteCode.Operator.Fork: cp = "fork"; opiptr = true; break;
                    default:
                        cp = "ERROR";
                        break;
                }

                instruction += 4;

                if (opf32)
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

        private GmlHeader ReadHeader(BinaryReader data)
        {
            var header = new GmlHeader();
            header.Id = data.ReadInt32();
            header.Flags = data.ReadInt32();
            header.StOffset = data.ReadInt32();
            header.ScOffset = data.ReadInt32();
            header.FnOffset = data.ReadInt32();
            return header;
        }

        private StringTable ReadStringTable(BinaryReader data)
        {
            var numStrings = data.ReadUInt32();
            var stringTab = new StringTable();

            var stringTable = data.ReadChars((int) numStrings);
            var sb = new StringBuilder();
            uint stringid = 0;
            uint li = 0;
            for (uint i = 0; i < numStrings - 1; ++i)
                if (stringTable[i] == 0)
                {
                    stringTab.Add(new StringTableIem {Id = (int) stringid, Offset = (int) li, Value = sb.ToString()});
                    sb = new StringBuilder();
                    ++stringid;
                    li = i + 1;
                }
                else
                {
                    sb.Append(stringTable[i]);
                }

            stringTab.Add(new StringTableIem {Id = (int) stringid, Offset = (int) li, Value = sb.ToString()});

            return stringTab;
        }

        private GmlFunction ReadFunction(BinaryReader data)
        {
            // Read out function entry
            var function = new GmlFunction();
            function.Func = data.ReadUInt32();
            function.Id = data.ReadUInt32();
            function.Flags = data.ReadUInt32();
            function.NumParams = data.ReadUInt32();
            function.NumLocals = data.ReadUInt32();
            function.MaxStackSize = data.ReadUInt32();
            function.ByteCodeLen = data.ReadUInt32();
            return function;
        }

        public bool Load(Machine machine, Stream fileStream)
        {
            var data = new BinaryReader(fileStream);

            var functionObjects = new List<FunctionObject>();

            uint numFunctions = 0;

            // Load Header
            var header = ReadHeader(data);

            // Load string table
            var stringTable = ReadStringTable(data);

            Debug.Assert(header.ScOffset == 0); // Can't handle included sourcecode for now

            // Jump to offset of functions
            data.BaseStream.Seek(header.FnOffset, SeekOrigin.Begin);
            // read number of functions
            numFunctions = data.ReadUInt32();

            for (var i = 0; i < numFunctions; ++i)
            {
                // Read out function entry
                var function = ReadFunction(data);

                if (function.Flags == 1)
                    _MainFunctionId = i;

                var bytecode = data.ReadBytes((int) function.ByteCodeLen);
                var end = function.ByteCodeLen;
                var instructionList = new List<Instruction>();

                for (var instruction = 0; instruction < end;)
                {
                    var instr = (Opcode) BitConverter.ToUInt32(bytecode, instruction);
                    var bytecodeoffset = instruction;
                    instruction += 4;
                    switch (instr)
                    {
                            #region Operators

                        case Opcode.OpRem:
                        case Opcode.OpAdd:
                        case Opcode.OpDiv:
                        case Opcode.OpEq:
                        case Opcode.OpGt:
                        case Opcode.OpGte:
                        case Opcode.OpLt:
                        case Opcode.OpLte:
                        case Opcode.OpMul:
                        case Opcode.OpNeg:
                        case Opcode.OpSub:
                        case Opcode.SetInd:
                        case Opcode.GetInd:
                        {
                            instructionList.Add(new Instruction(instr, bytecodeoffset));
                            break;
                        }
                        case Opcode.SetDot:
                        case Opcode.GetDot:
                        {
                            var reference = BitConverter.ToUInt32(bytecode, instruction);
                            instruction += sizeof(uint);
                            var s = stringTable.GetByOffset((int) reference);
                            instructionList.Add(new Instruction(instr, bytecodeoffset, new[] {new Variable(s.Value)}));
                            break;
                        }

                            #endregion

                        case Opcode.Ret:
                        case Opcode.Retv:
                        case Opcode.Dup:
                        case Opcode.Pop:
                        case Opcode.Pop2:
                        {
                            instructionList.Add(new Instruction(instr, bytecodeoffset));
                            break;
                        }

                        case Opcode.ForEach:
                        {
                            var localref = BitConverter.ToInt16(bytecode, instruction);
                            instruction += sizeof(short);
                            var localkey = BitConverter.ToInt16(bytecode, instruction);
                            instruction += sizeof(short);
                            instructionList.Add(new Instruction(instr, bytecodeoffset,
                                new[] {new Variable(localref), new Variable(localkey)}));
                            break;
                        }

                        case Opcode.Call:
                        {
                            var numParams = BitConverter.ToInt32(bytecode, instruction);
                            instruction += sizeof(int);

                            instructionList.Add(new Instruction(instr, bytecodeoffset, new[] {new Variable(numParams)}));
                            break;
                        }
                        case Opcode.GetLocal:
                        case Opcode.SetLocal:
                        {
                            var reference = BitConverter.ToUInt32(bytecode, instruction);
                            instruction += sizeof(uint);
                            var offset = reference;
                            instructionList.Add(new Instruction(instr, bytecodeoffset,
                                new[] {new Variable((int) offset)}));
                            break;
                        }

                        case Opcode.GetGlobal:
                        case Opcode.SetGlobal:
                        {
                            var reference = BitConverter.ToUInt32(bytecode, instruction);
                            instruction += sizeof(int);
                            var s = stringTable.GetByOffset((int) reference);
                            instructionList.Add(new Instruction(instr, bytecodeoffset, new[] {new Variable(s.Value)}));
                            break;
                        }

                            #region Branch

                        case Opcode.Brzk:
                        case Opcode.Brz:
                        case Opcode.Bra:
                        {
                            var offset = BitConverter.ToInt32(bytecode, instruction);
                            instruction += sizeof(int);
                            instructionList.Add(new Instruction(instr, bytecodeoffset, new[] {new Variable(offset)}));
                            break;
                        }

                            #endregion

                            #region Push Operators

                        case Opcode.PushThis:
                        case Opcode.PushNull:
                        {
                            instructionList.Add(new Instruction(instr, bytecodeoffset));
                            break;
                        }
                        case Opcode.PushInt0:
                        {
                            instructionList.Add(new Instruction(instr, bytecodeoffset, new[] {new Variable(0)}));
                            break;
                        }
                        case Opcode.PushInt1:
                        {
                            instructionList.Add(new Instruction(instr, bytecodeoffset, new[] {new Variable(1)}));
                            break;
                        }
                        case Opcode.PushInt:
                        {
                            var val = BitConverter.ToInt32(bytecode, instruction);
                            instruction += sizeof(int);

                            instructionList.Add(new Instruction(instr, bytecodeoffset, new[] {new Variable(val)}));
                            break;
                        }
                        case Opcode.PushFp:
                        {
                            var i32 = BitConverter.ToInt32(bytecode, instruction);
                            var f = BitConverter.ToSingle(bytecode, instruction);
                            instruction += sizeof(float);

                            instructionList.Add(new Instruction(instr, bytecodeoffset, new[] {new Variable(f)}));
                            break;
                        }
                        case Opcode.PushTbl:
                        {
                            instructionList.Add(new Instruction(instr, bytecodeoffset));
                            //, new Variable[] { new Variable(Machine.CreateTable()) }));
                            break;
                        }
                        case Opcode.PushStr:
                        {
                            var reference = BitConverter.ToUInt32(bytecode, instruction);
                            instruction += sizeof(uint);
                            var s = stringTable.GetByOffset((int) reference);
                            instructionList.Add(new Instruction(instr, bytecodeoffset, new[] {new Variable(s.Value)}));
                            break;
                        }

                        case Opcode.PushFn:
                        {
                            var reference = BitConverter.ToUInt32(bytecode, instruction);
                            instruction += sizeof(uint);
                            var offset = reference;
                            instructionList.Add(new Instruction(instr, bytecodeoffset,
                                new[] {new Variable((int) offset)}));
                            break;
                        }

                            #endregion

                        default:
                        {
                            throw new Exception("Invalid opcode " + instr);
                        }
                    }
                    ; // end of switch
                } // end of function loop


                functionObjects.Add(new FunctionObject());
                functionObjects[i].Initialise(instructionList, (int) function.NumLocals, (int) function.NumParams);
            }


            // Loop back over the newly loaded functions and set the functionobjects for each reference
            for (var funcid = 0; funcid < numFunctions; ++funcid)
            {
                var instructionList = functionObjects[funcid].Instructions;
                var instructionId = 0;
                foreach (var instruction in instructionList)
                {
                    if (instruction.OpCode == Opcode.PushFn)
                        instructionList[instructionId].SetOperand(0,
                            new Variable(functionObjects[instruction[0].GetInteger()]));
                    else if ((instruction.OpCode == Opcode.Bra)
                             || (instruction.OpCode == Opcode.Brz)
                             || (instruction.OpCode == Opcode.Brzk)
                    )
                        for (var num = 0; num < instructionList.Count; ++num)
                            if (instructionList[num].ByteCodeOffset == instruction[0].GetInteger())
                            {
                                instructionList[instructionId].SetOperand(0, new Variable(num));
                                break;
                            }

                    ++instructionId;
                }
            }

            _Functions = functionObjects;
            data.Close();
            return true;
        }
    }
}