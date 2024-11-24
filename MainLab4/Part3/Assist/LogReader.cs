using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Part3.Assist
{
    class LogReader
    {
        static string path = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString()).ToString() + "\\log.txt";
        string[] log = File.ReadAllLines(path);
        int cursor = 0;  //указывает, на какой строке мы сейчас

        public string[] GetBack()
        {
            try
            {
                return log[cursor --].Replace("\n", "").Replace("\r", "").Split(" ");
            }
            catch
            {
                cursor = 0;
                MessageBox.Show("Вы в самом начале!");
                return new[] { "0", "0", "0" };

            }

        }

        public string[] GetCurrent()
        {
            try
            {
                return log[cursor].Replace("\n", "").Replace("\r", "").Split(" ");
            }
            catch
            {
                MessageBox.Show("Пока нечего показывать");
                return new[] { "0", "0", "0" };

            }

        }

        public string[] GetNext()
        {
            try
            {
                return log[cursor++].Replace("\n", "").Replace("\r", "").Split(" ");
            }
            catch
            {
                cursor = log.Length-1;
                MessageBox.Show("Вы дошли до конца!");
                return new[] {"0","0","0" };
            }
        }

    }
}
