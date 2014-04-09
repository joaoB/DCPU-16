using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _0x10c
{

    public class operand
    {
        public operand()
            : this(0, 0)
        {
        }
        public operand(int type, ushort value)
        {
            _type = type;
            _value = value;

        }
        public ushort _value;
        public int _type;
        public const int REG = 0;
        public const int PC = 1;
        public const int SP = 2;
        public const int EX = 3;
        public const int RAM = 4;
        public const int LIT = 5;
        public const int STK = 6;
    }

   public class Processor
    {
       public delegate bool Action(ref UInt16 destination, UInt16 source);

       public Dictionary<int, Action> Actions { get; private set; }

       public int Accumulator;
       public UInt16[] Memory { get; private set; }
       public int MemorySize { get; private set; }
       public int InstructionPointer { get; private set; }

       enum reg_index { A, B, C, X, Y, Z, I, J, PC, SP, EX };
       // List of basic instructions. Chosen by the value of "o":
        enum basicInstructions { nbi,SET,ADD,SUB,MUL,DIV,MOD,SHL,SHR,AND,BOR,XOR,IFE,IFN,IFG,IFB };
        // List of non-basic instructions (nbi), chosen by the value of "aa":
        enum nonbasicInstructions { JSR = 0x11 };

       UInt16[] reg { get; private set; }

       private ushort[] _RAM;
       private const uint RAMSize = 0x10000u;
       private ushort[] _Register;
       private const uint RegisterCount = 8;
       private const string RegisterOrder = "ABCXYZIJ";
       private const int _A = 0;
       private const int _B = 1;
       private const int _C = 2;
       private const int _X = 3;
       private const int _Y = 4;
       private const int _Z = 5;
       private const int _I = 6;
       private const int _J = 7;
       private ushort _PC;
       private ushort _SP;
       private ushort _EX;
       private ushort _IA;
       private uint _Cycles;
       private bool _IntEnabled;
       private System.Collections.Generic.Queue<ushort> _IntQueue;
       private int _CycleDebt;

       public uint Cycles { get { return _Cycles; } }
       public uint ClockRatekHz { get; set; }
       public double ClockPeriodSec { get { return (double).001 / (double)ClockRatekHz; } }
       public double ElapsedTimeSec { get { return (double)Cycles * ClockPeriodSec; } }

       public Processor()
        {
            ClockRatekHz = 100; // 100 kHz
            _RAM = new ushort[RAMSize];
            _Register = new ushort[RegisterCount];
            _IntQueue = new Queue<ushort>();
            
           ClearMemory();
            Reset();
        }

       public string RegToString()
       {
           StringBuilder output = new StringBuilder();
           for (int i = 0; i < RegisterCount; i++)
           {
               output.Append(RegisterOrder[i] + ":  " + _Register[i].ToString("X4") + " ");
           }
           output.AppendLine();
           output.Append("PC: " + _PC.ToString("X4") + " ");
           output.Append("SP: " + _SP.ToString("X4") + " ");
           output.Append("EX: " + _EX.ToString("X4"));
           return output.ToString();
       }

       public string MemToString(ushort startAddr, ushort endAddr)
       {
           ushort currBlock = 0;
           ushort firstAddr = (ushort)(startAddr & (ushort)0xfff8u);
           ushort lastAddr = (ushort)(endAddr | (ushort)0x0003u);
           StringBuilder output = new StringBuilder();
           while (firstAddr + (currBlock * 8) < lastAddr)
           {
               output.Append(((firstAddr + (currBlock * 8)).ToString("X4")) + ":");
               for (ushort i = 0; i < 8; i++)
               {
                   output.Append(" " + _RAM[firstAddr + i + (currBlock * 8)].ToString("X4"));
               }
               output.Append("\n");
               currBlock++;
           }
           return output.ToString();
       }

       public override string ToString()
       {
           StringBuilder output = new StringBuilder();
           output.AppendLine(RegToString());
           output.Append(MemToString(_PC, (ushort)(_PC + 0x0040u)));
           return output.ToString();
       }

       public ushort A { get { return _Register[_A]; } }
       public ushort B { get { return _Register[_B]; } }
       public ushort C { get { return _Register[_C]; } }
       public ushort X { get { return _Register[_X]; } }
       public ushort Y { get { return _Register[_Y]; } }
       public ushort Z { get { return _Register[_Z]; } }
       public ushort I { get { return _Register[_I]; } }
       public ushort J { get { return _Register[_J]; } }
       public ushort PC { get { return _PC; } }
       public ushort SP { get { return _SP; } }
       public ushort EX { get { return _EX; } }
       public ushort IA { get { return _IA; } }
       public ushort RAM(uint addr) { return _RAM[addr]; }

       public void Reset()
       {
           for (int i = 0; i < RegisterCount; _Register[i++] = 0) ;
           _PC = 0;
           _SP = 0;
           _EX = 0;
           _IA = 0;
           _Cycles = 0;
           _CycleDebt = 0;
           _IntEnabled = true;
           _IntQueue.Clear();
           _state = ProcessorState.newInst;
           ClearMemory();

       }

       public void ClearMemory()
       {
           for (int i = 0; i < RAMSize; _RAM[i++] = 0) ;
       }

       public void LoadMemory(ushort[] data, uint startAddr = 0)
       {
           for (int i = 0; i < data.Length; _RAM[startAddr + i] = data[i++]) ;
       }

       public void LoadMemory(string pathName, uint startAddr = 0)
       {
           byte[] filebytes = File.ReadAllBytes(pathName);
           ushort[] data = new ushort[(filebytes.Length + 1) >> 1];
           for (int i = 0; i < filebytes.Length; i++)
           {
               if (i % 2 == 0)
               {
                   data[i >> 1] = (ushort)(filebytes[i] << 8);
               }
               else
               {
                   data[i >> 1] += (ushort)(filebytes[i]);
               }
           }
           LoadMemory(data, startAddr);
       }


       public void Step(int instructions = 1)
       {
           for (; instructions > 0; )
           {
               Tick();
               if (_state == ProcessorState.newInst) instructions--;
           }
       }

       private enum ProcessorState { newInst, readOpA, readOpB, executeInst };
       private ProcessorState _state;
       private ushort Tick_inst;
       private ushort Tick_opcode;
       private ushort Tick_b;
       private ushort Tick_a;
       private operand Tick_opA;
       private operand Tick_opB;

       private void parseSkippedOperand(ushort value)
       { 
       //TODO
       }
       private void Tick() { 
       //TODO
       }
       

       public void skipNext(bool chain = true)
       {
           ushort inst = nextWord();
           ushort opcode = (ushort)(inst & (ushort)0x001fu);
           ushort a = (ushort)((inst & (ushort)0x03e0u) >> 5);
           ushort b = (ushort)((inst & (ushort)0xfc00u) >> 10);
           if (opcode == 0) // Non-basic opcodes
           {
               parseSkippedOperand(b);
           }
           else // Basic opcodes
           {
               parseSkippedOperand(a);
               parseSkippedOperand(b);
               if (chain && opcode > 0x0f && opcode < 0x18) skipNext(false);
           }

       }

       private ushort nextWord() { return _RAM[_PC++]; }


       private ushort readValue(operand op)
       {
           if (op._type == operand.REG)
               return _Register[op._value];
           if (op._type == operand.RAM)
               return _RAM[op._value];
           if (op._type == operand.LIT)
               return op._value;
           if (op._type == operand.STK)
               return stackPOP();
           switch (op._type)
           {
               case operand.PC:
                   return _PC;
               case operand.SP:
                   return _SP;
               case operand.EX:
                   return _EX;
           }
           throw new Exception("Invalid op._type: " + op._type.ToString());
       }

       private void writeValue(operand op, ushort data)
       {
           if (op._type == operand.REG)
           {
               _Register[op._value] = data;
               return;
           }
           if (op._type == operand.RAM)
           {
               _RAM[op._value] = data;
               return;
           }
           if (op._type == operand.LIT)
           {
               _RAM[op._value] = data;
               return;
           }
           if (op._type == operand.STK)
           {
               stackPUSH(data);
               return;
           }
           switch (op._type)
           {
               case operand.PC:
                   _PC = data;
                   return;
               case operand.SP:
                   _SP = data;
                   return;
               case operand.EX:
                   _EX = data;
                   return;
           }
           throw new Exception("Invalid op._type: " + op._type.ToString());
       }


       private void stackPUSH(ushort value)
       {
           _RAM[--_SP] = value;
       }

       private ushort stackPOP()
       {
           return _RAM[_SP++];
       }


       public Processor(int memorySize)
       {
           MemorySize = memorySize;
           Memory = new UInt16[MemorySize];
           reg = new UInt16[11];

           Actions = new Dictionary<int, Action>
                          {
                              {0x01, SET},
                              {0x02, ADD},
                              {0x03, SUB},
                              {0x04, MUL},
                              {0x05, MLI},
                              {0x06, DIV},
                              {0x07, DVI},
                              {0x08, MOD},
                              {0x09, MDI},
                              {0x0a, AND},
                              {0x0b, BOR},
                              {0x0c, XOR},
                              {0x0d, SHR},
                              {0x0e, ASR},
                              {0x0f, SHL},
                              {0x10, IFB},
                              {0x11, IFC},
                              {0x12, IFE},
                              {0x13, IFN},
                              {0x14, IFG},
                              {0x15, IFA},
                              {0x16, IFL},
                              {0x17, IFU},
                              {0x18, RESERVED},
                              {0x19, RESERVED},
                              {0x1a, ADX},
                              {0x1b, SBX},
                              {0x1c, RESERVED},
                              {0x1d, RESERVED},
                              {0x1e, STI},
                              {0x1f, STD}

                          };
       }

       public bool SET(ref UInt16 destination, UInt16 source)
       {
           destination = source;
           return true;
       }

       public bool ADD(ref UInt16 destination, UInt16 source)
       {
           UInt16 wr = (UInt16)(destination + source);
           destination = wr;
           reg[(int)reg_index.EX] = (UInt16)(wr >> 16);

           return true;
       }

       public bool SUB(ref UInt16 destination, UInt16 source)
       {
           UInt16 wr = (UInt16)(destination - source);
           destination = wr;
           reg[(int)reg_index.EX] = (UInt16)(wr >> 16);
           return true;
       }

       public bool MUL(ref UInt16 destination, UInt16 source)
       {
           UInt16 wr;
           UInt16 wa = destination;

           wr = (UInt16)(wa * source); 
           destination = wr;
           reg[(int)reg_index.EX] = (UInt16)(wr >> 16); 
           return true;
       }

       public bool MLI(ref UInt16 destination, UInt16 source)
       {
           return true;
       }
       public bool DIV(ref UInt16 destination, UInt16 source)
       {
           return true;
       }
       public bool DVI(ref UInt16 destination, UInt16 source)
       {
           UInt16 wr;
           UInt16 wa = destination;

           if (source != 0)
           {
               wr = (UInt16)((wa << 16) / source);
           }
           else {
               wr = 0;
           }

           destination = (UInt16)(wr >> 16);
           reg[(int)reg_index.EX] = wr;

           return true;
       }

       public bool MOD(ref UInt16 destination, UInt16 source)
       {
           if (source != 0)
           {
               destination = (UInt16)(destination % source);
           }
           else{
               destination = 0;
           }
           return true;
       }

       public bool MDI(ref UInt16 destination, UInt16 source)
       {
           return true;
       }

       public bool AND(ref UInt16 destination, UInt16 source)
       {
           destination &= source;
           return true;
       }

       public bool BOR(ref UInt16 destination, UInt16 source)
       {
           destination |= source;
           return true;
       }

       public bool XOR(ref UInt16 destination, UInt16 source)
       {
           destination ^= source;
           return true;
       }

       public bool SHR(ref UInt16 destination, UInt16 source)
       {
           UInt16 wr;
           UInt16 wa = destination;
           wr = (UInt16)((wa << 16) >> source);
           destination = (UInt16)(wr >> 16); 
           reg[(int)reg_index.EX] = wr;

           return true;
       }
       public bool ASR(ref UInt16 destination, UInt16 source)
       {
           return true;
       }
       public bool SHL(ref UInt16 destination, UInt16 source)
       {
           return true;
       }
       public bool IFB(ref UInt16 destination, UInt16 source)
       {
           return true;
       }
       public bool IFC(ref UInt16 destination, UInt16 source)
       {
           return true;
       }
       public bool IFE(ref UInt16 destination, UInt16 source)
       {
           return true;
       }
       public bool IFN(ref UInt16 destination, UInt16 source)
       {
           return true;
       }
       public bool IFG(ref UInt16 destination, UInt16 source)
       {
           return true;
       }
       public bool IFA(ref UInt16 destination, UInt16 source)
       {
           return true;
       }
       public bool IFL(ref UInt16 destination, UInt16 source)
       {
           return true;
       }
       public bool IFU(ref UInt16 destination, UInt16 source)
       {
           return true;
       }
       public bool ADX(ref UInt16 destination, UInt16 source)
       {
           return true;
       }
       public bool SBX(ref UInt16 destination, UInt16 source)
       {
           return true;
       }
       public bool STI(ref UInt16 destination, UInt16 source)
       {
           return true;
       }
       public bool STD(ref UInt16 destination, UInt16 source)
       {
           return true;
       }
       public bool RESERVED(ref UInt16 destination, UInt16 source)
       {
           return true;
       }

       public void op() {
           reg_index _pc = reg_index.PC;

           UInt16 v = Memory[reg[(UInt16)_pc]++];
           // Instruction format: bbbbbb aaaaaa oooo
           UInt16 o = (UInt16)(v & 0x0F);
           UInt16 aa = (UInt16)((v >> 4) & 0x3F);
           UInt16 bb = (UInt16)((v >> 10) & 0x3F);

           // Parse the two parameters. (Note: "a" is skipped when nbi.)

           //UInt16 a = o == (UInt16)basicInstructions.nbi ? v : value<skipping>(aa); 
           //UInt32 wa = a;
           //UInt16 b = value<skipping>(bb); 
           //UInt32 wr;

           UInt16 _instruction = (UInt16)(o == (UInt16)basicInstructions.nbi ? aa + 0x10 : o);

           Action _action = new Action(Actions[_instruction]);
           _action(ref aa, bb);
       }


      
     /*  unsafe UInt16* value(ref UInt16 v, int skipping)
       {

           switch (((v & (UInt16)0x38) == 0x18) ? (v & 7) ^ skipping : 8 + v / 8)
           { 
            
               default: return (UInt16)0;
           }

           return 0;
        }*/
   
  
 }
}
