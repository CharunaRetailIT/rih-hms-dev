using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RIT.HMS.BLL.MasterData;
using System.Web.Mvc;
using System.Threading.Tasks;


namespace HospitalityManagement
{
    public class AppManager
    {
       private readonly BLL_UserMaster _bllusermaster;
        public AppManager()
        {
            _bllusermaster = new BLL_UserMaster();
        }

        public AppManager(string connectionaname)
        {
            _bllusermaster = new BLL_UserMaster(connectionaname);
        }

        public bool SetPermissions(int formid,string empcode, string functionname)
        {          
            var previlages = _bllusermaster.GetUserFunctionsByEmpCodeAndFormId(empcode, formid);
            if (previlages != null)
            {
                if (previlages.Contains(functionname))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }          
        }


        public bool CheckReportPermissions(int formid, string empcode, string functionname)
        {
            var previlages = _bllusermaster.GetUserFunctionsByEmpCodeAndFormId(empcode, formid);
            if (previlages != null)
            {
                if (previlages.Contains(functionname))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}