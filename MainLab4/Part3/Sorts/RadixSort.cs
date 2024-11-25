using Part2.Sorts;
using Part3.Assist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Part3.Sorts
{
    class RadixSort : InterfaceSort
    {
        LoggerForSort ls = new LoggerForSort();

        public List<string> Sort(List<string> inputData)
        {
            ls.ClearLog();
            string[] arr = inputData.ToArray();

            ls.WriteLog("inAr", arr);

            int maxLength = arr.Select(s => s.Length).Max();
            ls.WriteLog("maxLen", maxLength);

            for (int i = maxLength - 1; i >= 0; i--)
            {
                ls.WriteLog("curLet " + (i+1));
                List<string>[] groups = new List<string>[256];
                for (int j = 0; j < 256; j++)
                {
                    groups[j] = new List<string>();
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

                WriteGroup(GetRealGroups(groups));
            }

            ls.WriteLog("endAr", arr);
            return arr.ToList();
        }

        private List<List<string>> GetRealGroups(List<string>[] groups)
        {
            List<List<string>> result = new List<List<string>>();
            foreach (List<string> s in groups)
            {
                if (s.Count > 0)
                {
                    result.Add(s);
                }
            }

            return result;
        }

        private void WriteGroup(List<List<string>> groups)
        {
            int index = 1;
            StringBuilder sb = new StringBuilder();
            foreach (List<string> s in groups)
            {
                sb.Append(index + ":" + GetString(s));
                index++;
            }

            ls.WriteLog("curGroups " + sb.ToString());
        }

        private string GetString(List<string> st)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in st)
            {
                sb.Append(s + ",");
            }

            sb.Append(' ');
            return sb.ToString();

        }
    }

}

