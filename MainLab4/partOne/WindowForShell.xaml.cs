using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace partOne
{
    /// <summary>
    /// Логика взаимодействия для WindowForShell.xaml
    /// </summary>
    public partial class WindowForShell : Window
    {
        public WindowForShell()
        {
            InitializeComponent();
        }

        private static List<int> list = new List<int>();
        private ShellSort shellSort = new ShellSort();
        private static string filePath = "log.txt";
        LoggerForShell logger = new LoggerForShell(filePath);

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

        private void AddList(object sender, RoutedEventArgs e)
        {
            list.Clear();
            using (StreamWriter writer = new StreamWriter(filePath, append: false)) { }

            //List<string> data = List.Text.Trim().Split(' ');
            for (int i = 0; i < List.Text.Length; i++)
            {
                list.Add(int.Parse(List.Text[i].ToString()));
            }
            shellSort.Sort(list,logger);
        }
    }
}
