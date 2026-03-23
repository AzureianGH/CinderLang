using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackendInterface;
using LLVMSharp.Interop;

namespace LLVMBackend
{
    public class Block : IBlock
    {
        public Block(LLVMBasicBlockRef b) { block = b; }

        public LLVMBasicBlockRef block;

        public void RemoveTerminator() => block.Terminator.InstructionEraseFromParent();
        public void ClearBlock()
        {
            var instr = block.LastInstruction;
            while (instr != null)
            {
                var prev = instr.PreviousInstruction;
                instr.InstructionEraseFromParent();
                instr = prev;
            }
        }

        public override string ToString()
        {
            return block.ToString();
        }
    }
}
