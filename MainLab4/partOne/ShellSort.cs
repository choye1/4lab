using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace partOne
{
    internal class ShellSort
    {
        public List<int> Sort(List<int> list, LoggerForShell logger)
        {
            int n = list.Count;
            logger.Log("Начало сортировки. Исходный список: " + string.Join(", ", list));

            // Начинаем с большого зазора, затем уменьшаем его
            for (int gap = n / 2; gap > 0; gap /= 2)
            {
                logger.Log($"Текущий зазор: {gap}");

                // Выполняем сортировку вставками для элементов на расстоянии «зазор»
                for (int i = gap; i < n; i += 1)
                {
                    // Сохраняем текущий элемент для сравнения
                    int temp = list[i];
                    logger.Log($"Сравнение элемента {temp} (индекс {i})");

                    int j;

                    // Перемещаем элементы, которые больше temp, вправо на позиции «зазор»
                    for (j = i; j >= gap && list[j - gap] > temp; j -= gap)
                    {
                        logger.Log($"Перемещение элемента {list[j - gap]} с индекса {j - gap} на индекс {j}");
                        list[j] = list[j - gap];
                    }

                    // Вставляем сохранённый элемент на правильную позицию
                    list[j] = temp;
                    logger.Log($"Вставка элемента {temp} на позицию {j}");
                }

                logger.Log("Список после обработки зазора " + gap + ": " + string.Join(", ", list));
            }

            logger.Log("Сортировка завершена. Отсортированный список: " + string.Join(", ", list));
            return list;
        }
    }

}
