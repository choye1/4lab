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
using partOne;
using Part2;
using Part3;

namespace MainLab4
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

        private void Two(object sender, RoutedEventArgs e)
        {
           Part2.MainWindow win = new Part2.MainWindow();
           win.Show();

        }

        private void One(object sender, RoutedEventArgs e)
        {
            partOne.MainWindow win = new partOne.MainWindow();
            win.Show();
        }

        private void Three(object sender, RoutedEventArgs e)
        {
            Part3.MainWindow win = new Part3.MainWindow();
            win.Show();
        }
    }
}