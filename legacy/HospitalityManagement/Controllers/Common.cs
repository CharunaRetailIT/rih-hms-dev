using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Controllers
{
    public class Common
    {
        public bool CheckImageType(string contenttype)
        {
            string[] ctype = contenttype.Split('/');
            if (ctype[0] != "image" || ctype[0] == null || ctype[0] == "")
            {
                return false;
            }
            else
            {
                return true;
            }


        }
    }
}