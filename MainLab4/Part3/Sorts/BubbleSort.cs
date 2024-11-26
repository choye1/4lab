using Part3.Assist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Part2.Sorts
{
    internal class BubbleSort : InterfaceSort
    {
        LoggerForSort logger = new LoggerForSort();
        public List<string> Sort(List<string> inputData)
        {
            logger.ClearLog();

            string[] inputdata = inputData.ToArray();
            logger.WriteLog("inAr", inputdata);

            for (int i = 0; i < inputdata.Length; i++)
            {

                bool swapped = false;
                for (int j = 0; j < inputdata.Length - i - 1; j++)
                {

                    if (inputdata[j] != inputdata[j + 1] && CompareWords(inputdata[j], inputdata[j + 1]))
                    {
                        logger.WriteLog("cmpWr", inputdata[j] + " " + inputdata[j + 1] + " Да " + GetString(inputdata).ToString());


                        swapped = true;
                        string t = inputdata[j + 1];
                        inputdata[j + 1] = inputdata[j];
                        inputdata[j] = t;
                        //меняем местами
                    }

                    else
                    {
                        logger.WriteLog("cmpWr", inputdata[j] + " " + inputdata[j + 1] + " Нет " + GetString(inputdata).ToString());
                    }


                }
                if (!swapped) { break; }

            }


            logger.WriteLog("endAr", inputdata);

            return inputdata.ToList();
        }

        private bool CompareWords(string first, string second) //0 - word1 > word2, 1 - word2 > word1 
        {
            if (first.Length <= 1 && (byte)first[0] < (byte)second[0]) return false;
            else if (second.Length <= 1 && (byte)second[0] < (byte)first[0]) return true;
            else if (first.Length <= 1 && second.Length <= 1) return false;
            if (first.ToArray()[0] == second.ToArray()[0])
            {
                return CompareWords(first.Substring(1, first.Length - 1), second.Substring(1, second.Length - 1));
            }
            else
            {
                return ((byte)first[0]) > ((byte)second[0]);
            }

        }

        private string GetString<T>(T[] val)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T t in val)
            {
                sb.Append(t + " ");
            }

            return sb.ToString();
        }

    }
}
