using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Kontur.LogPacker
{
    static class CompressFactory
    {
        internal static void EncodeFile(FileStream inputStream, List<Key> keys, Stream outputStream)
        {
            using (outputStream)
            {
                using (inputStream)
                {
                    var streamReader = new StreamReader(inputStream);
                    List<bool> dump = new List<bool>();
                    List<bool> lineCode = new List<bool>();

                    byte[] keysTable = new byte[keys.Count * 2];

                    keysTable = GetBytes(keys); //TODO сделать метод переноса ключей в байты

                    outputStream.Write(keysTable);
                    outputStream.WriteByte(0x0A);

                    while (streamReader.Peek() > -1)
                    {
                        if (dump.Count != 0)
                        {
                            lineCode.AddRange(dump);
                        }
                        string line = streamReader.ReadLine();

                        foreach (char ch in line)
                        {
                            Key key = keys.Find(x => x.Symbol == ch);
                            lineCode.AddRange(key.BoolList); // делаем один список строки в бинарном представлении
                        }

                        if (streamReader.Peek() != -1)
                        {
                            Key endLineKey = keys.Find(x => x.Symbol == '\n');
                            lineCode.AddRange(endLineKey.BoolList);
                        }

                        int bytesCount = lineCode.Count % 8; // сколько символов остается "не пристроены"

                        dump = lineCode.GetRange(lineCode.Count - bytesCount, bytesCount);// обновляем dump

                        lineCode.RemoveRange(lineCode.Count - bytesCount, bytesCount);

                        byte[] info = GetBytes(lineCode);

                        lineCode.Clear();

                        outputStream.Write(info);  //записываем массив в файл

                    }

                    outputStream.WriteByte(0x0A);

                    byte[] dmp = new byte[dump.Count];

                    for (int i = 0; i < dmp.Length; i++)
                    {
                        bool b = dump[i];

                        if (b) dmp[i] = 0b1;
                        else dmp[i] = 0b0;

                    }

                    outputStream.Write(dmp);

                    Console.WriteLine("размер заглушки: " + dump.Count);
                }
            }
        }

        private static byte[] GetBytes(List<Key> keys)
        {
            byte[] test = new byte[keys.Count * 2];

            for (int i = 0; i < test.Length; i++)
            {
                test[i] = 0b111;
            }
            return test;
        }

        //взял с https://stackoverflow.com/questions/713057/convert-bool-to-byte
        private static byte[] GetBytes(List<bool> lineCode)
        {

            int len = lineCode.Count;
            int bytes = len >> 3;
            if ((len & 0x07) != 0) ++bytes;
            byte[] arrToFile = new byte[bytes];
            for (int i = 0; i < len; i++)
            {
                if (lineCode[i])
                    arrToFile[i >> 3] |= (byte)(1 << (i & 0x07));
            }
            return arrToFile;

        }

        static long numberOfBits;

        public static List<Node> MakeFrequencyList(Stream inputStream)
        {
            var streamReader = new StreamReader(inputStream);
            List<Node> frequencyList = new List<Node>();
            using (streamReader)
            {
                int endLineValue = 0;
                while (streamReader.Peek() > -1)
                {
                    string line = streamReader.ReadLine(); //TODO Узнать можно ли ускорить?
                                                           //long lineSize = System.Text.ASCIIEncoding.Unicode.GetByteCount(line);
                                                           //long streamSize = streamReader.BaseStream.Length;

                    //if (lineSize <= streamSize) endLineValue++; // тут я добавляю символ переноса строки.

                    foreach (char ch in line)
                    {
                        int value = 1;
                        if (frequencyList.Exists(x => x.CharList.Exists(y => y == ch)))
                        {
                            int index = frequencyList.FindIndex(x => x.CharList.Exists(y => y == ch));
                            frequencyList[index].Value++;
                        }
                        else frequencyList.Add(new Node(ch, value));
                    }
                    if (streamReader.Peek() != -1) endLineValue++;
                }
                //TODO: понять как определять наличие символа переноса строки для побитового сравнения

                if (endLineValue != 0)
                {
                    frequencyList.Add(new Node('\n', endLineValue));
                }

                return frequencyList.OrderBy(x => x.Value).ToList();
            }
        }



        public static List<Node> MakeHuffmanTree(List<Node> nodes) //трансформирует частотный список в бинарное дерево
        {
            List<Node> tree = nodes;

            if (tree.Count <= 1) return nodes;

            Node nodeZero = new Node();
            Node nodeOne = new Node();
            Node parrent = new Node();

            while (true)
            {
                nodeZero = tree[0];
                nodeOne = tree[1];
                List<char> charList = new List<char>(nodeZero.CharList.Concat(nodeOne.CharList)); //объеденяем символы двух элементов для родителя
                int value = nodeZero.Value + nodeOne.Value;

                parrent = new Node(charList, value, nodeZero, nodeOne);

                ref Node parrentRef = ref parrent;  // добавляем ссылку на родительский элемент

                nodeZero.Parrent = parrentRef;
                nodeOne.Parrent = parrentRef;

                tree.Remove(tree[0]);
                tree.Remove(tree[0]);
                tree.Add(parrent);

                tree = tree.OrderBy(x => x.Value).ToList();
                if (tree.Count == 1) break;
            }
            return tree;
        }

        internal static List<Key> GetKeys(List<Node> huffmanTree) //сюда должен приходить список с 1 элементом
        {
            List<Key> keys = new List<Key>();

            List<bool> code = new List<bool>();


            return FindKeys(huffmanTree[0], code, keys);

        }

        //рекурсивный метод, который добирается до символа и присваивает ему код
        private static List<Key> FindKeys(Node element, List<bool> code, List<Key> keys)
        {
            if (element.NodeZero != null)
            {
                if (element.NodeZero.Flag) element.NodeZero = null; //удаляем проверенные элементы
            }
            if (element.NodeOne != null)
            {
                if (element.NodeOne.Flag) element.NodeOne = null;
            }

            if (element.NodeZero == null && element.NodeOne == null)//если обе ветки пустые - возможно мы добрались до ключевого элемента
            {
                if (element.CharList.Count == 1) // в кодовую таблицу добавляем элеметы только с одним символом
                {

                    List<bool> elementCode = DeepClone(code);
                    Key key = new Key(element.CharList[0], elementCode);
                    keys.Add(key);
                    numberOfBits += key.BoolList.Count * element.Value;
                }
                element.Flag = true;
                if (element.Parrent == null)
                {
                    return keys;
                } // это означает, что мы в самом корне дерева и можно закончить проверку
                else
                {
                    code.RemoveAt(code.Count - 1); //возвращаем лист кодировки без последнего символа, так как мы спускаеся вниз
                    return FindKeys(element.Parrent, code, keys); //а если родитель есть - значит ищим дальше
                }
            }
            else if (element.NodeZero != null)
            {
                bool zero = false; // добавляем ноль к коду
                code.Add(zero);
                return FindKeys(element.NodeZero, code, keys);
            }
            else
            {
                bool one = true; // добавляем еденицу к коду элемента
                code.Add(one);
                return FindKeys(element.NodeOne, code, keys);
            }

        }



        //метод глубокого клонирования с 
        //https://stackoverflow.com/questions/129389/how-do-you-do-a-deep-copy-of-an-object-in-net-c-specifically
        //без него в списках List<bool> твориться какая-то дичь
        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }
    }
}
