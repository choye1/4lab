using System.Data;

namespace Part2
{
    public partial class MainWindow
    {
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
}

