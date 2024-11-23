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

            byte[] Gbytes = File.ReadAllBytes(".\file.txt");
            for (int i = 0; i < Gbytes.Length; i++)
            {
                bool swapped = false; // <-- добавлено
                for (int j = 0; j < Gbytes.Length - i - 1; j++)
                {
                    if (Gbytes[j] > Gbytes[j + 1])
                    {
                        swapped = true; // <-- добавлено
                        byte t = Gbytes[j + 1];
                        Gbytes[j + 1] = Gbytes[j];
                        Gbytes[j] = t;
                    }
                }
                if (!swapped) break; // <-- добавлено
            }
            throw new Exception("Не реализованно.");
            
        }
    }
}
