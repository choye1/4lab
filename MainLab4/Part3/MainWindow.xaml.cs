using Microsoft.Win32;
using Part2.Sorts;
using Part3.Assist;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Part3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtReviewClick(object sender, RoutedEventArgs e)
        {
            // Создаем экземпляр OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Устанавливаем фильтр для файлов (например, только текстовые файлы)
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

            // Устанавливаем заголовок диалогового окна
            openFileDialog.Title = "Выберите файл";

            // Показываем диалоговое окно и проверяем, был ли выбран файл
            if (openFileDialog.ShowDialog() == true)
            {
                // Получаем путь к выбранному файлу
                string filePath = openFileDialog.FileName;

                // Отображаем путь к файлу в текстовом поле
                tbSelectedFile.Text = filePath;
            }
        }

        private void BtStartClick(object sender, RoutedEventArgs e)
        {
            ClearOut();
            string path = "";
            string typeSort = "";
            try
            {
                path = tbSelectedFile.Text;
                typeSort = ((ComboBoxItem)CbSortAlg.SelectedItem).Content.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Некорректное заполнение полей");
            }

            if (path != "" && typeSort != "")
            {
                try
                {
                    Main(path, typeSort);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            new VisualSort().Show();

        }

        private void Main(string path, string typeSort)
        {
            List<string> inputData = new List<string>();
            List<string> result = new List<string>();
            Dictionary<string, int> wordsAndCount = new Dictionary<string, int>();

            try
            {
                inputData = ParseFile(path);
            }
            catch
            {
                throw new Exception("Ошибка при парсинге файла");
            }
            try
            {
                result = StartSort(inputData, typeSort);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            try
            {
                WordCounter<string> wordCounter = new WordCounter<string>(result);
                wordsAndCount = wordCounter.Main();
            }
            catch
            {
                throw new Exception("Ошибка подсчёта слов");
            }

            try
            {
                WriteList(result);
                WriteLine("");
                WriteLine("");
                WriteDictionary(wordsAndCount);

            }
            catch
            {
                throw new Exception("Ошибка вывода");
            }
        }

        private List<string> StartSort(List<string> inputData, string typeSort)
        {
            List<string> result = new List<string>();
            switch (typeSort)
            {
                case "Modificated Bubble Sort":
                    result = StartBubbleSort(inputData);
                    break;
                case "ABC-Sort":
                    throw new Exception("Ветка в разработке");
                    break;
                default: throw new Exception("Такой сортировки пока нет");

            }

            return result;
        }

        private List<string> StartBubbleSort(List<string> inputData)
        {
            try
            {
                BubbleSort bs = new BubbleSort();
                return bs.Sort(inputData);
            }
            catch
            {
                throw new Exception("Ошибка при сортировке");
            }

        }



        private List<string> ParseFile(string path)
        {
            string[] a = File.ReadAllLines(path); //{ "abc ", "xsaf", "asdfqegf \n", "qweq\r\n" };//
            StringBuilder sb = new StringBuilder();

            foreach (string s in a)
            {
                string[] line = s.Split(" ");
                foreach (string words in line)
                {
                    string word = RemoveNonLettersAndSpaces(words);
                    sb.Append(word + " ");
                }
            }

            List<string> result = new List<string>();
            foreach (string s in sb.ToString().Split(" "))
            {
                if (s != "") result.Add(s);

            }

            return result;
        }

        static string RemoveNonLettersAndSpaces(string input)
        {
            // Используем регулярное выражение для удаления всех символов, кроме букв и пробелов
            return Regex.Replace(input, @"[^a-zA-Zа-яА-Я\s]", "");
        }

        private void WriteList<T>(List<T> data)
        {
            foreach (T item in data)
            {
                Write(item + " ");
            }

            WriteLine();
        }

        private void WriteDictionary<T1, T2>(Dictionary<T1, T2> data)
        {
            foreach (T1 item in data.Keys)
            {
                WriteLine(item + " = " + data[item]);
            }
        }


        private void WriteLine<T>(T message)
        {
            tbOut.Text += message + "\n";
        }
        private void WriteLine()
        {
            tbOut.Text += "\n";
        }

        private void Write<T>(T message)
        {
            tbOut.Text += message;
        }

        private void ClearOut()
        {
            tbOut.Text = "";
        }






    }
}