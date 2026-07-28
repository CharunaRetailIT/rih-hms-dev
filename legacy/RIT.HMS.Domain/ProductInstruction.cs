using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RIT.HMS.Domain
{
    public class ProductInstruction
    {
        public ProductInstruction()
        {
            Idetail = new List<Detail>();
        }



        public long ProductInstructionId { get; set; }
        public string InstructionList { get; set; }
        public long ProductId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        [NotMapped]
        public long Department { get; set; }

        [NotMapped]
        public long Category { get; set; }

        [NotMapped]
        public long SubCategory { get; set; }

        [NotMapped]
        public long Product { get; set; }

        [NotMapped]
        public string ServingUnit { get; set; }

        [NotMapped]
        public string[] Instruction { get; set; }


        [NotMapped]
        public List<Detail> Idetail { get; set; }

        [NotMapped]
        public string ProductName { get; set; }

      
        public class Detail
        {
            public long ProductInstructionId { get; set; }
            public string InstructionList { get; set; }
            public long ProductId { get; set; }
            public DateTime CreateDate { get; set; }
            
        }

        [DefaultValue(0)]
        public int CompanyId { get; set; }

    }
}