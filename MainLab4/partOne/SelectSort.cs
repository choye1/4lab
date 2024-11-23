using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace partOne
{
    internal class SelectSort
    {
        private List<int> list { get; set; }
        private Logger logger { get; set; }

        public SelectSort(List<int> list)
        {
            this.list = list;
            this.logger = new Logger();
        }

        public List<int> Sort()
        {
            int n = list.Count;

            for (int i = 0; i < n - 1; i++)
            {
                // Находим минимальный элемент в неотсортированной части массива
                int minIndex = i;
                int swapElem = 0;
                int min = 0;
                for (int j = i + 1; j < n; j++)
                {
                    if (list[j] < list[minIndex])
                    {
                        minIndex = j;
                    }
                }
                logger.LogStep(list, "findMin", list[minIndex], 0);

                // Меняем найденный минимальный элемент с первым элементом неотсортированной части
                if (minIndex != i)
                {
                    min = list[minIndex];
                    int temp = list[i];
                    list[i] = list[minIndex];
                    list[minIndex] = temp;
                    swapElem = temp;
                }
                logger.LogStep(list, "swap", min, swapElem);
            }
            logger.LogStep(list, "sorted", 0, 0);
            return list;
        }
    }
}
