using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _0x10c
{
   public class Processor
    {
       public delegate bool Action(int source);

       public Dictionary<int, Action> Actions { get; private set; }

       public int Accumulator;
       public int[] Memory { get; private set; }
       public int MemorySize { get; private set; }
       public int InstructionPointer { get; private set; }


       public Processor(int memorySize)
       {
           MemorySize = memorySize;
           Memory = new int[MemorySize];

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


       public bool SET(int source)
       {
           return true;
       }

       public bool ADD(int source)
       {
           return true;
       }

       public bool SUB(int source)
       {
           return true;
       }

       public bool MUL(int source)
       {
           return true;
       }

       public bool MLI(int source)
       {
           return true;
       }
       public bool DIV(int source)
       {
           return true;
       }
       public bool DVI(int source)
       {
           return true;
       }
       public bool MOD(int source)
       {
           return true;
       }
       public bool MDI(int source)
       {
           return true;
       }
       public bool AND(int source)
       {
           return true;
       }
       public bool BOR(int source)
       {
           return true;
       }
       public bool XOR(int source)
       {
           return true;
       }
       public bool SHR(int source)
       {
           return true;
       }
       public bool ASR(int source)
       {
           return true;
       }
       public bool SHL(int source)
       {
           return true;
       }
       public bool IFB(int source)
       {
           return true;
       }
       public bool IFC(int source)
       {
           return true;
       }
       public bool IFE(int source)
       {
           return true;
       }
       public bool IFN(int source)
       {
           return true;
       }
       public bool IFG(int source)
       {
           return true;
       }
       public bool IFA(int source)
       {
           return true;
       }
       public bool IFL(int source)
       {
           return true;
       }
       public bool IFU(int source)
       {
           return true;
       }
       public bool ADX(int source)
       {
           return true;
       }
       public bool SBX(int source)
       {
           return true;
       }
       public bool STI(int source)
       {
           return true;
       }
       public bool STD(int source)
       {
           return true;
       }
       public bool RESERVED(int source)
       {
           return true;
       }
 }
}
