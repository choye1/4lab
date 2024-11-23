using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace partOne
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WindowForSelect : Window
    {
        public WindowForSelect()
        {
            InitializeComponent();
        }

        private static List<int> list = new List<int>();
        private SelectSort selectSort = new SelectSort(list);
        private string filePath = "log.txt";
        //private int count = 0;

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
            using (StreamWriter writer = new StreamWriter(filePath, append: false)){}

            //List<string> data = List.Text.Trim().Split(' ');
            for (int i = 0;i < List.Text.Length;i++) 
            {
                list.Add(int.Parse(List.Text[i].ToString()));
            }
            selectSort.Sort();
        }   
    }
}