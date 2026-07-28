using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.ConnectionManager
{
    public class CompanyUser
    {
        [Key]
        public int CompanyUserId { get; set; }
        [DefaultValue("")]
        [MaxLength(50)]
        [Column(TypeName = "varchar")]
        public string CompanyUserName { get; set; }
        [MaxLength(50)]
        [Column(TypeName = "varchar")]
        public string CompanyUserPassword { get; set; }
        [MaxLength(50)]
        [Column(TypeName = "varchar")]
        public string CompanyDbName { get; set; }
        [DefaultValue(0)]
        public int CompanyId { get; set; }
        [DefaultValue(0)]
        public int LocationId { get; set; }
        [MaxLength(50)]
        [Column(TypeName = "varchar")]
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        [MaxLength(50)]
        [Column(TypeName = "varchar")]
        public string ModifiedUser { get; set; }
        public DateTime ModifiedDate { get; set; }
        

    }
}
