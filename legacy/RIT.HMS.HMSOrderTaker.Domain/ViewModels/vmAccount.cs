using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.HMSOrderTaker.Domain.ViewModels
{
    public class vmAccount
    {
        [DefaultValue("")]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string FunctionName { get; set; }
        public string FunctionDescription { get; set; }
        public int Order { get; set; }

    }
}
