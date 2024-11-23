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
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using OfficeOpenXml;
using CsvHelper;
using System.Globalization;
using System.IO;

namespace ExternalSortingApp
{
    public partial class MainWindow : Window
    {
        private DataTable dataTable;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Excel Files|*.xlsx;*.xls|CSV Files|*.csv";

            if (dialog.ShowDialog() == true)
            {
                FileLabel.Text = dialog.FileName;
                dataTable = DataFileReader.ReadData(dialog.FileName);
                KeyAttributeComboBox.ItemsSource = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
            }
        }

        private void DelayTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (DelayTextBox.Text == "Delay (ms)")
            {
                DelayTextBox.Text = "";
            }
        }

        private void StartSorting_Click(object sender, RoutedEventArgs e)
        {
            if (dataTable == null)
            {
                MessageBox.Show("Please select a file first.");
                return;
            }

            string sortMethod = GetSelectedSortMethod();
            string keyAttribute = KeyAttributeComboBox.SelectedItem as string;
            if (string.IsNullOrEmpty(keyAttribute))
            {
                MessageBox.Show("Please select a key attribute.");
                return;
            }
            int delay = int.Parse(DelayTextBox.Text);

            Task.Run(() => SortData(sortMethod, keyAttribute, delay));
        }

        private string GetSelectedSortMethod()
        {
            foreach (RadioButton rb in SortMethodPanel.Children.OfType<RadioButton>())
            {
                if (rb.IsChecked == true)
                {
                    return rb.Content.ToString();
                }
            }
            return null;
        }

        private void SortData(string sortMethod, string keyAttribute, int delay)
        {
            DataTable sortedDataTable = null;

            if (sortMethod == "Direct Method")
            {
                sortedDataTable = DirectSort(dataTable, keyAttribute);
            }
            else if (sortMethod == "Natural Method")
            {
                sortedDataTable = NaturalSort(dataTable, keyAttribute);
            }
            else if (sortMethod == "Multi-way Merge")
            {
                sortedDataTable = MultiwayMergeSort(dataTable, keyAttribute);
            }

            DisplaySortingSteps(sortedDataTable, keyAttribute, delay);
            LogOperation($"Sorted data by {keyAttribute} using {sortMethod} method");
        }

        private DataTable DirectSort(DataTable dataTable, string keyAttribute)
        {
            return new DataView(dataTable).ToTable(true, keyAttribute);
        }

        private DataTable NaturalSort(DataTable dataTable, string keyAttribute)
        {
            // Implement natural sorting logic here
            return dataTable;
        }

        private DataTable MultiwayMergeSort(DataTable dataTable, string keyAttribute)
        {
            // Implement multi-way merge sorting logic here
            return dataTable;
        }

        private void DisplaySortingSteps(DataTable sortedDataTable, string keyAttribute, int delay)
        {
            foreach (DataRow row in sortedDataTable.Rows)
            {
                Dispatcher.Invoke(() =>
                {
                    SortingStepsTextBlock.Text += $"{row[keyAttribute]}\n";
                });
                Task.Delay(delay).Wait();
            }
        }

        private void LogOperation(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBlock.Text += $"{message}\n";
            });
        }
    }

    public class DataFileReader
    {
        public static DataTable ReadData(string filePath)
        {
            DataTable dataTable = new DataTable();

            if (filePath.EndsWith(".xlsx"))
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    dataTable.Load((IDataReader)worksheet.Cells);
                }
            }
            else if (filePath.EndsWith(".csv"))
            {
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    dataTable.Load((IDataReader)csv.GetRecords<dynamic>().GetEnumerator());
                }
            }
            else
            {
                throw new NotSupportedException("Unsupported file format");
            }

            return dataTable;
        }
    }
}
