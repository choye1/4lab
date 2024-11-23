using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
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
            // Установка лицензии для EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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
                    KeyAttributesListBox.ItemsSource = headers;
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
                KeyAttributesListBox.ItemsSource = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
            }
        }

        private void StartSort_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_filePath) || KeyAttributesListBox.SelectedItems.Count == 0 || SortMethodComboBox.SelectedItem == null)
            {
                MessageBox.Show("Заполните все поля перед началом сортировки.");
                return;
            }

            string sortMethod = ((ComboBoxItem)SortMethodComboBox.SelectedItem).Content.ToString();
            var keys = KeyAttributesListBox.SelectedItems.Cast<string>().ToList();
            int delay = int.TryParse(DelayBox.Text, out int d) ? d : 500;

            LogBox.Items.Add("Начинаем сортировку...");

            PerformSort(_filePath, keys, sortMethod, delay);
        }

        private void PerformSort(string filePath, List<string> keys, string method, int delay)
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
                var keyIndices = keys.Select(key => Array.IndexOf(headers, key)).ToList();

                if (keyIndices.Any(index => index == -1))
                    throw new Exception("Ключ сортировки не найден.");

                List<List<string>> chunks;
                List<string> sortedLines;

                switch (method)
                {
                    case "Прямое слияние":
                        chunks = SplitFile(lines.Skip(1), 1000);
                        SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList());
                        sortedLines = PerformDirectMergeSort(chunks, keyIndices, delay);
                        break;
                    case "Естественное слияние":
                        chunks = SplitFile(lines.Skip(1), 1000);
                        SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList());
                        sortedLines = PerformNaturalMergeSort(chunks, keyIndices, delay);
                        break;
                    case "Многопутевое слияние":
                        chunks = SplitFile(lines.Skip(1), 500); // Разделяем на большее количество кусков
                        SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList());
                        sortedLines = PerformMultiwayMergeSort(chunks, keyIndices, delay);
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
            _currentStateIndex = 0;
            UpdateState();
        }

        private List<string> PerformDirectMergeSort(List<List<string>> chunks, List<int> keyIndices, int delay)
        {
            foreach (var chunk in chunks)
            {
                chunk.Sort((a, b) => CompareRows(a, b, keyIndices));
                LogAction($"Отсортирован кусок. Количество строк: {chunk.Count}", delay);
                SaveState(ConvertChunkToDataTable(chunk, keyIndices), LogBox.Items.Cast<string>().ToList());
            }
            return MergeChunks(chunks, keyIndices, delay);
        }

        private List<string> PerformNaturalMergeSort(List<List<string>> chunks, List<int> keyIndices, int delay)
        {
            // Реализация естественного слияния
            // ...
            return MergeChunks(chunks, keyIndices, delay);
        }

        private List<string> PerformMultiwayMergeSort(List<List<string>> chunks, List<int> keyIndices, int delay)
        {
            foreach (var chunk in chunks)
            {
                chunk.Sort((a, b) => CompareRows(a, b, keyIndices));
                LogAction($"Отсортирован кусок. Количество строк: {chunk.Count}", delay);
                SaveState(ConvertChunkToDataTable(chunk, keyIndices), LogBox.Items.Cast<string>().ToList());
            }
            return MergeChunks(chunks, keyIndices, delay);
        }

        private DataTable ConvertChunkToDataTable(List<string> chunk, List<int> keyIndices)
        {
            DataTable dataTable = new DataTable();
            var headers = chunk.First().Split(',');

            foreach (var header in headers)
            {
                dataTable.Columns.Add(header);
            }

            foreach (var line in chunk)
            {
                var rowData = line.Split(',');
                dataTable.Rows.Add(rowData);
            }

            return dataTable;
        }

        private DataTable ConvertChunksToDataTable(List<List<string>> chunks, string[] headers)
        {
            DataTable dataTable = new DataTable();

            foreach (var header in headers)
            {
                dataTable.Columns.Add(header);
            }

            foreach (var chunk in chunks)
            {
                foreach (var line in chunk)
                {
                    var rowData = line.Split(',');
                    dataTable.Rows.Add(rowData);
                }
            }

            return dataTable;
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
            var chunks = lines.Select((line, index) => new { line, index })
                              .GroupBy(x => x.index / chunkSize)
                              .Select(g => g.Select(x => x.line).ToList())
                              .ToList();

            foreach (var chunk in chunks)
            {
                SaveState(ConvertChunkToDataTable(chunk, new List<int>()), LogBox.Items.Cast<string>().ToList());
            }

            return chunks;
        }

        private List<string> MergeChunks(List<List<string>> chunks, List<int> keyIndices, int delay)
        {
            var result = new List<string>();
            var priorityQueue = new SortedSet<(string Value, int ChunkIndex, int RowIndex)>(
                Comparer<(string Value, int ChunkIndex, int RowIndex)>.Create(
                    (a, b) => CompareRows(a.Value, b.Value, keyIndices)
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
                SaveState(ConvertChunkToDataTable(chunks[chunkIndex], keyIndices), LogBox.Items.Cast<string>().ToList());
            }

            return result;
        }

        private int CompareRows(string a, string b, List<int> keyIndices)
        {
            var aValues = a.Split(',');
            var bValues = b.Split(',');

            foreach (var keyIndex in keyIndices)
            {
                int comparison = string.Compare(aValues[keyIndex], bValues[keyIndex], StringComparison.Ordinal);
                if (comparison != 0)
                {
                    return comparison;
                }
            }

            return 0;
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

            // Обновляем отображение таблицы
            Dispatcher.Invoke(() =>
            {
                dataGrid.ItemsSource = null; // Очищаем текущий ItemsSource
                dataGrid.ItemsSource = dataTable.DefaultView;
            });
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
            Dispatcher.Invoke(() =>
            {
                // Очищаем текущий ItemsSource
                dataGrid.Items.Clear();
                LogBox.Items.Clear();

                // Устанавливаем новый ItemsSource
                dataGrid.ItemsSource = currentState.DataTable.DefaultView;
                LogBox.ItemsSource = currentState.Log;
            });
        }



        private void HighlightRow(int rowIndex)
        {
            ClearHighlight(); // Снимаем предыдущее выделение

            var dataGridRow = GetDataGridRow(rowIndex);
            if (dataGridRow != null)
            {
                dataGridRow.Style = (Style)FindResource("HighlightedRowStyle");
            }
        }

        private void ClearHighlight()
        {
            foreach (var item in dataGrid.Items)
            {
                var row = dataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (row != null)
                {
                    row.Style = null; // Сбрасываем стиль строки
                }
            }
        }

        // Метод для получения строки DataGrid по индексу
        private DataGridRow GetDataGridRow(int rowIndex)
        {
            if (dataGrid.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                dataGrid.UpdateLayout();
                dataGrid.ScrollIntoView(dataGrid.Items[rowIndex]);
            }

            return dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
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
                await PerformSortWithAutoPlay(_filePath, KeyAttributesListBox.SelectedItems.Cast<string>().ToList(), ((ComboBoxItem)SortMethodComboBox.SelectedItem).Content.ToString(), int.Parse(DelayBox.Text));
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

        private async Task PerformSortWithAutoPlay(string filePath, List<string> keys, string method, int delay)
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
                var keyIndices = keys.Select(key => Array.IndexOf(headers, key)).ToList();

                if (keyIndices.Any(index => index == -1))
                    throw new Exception("Ключ сортировки не найден.");

                List<List<string>> chunks;
                List<string> sortedLines;

                switch (method)
                {
                    case "Прямое слияние":
                        chunks = SplitFile(lines.Skip(1), 1000);
                        SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList());
                        sortedLines = PerformDirectMergeSort(chunks, keyIndices, delay);
                        break;
                    case "Естественное слияние":
                        chunks = SplitFile(lines.Skip(1), 1000);
                        SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList());
                        sortedLines = PerformNaturalMergeSort(chunks, keyIndices, delay);
                        break;
                    case "Многопутевое слияние":
                        chunks = SplitFile(lines.Skip(1), 500); // Разделяем на большее количество кусков
                        SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList());
                        sortedLines = PerformMultiwayMergeSort(chunks, keyIndices, delay);
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
