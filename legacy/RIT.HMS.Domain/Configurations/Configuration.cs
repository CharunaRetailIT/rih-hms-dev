using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Configurations
{
   public class Configuration
    {
        public int ConfigurationId { get; set; }
        [MaxLength(10)]
        public string ConfigurationKey { get; set; }
        [MaxLength(50)]
        public string ConfigurationDescription { get; set; }
        public int EffectLocationId { get; set; }
        public bool ConfigurationOn { get; set; }
        public bool ConfigurationActive { get; set; }
        public bool ConfigurationDelete { get; set; }
        public DateTime? CreateDate { get; set; }
        public int CreateUserId { get; set; }
        public int CompanyId { get; set; }
    }
}
