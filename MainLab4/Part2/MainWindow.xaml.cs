using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using OfficeOpenXml;
using static Part2.MainWindow;

namespace Part2
{
    public partial class MainWindow : Window
    {
        private static string curDir = Directory.GetCurrentDirectory();
        private string _filePath = System.IO.Path.Combine(curDir, "sorted_result.xlsx");
        private List<TableState> _states = new List<TableState>();
        private int _currentStateIndex = -1;
        private CancellationTokenSource _cancellationTokenSource;
        private ObservableCollection<string> _logItems = new ObservableCollection<string>();
        private bool _isAutoPlaying = false;
        String[]? headers;
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
                    headers = File.ReadLines(_filePath).First().Split(',');
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

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // Очистка всех данных и сброс состояния
            _filePath = string.Empty;
            FilePathBox.Text = string.Empty;
            KeyAttributesListBox.ItemsSource = null;
            SortMethodComboBox.SelectedItem = null;
            DelayBox.Text = string.Empty;

            _logItems.Clear();
            dataGrid.ItemsSource = null;
            _states.Clear();
            _currentStateIndex = -1;
            _isAutoPlaying = false;
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }

            // Очистка подсветки строк
            ClearHighlight();

            MessageBox.Show("Данные очищены. Вы можете начать сортировку заново.");

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

                headers = lines.First().Split(',');
                var keyIndices = keys.Select(key => Array.IndexOf(headers, key)).ToList();

                if (keyIndices.Any(index => index == -1))
                    throw new Exception("Ключ сортировки не найден.");

                List<string> sortedLines;

