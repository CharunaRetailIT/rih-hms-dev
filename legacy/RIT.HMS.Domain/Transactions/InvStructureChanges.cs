using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Transactions
{
    public class InvStructureChanges
    {
        [Key]
        public long strucid { get; set; }

        public string tableName { get; set; }
        public string query { get; set; }
        public string spName { get; set; }

        public string ViewName { get; set; }
        public string ColumnName { get; set; }
        public string Password { get; set; }
        public bool status { get; set; }
    }
}
