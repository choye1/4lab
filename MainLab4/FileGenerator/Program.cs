using OfficeOpenXml;

namespace FileGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = "test_data.xlsx";
            FileInfo fileInfo = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

                // Заголовки столбцов
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Name";
                worksheet.Cells[1, 3].Value = "Age";

                // Данные
                for (int i = 2; i <= 101; i++)
                {
                    worksheet.Cells[i, 1].Value = i - 1;
                    worksheet.Cells[i, 2].Value = $"Name{i - 1}";
                    worksheet.Cells[i, 3].Value = new Random().Next(18, 65);
                }

                package.Save();
            }

            Console.WriteLine($"Excel file generated at: {filePath}");
        }
    }
}
