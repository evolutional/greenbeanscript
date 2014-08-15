using System;
using System.Collections.Generic;
using System.Text;

namespace GreenBeanScript
{
	struct Block
	{
        public Variable BlockVar;
        public int ThreadId;
	}

    class BlockList
    {
        public Variable Block;
        List<Block> Blocks;
    }

    struct Signal
    {
        public Variable SignalVar;
        public int SourceThreadId;
        public int DestThreadId;
    }
}
