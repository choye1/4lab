﻿using System;
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
using System.Reflection.PortableExecutable;
using System.Windows.Controls.Primitives;
using System.Collections.Concurrent;

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

                List<List<string>> chunks;
                List<string> sortedLines;

                switch (method)
                {
                    case "Прямое слияние":
                        chunks = SplitFile(lines.Skip(1), (int) lines.Count()/2);
                        SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList(), new List<int>());
                        sortedLines = PerformDirectMergeSort(chunks, keyIndices, delay);
                        break;
                    case "Естественное слияние":
                        chunks = SplitFile(lines.Skip(1), 1000);
                        SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList(), new List<int>());
                        sortedLines = PerformNaturalMergeSort(chunks, keyIndices, delay);
                        break;
                    case "Многопутевое слияние":
                        chunks = SplitFile(lines.Skip(1), 500); // Разделяем на большее количество кусков
                        SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList(), new List<int>());
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


        private List<string> PerformDirectMergeSort(List<List<string>> chunks, List<int> keyIndices, int delay)
        {
            List<int> highlightedRows = new List<int>();
            var count = 0;

            // Проходим по каждому чанку (куску) данных
            LogAction("Начинаем сортировку данных внутри каждого чанка.", 0);
            SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList(), new List<int>());

            foreach (var chunk in chunks)
            {
                LogAction($"Сортируем данные внутри чанка номер {count}.", 0);
                SaveState(ConvertChunkToDataTable(chunk, keyIndices), LogBox.Items.Cast<string>().ToList(), highlightedRows);

                // Сортируем строки в чанке по ключам сортировки с использованием вставок
                for (int i = 1; i < chunk.Count; i++)
                {
                    var key = chunk[i];
                    int j = i - 1;

                    // Выделяем строку, которую будем перемещать
                    HighlightRow(i);
                    LogAction($"Выделяем строку {i} для перемещения: {key}", 0);

                    while (j >= 0 && CompareRows(chunk[j], key, keyIndices) > 0)
                    {
                        chunk[j + 1] = chunk[j];
                        j--;
                    }
                    chunk[j + 1] = key;

                    // Выделяем строку после перемещения
                    HighlightRow(j + 1);
                    LogAction($"Строка {i} перемещена на позицию {j + 1}: {key}", 0);

                    count++;
                }

                // Логируем сообщение о том, что чанк отсортирован
                LogAction($"Отсортирован кусок номер {count}. Количество строк: {chunk.Count}", 0);

                // Сохраняем состояние после сортировки чанка
                SaveState(ConvertChunkToDataTable(chunk, keyIndices), LogBox.Items.Cast<string>().ToList(), highlightedRows);
            }

            // Слияние отсортированных чанков
            LogAction("Начинаем слияние отсортированных чанков.", 0);
            return MergeChunks(chunks, keyIndices, delay, highlightedRows);
        }


        private List<List<string>> SplitIntoNaturalRuns(List<string> data, List<int> keyIndices)
        {
            var runs = new List<List<string>>();
            var currentRun = new List<string> { data[0] };

            for (int i = 1; i < data.Count; i++)
            {
                if (CompareRows(data[i - 1], data[i], keyIndices) <= 0)
                {
                    currentRun.Add(data[i]);
                }
                else
                {
                    runs.Add(currentRun);
                    currentRun = new List<string> { data[i] };
                }
            }
            runs.Add(currentRun);

            LogAction($"Разбито на {runs.Count} естественных подпоследовательностей.", 0);
            return runs;
        }

        private List<string> MergeChunks(List<List<string>> chunks, List<int> keyIndices, int delay, List<int> highlightedRows)
        {
            var result = new List<string>();
            var priorityQueue = new SortedSet<(string Value, int ChunkIndex, int RowIndex)>(
                Comparer<(string Value, int ChunkIndex, int RowIndex)>.Create(
                    (a, b) =>
                    {
                        // Подсвечиваем строки, которые сравниваются
                        HighlightRow(a.RowIndex); // Подсвечиваем строку из очереди
                        HighlightRow(b.RowIndex); // Подсвечиваем строку, с которой сравниваем
                        highlightedRows.Add(a.RowIndex);
                        highlightedRows.Add(b.RowIndex);

                        LogAction($"Сравниваем строки: '{a.Value}' (чанк {a.ChunkIndex}, строка {a.RowIndex}) и '{b.Value}' (чанк {b.ChunkIndex}, строка {b.RowIndex}).", 0);

                        SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList(), highlightedRows);

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
                    HighlightRow(0); // Подсвечиваем добавленную строку
                    highlightedRows.Add(0); // Добавляем строку в список подсветок
                    SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList(), highlightedRows);
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
                }

                LogAction("Текущее состояние очереди приоритетов:", 0);
                foreach (var item in priorityQueue)
                {
                    LogAction($"Чанк {item.ChunkIndex}, строка {item.RowIndex}, значение: {item.Value}.", 0);
                    HighlightRow(item.RowIndex); // Подсвечиваем строку в очереди приоритетов
                    highlightedRows.Add(item.RowIndex); // Добавляем строку в список подсветок
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


        private List<string> PerformMultiwayMergeSort(List<List<string>> chunks, List<int> keyIndices, int delay)
        {
            List<int> highlightedRows = new List<int>();

            // Параллельная сортировка каждого чанка
            LogAction("Начинаем параллельную сортировку каждого чанка.", 0);
            Parallel.ForEach(chunks, chunk =>
            {
                InsertionSort(chunk, keyIndices);
                LogAction($"Отсортирован кусок. Количество строк: {chunk.Count}", delay);
            });

            SaveState(ConvertChunksToDataTable(chunks, headers), LogBox.Items.Cast<string>().ToList(), highlightedRows);

            // Параллельное слияние чанков
            LogAction("Начинаем параллельное слияние чанков.", 0);
            return MergeChunksParallel(chunks, keyIndices, delay, highlightedRows);
        }

        private void InsertionSort(List<string> chunk, List<int> keyIndices)
        {
            for (int i = 1; i < chunk.Count; i++)
            {
                var key = chunk[i];
                int j = i - 1;

                while (j >= 0 && CompareRows(chunk[j], key, keyIndices) > 0)
                {
                    chunk[j + 1] = chunk[j];
                    j--;
                }
                chunk[j + 1] = key;
            }
        }
        private List<string> MergeChunksParallel(List<List<string>> chunks, List<int> keyIndices, int delay, List<int> highlightedRows)
        {
            var result = new ConcurrentQueue<string>();
            var priorityQueue = new ConcurrentQueue<(string Value, int ChunkIndex, int RowIndex)>();

            LogAction("Инициализация очереди приоритетов для слияния кусков.", 0);
            for (int i = 0; i < chunks.Count; i++)
            {
                if (chunks[i].Count > 0)
                {
                    priorityQueue.Enqueue((chunks[i][0], i, 0));
                    LogAction($"Добавлена первая строка из чанка {i} в очередь приоритетов: {chunks[i][0]}", 0);
                }
            }

            // Параллельная обработка очереди приоритетов
            Parallel.For(0, chunks.Count, _ =>
            {
                while (!priorityQueue.IsEmpty)
                {
                    if (priorityQueue.TryDequeue(out var minElement))
                    {
                        result.Enqueue(minElement.Value);

                        // Добавление следующего элемента чанка в очередь
                        if (minElement.RowIndex + 1 < chunks[minElement.ChunkIndex].Count)
                        {
                            var nextValue = chunks[minElement.ChunkIndex][minElement.RowIndex + 1];
                            priorityQueue.Enqueue((nextValue, minElement.ChunkIndex, minElement.RowIndex + 1));
                        }
                    }
                }
            });

            LogAction("Слияние кусков завершено.", delay);

            // Конвертируем ConcurrentQueue в List
            return result.ToList();
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
                foreach (var rowIndex in currentState.HighlightedRows)
                {
                    HighlightRow(rowIndex);
                }
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
                LogAction("Автопроигрывание остановлено.", 0);
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


        public class TableState
        {
            public DataTable DataTable { get; set; }
            public List<string> Log { get; set; }
            public List<int> HighlightedRows { get; set; }

            public TableState(DataTable dataTable, List<string> log, List<int> highlightedRows)
            {
                DataTable = dataTable;
                Log = log;
                HighlightedRows = highlightedRows;
            }
        }
    }
}