                switch (method)
                {
                    case "Прямое слияние":
                        sortedLines = PerformDirectMergeSort(lines.Skip(1).ToList(), keyIndices, 0);
                        break;
                    case "Естественное слияние":
                        var chunks = SplitFile(lines.Skip(1), (int)lines.Count() / 2);
                        SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList(), new List<int>());
                        sortedLines = PerformNaturalMergeSort(chunks, keyIndices, delay);
                        break;
                    case "Многопутевое слияние":
                        sortedLines = PerformMultiwayMergeSort(lines.Skip(1).ToList(), keyIndices);
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
                SaveState(sortedDataTable, LogBox.Items.Cast<string>().ToList(), new List<int>());
            }
            catch (Exception ex)
            {
                LogAction($"Ошибка: {ex.Message}", 0);
            }
            _currentStateIndex = 0;
            UpdateState();
            MessageBox.Show("Сортировка завершена, нажмите кнопку вперед");
        }


        private List<string> PerformDirectMergeSort(List<string> data, List<int> keyIndices, int delay)
        {
            if (data.Count <= 1)
            {
                return data; // Базовый случай: если массив состоит из одного элемента или пуст, он уже отсортирован
            }

            int mid = data.Count / 2;
            var left = data.GetRange(0, mid);
            var right = data.GetRange(mid, data.Count - mid);

            LogAction($"Делим массив на две части: левая часть ({left.Count} элементов), правая часть ({right.Count} элементов).", delay);
            SaveState(ConvertChunkToDataTable(data, keyIndices), LogBox.Items.Cast<string>().ToList(), new List<int>());

            var sortedLeft = PerformDirectMergeSort(left, keyIndices, delay);
            var sortedRight = PerformDirectMergeSort(right, keyIndices, delay);

            return Merge(sortedLeft, sortedRight, keyIndices, delay);
        }

        private List<string> Merge(List<string> left, List<string> right, List<int> keyIndices, int delay)
        {
            var result = new List<string>();
            int i = 0, j = 0;
            List<int> highlightedRows = new List<int>();

            while (i < left.Count && j < right.Count)
            {
                if (CompareRows(left[i], right[j], keyIndices) <= 0)
                {
                    result.Add(left[i]);
                    i++;
                    LogAction($"Добавляем элемент из левой части: {left[i - 1]}", delay);
                    highlightedRows.Add(i - 1);
                }
                else
                {
                    result.Add(right[j]);
                    j++;
                    LogAction($"Добавляем элемент из правой части: {right[j - 1]}", delay);
                    highlightedRows.Add(j - 1);
                }
                SaveState(ConvertChunkToDataTable(result, keyIndices), LogBox.Items.Cast<string>().ToList(), highlightedRows);
                highlightedRows.Clear();
            }

            while (i < left.Count)
            {
                result.Add(left[i]);
                i++;
                LogAction($"Добавляем оставшийся элемент из левой части: {left[i - 1]}", delay);
                highlightedRows.Add(i - 1);
                SaveState(ConvertChunkToDataTable(result, keyIndices), LogBox.Items.Cast<string>().ToList(), highlightedRows);
                highlightedRows.Clear();
            }

            while (j < right.Count)
            {
                result.Add(right[j]);
                j++;
                LogAction($"Добавляем оставшийся элемент из правой части: {right[j - 1]}", delay);
                highlightedRows.Add(j - 1);
                SaveState(ConvertChunkToDataTable(result, keyIndices), LogBox.Items.Cast<string>().ToList(), highlightedRows);
                highlightedRows.Clear();
            }

            LogAction("Слияние завершено.", delay);
            SaveState(ConvertChunkToDataTable(result, keyIndices), LogBox.Items.Cast<string>().ToList(), new List<int>());

            return result;
        }




        private List<List<string>> SplitIntoNaturalRuns(List<string> data, List<int> keyIndices)
        {
            var runs = new List<List<string>>();
            var currentRun = new List<string> { data[0] };
            List<int> highlightedRows = new List<int>();

            LogAction("Начинаем разбиение данных на естественные подпоследовательности.", 0);
            SaveState(ConvertChunkToDataTable(data, keyIndices), LogBox.Items.Cast<string>().ToList(), highlightedRows);

            for (int i = 1; i < data.Count; i++)
            {

                highlightedRows.Add(i - 1);
                highlightedRows.Add(i);

                LogAction($"Сравниваем строки: '{data[i - 1]}' и '{data[i]}'.", 0);
                SaveState(ConvertChunkToDataTable(data, keyIndices), LogBox.Items.Cast<string>().ToList(), highlightedRows);
                highlightedRows.Clear();
                if (CompareRows(data[i - 1], data[i], keyIndices) <= 0)
                {
                    currentRun.Add(data[i]);
                    LogAction($"Строка {i} добавлена в текущую подпоследовательность.", 0);
                }
                else
                {
                    runs.Add(currentRun);
                    LogAction($"Текущая подпоследовательность завершена. Количество строк: {currentRun.Count}", 0);
                    SaveState(ConvertChunkToDataTable(data, keyIndices), LogBox.Items.Cast<string>().ToList(), highlightedRows);

                    currentRun = new List<string> { data[i] };
                    LogAction($"Начинаем новую подпоследовательность с строки {i}.", 0);
                }
            }

            runs.Add(currentRun);
            LogAction($"Разбито на {runs.Count} естественных подпоследовательностей.", 0);
            SaveState(ConvertChunkToDataTable(data, keyIndices), LogBox.Items.Cast<string>().ToList(), highlightedRows);

            return runs;
        }


        private List<string> MergeChunks(List<List<string>> chunks, List<int> keyIndices, int delay, List<int> highlightedRows)
        {
            var result = new List<string>();
            var priorityQueue = new SortedSet<(string Value, int ChunkIndex, int RowIndex)>(
                Comparer<(string Value, int ChunkIndex, int RowIndex)>.Create(
                    (a, b) =>
                    {
                        // Очищаем предыдущие подсветки
                        ClearHighlight();

                        // Подсвечиваем строки, которые сравниваются
                        HighlightRow(a.RowIndex); // Подсвечиваем строку из очереди
                        HighlightRow(b.RowIndex); // Подсвечиваем строку, с которой сравниваем
                        highlightedRows.Add(a.RowIndex + a.ChunkIndex * chunks[a.ChunkIndex].Count);
                        highlightedRows.Add(b.RowIndex + b.ChunkIndex * chunks[b.ChunkIndex].Count);

                        LogAction($"Сравниваем строки: '{a.Value}' (чанк {a.ChunkIndex}, строка {a.RowIndex}) и '{b.Value}' (чанк {b.ChunkIndex}, строка {b.RowIndex}).", 0);

                        SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList(), highlightedRows);
                        highlightedRows.Clear();

                        // Сравнение строк на основе ключей
                        int comparison = CompareRows(a.Value, b.Value, keyIndices);
                        if (comparison == 0)
                        {
                            LogAction("Строки равны по ключам. Решаем по индексу чанка.", 0);
                            return a.ChunkIndex.CompareTo(b.ChunkIndex); // Для уникальности
                        }

                        LogAction($"Результат сравнения: {(comparison < 0 ? "меньше" : "больше")}", 0);
                        return comparison;
                    }
                )
            );

            LogAction("Инициализация очереди приоритетов для слияния кусков.", 0);

            // Инициализация: добавляем первую строку каждого чанка в очередь
            for (int i = 0; i < chunks.Count; i++)
            {
                if (chunks[i].Count > 0)
                {
                    priorityQueue.Add((chunks[i][0], i, 0));
                    LogAction($"Добавлена первая строка из чанка {i} в очередь приоритетов: {chunks[i][0]}", 0);

                    // Очищаем предыдущие подсветки
                    ClearHighlight();

                    HighlightRow(i * chunks[i].Count); // Подсвечиваем добавленную строку
                    highlightedRows.Add(i * chunks[i].Count); // Добавляем строку в список подсветок
                    SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList(), highlightedRows);
                    highlightedRows.Clear();
                }
            }

            LogAction("Начинаем процесс слияния кусков.", 0);

            // Пока есть элементы в очереди, продолжаем слияние
            while (priorityQueue.Count > 0)
            {
                // Извлекаем минимальный элемент из очереди
                var (minValue, chunkIndex, rowIndex) = priorityQueue.First();
                priorityQueue.Remove((minValue, chunkIndex, rowIndex));
                result.Add(minValue);

                // Обновление DataGrid для отображения текущего результата
                Dispatcher.Invoke(() =>
                {
                    var resultTable = new DataTable();
                    resultTable.Columns.Add("Result");
                    foreach (var item in result)
                    {
                        resultTable.Rows.Add(item);
                    }
                    ResultDataGrid.ItemsSource = resultTable.DefaultView; // Обновляем источник
                });

                LogAction($"Извлечена минимальная строка из очереди: '{minValue}' из чанка {chunkIndex}, строка {rowIndex}.", 0);


                // Подсвечиваем извлечённую строку
                HighlightRow(rowIndex);
                highlightedRows.Add(rowIndex);
                // Подсвечиваем строки в очереди
                foreach (var item in priorityQueue)
                {
                    HighlightRow(item.RowIndex); // Подсвечиваем строки, которые остаются в очереди
                    highlightedRows.Add(item.RowIndex);
                }

                SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList(), highlightedRows);
                highlightedRows.Clear();

                // Проверяем, есть ли следующая строка в текущем чанке
                if (rowIndex + 1 < chunks[chunkIndex].Count)
                {
                    var nextValue = chunks[chunkIndex][rowIndex + 1];
                    priorityQueue.Add((nextValue, chunkIndex, rowIndex + 1));
                    LogAction($"Добавлена следующая строка из чанка {chunkIndex}: '{nextValue}'.", 0);


                    // Подсвечиваем текущую строку чанка и её отображение в очереди
                    HighlightRow(rowIndex + 1); // Подсвечиваем добавляемую строку
                    highlightedRows.Add(rowIndex + 1); // Добавляем строку в список подсветок

                    SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList(), highlightedRows);
                    highlightedRows.Clear();
                }

                LogAction("Текущее состояние очереди приоритетов:", 0);
                foreach (var item in priorityQueue)
                {
                    LogAction($"Чанк {item.ChunkIndex}, строка {item.RowIndex}, значение: {item.Value}.", 0);
                    HighlightRow(item.RowIndex); // Подсвечиваем строку в очереди приоритетов
                    highlightedRows.Add(item.RowIndex); // Добавляем строку в список подсветок
                    SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList(), highlightedRows);
                    highlightedRows.Clear();
                }
            }

            LogAction("Слияние кусков завершено.", 0);
            return result;
        }

        private List<string> PerformNaturalMergeSort(List<List<string>> chunks, List<int> keyIndices, int delay)
        {
            List<int> highlightedRows = new List<int>();
            var count = 0;

            // Разбиваем данные на естественные подпоследовательности
            LogAction("Начинаем разбиение данных на естественные подпоследовательности.", 0);
            SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList(), new List<int>());

            var naturalRuns = new List<List<string>>();
            foreach (var chunk in chunks)
            {
                naturalRuns.AddRange(SplitIntoNaturalRuns(chunk, keyIndices));
            }

            LogAction("Разбиение на естественные подпоследовательности завершено.", 0);
            SaveState(ConvertChunksToDataTable(naturalRuns, headers), LogBox.Items.Cast<string>().ToList(), highlightedRows);

            // Слияние естественных подпоследовательностей
            LogAction("Начинаем слияние естественных подпоследовательностей.", 0);
            return MergeChunks(naturalRuns, keyIndices, delay, highlightedRows);
        }


        private List<string> PerformMultiwayMergeSort(List<string> data, List<int> keyIndices, int delay)
        {
            if (data.Count <= 1)
            {
                return data; // Базовый случай: если массив состоит из одного элемента или пуст, он уже отсортирован
            }

            int quarter = data.Count / 4;
            var part1 = data.GetRange(0, quarter);
            var part2 = data.GetRange(quarter, quarter);
            var part3 = data.GetRange(2 * quarter, quarter);
            var part4 = data.GetRange(3 * quarter, data.Count - 3 * quarter);

            LogAction($"Делим массив на четыре части: часть 1 ({part1.Count} элементов), часть 2 ({part2.Count} элементов), часть 3 ({part3.Count} элементов), часть 4 ({part4.Count} элементов).", delay);
            SaveState(ConvertChunkToDataTable(data, keyIndices), LogBox.Items.Cast<string>().ToList(), new List<int>());

            var task1 = Task.Run(() => PerformMultiwayMergeSort(part1, keyIndices, delay));
            var task2 = Task.Run(() => PerformMultiwayMergeSort(part2, keyIndices, delay));
            var task3 = Task.Run(() => PerformMultiwayMergeSort(part3, keyIndices, delay));
            var task4 = Task.Run(() => PerformMultiwayMergeSort(part4, keyIndices, delay));

            Task.WaitAll(task1, task2, task3, task4);

            var sortedPart1 = task1.Result;
            var sortedPart2 = task2.Result;
            var sortedPart3 = task3.Result;
            var sortedPart4 = task4.Result;

            var merged12 = Merge(sortedPart1, sortedPart2, keyIndices, delay);
            var merged34 = Merge(sortedPart3, sortedPart4, keyIndices, delay);

            return Merge(merged12, merged34, keyIndices, delay);
        }

        private List<string> MergeChunksParallel(List<List<string>> chunks, List<int> keyIndices, int delay, List<int> highlightedRows)
        {
            var result = new BlockingCollection<string>();
            var priorityQueue = new BlockingCollection<(string Value, int ChunkIndex, int RowIndex)>();

            LogAction("Инициализация очереди приоритетов для слияния кусков.", 0);
            // Инициализация: добавляем первую строку каждого чанка в очередь
            for (int i = 0; i < chunks.Count; i++)
            {
                if (chunks[i].Count > 0)
                {
                    priorityQueue.Add((chunks[i][0], i, 0));
                    LogAction($"Добавлена первая строка из чанка {i} в очередь приоритетов: {chunks[i][0]}", 0);

                    highlightedRows.Add(0 + i * chunks[i].Count); // Добавляем строку в список подсветок
                    SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList(), highlightedRows);
                    highlightedRows.Clear();
                }
            }

            // Параллельная обработка очереди приоритетов
            Parallel.For(0, chunks.Count, _ =>
            {
                while (!priorityQueue.IsCompleted)
                {
                    if (priorityQueue.TryTake(out var minElement, Timeout.Infinite))
                    {
                        result.Add(minElement.Value);

                        highlightedRows.Add(minElement.RowIndex);
                        LogAction($"Извлечена минимальная строка из очереди: '{minElement.Value}' из чанка {minElement.ChunkIndex}, строка {minElement.RowIndex}.", 0);
                        SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList(), highlightedRows);
                        highlightedRows.Clear();

                        // Добавление следующего элемента чанка в очередь
                        if (minElement.RowIndex + 1 < chunks[minElement.ChunkIndex].Count)
                        {
                            var nextValue = chunks[minElement.ChunkIndex][minElement.RowIndex + 1];
                            priorityQueue.Add((nextValue, minElement.ChunkIndex, minElement.RowIndex + 1));
                            LogAction($"Добавлена следующая строка из чанка {minElement.ChunkIndex}: '{nextValue}'.", 0);

                            highlightedRows.Add(minElement.RowIndex + 1 + minElement.ChunkIndex * chunks[minElement.ChunkIndex].Count);
                            SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList(), highlightedRows);
                            highlightedRows.Clear();
                        }
                    }
                }
            });

            LogAction("Слияние кусков завершено.", 0);

            // Конвертируем BlockingCollection в List
            return result.ToList();
        }



        private void InsertionSort(List<string> chunk, List<int> keyIndices)
        {
            List<int> highlightedRows = new List<int>();

            LogAction("Начинаем сортировку данных внутри чанка методом вставок.", 0);
            SaveState(ConvertChunkToDataTable(chunk, keyIndices), LogBox.Items.Cast<string>().ToList(), highlightedRows);

            for (int i = 1; i < chunk.Count; i++)
            {
                var key = chunk[i];
                int j = i - 1;

                // Очищаем предыдущие подсветки
                ClearHighlight();

                // Выделяем строку, которую будем перемещать
                HighlightRow(i);
                highlightedRows.Add(i);
                LogAction($"Выделяем строку {i} для перемещения: {key}", 0);
                SaveState(ConvertChunkToDataTable(chunk, keyIndices), LogBox.Items.Cast<string>().ToList(), highlightedRows);
                highlightedRows.Clear();

                while (j >= 0 && CompareRows(chunk[j], key, keyIndices) > 0)
                {
                    chunk[j + 1] = chunk[j];
                    j--;

                    highlightedRows.Add(j + 1);
                    LogAction($"Перемещаем строку {j + 1} на позицию {j + 2}: {chunk[j + 1]}", 0);
                    SaveState(ConvertChunkToDataTable(chunk, keyIndices), LogBox.Items.Cast<string>().ToList(), highlightedRows);
                    highlightedRows.Clear();
                }

                chunk[j + 1] = key;

                highlightedRows.Add(j + 1);
                LogAction($"Строка {i} перемещена на позицию {j + 1}: {key}", 0);
                SaveState(ConvertChunkToDataTable(chunk, keyIndices), LogBox.Items.Cast<string>().ToList(), highlightedRows);
                highlightedRows.Clear();
            }

            LogAction("Сортировка данных внутри чанка методом вставок завершена.", 0);
            SaveState(ConvertChunkToDataTable(chunk, keyIndices), LogBox.Items.Cast<string>().ToList(), highlightedRows);
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
            int count = 0;
            var chunks = lines.Select((line, index) => new { line, index })
                              .GroupBy(x => x.index / chunkSize)
                              .Select(g => g.Select(x => x.line).ToList())
                              .ToList();

            foreach (var chunk in chunks)
            {
                LogAction("Делим файл на чанки, на экране чанк номер: " + count, 0);
                SaveState(ConvertChunkToDataTable(chunk, new List<int>()), LogBox.Items.Cast<string>().ToList(), new List<int>());
                count++;
            }

            return chunks;
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
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

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

        private void SaveState(DataTable dataTable, List<string> log, List<int> highlightedRows)
        {
            // Удаляем все состояния после текущего индекса
            if (_currentStateIndex < _states.Count - 1)
            {
                _states.RemoveRange(_currentStateIndex + 1, _states.Count - _currentStateIndex - 1);
            }

            // Сохраняем новое состояние
            _states.Add(new TableState(dataTable.Copy(), new List<string>(log), new List<int>(highlightedRows)));
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
                ClearHighlight();
                UpdateState();
            }
        }

        private void UpdateState()
        {
            var currentState = _states[_currentStateIndex];

            Dispatcher.Invoke(() =>
            {
                // Очистка Items перед обновлением ItemsSource
                dataGrid.ItemsSource = null;
                dataGrid.Items.Clear();

                // Устанавливаем новое ItemsSource
                dataGrid.ItemsSource = currentState.DataTable.DefaultView;
                dataGrid.Items.Refresh();

                // Очистка и обновление LogBox
                LogBox.ItemsSource = null;
                LogBox.Items.Clear();
                LogBox.ItemsSource = currentState.Log;
                LogBox.Items.Refresh();

                // Применяем подсветку строк
                ClearHighlight();
                foreach (var rowIndex in currentState.HighlightedRows)
                {
                    HighlightRow(rowIndex);
                }
            });
        }






        private void HighlightRow(int rowIndex)
        {
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
            // Проверяем, что индекс находится в допустимых пределах
            if (rowIndex < 0 || rowIndex >= dataGrid.Items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex), "Индекс выходит за пределы допустимого диапазона.");
            }

            // Убеждаемся, что ItemContainerGenerator готов
            if (dataGrid.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            {
                // Обновляем макет и принудительно прокручиваем к нужной строке
                dataGrid.UpdateLayout();
                dataGrid.ScrollIntoView(dataGrid.Items[rowIndex]);
            }

            // Пробуем получить строку через ContainerFromIndex
            var row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;

            // Если контейнер всё ещё не найден, выполняем дополнительные действия
            if (row == null)
            {
                // Прокручиваем к элементу ещё раз на случай виртуализации
                dataGrid.ScrollIntoView(dataGrid.Items[rowIndex]);

                // Обновляем макет снова
                dataGrid.UpdateLayout();

                // Пробуем снова получить контейнер
                row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            }

            return row;
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
                await AutoPlay(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
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

        private async Task AutoPlay(CancellationToken cancellationToken)
        {
            int delay = int.TryParse(DelayBox.Text, out int d) ? d : 500;

            while (_currentStateIndex < _states.Count - 1)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                GoForward();
                await Task.Delay(delay, cancellationToken);
            }
        }

        private List<string> PerformMultiwayMergeSort(List<string> data, List<int> keyIndices)
        {
            return PerformDirectMergeSort(data, keyIndices, 0);
        }

    }
}