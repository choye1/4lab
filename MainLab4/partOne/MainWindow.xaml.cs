using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace partOne
{
    public partial class MainWindow : Window
    {
        private int delay;
        private Rectangle[] rectangles;

        private SolidColorBrush defaultColor = (SolidColorBrush) new BrushConverter().ConvertFromString("#B2F2BB"); // SoftMintGreen
        private SolidColorBrush compareColor = (SolidColorBrush)new BrushConverter().ConvertFromString("#74C0FC"); // SkyBlue
        private SolidColorBrush swapColor = new SolidColorBrush(Color.FromRgb(220, 20, 60)); // Crimson (насыщенный красный)
        private double rectHeight;


        private static List<int> list = new List<int>();
        private static string filePath = "log.txt";
        private static Logger logger = new Logger(filePath);


        public MainWindow()
        {
            InitializeComponent();
        }

        private async void StartSorting_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Clear();
            try
            {
                delay = int.Parse(Delay.Text);
            }
            catch (Exception) 
            {
                MessageBox.Show("Пожалуйста, введите задержку.");
            }

            string input = AddList.Text; // Получаем текст из TextBox
            list.Clear(); // Очищаем список перед добавлением новых значений

            try
            {
   
                // Разделяем строку по запятой и пробелам, преобразуем в числа
                list.AddRange(input.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(int.Parse));
            }
            catch (FormatException)
            {
                MessageBox.Show("Пожалуйста, введите только числа, разделённые запятыми или пробелами.");
            }
            


                File.WriteAllText(filePath, string.Empty);
            CreateRectangles(list);

            

            string selectedSortMethod = ((ComboBoxItem)SortMethodComboBox.SelectedItem)?.Content.ToString();

            if (selectedSortMethod == null)
            {
                MessageBox.Show("Выберите метод сортировки.");
                return;
            }

            switch (selectedSortMethod)
            {
                case "Merge Sort":
                    await MergeSort(list, 0, list.Count - 1);
                    break;
                case "Selection Sort":
                    await SelectionSort(list);
                    break;
                case "Shell Sort":
                    await ShellSort(list);
                    break;
                case "Quick Sort":
                    await QuickSort(list, 0, list.Count - 1);
                    break;
                default:
                    MessageBox.Show("Неизвестный метод сортировки.");
                    break;
            }
        }

        private void CreateRectangles(List<int> arr)
        {
            SortCanvas.Children.Clear();
            rectangles = new Rectangle[arr.Count];

            double canvasWidth = SortCanvas.ActualWidth;
            double rectWidth = canvasWidth / arr.Count;
            if (arr.Max() < 10)
            {
                rectHeight = 20;
            }
            else if (arr.Max() < 20)
            {
                rectHeight = 10;
            }
            else if (arr.Max() < 40)
            {
                rectHeight = 5;
            }
            else if (arr.Max() < 80)
            {
                rectHeight = 2.5;
            }
            else
            {
                rectHeight = 1.25;
            }
            for (int i = 0; i < arr.Count; i++)
            {
                rectangles[i] = new Rectangle
                {
                    Width = rectWidth,
                    Height = arr[i] * rectHeight, // Масштабируем высоту (можно настроить)
                    Fill = defaultColor,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                Canvas.SetLeft(rectangles[i], i * rectWidth);
                Canvas.SetBottom(rectangles[i], 0);
                SortCanvas.Children.Add(rectangles[i]);


                // Добавляем номер столбца
                TextBlock textBlock = new TextBlock
                {
                    Text = arr[i].ToString(),  // Номер столбца (начиная с 1)
                    Foreground = Brushes.Black,
                    FontSize = 10, // Размер шрифта
                    TextAlignment = TextAlignment.Center
                };
                Canvas.SetLeft(textBlock, i * rectWidth + rectWidth / 2 - textBlock.ActualWidth / 2);
                Canvas.SetBottom(textBlock, rectangles[i].Height + 2);
                SortCanvas.Children.Add(textBlock);

            }
        }
        #region Сортировка выбором
        public async Task SelectionSort(List<int> list)
        {
            int n = list.Count;

            logger.Log("Начало сортировки выбором:");
            LoadLogsToTextBox();
            logger.Log($"Исходный массив: {string.Join(", ", list)}");
            LoadLogsToTextBox();

            for (int i = 0; i < n - 1; i++)
            {
                int minIndex = i;

                // Лог начала итерации
                logger.Log($"Итерация {i + 1}: ищем минимальный элемент от позиции {i} до {n - 1}.");
                LoadLogsToTextBox();

                for (int j = i + 1; j < n; j++)
                {
                    // Подсветка сравниваемых элементов
                    rectangles[j].Fill = compareColor;
                    rectangles[minIndex].Fill = compareColor;
                    Dispatcher.Invoke(() => UpdateRectangles(list));
                    await Task.Delay(delay);

                    // Лог сравнения
                    logger.Log($"Сравниваем: текущий минимум {list[minIndex]} (индекс {minIndex}) и {list[j]} (индекс {j}).");
                    LoadLogsToTextBox();

                    if (list[j] < list[minIndex])
                    {
                        // Лог обновления минимального элемента
                        logger.Log($"Найден новый минимальный элемент: {list[j]} (индекс {j}).");
                        LoadLogsToTextBox();

                        rectangles[minIndex].Fill = defaultColor; // Сброс цвета старого minIndex
                        minIndex = j;
                        rectangles[minIndex].Fill = compareColor; // Новый минимальный элемент
                    }

                    // Сброс цвета сравниваемого элемента
                    rectangles[j].Fill = defaultColor;
                    Dispatcher.Invoke(() => UpdateRectangles(list));
                }

                if (minIndex != i)
                {
                    // Лог обмена
                    logger.Log($"Меняем местами: {list[i]} (индекс {i}) и {list[minIndex]} (индекс {minIndex}).");
                    LoadLogsToTextBox();

                    // Подсветка элементов для обмена
                    rectangles[i].Fill = swapColor;
                    rectangles[minIndex].Fill = swapColor;
                    Dispatcher.Invoke(() => UpdateRectangles(list));
                    await Task.Delay(delay);

                    // Обмен элементов
                    (list[i], list[minIndex]) = (list[minIndex], list[i]);
                    Dispatcher.Invoke(() => UpdateRectangles(list));

                    // Сброс цвета после обмена
                    rectangles[i].Fill = defaultColor;
                    rectangles[minIndex].Fill = defaultColor;
                }
                else
                {
                    // Лог отсутствия обмена
                    logger.Log($"Элемент на позиции {i} уже минимальный, обмен не требуется.");
                    LoadLogsToTextBox();
                }

                // Лог состояния массива после итерации
                logger.Log($"Массив после итерации {i + 1}: {string.Join(", ", list)}");
                LoadLogsToTextBox();

                // Сброс цвета для текущего элемента
                rectangles[i].Fill = defaultColor;
                Dispatcher.Invoke(() => UpdateRectangles(list));
                await Task.Delay(delay);
            }

            // Лог завершения сортировки
            logger.Log($"Сортировка завершена: {string.Join(", ", list)}");
            LoadLogsToTextBox();
        }
        #endregion

        #region Сортировка слиянием
        private async Task MergeSort(List<int> arr, int left, int right)
        {
            logger.Log($"Начало сортировки слиянием для диапазона [{left}, {right}].");
            LoadLogsToTextBox();

            if (left < right)
            {
                int mid = (left + right) / 2;

                logger.Log($"Разделяем массив на [{left}:{mid}] и [{mid + 1}:{right}].");
                LoadLogsToTextBox();

                await MergeSort(arr, left, mid);
                await MergeSort(arr, mid + 1, right);

                await Merge(arr, left, mid, right);

                // Сброс цвета после объединения
                for (int i = left; i <= right; i++)
                {
                    rectangles[i].Fill = defaultColor;
                }
                Dispatcher.Invoke(() => UpdateRectangles(arr));
            }
        }

        private async Task Merge(List<int> arr, int left, int mid, int right)
        {
            logger.Log($"Начинаем слияние диапазонов [{left}:{mid}] и [{mid + 1}:{right}].");
            LoadLogsToTextBox();

            int n1 = mid - left + 1;
            int n2 = right - mid;

            // Создаем временные массивы
            int[] leftArray = new int[n1];
            int[] rightArray = new int[n2];

            for (int i = 0; i < n1; i++) leftArray[i] = arr[left + i];
            for (int j = 0; j < n2; j++) rightArray[j] = arr[mid + 1 + j];

            logger.Log($"Левая часть: {string.Join(", ", leftArray)}.");
            LoadLogsToTextBox();
            logger.Log($"Правая часть: {string.Join(", ", rightArray)}.");
            LoadLogsToTextBox();

            int iLeft = 0, iRight = 0, k = left;

            while (iLeft < n1 && iRight < n2)
            {
                // Подсветка сравниваемых элементов
                rectangles[left + iLeft].Fill = compareColor;
                rectangles[mid + 1 + iRight].Fill = compareColor;
                Dispatcher.Invoke(() => UpdateRectangles(arr));
                await Task.Delay(delay);

                logger.Log($"Сравниваем элементы: {leftArray[iLeft]} и {rightArray[iRight]}.");
                LoadLogsToTextBox();

                if (leftArray[iLeft] <= rightArray[iRight])
                {
                    arr[k] = leftArray[iLeft];
                    iLeft++;

                    logger.Log($"Элемент {arr[k]} из левой части вставлен на позицию {k}.");
                    LoadLogsToTextBox();
                }
                else
                {
                    arr[k] = rightArray[iRight];
                    iRight++;

                    logger.Log($"Элемент {arr[k]} из правой части вставлен на позицию {k}.");
                    LoadLogsToTextBox();
                }

                // Подсветка перемещенного элемента
                rectangles[k].Fill = swapColor;
                Dispatcher.Invoke(() => UpdateRectangles(arr));
                await Task.Delay(delay);

                // Сброс цвета после обработки
                rectangles[k].Fill = defaultColor;
                if (iLeft < n1) rectangles[left + iLeft].Fill = defaultColor;
                if (iRight < n2) rectangles[mid + 1 + iRight].Fill = defaultColor;

                Dispatcher.Invoke(() => UpdateRectangles(arr));
                k++;
            }

            // Копируем оставшиеся элементы левой части
            while (iLeft < n1)
            {
                arr[k] = leftArray[iLeft];

                // Подсветка перемещенного элемента
                rectangles[k].Fill = swapColor;
                Dispatcher.Invoke(() => UpdateRectangles(arr));
                await Task.Delay(delay);

                logger.Log($"Элемент {arr[k]} из левой части перемещен на позицию {k}.");
                LoadLogsToTextBox();

                rectangles[k].Fill = defaultColor;
                iLeft++;
                k++;
            }

            // Копируем оставшиеся элементы правой части
            while (iRight < n2)
            {
                arr[k] = rightArray[iRight];

                // Подсветка перемещенного элемента
                rectangles[k].Fill = swapColor;
                Dispatcher.Invoke(() => UpdateRectangles(arr));
                await Task.Delay(delay);

                logger.Log($"Элемент {arr[k]} из правой части перемещен на позицию {k}.");
                LoadLogsToTextBox();

                rectangles[k].Fill = defaultColor;
                iRight++;
                k++;
            }

            logger.Log($"Слияние завершено: текущий массив: {string.Join(", ", arr.Skip(left).Take(right - left + 1))}.");
            LoadLogsToTextBox();
        }
        #endregion

        #region Сортировка Шелла
        private async Task ShellSort(List<int> arr)
        {
            int n = arr.Count;
            logger.Log("Запуск сортировки Шелла...");
            LoadLogsToTextBox();

            for (int gap = n / 2; gap > 0; gap /= 2)
            {
                logger.Log($"Обработка зазора {gap}.");
                LoadLogsToTextBox();

                for (int i = gap; i < n; i++)
                {
                    int temp = arr[i];
                    int j;

                    // Лог сравнения
                    logger.Log($"Сравнение элемента {temp} (индекс {i}).");
                    LoadLogsToTextBox();

                    for (j = i; j >= gap; j -= gap)
                    {
                        // Выделяем сравниваемые элементы
                        rectangles[j].Fill = compareColor;
                        rectangles[j - gap].Fill = compareColor;
                        Dispatcher.Invoke(() => UpdateRectangles(arr));
                        await Task.Delay(delay);

                        if (arr[j - gap] <= temp)
                        {
                            // Если элемент не нужно перемещать, сбрасываем цвета
                            rectangles[j].Fill = defaultColor;
                            rectangles[j - gap].Fill = defaultColor;

                            // Лог об отсутствии перемещения
                            logger.Log($"Элементы {arr[j - gap]} (индекс {j - gap}) и {temp} (индекс {j}) не требуют перемещения.");
                            LoadLogsToTextBox();
                            break;
                        }

                        // Лог перемещения
                        logger.Log($"Перемещение {arr[j - gap]} с индекса {j - gap} на индекс {j}.");
                        LoadLogsToTextBox();

                        // Выделяем элементы, подверженные смене позиции
                        rectangles[j].Fill = swapColor;
                        rectangles[j - gap].Fill = swapColor;
                        Dispatcher.Invoke(() => UpdateRectangles(arr));
                        await Task.Delay(delay);

                        // Меняем местами элементы
                        arr[j] = arr[j - gap];

                        // Сбрасываем цвета после перемещения
                        rectangles[j].Fill = defaultColor;
                        rectangles[j - gap].Fill = defaultColor;
                    }

                    // Вставляем элемент
                    arr[j] = temp;

                    // Лог вставки
                    logger.Log($"Элемент {temp} вставлен на позицию {j}.");
                    LoadLogsToTextBox();

                    // Подсвечиваем вставленный элемент
                    rectangles[j].Fill = swapColor;
                    Dispatcher.Invoke(() => UpdateRectangles(arr));
                    await Task.Delay(delay);

                    rectangles[j].Fill = defaultColor;
                }

                // Лог состояния массива после обработки зазора
                logger.Log($"Состояние списка после зазора {gap}: {string.Join(", ", arr)}.");
                LoadLogsToTextBox();
            }

            // Лог завершения
            logger.Log("Сортировка завершена. Отсортированный список: " + string.Join(", ", arr));
            LoadLogsToTextBox();

            // Финальное выделение отсортированного массива
            
            Dispatcher.Invoke(() => UpdateRectangles(arr));
            await Task.Delay(delay * 2);

            foreach (var rect in rectangles)
            {
                rect.Fill = defaultColor;
            }
        }
        #endregion

        #region Быстрая сортировка
        private async Task QuickSort(List<int> arr, int left, int right)
        {
            if (left < right)
            {
                logger.Log($"QuickSort вызывается для диапазона [{left}, {right}].");
                LoadLogsToTextBox();

                int pi = await Partition(arr, left, right);

                logger.Log($"Опорный элемент установлен на индекс {pi} со значением {arr[pi]}.");
                LoadLogsToTextBox();

                await QuickSort(arr, left, pi - 1);
                await QuickSort(arr, pi + 1, right);
            }
        }

        private async Task<int> Partition(List<int> arr, int left, int right)
        {
            int pivot = arr[right];
            logger.Log($"Выбран опорный элемент: {pivot} (индекс {right}).");
            LoadLogsToTextBox();

            rectangles[right].Fill = compareColor; // Подсветка опорного элемента
            Dispatcher.Invoke(() => UpdateRectangles(arr));
            await Task.Delay(delay);

            int i = left - 1; // Индекс для элементов меньше опорного

            for (int j = left; j < right; j++)
            {
                // Подсветка сравниваемых элементов
                rectangles[j].Fill = compareColor;
                rectangles[right].Fill = compareColor;
                Dispatcher.Invoke(() => UpdateRectangles(arr));
                await Task.Delay(delay);

                logger.Log($"Сравниваем array[{j}] = {arr[j]} с опорным элементом {pivot}.");
                LoadLogsToTextBox();

                if (arr[j] < pivot)
                {
                    i++;

                    // Лог обмена
                    logger.Log($"Обмен: array[{i}] = {arr[i]} и array[{j}] = {arr[j]}.");
                    LoadLogsToTextBox();

                    // Подсветка элементов для обмена
                    rectangles[i].Fill = swapColor;
                    rectangles[j].Fill = swapColor;

                    (arr[i], arr[j]) = (arr[j], arr[i]); // Обмен значений
                    Dispatcher.Invoke(() => UpdateRectangles(arr));
                    await Task.Delay(delay);

                    // Сброс подсветки после обмена
                    rectangles[i].Fill = defaultColor;
                    rectangles[j].Fill = defaultColor;
                }
                else
                {
                    logger.Log($"Элемент array[{j}] = {arr[j]} больше опорного, обмен не требуется.");
                    LoadLogsToTextBox();
                }

                // Сброс подсветки текущего элемента
                rectangles[j].Fill = defaultColor;
                Dispatcher.Invoke(() => UpdateRectangles(arr));
            }

            // Обмен опорного элемента с элементом на позиции i + 1
            logger.Log($"Устанавливаем опорный элемент {pivot} на позицию {i + 1}.");
            LoadLogsToTextBox();

            // Подсветка для обмена
            rectangles[i + 1].Fill = swapColor;
            rectangles[right].Fill = swapColor;

            (arr[i + 1], arr[right]) = (arr[right], arr[i + 1]); // Обмен значений
            Dispatcher.Invoke(() => UpdateRectangles(arr));
            await Task.Delay(delay);

            // Сброс подсветки после обмена
            rectangles[i + 1].Fill = defaultColor;
            rectangles[right].Fill = defaultColor;

            return i + 1;
        }
        #endregion

        private void UpdateRectangles(List<int> arr)
        {
            for (int i = 0; i < arr.Count; i++)
            {
                rectangles[i].Height = arr[i] * rectHeight; // Обновляем высоту
                // Обновляем позицию текста
                TextBlock textBlock = (TextBlock)SortCanvas.Children[i * 2 + 1]; // Находим TextBlock (нечетные индексы)
                textBlock.Text = arr[i].ToString();
                Canvas.SetLeft(textBlock, i * (SortCanvas.ActualWidth / arr.Count) + (SortCanvas.ActualWidth / arr.Count) / 2 - textBlock.ActualWidth / 2);
                Canvas.SetBottom(textBlock, rectangles[i].Height + 2);
            }
        }

        private void LoadLogsToTextBox()
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);

                // Проверка, что файл не пустой
                if (lines.Length > 0)
                {
                    // Отображение последней строки в LogTextBox
                    LogTextBox.Text += lines[lines.Length - 1] + "\n";
                }
                else
                {
                    // Если файл пустой, отображаем сообщение
                    LogTextBox.Text += "Файл логов пуст. \n";
                }

                // Автоматическая прокрутка к последней записи
                LogTextBox.ScrollToEnd();
            }
            catch (Exception ex)
            {
                // Обработка ошибок чтения файла
                LogTextBox.Text = $"Ошибка при загрузке логов: {ex.Message}";
            }
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Text doc | *.txt";

            bool? success = fileDialog.ShowDialog();
            if (success == true)
            {
                string path = fileDialog.FileName;
                
                string str = File.ReadAllText(path);
                
                AddList.Text = str;
            }
            else
            {

            }
        }
    }
}