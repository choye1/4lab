using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Part3.Assist
{

    /* Структура лога:
     * Входной массив с данными через " "
     * [Тип строки] Данные
     * 
     * Под типом строки понимается: 
     *      inAr - входной массив
     *      curAr - текущее состояние массива
     *      cmpWr - Сравнение слов             [] [два слова] [меняем? (да/нет)]
     *      endAr - готовый массив
     *      
     * 
     * Данные вводятся через пробел.
     * 
    */

    class LoggerForSort
    {
        private static string path = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString()).ToString() + "\\log.txt";
        public void WriteLog(string message)
        {
            File.AppendAllText(path, message + "\n");
        }

        public void WriteLog<T1, T2>(T1 mod, T2 msg)
        {
            File.AppendAllText(path, mod + " " + msg + "\n");
        }

        public void WriteLog<T>(T[] values)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T value in values)
            {
                sb.Append(value + " ");
            }

            WriteLog(sb.ToString());
        }

        public void WriteLog<T1, T2>(T1 modificator, T2[] values)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(modificator + " ");
            foreach (T2 value in values)
            {
                sb.Append(value + " ");
            }

            WriteLog(sb.ToString());
        }


        public void ClearLog()
        {
            File.Delete(path);
            File.Create(path).Close();
        }

    }
}
