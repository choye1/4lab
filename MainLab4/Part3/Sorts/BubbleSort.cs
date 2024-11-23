using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Part2.Sorts
{
    internal class BubbleSort : InterfaceSort
    {
        public List<string> Sort(List<string> inputData)
        {
            string[] inputdata = inputData.ToArray();

            for(int i = 0;i < inputdata.Length; i++)
            {
                bool swapped = false;
                for (int j = 0; j < inputdata.Length - i - 1; j++)
                {
                    if (inputdata[j] != inputdata[j+1] &&  CompareWords(inputdata[j],inputdata[j+1]))
                    {
                        swapped = true;
                        string t = inputdata[j+1];
                        inputdata[j+1] = inputdata[j];
                        inputdata[j] = t;
                        //меняем местами
                    }
                }
                if (!swapped) { break; }

            }

            return inputdata.ToList();
        }

        private bool CompareWords(string first, string second) //0 - word1 > word2, 1 - word2 > word1 
        {
            if (first.Length <= 1) return false;
            else if (second.Length <= 1) return true;

            if (first.ToArray()[0] == second.ToArray()[0])
            {
                return CompareWords(first.Substring(1,first.Length-1), second.Substring(1, second.Length - 1));
            }
            else
            {
                return ((byte)first[0]) > ((byte)second[0]);
            }

        }
    }
}
