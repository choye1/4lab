using Part2.Sorts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Part3.Sorts
{
    class RadixSort : InterfaceSort
    {
        public List<string> Sort(List<string> inputData)
        {
            string[] arr = inputData.ToArray();

            int maxLength = arr.Select(s => s.Length).Max();

            for (int i = maxLength - 1; i >= 0; i--)
            {
                var groups = new System.Collections.Generic.List<string>[256];
                for (int j = 0; j < 256; j++)
                {
                    groups[j] = new System.Collections.Generic.List<string>();
                }

                foreach (string s in arr)
                {
                    if (i >= s.Length)
                    {
                        groups[0].Add(s);
                    }
                    else
                    {
                        groups[s[i]].Add(s);
                    }
                }

                int index = 0;
                for (int j = 0; j < 256; j++)
                {
                    foreach (string s in groups[j])
                    {
                        arr[index++] = s;
                    }
                }
            }

            return arr.ToList();


        }

    }

}

