using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace partOne
{
    internal class QuickSort
    {
        public void Sort(List<int> array, int left, int right, LoggerForQuick logger)
        {
            if (left < right)
            {
                logger.Log($"QuickSort вызывается для диапазона [{left}, {right}].");

                int pivotIndex = Partition(array, left, right, logger);

                logger.Log($"Опорный элемент установлен на индекс {pivotIndex} со значением {array[pivotIndex]}.");
                Sort(array, left, pivotIndex - 1, logger);  // Сортируем левую часть
                Sort(array, pivotIndex + 1, right, logger); // Сортируем правую часть
            }
        }

        static int Partition(List<int> array, int left, int right, LoggerForQuick logger)
        {
            int pivot = array[right]; // Выбираем последний элемент как опорный
            logger.Log($"Выбран опорный элемент: {pivot} (индекс {right}).");
            int i = left - 1;

            for (int j = left; j < right; j++)
            {
                logger.Log($"Сравниваем array[{j}] = {array[j]} с pivot = {pivot}.");
                if (array[j] <= pivot)
                {
                    i++;
                    Swap(array, i, j);
                    logger.Log($"Обмен: array[{i}] = {array[i]}, array[{j}] = {array[j]} -> {string.Join(", ", array)}");
                }
            }
            Swap(array, i + 1, right); // Перемещаем опорный элемент на правильное место
            logger.Log($"Обмен опорного элемента: array[{i + 1}] = {array[i + 1]}, array[{right}] = {array[right]} -> {string.Join(", ", array)}");

            return i + 1; // Возвращаем индекс опорного элемента
        }
        static void Swap(List<int> array, int i, int j)
        {
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
}
