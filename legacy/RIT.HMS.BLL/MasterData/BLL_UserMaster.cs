using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using RIT.HMS.Domain.ConnectionManager;
using RIT.HMS.BLL.Configurations;
using RIT.HMS.Domain.Common;
using System.Data.SqlClient;
using System.Data;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_UserMaster
    {
        private  UnitOfWork _unitofwork;
        private BLL_Configuration _bllconfiguration;
        public BLL_UserMaster(string actualdb)
        {
            _unitofwork = new UnitOfWork(actualdb);
            _bllconfiguration= new  BLL_Configuration(actualdb);
        }
        public BLL_UserMaster()
        {
            _unitofwork = new UnitOfWork();
            _bllconfiguration =  new BLL_Configuration();
        }
       
        public SysUserMaster GetUserDetails(LoginViewModel model, int LocationId)
        {
            try
            {
                
                var sysUserMaster = _unitofwork.SysUserMasterRepository.Get(c => c.UserName == model.Email &&
                                                                             c.Password == model.Password && c.IsDelete == false
                                                                             && c.IsActive == true
                                                                            //c.LocationId == LocationId
                                                                            ).FirstOrDefault();
                if (sysUserMaster != null)
                {
                    return sysUserMaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public SysUserMaster GetUser(string uname, string pw)
        {
            try
            {
                SysUserMaster sysUserMaster = _unitofwork.SysUserMasterRepository.Get(u => u.UserName == uname && u.Password == pw).FirstOrDefault();
                if (sysUserMaster != null)
                {
                    return sysUserMaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public SysUserMaster GetUserByUserName(string uname)
        {
            try
            {
                SysUserMaster sysUserMaster = _unitofwork.SysUserMasterRepository.Get(u => u.UserName == uname).FirstOrDefault();
                if (sysUserMaster != null)
                {
                    return sysUserMaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int ChangePassword(SysUserMaster um)
        {
            try
            {

                var us = _unitofwork.SysUserMasterRepository.Get(u => u.UserName == um.UserName && u.SysUserMasterID == um.SysUserMasterID).FirstOrDefault();
                us.Password = um.Password;
                us.ConfirmPassword = um.ConfirmPassword;
                _unitofwork.SysUserMasterRepository.Update(us);
                if (_unitofwork.Save() == 1)
                { return 1; }
                else { return 0; }


            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public string GetUserRolesByUsername(SysUserMaster sysUserMaster)
        {

            List<string> lstRoles = new List<string>();
            string AllRols = "";
            try
            {
                var lstRolesg = (from ur in _unitofwork.SysUserPermissionRepository.Get(ur=> ur.FormId == 0 && ur.CompanyID == sysUserMaster.CompanyID)
                                 join us in _unitofwork.SysUserMasterRepository.Get(us=>us.Password == sysUserMaster.Password 
                                 && us.UserName == sysUserMaster.UserName && us.CompanyID == sysUserMaster.CompanyID) on ur.EmployeeCode equals us.EmployeeCode
                                // where us.Password == sysUserMaster.Password && us.UserName == sysUserMaster.UserName
                                // && ur.FormId==0 && ur.CompanyID==sysUserMaster.CompanyID && us.CompanyID==sysUserMaster.CompanyID
                                 select new
                                 {
                                     Roles = ur.FunctionName

                                 }).ToList();



                foreach (var item in lstRolesg)
                {
                    if (AllRols != "")
                    {
                        AllRols = AllRols + "," + item.Roles;
                    }
                    if (AllRols == "")
                    {
                        AllRols = item.Roles;
                    }
                }
                return AllRols;

            }
            catch (Exception ex)
            {

                return null;
                //  throw;
            }
        }

        public List<string> GetUserFunctionsByEmpCodeAndFormId(string empcode,int formid)
        {
            List<string> lstpermissions = new List<string>();
          
            try
            {
                var permissions = (from up in _unitofwork.SysUserPermissionRepository.Get()                               
                                 where up.EmployeeCode == empcode && up.FormId == formid
                                 select new
                                 {
                                     FunctionName = up.FunctionName

                                 }).ToList();
            
                foreach (var item in permissions)
                {                    
                    lstpermissions.Add(item.FunctionName);                                     
                }
                return lstpermissions;

            }
            catch (Exception ex)
            {

                return null;
               
            }
        }

        public string GetUserPermissionsTypeId(int typeid,string empcode)
        {

            List<string> lstRoles = new List<string>();
            string AllRols = "";
            try
            {
                var lstRolesg = (from u in _unitofwork.SysUserPermissionRepository.Get()                               
                                 where u.EmployeeCode == empcode && u.TypeID==typeid
                                 select new
                                 {
                                     Roles = u.FunctionName

                                 }).ToList();



                foreach (var item in lstRolesg)
                {
                    if (AllRols != "")
                    {
                        AllRols = AllRols + "," + item.Roles;
                    }
                    if (AllRols == "")
                    {
                        AllRols = item.Roles;
                    }
                }
                return AllRols;

            }
            catch (Exception ex)
            {

                return null;
                //  throw;
            }
        }

        public IEnumerable<SysUserMaster> GetUsers(Int32 compid)
        {
            try
            {
                IEnumerable<SysUserMaster> sysUserMaster = _unitofwork.SysUserMasterRepository.Get(u=>u.CompanyID==compid).OrderBy(u => u.SysUserMasterID);
                if (sysUserMaster != null)
                {
                    return sysUserMaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<SysUserGroupPermission> GetUserPermissionsByEmpNo(string empno)
        {
            try
            {
                List<SysUserPermission> permissions = _unitofwork.SysUserPermissionRepository.Get(u => u.EmployeeCode == empno).ToList();

                List<SysUserGroupPermission> grouppermission = new List<SysUserGroupPermission>();
                foreach (var permission in permissions)
                {
                    SysUserGroupPermission group = new SysUserGroupPermission();
                    group.SysUserGroupPermissionID = permission.SysUserPermissionID;
                    group.IsGrant = true;
                    group.FunctionName = permission.FunctionName;
                    group.FunctionDescription = permission.FunctionDescription;
                    grouppermission.Add(group);
                }


                return grouppermission;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<SysUserMaster> GetActiveUsers()
        {
            try
            {
                IEnumerable<SysUserMaster> sysUserMaster = _unitofwork.SysUserMasterRepository.Get(u => u.IsDelete == false).OrderBy(u => u.SysUserMasterID);
                if (sysUserMaster != null)
                {
                    return sysUserMaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public SysUserMaster GetUserById(long id)
        {
            try
            {
                SysUserMaster sysUserMaster =_unitofwork.SysUserMasterRepository.Get(u => u.SysUserMasterID == id).FirstOrDefault();
                if (sysUserMaster != null)
                {
                    return sysUserMaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public SysUserMaster GetUserEmpNoAndUserName(string empno, string username)
        {
            try
            {
                SysUserMaster sysUserMaster = _unitofwork.SysUserMasterRepository.Get(u => u.UserName == username || u.EmployeeCode == empno).FirstOrDefault();
                if (sysUserMaster != null)
                {
                    return sysUserMaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public long DeleteUserPermissionsByEmpCode(string empcode, long empid)
        {
            var res = 0;
            try
            {
               _unitofwork.SysUserPermissionRepository.DeleteRange(_unitofwork.SysUserPermissionRepository.Get(x => x.EmployeeCode == empcode && x.EmployeeID == empid));
                res = _unitofwork.Save();


            }
            catch (Exception)
            {

                throw;
            }
            return res;
        }

        public long DeleteUserPermissionsByEmployeeCode(string empcode)
        {
            var res = 0;
            try
            {
                _unitofwork.SysUserPermissionRepository.DeleteRange(_unitofwork.SysUserPermissionRepository.Get(x => x.EmployeeCode == empcode ));
                res = _unitofwork.Save();


            }
            catch (Exception)
            {

                throw;
            }
            return res;
        }




        public bool SaveUserMaster(SysUserMaster um)
        {

            _unitofwork.CreateTransaction();

                try
                {
                    var emp = _unitofwork.EmployeeRepository.Get(e => e.EmployeeCode == um.EmployeeCode).FirstOrDefault();                  
                    _unitofwork.SysUserMasterRepository.Insert(um);
                    if (_unitofwork.Save() == 1)
                    {
                        foreach (var permissions in um.SysUserGroupPermission.Where(p => p.IsGrant == true).ToList())
                        {
                            SysUserPermission userpermissions = new SysUserPermission();
                            userpermissions.IsAccess = true;
                            userpermissions.IsActive = true;
                            userpermissions.EmployeeID = Convert.ToInt32(emp.EmployeeID);
                            userpermissions.EmployeeCode = um.EmployeeCode;
                            userpermissions.EnCode = "";
                            userpermissions.FunctionName = permissions.FunctionName;
                            userpermissions.FunctionDescription = permissions.FunctionDescription;
                            var usergroup = GetUserGroupPermissionByGroupIdFunctionName(um.UserGroupID,
                                                                                               permissions.FunctionName);
                            userpermissions.TypeID = usergroup.TypeID;
                            userpermissions.FormId = usergroup.FormId;
                            userpermissions.GroupId = um.UserGroupID;
                            userpermissions.CreatedUser = um.CreatedUser;
                            userpermissions.CreatedDate = um.CreatedDate;
                            userpermissions.CompanyID = um.CompanyID;
                            _unitofwork.SysUserPermissionRepository.Insert(userpermissions);
                        }

                        var mode = ConfigurationManager.AppSettings["SubscriptionMode"];
                        if (mode == "ON")
                        {
                            var db = _bllconfiguration.GetConfiguration("RptDb",um.CompanyID).ConfigurationDescription;
                            UpdateHMSLoginManagerDb(um,1,db);       // 1 because insert                    
                        }

                        _unitofwork.Save();
                        _unitofwork.Commit();
                        return true;
                    }
                    else
                    {
                        _unitofwork.Rollback();
                        return false;
                    }

                }
                catch (Exception)
                {
                _unitofwork.Rollback();
                    return false;
                }
           
        }

        public bool UpdateUserMaster(SysUserMaster um)
        {
            _unitofwork.CreateTransaction();
                try
                {
                    var emp = _unitofwork.EmployeeRepository.Get(e => e.EmployeeCode == um.EmployeeCode).FirstOrDefault();
                if (emp != null)
                    //DeleteUserPermissionsByEmpCode(um.EmployeeCode, emp.EmployeeID);
                    DeleteUserPermissionsByEmployeeCode(um.EmployeeCode);
                else
                    return false;
                    // context.SysUserMasters.Add(um);
                    //if (context.SaveChanges() == 1)
                    //{

                    //context.SaveChanges();
                    if (um.SysUserGroupPermission.Count() > 0)
                    {
                        foreach (var permissions in um.SysUserGroupPermission.Where(p => p.IsGrant == true).ToList())
                        {
                            SysUserPermission userpermissions = new SysUserPermission();
                            userpermissions.IsAccess = true;
                            userpermissions.IsActive = true;
                        if (emp != null)
                            userpermissions.EmployeeID = Convert.ToInt32(emp.EmployeeID);
                        else
                            userpermissions.EmployeeID = 1;


                           userpermissions.EmployeeCode = um.EmployeeCode;
                            userpermissions.EnCode = "";
                            userpermissions.FunctionName = permissions.FunctionName;
                            userpermissions.FunctionDescription = permissions.FunctionDescription;

                            var usergroup = GetUserGroupPermissionByGroupIdFunctionName(um.UserGroupID,permissions.FunctionName);

                            userpermissions.TypeID = usergroup.TypeID;
                            userpermissions.FormId = usergroup.FormId;
                            userpermissions.GroupId = um.UserGroupID;
                            userpermissions.ModifiedUser = um.ModifiedUser;
                            userpermissions.ModifiedDate = um.ModifiedDate;
                            _unitofwork.SysUserPermissionRepository.Insert(userpermissions);
                        }


                        var mode = ConfigurationManager.AppSettings["SubscriptionMode"];
                        if (mode == "ON")
                        {                          
                            var db = _bllconfiguration.GetConfiguration("RptDb", um.CompanyID).ConfigurationDescription;
                            UpdateHMSLoginManagerDb(um,2,db);                                                       
                        }


                        if (_unitofwork.Save() != 0)
                        {
                            _unitofwork.Commit();
                            return true;
                        }
                        else
                        {
                            _unitofwork.Rollback();
                            return false;
                        }
                    }
                    else
                    {
                         _unitofwork.Rollback();
                        return false;
                    }


                }
                catch (Exception e)
                {
                     _unitofwork.Rollback();
                    return false;
                }

            

        }

        public SysUserGroupPermission GetUserGroupPermissionByGroupIdFunctionName(long groupid,string functionname)
        {

            var prm = _unitofwork.UserGroupPermissionRepository.Get(p => p.SysUserGroupId == groupid && p.FunctionName == functionname).FirstOrDefault();
            return prm;
        }

        public long DeleteUserPermissionsByEmpCodeGroupId(string empcode, long empid, long groupid)
        {
            var res = 0;
            try
            {
               _unitofwork.SysUserPermissionRepository.DeleteRange(_unitofwork.SysUserPermissionRepository.Get(x => x.EmployeeCode == empcode &&
                                                                                        x.EmployeeID == empid &&
                                                                                        x.GroupId == groupid));

                res = _unitofwork.Save();


            }
            catch (Exception)
            {

                throw;
            }
            return res;
        }

        public List<SysUserGroupPermission> CompairPermissions(long groupid, string empcode)
        {
            try
            {
                List<SysUserGroupPermission> group = _unitofwork.UserGroupPermissionRepository.Get(u => u.SysUserGroupId == groupid && u.IsDelete==false).ToList();
                List<SysUserPermission> user = _unitofwork.SysUserPermissionRepository.Get(p => p.GroupId == groupid && p.EmployeeCode == empcode).ToList();

                List<SysUserGroupPermission> permissions = new List<SysUserGroupPermission>();
                foreach (var g in group)
                {
                    foreach (var u in user)
                    {
                        if (u.FunctionName == g.FunctionName)
                        {
                            g.IsGrant = true;
                        }
                    }
                    // permissions.Add(g);
                }

                return group;
            }

            catch (Exception ex)
            {

                throw;
            }
        }

        public List<SysUserPermission> GetUserFunctionsByEmpCode(string empcode)
        {
            try
            {
               
                List<SysUserPermission> userfunctions = _unitofwork.SysUserPermissionRepository.Get(p => p.EmployeeCode == empcode).ToList();
                if (userfunctions != null)
                {
                    return userfunctions;
                }
                else
                {
                    return null;
                }
            }

            catch (Exception ex)
            {

                throw;
            }
        }

        public List<int> UpdateHMSLoginManagerDb(SysUserMaster um,int status,string db)
        {
            string user = string.Empty;
            if (status == 1)
            {
                user = um.CreatedUser;
            }
            else
            {
                user=um.ModifiedUser;

            }

            

                var result = _unitofwork.SysUserMasterRepository.SQLQuery<int>("SP_UpdateHMSLoginManager @CompanyId,@LocationId,@Username,@Password,@CreateUser,@DbName,@Status",
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value =um.CompanyID },
                    new SqlParameter("@LocationId", SqlDbType.Int) { Value = um.LocationId },
                    new SqlParameter("@Username", SqlDbType.VarChar) { Value = um.UserName },
                    new SqlParameter("@Password", SqlDbType.VarChar) { Value = um.Password },
                    new SqlParameter("@CreateUser", SqlDbType.VarChar) { Value = user },
                    new SqlParameter("@DbName", SqlDbType.VarChar) { Value = db },
                    new SqlParameter("@Status", SqlDbType.Int) { Value = status }                  
                    ).ToList();
                return result;
        }

    }
}
