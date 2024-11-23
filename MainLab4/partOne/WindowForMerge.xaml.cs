using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace partOne
{
    /// <summary>
    /// Логика взаимодействия для WindowForMerge.xaml
    /// </summary>
    public partial class WindowForMerge : Window
    {
        public WindowForMerge()
        {
            InitializeComponent();
        }
        private static string filePath = "log.txt";
        private LoggerForQuick logger = new LoggerForQuick(filePath);
        private static List<int> list = new List<int>();
        private MergeSort mergeSort = new MergeSort();
        private void AddList(object sender, RoutedEventArgs e)
        {
            list.Clear();
            using (StreamWriter writer = new StreamWriter(filePath, append: false)) { }

            //List<string> data = List.Text.Trim().Split(' ');
            for (int i = 0; i < List.Text.Length; i++)
            {
                list.Add(int.Parse(List.Text[i].ToString()));
            }
            mergeSort.Sort(list, 0, list.Count - 1, logger);
        }

        private async void Explain(object sender, RoutedEventArgs e)
        {
            screen.Text = string.Empty;

            using (StreamReader sr = new StreamReader(filePath))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    screen.Text += line;
                    screen.Text += "\n";
                    await Task.Delay(int.Parse(Delay.Text) * 1000);
                }

            }
        }
    }
}
