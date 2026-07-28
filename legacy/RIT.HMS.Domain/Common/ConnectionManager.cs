using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RIT.HMS.Domain.Common
{
    public static class ConnectionManager
    {
      // public static string CurrentConnectionName { get; set; }

        public static string CurrentConnectionName
        {
            get
            {
                return HttpContext.Current.Application["CurrentConnectionName"] as string;
            }
            set
            {
                HttpContext.Current.Application["CurrentConnectionName"] = value;
            }
        }

    }
}
