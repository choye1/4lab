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
        public int cursor = 0;  //указывает, на какой строке мы сейчас

        public string[] GetBack()
        {
            try
            {
                if (cursor <= 1)
                {
                    MessageBox.Show("Вы в самом начале!");
                    throw new Exception();

                }
                else
                {
                    var output = log[cursor - 1].Replace("\n", "").Replace("\r", "").Split(" ");
                    cursor--;
                    return output;
                }
            }
            catch
            {
                cursor = 1;
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
                var output = log[cursor + 1].Replace("\n", "").Replace("\r", "").Split(" ");
                cursor ++;
                return output;
            }
            catch
            {
                cursor = log.Length-1;
                MessageBox.Show("Вы дошли до конца!");
                throw new Exception();
                return new[] {"0","0","0" };
            }
        }

    }
}
