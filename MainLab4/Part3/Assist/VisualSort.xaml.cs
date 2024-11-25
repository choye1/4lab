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
    /// Логика взаимодействия для VisualSort.xaml
    /// </summary>
    public partial class VisualSort : Window
    {
        LogReader logReader = new LogReader();
        public VisualSort()
        {
            InitializeComponent();
            ParseStep(logReader.GetCurrent());
        }

        



        private void ParseStep(string[] step)
        {
            switch (step[0])
            {
                case "inAr":
                    tbStartArray.Text = GiveStringStep(step);
                    tbCurrentArray.Text = "";
                    tbFirst.Text = "";
                    tbSecond.Text = "";
                    tbFlip.Text = "";
                    tbFinalAr.Text = "";
                    break;
                case "curAr":
                    tbCurrentArray.Text = GiveStringStep(step);
                    break;
                case "cmpWr":
                    tbFirst.Text = step[1];
                    tbSecond.Text = step[2];
                    tbFlip.Text = step[3] + 
                        (step[3] == "да" ? 
                        ", так как " + step[1] + " по лексикографическому порядку стоит перед " + step [2] :
                        ", так как " + step[1] + " по лексикографическому порядку стоит после " + step[2]);
                    tbCurrentArray.Text = GiveStringStep(step, 3);
                    break;
                case "endAr":
                    tbCurrentArray.Text = GiveStringStep(step);
                    tbFinalAr.Text = GiveStringStep(step);
                    break;
            }
        }




        private string GiveStringStep(string[] step)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 1; i < step.Length; i++)
            {
                stringBuilder.Append(step[i] + " "); 
            }

            return stringBuilder.ToString();
        }

        private string GiveStringStep(string[] step, int skip)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 1 + skip; i < step.Length; i++)
            {
                stringBuilder.Append(step[i] + " ");
            }

            return stringBuilder.ToString();
        }

        private void BackStep(object sender, RoutedEventArgs e)
        {
            ParseStep(logReader.GetBack());
        }

        private void NextStep(object sender, RoutedEventArgs e)
        {
            ParseStep(logReader.GetNext());
        }

        private void StartStop(object sender, RoutedEventArgs e)
        {


        }
    }
}
