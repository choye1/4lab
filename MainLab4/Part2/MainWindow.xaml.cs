using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using ClosedXML.Excel;
using Microsoft.Win32;
using OfficeOpenXml;
using static Part2.MainWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using ClosedXML.Excel;

namespace Part2
{
    public partial class MainWindow : Window
    {
        private StringBuilder log;
        private int delay;
        private SolidColorBrush defaultColor = Brushes.LightBlue;
        private List<Dictionary<string, string>> table;
        private string[] headers;
        private static string curDir = Directory.GetCurrentDirectory();
        private string _filePath = System.IO.Path.Combine(curDir, "sorted_result.xlsx");
        private List<TableState> _states = new List<TableState>();
        private int _currentStateIndex = -1;
        private CancellationTokenSource _cancellationTokenSource;
        private ObservableCollection<string> _logItems = new ObservableCollection<string>();
        private bool _isAutoPlaying = false;

        public MainWindow()
        {
            InitializeComponent();
            log = new StringBuilder();
            delay = 500; // Default delay in ms
        }

        private void LoadExcelButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                using (var workbook = new XLWorkbook(openFileDialog.FileName))
                {
                    var worksheet = workbook.Worksheet(1);
                    headers = worksheet.FirstRowUsed()
                                       .CellsUsed()
                                       .Select(cell => cell.Value.ToString())
                                       .ToArray();

                    table = worksheet.RowsUsed()
                                     .Skip(1)
                                     .Select(row => headers.Zip(row.CellsUsed().Select(cell => cell.Value.ToString()),
                                                                (header, value) => new { header, value })
                                                            .ToDictionary(x => x.header, x => x.value))
                                     .ToList();
                }

                // Display data in DataGrid
                
                    _filePath = openFileDialog.FileName;

                    if (System.IO.Path.GetExtension(_filePath).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                    {
                        LoadExcelData(_filePath);
                    }
                    else
                    {
                        // Загрузить атрибуты из файла
                        headers = File.ReadLines(_filePath).First().Split(',');
                    }
                

                ExcelColumnComboBox.ItemsSource = headers;
                Log("Excel-файл загружен. Выберите колонку для сортировки.");
            }
        }

        private async void StartSorting_Click(object sender, RoutedEventArgs e)
        {
            if (table == null || headers == null || ExcelColumnComboBox.SelectedItem == null)
            {
                Log("Не выбран файл или колонка для сортировки.");
                return;
            }

            StartSorting.IsEnabled = false;
            string sortKey = ExcelColumnComboBox.SelectedItem.ToString();
            Log($"Сортировка по колонке: {sortKey}");

            if (DirectMergeSortRadioButton.IsChecked == true)
            {
                await DirectMergeSort(table, sortKey);
            }
            else if (NaturalMergeSortRadioButton.IsChecked == true)
            {
                await NaturalMergeSort(table, sortKey);
            }
            else if (MultiwayMergeSortRadioButton.IsChecked == true)
            {
                await ThreeWayMergeSort(table, sortKey);
            }

            Log("Сортировка завершена.");
            StartSorting.IsEnabled = true;
        }

