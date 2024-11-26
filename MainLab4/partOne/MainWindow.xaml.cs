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
            delay = int.Parse(Delay.Text) * 1000;

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


        public async Task SelectionSort(List<int> list)
        {
            logger.Log("Сортировка выбором – это алгоритм, суть которого заключается в постоянном сравнении элементов неотсортированой части. Наименьший элемент в неотсортированой части меняется местами с первым ее элементом и сразу становится отсортированой частью.");
            LoadLogsToTextBox();

            int n = list.Count;

            logger.Log("Начало сортировки выбором:");
            LoadLogsToTextBox();

            logger.Log($"Исходный массив: {string.Join(", ", list)}");
            LoadLogsToTextBox();

            for (int i = 0; i < n - 1; i++)
            {
                int minIndex = i;

                rectangles[i].Fill = compareColor; 
                Dispatcher.Invoke(() => UpdateRectangles(list));

                
                for (int j = i + 1; j < n; j++)
                {
                    rectangles[j].Fill = compareColor;
                    Dispatcher.Invoke(() => UpdateRectangles(list));

                    if (list[j] < list[minIndex])
                    {
                        rectangles[minIndex].Fill = defaultColor;
                        minIndex = j;
                        rectangles[minIndex].Fill = compareColor;
                    }
                    await Task.Delay(delay);
                    Dispatcher.Invoke(() =>
                    {
                        rectangles[j].Fill = defaultColor; 

                        UpdateRectangles(list);
                    });
                }

              
                logger.Log($"Итерация {i + 1}: Текущий массив: {string.Join(", ", list)}");
                LoadLogsToTextBox();

                logger.Log($"Найден минимальный элемент: {list[minIndex]} на позиции {minIndex}");
                LoadLogsToTextBox();
                if (minIndex != i)
                {
                    rectangles[i].Fill = swapColor;
                    rectangles[minIndex].Fill = swapColor;

                    await Task.Delay(delay);
                    Dispatcher.Invoke(() =>
                    {
                        

                        (list[i], list[minIndex]) = (list[minIndex], list[i]);

                        UpdateRectangles(list);
                        LoadLogsToTextBox();

                    });

                    rectangles[i].Fill = defaultColor;
                    rectangles[minIndex].Fill = defaultColor;

                    logger.Log($"Поменяли местами элементы на позициях {i} и {minIndex}");
                    LoadLogsToTextBox();

                }
                else
                {
                    logger.Log($"Элемент на позиции {i} уже на своем месте");
                    LoadLogsToTextBox();
                } 
                

                logger.Log($"Массив после итерации {i + 1}: {string.Join(", ", list)}");

                await Task.Delay(delay);
                Dispatcher.Invoke(() =>
                {
                    rectangles[i].Fill = defaultColor;

                    UpdateRectangles(list);
                    LoadLogsToTextBox();


                });
            }

            logger.Log($"Сортировка завершена: {string.Join(", ", list)}");
            LoadLogsToTextBox();

        }

        private async Task MergeSort(List<int> arr, int left, int right)
        {
            logger.Log("Сортировка слиянием – это алгоритм который разделяет массив данных на две равные части, потом вызывает сам себя для каждой из этих частей и так до тех пор, пока массивы не станут единичными. Дальше эти подмассивы сравниваются со своей «половинкой» и в упорядоченом виде объединяются в новый отсортированый массив.");
            LoadLogsToTextBox();

            logger.Log("Начало сортировки слияннием:");
            LoadLogsToTextBox();

            logger.Log($"Исходный массив: {string.Join(", ", list)}");
            LoadLogsToTextBox();

            if (left < right)
            {
                int mid = (left + right) / 2;

                logger.Log($"Разделяем массив: [{left}:{mid}] и [{mid + 1}:{right}]");
                LoadLogsToTextBox();

                await MergeSort(arr, left, mid);
                await MergeSort(arr, mid + 1, right);

                await Merge(arr, left, mid, right);

                for (int i = left; i <= right; i++)
                {
                    rectangles[i].Fill = defaultColor;
                }
                Dispatcher.Invoke(() => UpdateRectangles(arr));
            }
        }



        private async Task Merge(List<int> arr, int left, int mid, int right)
        {
            logger.Log($"Сливаем массивы: [{left}:{mid}] и [{mid + 1}:{right}]");

            int n1 = mid - left + 1;
            int n2 = right - mid;

            int[] leftArray = new int[n1];
            int[] rightArray = new int[n2];

            for (int i = 0; i < n1; i++)
                leftArray[i] = arr[left + i];
            for (int j = 0; j < n2; j++)
                rightArray[j] = arr[mid + 1 + j];

            logger.Log("Левая часть: " + string.Join(", ", leftArray));
            LoadLogsToTextBox();

            logger.Log("Правая часть: " + string.Join(", ", rightArray));
            LoadLogsToTextBox();

            int iLeft = 0, iRight = 0;
            int k = left;

            while (iLeft < n1 && iRight < n2)
            {
                rectangles[k].Fill = compareColor; 
                rectangles[left + iLeft].Fill = compareColor;
                rectangles[mid + 1 + iRight].Fill = compareColor;
                Dispatcher.Invoke(() => UpdateRectangles(arr));

                await Task.Delay(delay);

                logger.Log($"Сравниваем: {leftArray[iLeft]} и {rightArray[iRight]}");
                LoadLogsToTextBox();

                if (leftArray[iLeft] <= rightArray[iRight])
                {
                    arr[k] = leftArray[iLeft];
                    iLeft++;
                    logger.Log($"Элемент {leftArray[iLeft - 1]} из левой части остался на своем месте.");
                    LoadLogsToTextBox();

                }
                else
                {
                    arr[k] = rightArray[iRight];
                    iRight++;
                    logger.Log($"Элемент {rightArray[iRight - 1]} из правой части переместился на позицию {k}.");
                    LoadLogsToTextBox();

                }

                rectangles[k].Fill = swapColor;
                Dispatcher.Invoke(() => UpdateRectangles(arr));
                await Task.Delay(delay);

                logger.Log($"Массив после замены: {string.Join(", ", arr)}");
                LoadLogsToTextBox();

                rectangles[k].Fill = defaultColor;
                if (left + iLeft - 1 >= left && left + iLeft - 1 < left + n1)
                {
                    rectangles[left + iLeft - 1].Fill = defaultColor;
                }
                if (mid + 1 + iRight - 1 >= mid + 1 && mid + 1 + iRight - 1 < mid + 1 + n2)
                {
                    rectangles[mid + 1 + iRight - 1].Fill = defaultColor;
                }
                Dispatcher.Invoke(() => UpdateRectangles(arr));

                k++;
            }

            while (iLeft < n1)
            {
                arr[k] = leftArray[iLeft];

                rectangles[k].Fill = swapColor;
                rectangles[left + iLeft].Fill = compareColor;
                Dispatcher.Invoke(() => UpdateRectangles(arr));
                await Task.Delay(delay);

                logger.Log($"Элемент {leftArray[iLeft]} из левой части переместился на позицию {k}.");
                LoadLogsToTextBox();

                logger.Log($"Массив после замены: {string.Join(", ", arr)}");
                LoadLogsToTextBox();

                rectangles[k].Fill = defaultColor;
                rectangles[left + iLeft].Fill = defaultColor;
                Dispatcher.Invoke(() => UpdateRectangles(arr));

                iLeft++;
                k++;
            }

            while (iRight < n2)
            {
                arr[k] = rightArray[iRight];

                rectangles[k].Fill = swapColor;
                rectangles[mid + 1 + iRight].Fill = compareColor;
                Dispatcher.Invoke(() => UpdateRectangles(arr));
                await Task.Delay(delay);

                logger.Log($"Элемент {rightArray[iRight]} из правой части переместился на позицию {k}.");
                LoadLogsToTextBox();

                logger.Log($"Массив после замены: {string.Join(", ", arr)}");
                LoadLogsToTextBox();


                rectangles[k].Fill = defaultColor;
                rectangles[mid + 1 + iRight].Fill = defaultColor;
                Dispatcher.Invoke(() => UpdateRectangles(arr));

                iRight++;
                k++;
            }
        }



        private async Task ShellSort(List<int> arr)
        {
            logger.Log("Сортировка Шелла – это модифицированая сортировка вставками. Ее суть в том, что массив данных разбивается на так называемые Gap’ы (зазоры). Каждая итерация сортировки – это сравнение элементов массива на расстоянии текущего зазора друг от друга, а затем упорядочивание этих элементов. С каждым проходом по массиву зазор уменьшается вдвое и эти действия повторяются до окончания сортировки.");
            LoadLogsToTextBox();

            int n = arr.Count;
            logger.Log("Начало сортировки Шелла. Исходный список: " + string.Join(", ", arr));
            LoadLogsToTextBox();

            for (int gap = n / 2; gap > 0; gap /= 2)
            {
                logger.Log($"Текущий зазор: {gap}");
                LoadLogsToTextBox();

                for (int i = gap; i < n; i += 1)
                {
                    int temp = arr[i];
                    logger.Log($"Сравнение элемента {temp} (индекс {i})");
                    LoadLogsToTextBox();

                    int j;

                    for (j = i; j >= gap && arr[j - gap] > temp; j -= gap)
                    {
                        logger.Log($"Перемещение элемента {arr[j - gap]} с индекса {j - gap} на индекс {j}");
                        LoadLogsToTextBox();

                        // Выделяем сравниваемые элементы
                        rectangles[j].Fill = compareColor;
                        rectangles[j - gap].Fill = compareColor;
                        Dispatcher.Invoke(() => UpdateRectangles(arr));
                        await Task.Delay(delay);

                        arr[j] = arr[j - gap];

                        // Выделяем перемещаемые элементы
                        rectangles[j].Fill = swapColor;
                        rectangles[j - gap].Fill = swapColor;
                        Dispatcher.Invoke(() => UpdateRectangles(arr));
                        await Task.Delay(delay);

                        rectangles[j].Fill = defaultColor;
                        rectangles[j - gap].Fill = defaultColor;
                    }

                    arr[j] = temp;
                    logger.Log($"Вставка элемента {temp} на позицию {j}");
                    LoadLogsToTextBox();

                    // Выделяем вставляемый элемент
                    rectangles[j].Fill = swapColor;
                    Dispatcher.Invoke(() => UpdateRectangles(arr));
                    await Task.Delay(delay);
                    rectangles[j].Fill = defaultColor;
                }

                logger.Log("Список после обработки зазора " + gap + ": " + string.Join(", ", arr));
                LoadLogsToTextBox();
            }

            logger.Log("Сортировка завершена. Отсортированный список: " + string.Join(", ", arr));
            LoadLogsToTextBox();
        }

        private async Task QuickSort(List<int> arr, int left, int right)
        {
            logger.Log("Быстрая сортировка - это алгоритм \"разделяй и властвуй\", который рекурсивно сортирует массив, разделяя его на части относительно опорного элемента и объединяя отсортированные части.");
            LoadLogsToTextBox();

            logger.Log("Начало быстрой сортировки:");
            LoadLogsToTextBox();

            logger.Log($"Исходный массив: {string.Join(", ", list)}");
            LoadLogsToTextBox();
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
       

        private async Task<int> Partition(List<int> array, int left, int right)
        {
            int pivot = array[right];
            logger.Log($"Выбран опорный элемент: {pivot} (индекс {right}).");
            LoadLogsToTextBox();

            rectangles[right].Fill = compareColor; 
            int i = (left - 1); 

            for (int j = left; j < right; j++)
            {
                logger.Log($"Сравниваем array[{j}] = {array[j]} с pivot = {pivot}.");
                LoadLogsToTextBox();

                rectangles[j].Fill = compareColor; 
                Dispatcher.Invoke(() => UpdateRectangles(array));

                if (array[j] < pivot)
                {
                    i++;
                    (array[i], array[j]) = (array[j], array[i]);

                    logger.Log($"Обмен: array[{i}] = {array[i]}, array[{j}] = {array[j]} -> {string.Join(", ", array)}");
                    LoadLogsToTextBox();

                    rectangles[i].Fill = swapColor;
                    rectangles[j].Fill = swapColor;

                    await Task.Delay(delay);
                    Dispatcher.Invoke(() =>
                    {
                        UpdateRectangles(array);
                        rectangles[i].Fill = defaultColor;
                        rectangles[j].Fill = defaultColor;
                        UpdateRectangles(array);
                    });
                }
                else
                {
                    await Task.Delay(delay);
                    Dispatcher.Invoke(() =>
                    {
                        rectangles[j].Fill = defaultColor; 

                        UpdateRectangles(array);
                    });
                }
            }

            (array[i + 1], array[right]) = (array[right], array[i + 1]);
            rectangles[right].Fill = defaultColor;  
            logger.Log($"Обмен опорного элемента: array[{i + 1}] = {array[i + 1]}, array[{right}] = {array[right]} -> {string.Join(", ", array)}");
            LoadLogsToTextBox();

            await Task.Delay(delay);
            Dispatcher.Invoke(() =>
            {
                UpdateRectangles(array);
            });
            return i + 1;
        }

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

        private void Back(object sender, RoutedEventArgs e)
        {

        }

        private void Stop(object sender, RoutedEventArgs e)
        {

        }

        private void Forward(object sender, RoutedEventArgs e)
        {

        }
    }
}