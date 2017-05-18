using System.Collections.Generic;

namespace GreenBeanScript.VirtualMachine
{
    internal class StringTable
    {
        private readonly Dictionary<int, StringTableIem> _offsets;
        private readonly Dictionary<int, StringTableIem> _strings;

        public StringTable()
        {
            _offsets = new Dictionary<int, StringTableIem>();
            _strings = new Dictionary<int, StringTableIem>();
        }

        public int Count
        {
            get { return _strings.Count; }
        }

        public int Size { get; private set; }

        public void Add(StringTableIem stringTableIem)
        {
            _strings.Add(stringTableIem.Id, stringTableIem);
            _offsets.Add(stringTableIem.Offset, stringTableIem);
            Size += stringTableIem.Value.Length + 1;
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
}