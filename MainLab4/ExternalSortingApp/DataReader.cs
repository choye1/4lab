using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using CsvHelper;
using System.Globalization;
using System.IO;
using System.Data;
using System.Formats.Asn1;

namespace ExternalSortingApp
{

    public class DataReader
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
