using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
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
        private CancellationTokenSource cancellationTokenSource;

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

        private void tbCurGroups_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        bool isStarted = false;
        private void StartStop(object sender, RoutedEventArgs e)
        {
            if (isStarted)
            {
                Stop();
            }
            else
            {
                Start(e);
            }
        }
        private async void Start(RoutedEventArgs e)
        {
            cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await Timer_Tick(cancellationTokenSource.Token, e);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void Stop()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                isStarted = false;
            }
        }


        private async Task Timer_Tick(CancellationToken token, RoutedEventArgs e)
        {
            isStarted = true;

            while (isStarted)
            {
                try
                {
                    double.TryParse(tbStopTime.Text, out double time);
                    ParseStep(lR.GetNext());
                    ParseStep(lR.GetNext());
                    await Task.Delay(Convert.ToInt32(time * 1000));
                }
                catch
                {
                    cancellationTokenSource.Cancel();
                    isStarted = false;
                }
            }
        }

    }
}
