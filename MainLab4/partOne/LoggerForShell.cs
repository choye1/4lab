using System;
using System.Collections.Generic;
using System.IO;

namespace partOne
{
    internal class LoggerForShell
    {
        private readonly string _filePath;

        public LoggerForShell(string filePath)
        {
            _filePath = filePath;
            // Очищаем файл при создании логгера
            File.WriteAllText(_filePath, "Лог сортировки:\n");
        }

        public void Log(string message)
        {
            // Записываем сообщение в файл
            File.AppendAllText(_filePath, message + "\n");
        }
    }
}