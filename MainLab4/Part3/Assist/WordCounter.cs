using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Part3
{
    internal class WordCounter<T>
    {
        List<T> data = new List<T>();
        public WordCounter(List<T> data)
        {
            this.data = data;
        }

        Dictionary<T, int> wordAndCount = new Dictionary<T, int>();

        public Dictionary<T, int> Main()
        {
            foreach (T item in data)
            {
                if (wordAndCount.ContainsKey(item))
                {
                    wordAndCount[item]++;
                }
                else
                {
                    wordAndCount[item] = 1;
                }
            }


            return wordAndCount;
        }

    }
}
