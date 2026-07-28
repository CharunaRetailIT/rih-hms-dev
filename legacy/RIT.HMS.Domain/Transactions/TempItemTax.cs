using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain.Transactions
{
    public class TempItemTax
    {



        [Key]
        public Int64 Idx { get; set; }
        public int LocationId { get; set; }
        [Column(TypeName = "VARCHAR")]
        [StringLength(20)]
        public string UnitNo { get; set; }
        [StringLength(10)]
        [Column(TypeName = "char")]
        public string Receipt { get; set; }
        [Column(TypeName = "Date")]
        public DateTime TDate { get; set; }
        public int RowNo { get; set; }
        public Int64 ProductId { get; set; }
        public decimal Nett { get; set; }
        public Int64 TaxId { get; set; }
        [StringLength(50)]
        [Column(TypeName = "char")]
        public string TaxCode { get; set; }
        [StringLength(50)]
        [Column(TypeName = "char")]
        public string TaxName { get; set; }
        public decimal TaxRate { get; set; }
        public decimal CalcAmt { get; set; }
        public decimal TaxAmount { get; set; }
        public Int64 ZNo { get; set; }
        public Int16 Online { get; set; }

        public int DataTransfer { get; set; }

    }
}