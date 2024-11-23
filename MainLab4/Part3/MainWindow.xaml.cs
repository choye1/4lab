using Microsoft.Win32;
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
            string path = "";
            string typeSort = "";
            try
            {
                path = tbSelectedFile.Text;
                typeSort = CbSortAlg.SelectedItem.ToString();
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
        }

        private void Main(string path, string typeSort)
        {
            List<string> inputData = new List<string>();
            List<string> result = new List<string>();

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
                result = StartSort(inputData,typeSort);
            }
            catch
            {
                throw new Exception("Ошибка при сортировке");
            }
        }

        private List<string> StartSort(List<string> inputData,string typeSort)
        {


            return null;
        }



        private List<string> ParseFile(string path)
        {
            string[] a = { "abc ", "xsaf", "asdfqegf \n", "qweq\r\n" };//File.ReadAllLines(path);
            StringBuilder sb = new StringBuilder();
            foreach (string line in a)
            {
                sb.Append(line.Replace("\n", " ").Replace("\r", " ").Replace(" ", "") + " ");
            }

            string input = RemoveNonLettersAndSpaces(sb.ToString());
            List<string> inp = input.Split(" ").ToList();
            inp.RemoveAt(inp.Count - 1);
            return inp;
        }

        static string RemoveNonLettersAndSpaces(string input)
        {
            // Используем регулярное выражение для удаления всех символов, кроме букв и пробелов
            return Regex.Replace(input, @"[^a-zA-Zа-яА-Я\s]", "");
        }
    }
}