using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using OfficeOpenXml;

namespace Part2
{
    public partial class MainWindow : Window
    {
        private static string curDir = Directory.GetCurrentDirectory();
        private string _filePath = System.IO.Path.Combine(curDir, "sorted_result.xlsx");
        private List<TableState> _states = new List<TableState>();
        private int _currentStateIndex = -1;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isAutoPlaying = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _filePath = openFileDialog.FileName;
                FilePathBox.Text = _filePath;

                if (System.IO.Path.GetExtension(_filePath).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    LoadExcelData(_filePath);
                }
                else
                {
                    // Загрузить атрибуты из файла
                    var headers = File.ReadLines(_filePath).First().Split(',');
                    KeyAttributeComboBox.ItemsSource = headers;
                }
            }
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        private void GoForward_Click(object sender, RoutedEventArgs e)
        {
            GoForward();
        }

        private void LoadExcelData(string filePath)
        {
            LoadDataInDataTable(filePath);
        }

        private void LoadDataInDataTable(string filePath)
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
                KeyAttributeComboBox.ItemsSource = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
            }
        }

        private async void StartSort_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_filePath) || KeyAttributeComboBox.SelectedItem == null || SortMethodComboBox.SelectedItem == null)
            {
                MessageBox.Show("Заполните все поля перед началом сортировки.");
                return;
            }

            string sortMethod = ((ComboBoxItem)SortMethodComboBox.SelectedItem).Content.ToString();
            string key = KeyAttributeComboBox.SelectedItem.ToString();
            int delay = int.TryParse(DelayBox.Text, out int d) ? d : 500;

            LogBox.Items.Add("Начинаем сортировку...");

            await Task.Run(() => PerformSort(_filePath, key, sortMethod, delay));
        }

        private void PerformSort(string filePath, string key, string method, int delay)
        {
            try
            {
                List<string> lines = new List<string>();
                if (System.IO.Path.GetExtension(filePath).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    lines = LoadExcelDataAsXlsx(filePath);
                }
                else
                {
                    lines = File.ReadAllLines(filePath).ToList();
                }

                var headers = lines.First().Split(',');
                int keyIndex = Array.IndexOf(headers, key);

                if (keyIndex == -1)
                    throw new Exception("Ключ сортировки не найден.");

                List<List<string>> chunks;
                List<string> sortedLines;

                switch (method)
                {
                    case "Прямое слияние":
                        chunks = SplitFile(lines.Skip(1), 1000);
                        sortedLines = PerformDirectMergeSort(chunks, keyIndex, delay);
                        break;
                    case "Естественное слияние":
                        chunks = SplitFile(lines.Skip(1), 1000);
                        sortedLines = PerformNaturalMergeSort(chunks, keyIndex, delay);
                        break;
                    case "Многопутевое слияние":
                        chunks = SplitFile(lines.Skip(1), 500); // Разделяем на большее количество кусков
                        sortedLines = PerformMultiwayMergeSort(chunks, keyIndex, delay);
                        break;
                    default:
                        throw new Exception("Неизвестный метод сортировки.");
                }

                // Получаем текущую директорию
                string currentDirectory = Directory.GetCurrentDirectory();
                string relativePath = System.IO.Path.Combine(currentDirectory, "../../../../");

                // Формируем полный путь к файлу
                string resultFilePath = System.IO.Path.Combine(relativePath, "sorted_result.xlsx");

                // Создаем DataTable из отсортированных данных
                DataTable sortedDataTable = new DataTable();
                foreach (var header in headers)
                {
                    sortedDataTable.Columns.Add(header);
                }

                foreach (var line in sortedLines)
                {
                    var rowData = line.Split(',');
                    if (rowData.Length != sortedDataTable.Columns.Count)
                    {
                        throw new Exception($"Количество столбцов в строке ({rowData.Length}) не соответствует количеству столбцов в таблице ({sortedDataTable.Columns.Count}).");
                    }
                    sortedDataTable.Rows.Add(rowData);
                }

                // Сохраняем результат в Excel файл
                SaveDataTableToExcel(sortedDataTable, resultFilePath);
                LogAction($"Сортировка завершена! Результат сохранен в {resultFilePath}", 0);

                // Сохраняем состояние
                SaveState(sortedDataTable, LogBox.Items.Cast<string>().ToList());
            }
            catch (Exception ex)
            {
                LogAction($"Ошибка: {ex.Message}", 0);
            }
        }

        private List<string> PerformDirectMergeSort(List<List<string>> chunks, int keyIndex, int delay)
        {
            foreach (var chunk in chunks)
            {
                chunk.Sort((a, b) => CompareRows(a, b, keyIndex));
                LogAction($"Отсортирован кусок. Количество строк: {chunk.Count}", delay);
            }
            return MergeChunks(chunks, keyIndex, delay);
        }

        private List<string> PerformNaturalMergeSort(List<List<string>> chunks, int keyIndex, int delay)
        {
            // Реализация естественного слияния
            // ...
            return MergeChunks(chunks, keyIndex, delay);
        }

        private List<string> PerformMultiwayMergeSort(List<List<string>> chunks, int keyIndex, int delay)
        {
            foreach (var chunk in chunks)
            {
                chunk.Sort((a, b) => CompareRows(a, b, keyIndex));
                LogAction($"Отсортирован кусок. Количество строк: {chunk.Count}", delay);
            }
            return MergeChunks(chunks, keyIndex, delay);
        }

        private List<string> LoadExcelDataAsXlsx(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            List<string> xlsxLines = new List<string>();

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // Предполагается, что данные находятся на первом листе

                // Загрузка заголовков столбцов
                string[] headers = new string[worksheet.Dimension.End.Column];
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    headers[col - 1] = worksheet.Cells[1, col].Text;
                }
                xlsxLines.Add(string.Join(",", headers));

                // Загрузка данных
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    string[] rowData = new string[worksheet.Dimension.End.Column];
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        rowData[col - 1] = worksheet.Cells[row, col].Text;
                    }
                    xlsxLines.Add(string.Join(",", rowData));
                }
            }

            return xlsxLines;
        }

        private List<List<string>> SplitFile(IEnumerable<string> lines, int chunkSize)
        {
            return lines.Select((line, index) => new { line, index })
                        .GroupBy(x => x.index / chunkSize)
                        .Select(g => g.Select(x => x.line).ToList())
                        .ToList();
        }

        private List<string> MergeChunks(List<List<string>> chunks, int keyIndex, int delay)
        {
            var result = new List<string>();
            var priorityQueue = new SortedSet<(string Value, int ChunkIndex, int RowIndex)>(
                Comparer<(string Value, int ChunkIndex, int RowIndex)>.Create(
                    (a, b) => CompareRows(a.Value, b.Value, keyIndex)
                )
            );

            // Инициализация очереди приоритетов
            for (int i = 0; i < chunks.Count; i++)
            {
                if (chunks[i].Count > 0)
                {
                    priorityQueue.Add((chunks[i][0], i, 0));
                }
            }

            // Слияние кусков
            while (priorityQueue.Count > 0)
            {
                var (minValue, chunkIndex, rowIndex) = priorityQueue.First();
                priorityQueue.Remove((minValue, chunkIndex, rowIndex));
                result.Add(minValue);

                if (rowIndex + 1 < chunks[chunkIndex].Count)
                {
                    priorityQueue.Add((chunks[chunkIndex][rowIndex + 1], chunkIndex, rowIndex + 1));
                }

                LogAction($"Обработана строка: {minValue}", delay);
                HighlightRow(rowIndex);
            }

            return result;
        }

        private int CompareRows(string a, string b, int keyIndex)
        {
            var aValue = a.Split(',')[keyIndex];
            var bValue = b.Split(',')[keyIndex];
            return string.Compare(aValue, bValue, StringComparison.Ordinal);
        }

        private void LogAction(string message, int delay)
        {
            Dispatcher.Invoke(() =>
            {
                LogBox.Items.Add(message);
                LogBox.ScrollIntoView(message);
            });

            if (delay > 0)
                System.Threading.Thread.Sleep(delay);
        }

        private void SaveDataTableToExcel(DataTable dataTable, string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

                // Записываем заголовки столбцов
                for (int col = 0; col < dataTable.Columns.Count; col++)
                {
                    worksheet.Cells[1, col + 1].Value = dataTable.Columns[col].ColumnName;
                }

                // Записываем данные
                for (int row = 0; row < dataTable.Rows.Count; row++)
                {
                    for (int col = 0; col < dataTable.Columns.Count; col++)
                    {
                        worksheet.Cells[row + 2, col + 1].Value = dataTable.Rows[row][col];
                    }
                }

                package.Save();
            }
        }

        private void SaveState(DataTable dataTable, List<string> log)
        {
            // Удаляем все состояния после текущего индекса
            if (_currentStateIndex < _states.Count - 1)
            {
                _states.RemoveRange(_currentStateIndex + 1, _states.Count - _currentStateIndex - 1);
            }

            // Сохраняем новое состояние
            _states.Add(new TableState(dataTable.Copy(), new List<string>(log)));
            _currentStateIndex = _states.Count - 1;
        }

        private void GoBack()
        {
            if (_currentStateIndex > 0)
            {
                _currentStateIndex--;
                UpdateState();
            }
        }

        private void GoForward()
        {
            if (_currentStateIndex < _states.Count - 1)
            {
                _currentStateIndex++;
                UpdateState();
            }
        }

        private void UpdateState()
        {
            var currentState = _states[_currentStateIndex];
            dataGrid.ItemsSource = currentState.DataTable.DefaultView;
            LogBox.ItemsSource = currentState.Log;
        }

        private void HighlightRow(int rowIndex)
        {
            var view = dataGrid.ItemsSource as DataView;
            if (view != null)
            {
                var row = view[rowIndex].Row;
                row["Highlight"] = true;
            }
        }

        private void ClearHighlight()
        {
            var view = dataGrid.ItemsSource as DataView;
            if (view != null)
            {
                foreach (DataRowView rowView in view)
                {
                    rowView.Row["Highlight"] = false;
                }
            }
        }


        private async void StartAutoPlay_Click(object sender, RoutedEventArgs e)
        {
            if (_isAutoPlaying)
            {
                MessageBox.Show("Автопроигрывание уже запущено.");
                return;
            }

            _isAutoPlaying = true;
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await PerformSortWithAutoPlay(_filePath, KeyAttributeComboBox.SelectedItem.ToString(), ((ComboBoxItem)SortMethodComboBox.SelectedItem).Content.ToString(), int.Parse(DelayBox.Text));
            }
            catch (Exception ex)
            {
                LogAction($"Ошибка: {ex.Message}", 0);
            }
            finally
            {
                _isAutoPlaying = false;
            }
        }

        private void StopAutoPlay_Click(object sender, RoutedEventArgs e)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        private async Task PerformSortWithAutoPlay(string filePath, string key, string method, int delay)
        {
            try
            {
                List<string> lines = new List<string>();
                if (System.IO.Path.GetExtension(filePath).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    lines = LoadExcelDataAsXlsx(filePath);
                }
                else
                {
                    lines = File.ReadAllLines(filePath).ToList();
                }

                var headers = lines.First().Split(',');
                int keyIndex = Array.IndexOf(headers, key);

                if (keyIndex == -1)
                    throw new Exception("Ключ сортировки не найден.");

                List<List<string>> chunks;
                List<string> sortedLines;

                switch (method)
                {
                    case "Прямое слияние":
                        chunks = SplitFile(lines.Skip(1), 1000);
                        sortedLines = PerformDirectMergeSort(chunks, keyIndex, delay);
                        break;
                    case "Естественное слияние":
                        chunks = SplitFile(lines.Skip(1), 1000);
                        sortedLines = PerformNaturalMergeSort(chunks, keyIndex, delay);
                        break;
                    case "Многопутевое слияние":
                        chunks = SplitFile(lines.Skip(1), 500); // Разделяем на большее количество кусков
                        sortedLines = PerformMultiwayMergeSort(chunks, keyIndex, delay);
                        break;
                    default:
                        throw new Exception("Неизвестный метод сортировки.");
                }

                // Получаем текущую директорию
                string currentDirectory = Directory.GetCurrentDirectory();
                string relativePath = System.IO.Path.Combine(currentDirectory, "../../../../");

                // Формируем полный путь к файлу
                string resultFilePath = System.IO.Path.Combine(relativePath, "sorted_result.xlsx");

                // Создаем DataTable из отсортированных данных
                DataTable sortedDataTable = new DataTable();
                foreach (var header in headers)
                {
                    sortedDataTable.Columns.Add(header);
                }

                foreach (var line in sortedLines)
                {
                    var rowData = line.Split(',');
                    if (rowData.Length != sortedDataTable.Columns.Count)
                    {
                        throw new Exception($"Количество столбцов в строке ({rowData.Length}) не соответствует количеству столбцов в таблице ({sortedDataTable.Columns.Count}).");
                    }
                    sortedDataTable.Rows.Add(rowData);
                }

                // Сохраняем результат в Excel файл
                SaveDataTableToExcel(sortedDataTable, resultFilePath);
                LogAction($"Сортировка завершена! Результат сохранен в {resultFilePath}", 0);

                // Сохраняем состояние
                SaveState(sortedDataTable, LogBox.Items.Cast<string>().ToList());
            }
            catch (OperationCanceledException)
            {
                LogAction("Автопроигрывание остановлено.", 0);
            }
            catch (Exception ex)
            {
                LogAction($"Ошибка: {ex.Message}", 0);
            }
        }
    }

    public class TableState
    {
        public DataTable DataTable { get; set; }
        public List<string> Log { get; set; }

        public TableState(DataTable dataTable, List<string> log)
        {
            DataTable = dataTable;
            Log = log;
        }
    }
}
