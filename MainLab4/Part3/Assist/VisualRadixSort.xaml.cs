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
        LogReader lR = new LogReader();

        public VisualRadixSort()
        {
            InitializeComponent();
            ParseStep(lR.GetCurrent());
        }
        private void ParseStep(string[] step)
        {
            switch (step[0])
            {
                case "inAr":
                    tbStartArray.Text = GiveStringStep(step);
                    tbFinalAr.Text = "";
                    break;

                case "endAr":
                    tbFinalAr.Text = GiveStringStep(step);
                    break;

                case "curLet":
                    tbCurLet.Text = step[1];
                    break;

                case "maxLen":
                    tbMaxLen.Text = step[1];
                    break;

                case "curGroups":
                    WriteCurGroups(step);
                    break;
            }
        }

        private void WriteCurGroups(string[] step)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < step.Length; i++)
            {
                sb.Append(step[i] + "\n");
            }
            tbCurGroups.Text = sb.ToString();
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

        private void Next(object sender, RoutedEventArgs e)
        {
            try
            {
                ParseStep(lR.GetNext());
                ParseStep(lR.GetNext());
            }
            catch { }

        }

        private void Back(object sender, RoutedEventArgs e)
        {
            try
            {
                ParseStep(lR.GetBack());
                ParseStep(lR.GetBack());
            }
            catch { }

        }
    }
}
