using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace partOne
{
    internal class MergeSort
    {
        public void Sort(List<int> array, int left, int right, LoggerForQuick logger)
        {
            if (left < right)
            {
                int mid = (left + right) / 2;

                logger.Log($"Разделяем массив: [{left}:{mid}] и [{mid + 1}:{right}]");

                // Рекурсивно сортируем первую и вторую половины
                Sort(array, left, mid, logger);
                Sort(array, mid + 1, right, logger);

                // Сливаем отсортированные части
                Merge(array, left, mid, right, logger);
            }
        }

        public static void Merge(List<int> array, int left, int mid, int right, LoggerForQuick logger)
        {
            logger.Log($"Сливаем массивы: [{left}:{mid}] и [{mid + 1}:{right}]");

            int n1 = mid - left + 1;
            int n2 = right - mid;

            int[] leftArray = new int[n1];
            int[] rightArray = new int[n2];

            // Копируем данные во временные массивы
            for (int i = 0; i < n1; i++)
                leftArray[i] = array[left + i];
            for (int j = 0; j < n2; j++)
                rightArray[j] = array[mid + 1 + j];

            logger.Log("Левая часть: " + string.Join(", ", leftArray));
            logger.Log("Правая часть: " + string.Join(", ", rightArray));

            int iLeft = 0, iRight = 0;
            int k = left;

            // Сливаем временные массивы обратно в основной массив
            while (iLeft < n1 && iRight < n2)
            {
                if (leftArray[iLeft] <= rightArray[iRight])
                {
                    array[k] = leftArray[iLeft];
                    iLeft++;
                }
                else
                {
                    array[k] = rightArray[iRight];
                    iRight++;
                }
                logger.Log($"Изменение массива: {string.Join(", ", array)}");
                k++;
            }

            // Копируем оставшиеся элементы из leftArray, если они есть
            while (iLeft < n1)
            {
                array[k] = leftArray[iLeft];
                iLeft++;
                k++;
                logger.Log($"Копируем остаток из левой части: {string.Join(", ", array)}");
            }

            // Копируем оставшиеся элементы из rightArray, если они есть
            while (iRight < n2)
            {
                array[k] = rightArray[iRight];
                iRight++;
                k++;
                logger.Log($"Копируем остаток из правой части: {string.Join(", ", array)}");
            }
        }
    }
}
