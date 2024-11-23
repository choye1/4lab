using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace partOne
{
    internal class Logger
    {
        private int step = 0;
        
        public void LogStep(List<int> list, string action, int min, int swapElem)
        {
            step++;
            //вывод на экран 
            string filePath = "log.txt";

            // Открываем файл для записи (режим добавления)
            using (StreamWriter writer = new StreamWriter(filePath, append: true))
            {
                string logMessage = $"Шаг {step}: ";

                // В зависимости от действия, выводим соответствующее сообщение
                switch (action)
                {
                    case "findMin":
                        if (step == 1)
                        {
                            logMessage += $"Ищем минимальный элемент в списк: {string.Join(", ", list)}. Это {min}";
                            break;
                        }
                        logMessage += $"Ищем минимальный элемент в неотсортированной части списка: {string.Join(", ", list)}. Это {min}";
                        break;

                    case "swap":
                        if (step == 2)
                        {
                            logMessage += $"Меняем местами минимальный элемент {min} и первый элемент списка {swapElem}.\nПолучаем список: {string.Join(", ", list)}.\nПереходим к следующему шагу";
                            break;
                        }
                        logMessage += $"Меняем местами минимальный элемент {min} и первый элемент после отсортированной части {swapElem}.\nПолучаем список: {string.Join(", ", list)}.\nПереходим к следующему шагу";
                        break;

                    case "sorted":
                        logMessage += $"Список отсортирован!";
                        step = 0;
                        break;

                    default:
                        logMessage += $"Неизвестное действие: {string.Join(", ", list)}";
                        break;
                }
                writer.WriteLine(logMessage);
                writer.Flush();
            }
        }
    }
}
