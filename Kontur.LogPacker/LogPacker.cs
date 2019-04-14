using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Kontur.LogPacker
{
    internal class LogPacker
    {
        public void Compress(string fileName, Stream outputStream)
        {
            //using (var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal, true))
            //    inputStream.CopyTo(gzipStream);

            List<Node> frequencyList = new List<Node>();
            using (var inputStream = File.OpenRead(fileName)) //первый раз открываем файл для анализа
            {
                frequencyList = CompressFactory.MakeFrequencyList(inputStream);
            }
            GetFreqList(frequencyList);

            List<Node> huffmanTree = CompressFactory.MakeHuffmanTree(frequencyList);

            List<Key> keys = CompressFactory.GetKeys(huffmanTree);



            Console.WriteLine("Кодировка:");

            keys = keys.OrderBy(x => x.BoolList.Count).ToList();

            foreach (Key key in keys)
            {
                System.Console.WriteLine(key.Symbol + "    " + GetCode(key.BoolList));
            }

            using (var inputStream = File.OpenRead(fileName)) // второй раз открываем файл для кодировки
            {
                CompressFactory.EncodeFile(inputStream, keys, outputStream);
            }


        }

        private void GetFreqList(List<Node> frequencyList)
        {

            Console.WriteLine("Частотный список:");

            foreach (Node n in frequencyList)
            {
                Console.WriteLine(n.CharList[0] + "   " + n.Value);
            }
            Console.WriteLine();
        }



        private string GetCode(List<bool> boolList)
        {




            StringBuilder code = new StringBuilder();
            foreach (bool b in boolList)
            {
                if (b) code.Append(1);
                else code.Append(0);

            }
            return code.ToString();
        }

        public void Decompress(Stream inputStream, Stream outputStream)
        {
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress, true))
                gzipStream.CopyTo(outputStream);
        }
    }
}
