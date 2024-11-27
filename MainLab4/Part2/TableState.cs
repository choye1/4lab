using System.Data;

namespace Part2
{
    public partial class MainWindow
    {
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