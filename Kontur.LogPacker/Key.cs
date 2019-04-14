using System.Collections.Generic;

namespace Kontur.LogPacker
{
    class Key
    {
        private char _symbol;
        private List<bool> _boolList;

        public Key(char symbol, List<bool> boolList)
        {
            Symbol = symbol;
            BoolList = boolList;
        }

        public char Symbol { get => _symbol; set => _symbol = value; }
        public List<bool> BoolList { get => _boolList; set => _boolList = value; }
    }
}
