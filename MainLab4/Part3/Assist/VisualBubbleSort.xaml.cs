using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Part3.Assist
{
    /// <summary>
    /// Логика взаимодействия для VisualSort.xaml
    /// </summary>
    public partial class VisualBubbleSort : Window
    {
        private CancellationTokenSource cancellationTokenSource;

        LogReader logReader = new LogReader();
        public VisualBubbleSort()
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
                        ", так как " + step[1] + " по лексикографическому порядку стоит перед " + step[2] :
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
            try
            {
                ParseStep(logReader.GetBack());
            }
            catch
            {

            }
        }

        private void NextStep(object sender, RoutedEventArgs e)
        {
            try
            {
                ParseStep(logReader.GetNext());
            }
            catch
            {
            }
        }


        bool isStarted = false;
        private  void StartStop(object sender, RoutedEventArgs e)
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
                    ParseStep(logReader.GetNext());
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