        private void DelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            delay = (int)e.NewValue;
            DelayLabel.Content = $"Задержка: {delay} мс"; // Обновляем Label с задержкой
        }

        private async Task NaturalMergeSort(List<Dictionary<string, string>> table, string sortKey)
        {
            int n = table.Count;
            int step = 1; // Номер шага сортировки

            while (true)
            {
                // Шаг 1: Разделение на естественные серии
                var fileB = new List<Dictionary<string, string>>();
                var fileC = new List<Dictionary<string, string>>();
                bool writeToB = true; // Чередуем запись между B и C

                Log($"\nШаг {step}: Начинаем разбиение исходного массива на естественные серии.");
                int i = 0;
                while (i < n)
                {
                    var series = new List<Dictionary<string, string>>();
                    series.Add(table[i]); // Начинаем серию с текущего элемента
                    Log($"Создаём новую серию, добавляем элемент {table[i][sortKey]}");
                    Dictionary<string, string> highlightedA = table[i];
                    await VisualizeCurrentState(table, fileB, fileC, sortKey, highlightedA: highlightedA);

                    // Находим серию максимальной длины
                    while (i + 1 < n && CompareValues(table[i][sortKey], table[i + 1][sortKey]) <= 0)
                    {
                        i++;
                        series.Add(table[i]);
                        Log($"Добавляем элемент {table[i][sortKey]} в текущую серию.");
                        highlightedA = table[i];
                        await VisualizeCurrentState(table, fileB, fileC, sortKey, highlightedA: highlightedA);
                    }
                    i++;

                    // Записываем серию либо в B, либо в C
                    if (writeToB)
                    {
                        fileB.AddRange(series);
                        Log($"Серия {string.Join(", ", series.Select(row => row[sortKey]))} записана в файл B.");
                    }
                    else
                    {
                        fileC.AddRange(series);
                        Log($"Серия {string.Join(", ", series.Select(row => row[sortKey]))} записана в файл C.");
                    }

                    writeToB = !writeToB; // Чередуем файл

                    // Визуализация текущего разбиения
                    await VisualizeCurrentState(table, fileB, fileC, sortKey);
                }

                Log($"\nПосле разбиения:\nB: {string.Join(", ", fileB.Select(row => row[sortKey]))}\nC: {string.Join(", ", fileC.Select(row => row[sortKey]))}");

                // Если файл C пуст, значит сортировка завершена
                if (fileC.Count == 0)
                {
                    Log("Файл C пуст. Сортировка завершена.");
                    return;
                }

                // Шаг 2: Слияние серий
                Log("\nНачинаем слияние файлов B и C обратно в файл A.");
                table.Clear();
                int bIndex = 0, cIndex = 0;

                while (bIndex < fileB.Count || cIndex < fileC.Count)
                {
                    // Определяем границы серий для слияния
                    int bEnd = bIndex;
                    int cEnd = cIndex;

                    if (bIndex < fileB.Count)
                    {
                        bEnd++;
                        while (bEnd < fileB.Count && CompareValues(fileB[bEnd - 1][sortKey], fileB[bEnd][sortKey]) <= 0)
                        {
                            bEnd++;
                        }
                    }

                    if (cIndex < fileC.Count)
                    {
                        cEnd++;
                        while (cEnd < fileC.Count && CompareValues(fileC[cEnd - 1][sortKey], fileC[cEnd][sortKey]) <= 0)
                        {
                            cEnd++;
                        }
                    }

                    Log($"\nПодготавливаем к слиянию серии из файла B: {string.Join(", ", fileB.GetRange(bIndex, bEnd - bIndex).Select(row => row[sortKey]))}");
                    Log($"Подготавливаем к слиянию серии из файла C: {string.Join(", ", fileC.GetRange(cIndex, cEnd - cIndex).Select(row => row[sortKey]))}");
                    await VisualizeCurrentState(table, fileB, fileC, sortKey, highlightedSeriesB: fileB.GetRange(bIndex, bEnd - bIndex), highlightedSeriesC: fileC.GetRange(cIndex, cEnd - cIndex));

                    // Слияние серий обратно в A
                    while (bIndex < bEnd || cIndex < cEnd)
                    {
                        Dictionary<string, string> highlightedB = bIndex < bEnd ? fileB[bIndex] : null;
                        Dictionary<string, string> highlightedC = cIndex < cEnd ? fileC[cIndex] : null;
                        await VisualizeCurrentState(table, fileB, fileC, sortKey, highlightedB, highlightedC);

                        if (bIndex < bEnd && (cIndex >= cEnd || CompareValues(fileB[bIndex][sortKey], fileC[cIndex][sortKey]) <= 0))
                        {
                            Log($"Сравнение: {fileB[bIndex][sortKey]} (из B) < {fileC.ElementAtOrDefault(cIndex)?[sortKey]} (из C): Берём {fileB[bIndex][sortKey]} из B.");
                            table.Add(fileB[bIndex]);
                            fileB.RemoveAt(bIndex);
                            bEnd--;
                            await VisualizeCurrentState(table, fileB, fileC, sortKey);
                        }
                        else if (cIndex < cEnd)
                        {
                            Log($"Сравнение: {fileC[cIndex][sortKey]} (из C) <= {fileB.ElementAtOrDefault(bIndex)?[sortKey]} (из B): Берём {fileC[cIndex][sortKey]} из C.");
                            table.Add(fileC[cIndex]);
                            fileC.RemoveAt(cIndex);
                            cEnd--;
                            await VisualizeCurrentState(table, fileB, fileC, sortKey);
                        }
                    }
                }

                Log($"\nПосле слияния: {string.Join(", ", table.Select(row => row[sortKey]))}");
                await VisualizeCurrentState(table, fileB, fileC, sortKey);

                step++;
            }
        }

        private async Task ThreeWayMergeSort(List<Dictionary<string, string>> table, string sortKey)
        {
            int n = table.Count;
            int seriesLength = 1; // Начальная длина цепочки
            int step = 1;

            while (seriesLength < n)
            {
                Log($"Шаг {step}: Длина цепочек для разбиения = {seriesLength}");

                // 1. Разделение массива на три вспомогательных файла
                var fileB = new List<Dictionary<string, string>>();
                var fileC = new List<Dictionary<string, string>>();
                var fileD = new List<Dictionary<string, string>>();

                int i = 0;
                while (i < table.Count)
                {
                    Log("Начинаем разбиение массива A на файлы B, C и D.");

                    for (int j = 0; j < seriesLength && i < table.Count; j++)
                    {
                        // Подсветка элемента перед переносом в файл B
                        if (i < table.Count)
                        {
                            Dictionary<string, string> highlightedA = table[i];
                            await VisualizeCurrentState(table, fileB, fileC, fileD, sortKey, highlightedA: highlightedA);
                            fileB.Add(table[i]);
                            Log($"Добавляем элемент {table[i][sortKey]} в файл B.");
                            table.RemoveAt(i);
                        }
                    }

                    for (int j = 0; j < seriesLength && i < table.Count; j++)
                    {
                        // Подсветка элемента перед переносом в файл C
                        if (i < table.Count)
                        {
                            Dictionary<string, string> highlightedA = table[i];
                            await VisualizeCurrentState(table, fileB, fileC, fileD, sortKey, highlightedA: highlightedA);
                            fileC.Add(table[i]);
                            Log($"Добавляем элемент {table[i][sortKey]} в файл C.");
                            table.RemoveAt(i);
                        }
                    }

                    for (int j = 0; j < seriesLength && i < table.Count; j++)
                    {
                        // Подсветка элемента перед переносом в файл D
                        if (i < table.Count)
                        {
                            Dictionary<string, string> highlightedA = table[i];
                            await VisualizeCurrentState(table, fileB, fileC, fileD, sortKey, highlightedA: highlightedA);
                            fileD.Add(table[i]);
                            Log($"Добавляем элемент {table[i][sortKey]} в файл D.");
                            table.RemoveAt(i);
                        }
                    }

                    // Визуализация текущего разбиения
                    await VisualizeCurrentState(table, fileB, fileC, fileD, sortKey);
                }

                Log($"После разбиения:\nB: {string.Join(", ", fileB.Select(row => row[sortKey]))}\nC: {string.Join(", ", fileC.Select(row => row[sortKey]))}\nD: {string.Join(", ", fileD.Select(row => row[sortKey]))}");

                // 2. Слияние данных из трех вспомогательных файлов обратно в основной массив (A)
                Log("\nНачинаем слияние файлов B, C и D обратно в файл A.");
                table.Clear();
                int bIndex = 0, cIndex = 0, dIndex = 0;

                while (bIndex < fileB.Count || cIndex < fileC.Count || dIndex < fileD.Count)
                {
                    // Определяем границы серий для слияния
                    int bEnd = Math.Min(bIndex + seriesLength, fileB.Count);
                    int cEnd = Math.Min(cIndex + seriesLength, fileC.Count);
                    int dEnd = Math.Min(dIndex + seriesLength, fileD.Count);

                    // Подсветка серий, которые сливаются
                    await VisualizeCurrentState(table, fileB, fileC, fileD, sortKey, highlightedSeriesB: fileB.GetRange(bIndex, bEnd - bIndex), highlightedSeriesC: fileC.GetRange(cIndex, cEnd - cIndex), highlightedSeriesD: fileD.GetRange(dIndex, dEnd - dIndex));

                    // Слияние серий обратно в A
                    while (bIndex < bEnd || cIndex < cEnd || dIndex < dEnd)
                    {
                        Dictionary<string, string> highlightedB = bIndex < bEnd ? fileB[bIndex] : null;
                        Dictionary<string, string> highlightedC = cIndex < cEnd ? fileC[cIndex] : null;
                        Dictionary<string, string> highlightedD = dIndex < dEnd ? fileD[dIndex] : null;
                        await VisualizeCurrentState(table, fileB, fileC, fileD, sortKey, highlightedB, highlightedC, highlightedD);

                        if (bIndex < bEnd && (cIndex >= cEnd || CompareValues(fileB[bIndex][sortKey], fileC[cIndex][sortKey]) <= 0) && (dIndex >= dEnd || CompareValues(fileB[bIndex][sortKey], fileD[dIndex][sortKey]) <= 0))
                        {
                            Log($"Сравнение: {fileB[bIndex][sortKey]} (из B) < {fileC.ElementAtOrDefault(cIndex)?[sortKey]} (из C) и {fileD.ElementAtOrDefault(dIndex)?[sortKey]} (из D): Берём {fileB[bIndex][sortKey]} из B.");
                            table.Add(fileB[bIndex]);
                            fileB.RemoveAt(bIndex);
                            bEnd--;
                            await VisualizeCurrentState(table, fileB, fileC, fileD, sortKey);
                        }
                        else if (cIndex < cEnd && (dIndex >= dEnd || CompareValues(fileC[cIndex][sortKey], fileD[dIndex][sortKey]) <= 0))
                        {
                            Log($"Сравнение: {fileC[cIndex][sortKey]} (из C) <= {fileD.ElementAtOrDefault(dIndex)?[sortKey]} (из D): Берём {fileC[cIndex][sortKey]} из C.");
                            table.Add(fileC[cIndex]);
                            fileC.RemoveAt(cIndex);
                            cEnd--;
                            await VisualizeCurrentState(table, fileB, fileC, fileD, sortKey);
                        }
                        else if (dIndex < dEnd)
                        {
                            Log($"Берём {fileD[dIndex][sortKey]} из D.");
                            table.Add(fileD[dIndex]);
                            fileD.RemoveAt(dIndex);
                            dEnd--;
                            await VisualizeCurrentState(table, fileB, fileC, fileD, sortKey);
                        }
                    }
                }

                Log($"\nПосле слияния: {string.Join(", ", table.Select(row => row[sortKey]))}");
                await VisualizeCurrentState(table, fileB, fileC, fileD, sortKey);

                // Увеличение длины серии и номера шага
                seriesLength *= 3; // Увеличиваем длину цепочки втрое
                step++;
            }
        }

        private async Task DirectMergeSort(List<Dictionary<string, string>> table, string sortKey)
        {
            int n = table.Count;
            int seriesLength = 1;
            int step = 1; // Номер шага сортировки

            // Создаем постоянное отображение для всех файлов: A, B, и C
            var fileB = new List<Dictionary<string, string>>();
            var fileC = new List<Dictionary<string, string>>();

            while (seriesLength < n)
            {
                Log($"\nШаг {step}: Длина цепочек для разбиения = {seriesLength}");

                // 1. Разделение исходного массива на два вспомогательных файла
                fileB.Clear();
                fileC.Clear();

                int i = 0;
                while (i < table.Count)
                {
                    Log("\nНачинаем разбиение массива A на файлы B и C.");
                    for (int j = 0; j < seriesLength && i < table.Count; j++)
                    {
                        Log($"Подготовка к переносу элемента {table[i][sortKey]} в файл B.");
                        Dictionary<string, string> highlightedA = table[i];
                        await VisualizeCurrentState(table, fileB, fileC, sortKey, highlightedA: highlightedA);

                        fileB.Add(table[i]);
                        Log($"Добавляем элемент {table[i][sortKey]} в файл B и удаляем его из файла A.");
                        table.RemoveAt(i);
                    }

                    for (int j = 0; j < seriesLength && i < table.Count; j++)
                    {
                        Log($"Подготовка к переносу элемента {table[i][sortKey]} в файл C.");
                        Dictionary<string, string> highlightedA = table[i];
                        await VisualizeCurrentState(table, fileB, fileC, sortKey, highlightedA: highlightedA);

                        fileC.Add(table[i]);
                        Log($"Добавляем элемент {table[i][sortKey]} в файл C и удаляем его из файла A.");
                        table.RemoveAt(i);
                    }

                    Log("\nТекущее состояние после разбиения:");
                    await VisualizeCurrentState(table, fileB, fileC, sortKey);
                }

                Log($"\nПосле разбиения:\nB: {string.Join(", ", fileB.Select(row => row[sortKey]))}\nC: {string.Join(", ", fileC.Select(row => row[sortKey]))}");

                // 2. Слияние вспомогательных файлов обратно в основной массив (A)
                Log("\nНачинаем слияние файлов B и C обратно в файл A.");
                table.Clear();
                int bIndex = 0, cIndex = 0;

                while (bIndex < fileB.Count || cIndex < fileC.Count)
                {
                    int bEnd = Math.Min(bIndex + seriesLength, fileB.Count);
                    int cEnd = Math.Min(cIndex + seriesLength, fileC.Count);

                    Log($"\nПодготавливаем к слиянию серии из файла B: {string.Join(", ", fileB.GetRange(bIndex, bEnd - bIndex).Select(row => row[sortKey]))}");
                    Log($"Подготавливаем к слиянию серии из файла C: {string.Join(", ", fileC.GetRange(cIndex, cEnd - cIndex).Select(row => row[sortKey]))}");
                    await VisualizeCurrentState(table, fileB, fileC, sortKey, highlightedSeriesB: fileB.GetRange(bIndex, bEnd - bIndex), highlightedSeriesC: fileC.GetRange(cIndex, cEnd - cIndex));

                    while (bIndex < bEnd || cIndex < cEnd)
                    {
                        Dictionary<string, string> highlightedB = bIndex < bEnd ? fileB[bIndex] : null;
                        Dictionary<string, string> highlightedC = cIndex < cEnd ? fileC[cIndex] : null;
                        await VisualizeCurrentState(table, fileB, fileC, sortKey, highlightedB, highlightedC);

                        if (bIndex < bEnd && (cIndex >= cEnd || CompareValues(fileB[bIndex][sortKey], fileC[cIndex][sortKey]) <= 0))
                        {
                            Log($"Сравнение: {fileB[bIndex][sortKey]} (из B) < {fileC.ElementAtOrDefault(cIndex)?[sortKey]} (из C): Берём {fileB[bIndex][sortKey]} из B.");
                            table.Add(fileB[bIndex]);
                            fileB.RemoveAt(bIndex); // Удаляем элемент из файла B
                            bEnd--; // Корректируем конечный индекс для серии B
                            await VisualizeCurrentState(table, fileB, fileC, sortKey);
                        }
                        else if (cIndex < cEnd)
                        {
                            Log($"Сравнение: {fileC[cIndex][sortKey]} (из C) <= {fileB.ElementAtOrDefault(bIndex)?[sortKey]} (из B): Берём {fileC[cIndex][sortKey]} из C.");
                            table.Add(fileC[cIndex]);
                            fileC.RemoveAt(cIndex); // Удаляем элемент из файла C
                            cEnd--; // Корректируем конечный индекс для серии C
                            await VisualizeCurrentState(table, fileB, fileC, sortKey);
                        }
                    }
                }

                Log($"\nПосле слияния: {string.Join(", ", table.Select(row => row[sortKey]))}");
                await VisualizeCurrentState(table, fileB, fileC, sortKey);

                // Увеличение длины серии и номера шага
                seriesLength *= 2;
                step++;
            }
        }

        private async Task VisualizeCurrentState(List<Dictionary<string, string>> table, List<Dictionary<string, string>> fileB, List<Dictionary<string, string>> fileC, List<Dictionary<string, string>> fileD, string sortKey, Dictionary<string, string> highlightedB = null, Dictionary<string, string> highlightedC = null, Dictionary<string, string> highlightedD = null, Dictionary<string, string> highlightedA = null, List<Dictionary<string, string>> highlightedSeriesB = null, List<Dictionary<string, string>> highlightedSeriesC = null, List<Dictionary<string, string>> highlightedSeriesD = null)
        {
            SortCanvas.Children.Clear();
            double canvasWidth = SortCanvas.ActualWidth;
            double blockWidth = canvasWidth / 4; // We have four blocks: A, B, C, D

            // Draw file A with highlight
            DrawBlock(table, blockWidth, 0, "A", sortKey, highlightedA, Brushes.LightBlue);

            // Draw file B with highlighted series
            DrawBlock(fileB, blockWidth, blockWidth, "B", sortKey, highlightedB, Brushes.LightSkyBlue, highlightedSeriesB, Brushes.LightGreen);

            // Draw file C with highlighted series
            DrawBlock(fileC, blockWidth, 2 * blockWidth, "C", sortKey, highlightedC, Brushes.MediumPurple, highlightedSeriesC, Brushes.LightPink);

            // Draw file D with highlighted series
            DrawBlock(fileD, blockWidth, 3 * blockWidth, "D", sortKey, highlightedD, Brushes.LightCoral, highlightedSeriesD, Brushes.LightYellow);

            await Task.Delay(delay); // Небольшая задержка для визуализации
        }

        private async Task VisualizeCurrentState(List<Dictionary<string, string>> table, List<Dictionary<string, string>> fileB, List<Dictionary<string, string>> fileC, string sortKey, Dictionary<string, string> highlightedB = null, Dictionary<string, string> highlightedC = null, Dictionary<string, string> highlightedA = null, List<Dictionary<string, string>> highlightedSeriesB = null, List<Dictionary<string, string>> highlightedSeriesC = null)
        {
            SortCanvas.Children.Clear();
            double canvasWidth = SortCanvas.ActualWidth;
            double blockWidth = canvasWidth / 3; // We have three blocks: A, B, C

            // Draw file A with highlight
            DrawBlock(table, blockWidth, 0, "A", sortKey, highlightedA, Brushes.LightBlue);

            // Draw file B with highlighted series
            DrawBlock(fileB, blockWidth, blockWidth, "B", sortKey, highlightedB, Brushes.LightSkyBlue, highlightedSeriesB, Brushes.LightGreen);

            // Draw file C with highlighted series
            DrawBlock(fileC, blockWidth, 2 * blockWidth, "C", sortKey, highlightedC, Brushes.MediumPurple, highlightedSeriesC, Brushes.LightPink);

            await Task.Delay(delay); // Небольшая задержка для визуализации
        }

        private void DrawBlock(List<Dictionary<string, string>> block, double blockWidth, double offsetX, string label, string sortKey, Dictionary<string, string> highlighted = null, Brush highlightColor = null, List<Dictionary<string, string>> highlightedSeries = null, Brush seriesHighlightColor = null)
        {
            double rectHeight = block.Count * 20;

            var rect = new Rectangle
            {
                Width = blockWidth - 10,
                Height = rectHeight,
                Fill = defaultColor,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            Canvas.SetLeft(rect, offsetX);
            Canvas.SetTop(rect, SortCanvas.ActualHeight - rectHeight - 20);
            SortCanvas.Children.Add(rect);

            // Label for the block
            var blockLabel = new TextBlock
            {
                Text = label,
                Foreground = Brushes.Black,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            };
            Canvas.SetLeft(blockLabel, offsetX + blockWidth / 2 - 30);
            Canvas.SetTop(blockLabel, SortCanvas.ActualHeight - rectHeight - 40);
            SortCanvas.Children.Add(blockLabel);

            for (int j = 0; j < block.Count; j++)
            {
                if (block[j] == null) continue;

                string key = block[j][headers[0]]; // Название строки (значение первого столбца)
                string value = block[j][sortKey]; // Значение для сортировки

                var labelItem = new TextBlock
                {
                    Text = $"{key} ({value})",
                    Foreground = Brushes.Black,
                    Background = (block[j] == highlighted && highlightColor != null) ? highlightColor :
                                 (highlightedSeries != null && highlightedSeries.Contains(block[j]) && seriesHighlightColor != null) ? seriesHighlightColor :
                                 Brushes.White,
                    TextAlignment = TextAlignment.Center,
                    Width = blockWidth - 10,
                    Height = 20
                };

                Canvas.SetLeft(labelItem, offsetX);
                Canvas.SetTop(labelItem, SortCanvas.ActualHeight - rectHeight + j * 20 - 20);
                SortCanvas.Children.Add(labelItem);
            }
        }

        private int CompareValues(string value1, string value2)
        {
            if (double.TryParse(value1, out var num1) && double.TryParse(value2, out var num2))
            {
                return num1.CompareTo(num2);
            }
            return string.Compare(value1, value2, StringComparison.Ordinal);
        }

        private void Log(string message)
        {
            log.AppendLine(message);
            LogTextBox.Text = log.ToString();
            LogTextBox.ScrollToEnd();
        }

        private void DirectMergeSortRadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            LogTextBox_Algorithm.Text = "Алгоритм сортировки простым слиянием является простейшим алгоритмом внешней сортировки, основанный на процедуре слияния серией.\r\n\r\nВ данном алгоритме длина серий фиксируется на каждом шаге. В исходном файле все серии имеют длину 1, после первого шага она равна 2, после второго – 4, после третьего – 8, после k -го шага – 2k.\r\n\r\nАлгоритм сортировки простым слиянием\r\n\r\nШаг 1. Исходный файл A разбивается на два вспомогательных файла B и C.\r\n\r\nШаг 2. Вспомогательные файлы B и C сливаются в файл A, при этом одиночные элементы образуют упорядоченные пары.\r\n\r\nШаг 3. Полученный файл A вновь обрабатывается, как указано в шагах 1 и 2. При этом упорядоченные пары переходят в упорядоченные четверки.\r\n\r\nШаг 4. Повторяя шаги, сливаем четверки в восьмерки и т.д., каждый раз удваивая длину слитых последовательностей до тех пор, пока не будет упорядочен целиком весь файл.\r\n\r\nПосле выполнения i проходов получаем два файла, состоящих из серий длины 2i. Окончание процесса происходит при выполнении условия 2i>=n. Следовательно, процесс сортировки простым слиянием требует порядка O(log n) проходов по данным.\r\n\r\nПризнаками конца сортировки простым слиянием являются следующие условия:\r\n\r\nдлина серии не меньше количества элементов в файле (определяется после фазы слияния);\r\nколичество серий равно 1 (определяется на фазе слияния).\r\nпри однофазной сортировке второй по счету вспомогательный файл после распределения серий остался пустым.";
        }

        private void NaturalMergeSortRadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            LogTextBox_Algorithm.Text = "Алгоритм сортировки естественным слиянием\r\n\r\nШаг 1. Исходный файл A разбивается на два вспомогательных файла B и C. Распределение происходит следующим образом: поочередно считываются записи ai исходной последовательности (неупорядоченной) таким образом, что если значения ключей соседних записей удовлетворяют условию A(ai)<=A(ai+1), то они записываются в первый вспомогательный файл B. Как только встречаются A(ai)>A(ai+1), то записи ai+1 копируются во второй вспомогательный файл C. Процедура повторяется до тех пор, пока все записи исходной последовательности не будут распределены по файлам.\r\n\r\nШаг 2. Вспомогательные файлы B и C сливаются в файл A, при этом серии образуют упорядоченные последовательности.\r\n\r\nШаг 3. Полученный файл A вновь обрабатывается, как указано в шагах 1 и 2.\r\n\r\nШаг 4. Повторяя шаги, сливаем упорядоченные серии до тех пор, пока не будет упорядочен целиком весь файл.\r\n\r\n Признаками конца сортировки естественным слиянием являются следующие условия:\r\n\r\nколичество серий равно 1 (определяется на фазе слияния).\r\nпри однофазной сортировке второй по счету вспомогательный файл после распределения серий остался пустым.";
        }

        private void MultiwayMergeSortRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            LogTextBox_Algorithm.Text = "Процесс многопутевого слияния почти как две капли воды схож с процессом прямого слияния. За одним лишь тем исключением, что мы будем использовать больше двух подфайлов. В своём примере я продемонстрирую трёхпутевой метод (значит, кол-во подфайлов будет равно трём).";
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

                DataGrid.ItemsSource = dataTable.DefaultView;
            }
        }
    }
}
