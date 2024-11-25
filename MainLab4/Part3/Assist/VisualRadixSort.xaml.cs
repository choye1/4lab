using System;
using System.Collections.Generic;
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

namespace Part3.Assist
{
    /// <summary>
    /// Логика взаимодействия для VisualRadixSort.xaml
    /// </summary>
    public partial class VisualRadixSort : Window
    {

        private List<string> arr;
        private int currentStep;
        private int maxLength;
        private List<List<string>> bins;

        public VisualRadixSort()
        {
            InitializeComponent();
            arr = new List<string> { "apple", "banana", "cherry", "date", "fig", "grape" };
            currentStep = 0;
            maxLength = arr.Select(s => s.Length).Max();
            bins = Enumerable.Range(0, 256).Select(_ => new List<string>()).ToList();
            StepTextBlock.Text = "Шаг " + (currentStep + 1);
            UpdateBinsListBox();
        }
       private void UpdateBinsListBox()
        {
            BinsListBox.Items.Clear();
            for (int i = 0; i < bins.Count; i++)
            {
                if (bins[i].Count > 0)
                {
                    BinsListBox.Items.Add("Bin " + i + ": " + string.Join(", ", bins[i]));
                }
            }


        }

        private void Window_MouseDoubleClick(object sender,  EventArgs e)
        {
            if (currentStep < maxLength)
            {
                foreach (string s in arr)
                {
                    int index = currentStep < s.Length ? s[currentStep] : 0;
                    bins[index].Add(s);
                }

                int idx = 0;
                foreach (List<string> bin in bins)
                {
                    foreach (string s in bin)
                    {
                        arr[idx++] = s;
                    }
                }

                bins = Enumerable.Range(0, 256).Select(_ => new List<string>()).ToList();
                currentStep++;
                StepTextBlock.Text = "Шаг " + (currentStep + 1);
                UpdateBinsListBox();
            }
            else
            {
                MessageBox.Show("Сортировка завершена!", "Информация");
            }
        }
    }
}
