using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Transactions
{
    public class InvGiftVoucherDocumentNumber
    {
        [Key]
        public long DocumentNumberId { get; set; }
        [DefaultValue(0)]
        public int DocumentId { get; set; }
        [DefaultValue("")]
        [MaxLength(20)]
        public string DocumentName { get; set; }

        [DefaultValue("")]
        [MaxLength(20)]
        public string DocumentNo { get; set; }
        [DefaultValue(0)]
        public int GroupOfCompanyID { get; set; }
        [DefaultValue(0)]
        public int CompanyID { get; set; }
        [DefaultValue(0)]
        public int LocationId { get; set; }
    }
}
