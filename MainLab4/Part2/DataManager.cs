using System.Data;
using System.IO;
using System.Windows.Controls;
using OfficeOpenXml;

namespace Part2
{
    public class DataManager
    {
        public void LoadExcelData(string filePath, DataGrid dataGrid, ListBox keyAttributesListBox)
        {
            LoadDataInDataTable(filePath, dataGrid, keyAttributesListBox);
        }

        public void LoadCsvData(string filePath, ListBox keyAttributesListBox)
        {
            var headers = File.ReadLines(filePath).First().Split(',');
            keyAttributesListBox.ItemsSource = headers;
        }

        private void LoadDataInDataTable(string filePath, DataGrid dataGrid, ListBox keyAttributesListBox)
        {
            FileInfo fileInfo = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // Предполагается, что данные находятся на первом листе
                DataTable dataTable = new DataTable();

                // Загрузка заголовков столбцов
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    dataTable.Columns.Add(worksheet.Cells[1, col].Text);
                }

                // Загрузка данных
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    DataRow dataRow = dataTable.NewRow();
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        dataRow[col - 1] = worksheet.Cells[row, col].Text;
                    }
                    dataTable.Rows.Add(dataRow);
                }

                dataGrid.ItemsSource = dataTable.DefaultView;
                keyAttributesListBox.ItemsSource = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
            }
        }

        public void ClearData(TextBox filePathBox, ListBox keyAttributesListBox, ComboBox sortMethodComboBox, TextBox delayBox, ListBox logBox, DataGrid dataGrid)
        {
            filePathBox.Text = string.Empty;
            keyAttributesListBox.ItemsSource = null;
            sortMethodComboBox.SelectedItem = null;
            delayBox.Text = string.Empty;
            logBox.Items.Clear();
            dataGrid.ItemsSource = null;
        }
    }
}