using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Loyalty
{
    public  class CardGenerationLocationSetting : BaseEntity
    {
        public long CardGenerationLocationSettingId { get; set; }
        [DefaultValue(0)]
        public int CardNoLength { get; set; }
        [DefaultValue(0)]
        public long CardStartingNo { get; set; }
        [DefaultValue(0)]
        public long EncodeStartingNo { get; set; }
        [DefaultValue(false)]
        public bool IsDelete { get; set; }

        [NotMapped]
        public string LocationPrefix { get; set; }

        

    }
}
