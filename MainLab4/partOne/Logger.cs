using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace partOne
{
    internal class Logger
    {
    
        private StreamWriter _writer;
        private string filePath;

        public Logger(string filePath)
        {
            this.filePath = filePath;
            // Очищаем файл при создании логгера
            File.WriteAllText(filePath, "Лог сортировки:\n");
        }

        public void Log(string message)
        { 
            File.AppendAllText(filePath, message + "\n");
        }

       
        
    }
}
