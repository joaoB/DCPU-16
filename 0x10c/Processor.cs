using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _0x10c
{
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

           return true;
       }

       public bool SUB(ref UInt16 destination, UInt16 source)
       {
           return true;
       }

       public bool MUL(ref UInt16 destination, UInt16 source)
       {
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
