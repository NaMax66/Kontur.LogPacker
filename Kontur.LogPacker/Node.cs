using System.Collections.Generic;

namespace Kontur.LogPacker
{
    internal class Node
    {
        private int _value;
        private List<char> _charList;

        public Node()
        {

        }
        public Node(char ch, int value)
        {
            Value = value;
            this.CharList = new List<char>();
            CharList.Add(ch);
        }

        public Node(List<char> charList, int value, Node nodeZero, Node nodeOne)
        {
            CharList = charList;
            Value = value;
            NodeZero = nodeZero;
            NodeOne = nodeOne;
        }

        public Node NodeZero { get; set; }
        public Node NodeOne { get; set; }
        public Node Parrent { get; set; }
        public bool Flag { get; set; }
        public int Value { get => _value; set => _value = value; }

        public List<char> CharList { get => _charList; set => _charList = value; }

    }
}