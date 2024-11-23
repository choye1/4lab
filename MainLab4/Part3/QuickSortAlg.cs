using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Part3
{
    internal class QuickSortAlg
    {
        static void QuickSort(string[] array, int low, int high)
        {
            if (low < high)
            {
                int pi = Partition(array, low, high);

                QuickSort(array, low, pi - 1);
                QuickSort(array, pi + 1, high);
            }
        }

        static int Partition(string[] array, int low, int high)
        {
            string pivot = array[high];
            int i = (low - 1);

            for (int j = low; j < high; j++)
            {
                if (string.Compare(array[j], pivot, StringComparison.Ordinal) <= 0)
                {
                    i++;
                    Swap(array, i, j);
                }
            }
            Swap(array, i + 1, high);
            return (i + 1);
        }

        static void Swap(string[] array, int i, int j)
        {
            string temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
}
