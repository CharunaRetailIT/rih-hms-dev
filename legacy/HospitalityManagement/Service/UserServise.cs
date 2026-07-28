using HospitalityManagement.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public  class UserServise
    {
       ApplicationDbContext context = new ApplicationDbContext();

        ////static List<User> users = new List<User>() {

        //    new User() {Email="abc@gmail.com",Roles="Admin,Editor",Password="abcadmin" },
        //    new User() {Email="xyz@gmail.com",Roles="Editor",Password="xyzeditor" }
        //};


        //public  User GetUserDetails(User user)
        //{
        //    return users.Where(u => u.Email.ToLower() == user.Email.ToLower() &&
        //    u.Password == user.Password).FirstOrDefault();
        //}


        //public string[] GetUserRolesByUsername(string Username)
        //{

        //    List<string> lstRoles = new List<string>();

        //    try
        //    {
        //        var lstRolesg = (from ur in context.SysUserPermissions
        //                         join us in context.SysUserMasters on ur.EmployeeID equals us.SysUserMasterID
        //                         where us.Password == Username
        //                         select new
        //                         {
        //                             Roles = ur.FunctionName

        //                         }).ToList();


        //        foreach (var item in lstRolesg)
        //        {
        //            lstRoles.Add(item.Roles);
        //        }


        //            string[] arrRoles = lstRoles.ToArray();
        //            return arrRoles;

        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}

        public SysUserMaster GetUserDetails(LoginViewModel model,int LocationId)
        {
            try
            {
                SysUserMaster sysUserMaster = context.SysUserMasters.Where(c => c.UserName == model.Email && 
                                                                            c.Password == model.Password 
                                                                            //c.LocationId == LocationId
                                                                            ).FirstOrDefault();
                if (sysUserMaster != null)
                {
                    return sysUserMaster;
                }
                else
                  return  null;
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
                SysUserMaster sysUserMaster = context.SysUserMasters.Where(u=>u.UserName==uname && u.Password==pw).FirstOrDefault();
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

                var us = context.SysUserMasters.Where(u => u.UserName == um.UserName && u.SysUserMasterID == um.SysUserMasterID).FirstOrDefault();
                us.Password = um.Password;
                us.ConfirmPassword = um.ConfirmPassword;
                if (context.SaveChanges() == 1)
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
                //var lstRolesg = (from ur in context.SysUserPermissions
                //                 join us in context.SysUserMasters on ur.EmployeeID equals us.SysUserMasterID
                //                 where us.Password == sysUserMaster.Password && us.UserName == sysUserMaster.UserName
                //                 select new
                //                 {
                //                     Roles = ur.FunctionName

                //                 }).ToList();

                var lstRolesg = (from ur in context.SysUserPermissions
                                 join us in context.SysUserMasters on ur.EmployeeCode equals us.EmployeeCode
                                 where us.Password == sysUserMaster.Password && us.UserName == sysUserMaster.UserName
                                 select new
                                 {
                                     Roles = ur.FunctionName

                                 }).ToList();



                foreach (var item in lstRolesg)
                {
                    if (AllRols !="")
                    {
                        AllRols = AllRols+","+item.Roles;
                    }
                    if (AllRols == "")
                    {
                        AllRols =item.Roles;
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

        public IEnumerable<SysUserMaster> GetUsers()
        {
            try
            {
                IEnumerable<SysUserMaster> sysUserMaster = context.SysUserMasters.OrderBy(u => u.SysUserMasterID);
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
                List<SysUserPermission> permissions = context.SysUserPermissions.Where(u =>
                                                                                    u.EmployeeCode==empno).ToList();

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
                IEnumerable<SysUserMaster> sysUserMaster = context.SysUserMasters.Where(u => u.IsDelete == false).OrderBy(u => u.SysUserMasterID);
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
                SysUserMaster sysUserMaster = context.SysUserMasters.Where(u => u.SysUserMasterID == id).FirstOrDefault();
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

        public SysUserMaster GetUserEmpNoAndUserName(string empno,string username)
        {
            try
            {
                SysUserMaster sysUserMaster = context.SysUserMasters.Where(u => u.UserName == username || u.EmployeeCode==empno).FirstOrDefault();
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

        public long DeleteUserPermissionsByEmpCode(string empcode,long empid)
        {
            var res = 0;
            try
            {
                context.SysUserPermissions.RemoveRange(context.SysUserPermissions.Where(x => x.EmployeeCode == empcode && x.EmployeeID==empid));
                res = context.SaveChanges();


            }
            catch (Exception)
            {

                throw;
            }
            return res;
        }



        public bool SaveUserMaster(SysUserMaster um)
        {
            using (var dbtransaction = context.Database.BeginTransaction())
            {


                try
                {
                    var emp = context.Employees.Where(e => e.EmployeeCode == um.EmployeeCode).FirstOrDefault();
                   // DeleteUserPermissionsByEmpCode(um.EmployeeCode,emp.EmployeeID);
                    context.SysUserMasters.Add(um);
                    if (context.SaveChanges() == 1)
                    {
                        foreach (var permissions in um.SysUserGroupPermission.Where(p=>p.IsGrant==true).ToList())
                        {
                            SysUserPermission userpermissions = new SysUserPermission();

                            userpermissions.EmployeeID =Convert.ToInt32(emp.EmployeeID);
                            userpermissions.EmployeeCode = um.EmployeeCode;
                            userpermissions.EnCode = "";
                            userpermissions.FunctionName = permissions.FunctionName;
                            userpermissions.FunctionDescription = permissions.FunctionDescription;
                            userpermissions.GroupId = um.UserGroupID;
                            context.SysUserPermissions.Add(userpermissions);
                        }
                        context.SaveChanges();
                        dbtransaction.Commit();
                        return true;
                    }
                    else
                    {
                        dbtransaction.Rollback();
                        return false;
                    }
                    
                }
                catch (Exception)
                {
                    dbtransaction.Rollback();
                    return false;
                }

            }

        }


        public bool UpdateUserMaster(SysUserMaster um)
        {
            using (var dbtransaction = context.Database.BeginTransaction())
            {


                try
                {
                    var emp = context.Employees.Where(e => e.EmployeeCode == um.EmployeeCode).FirstOrDefault();
                    DeleteUserPermissionsByEmpCode(um.EmployeeCode, emp.EmployeeID);
                    // context.SysUserMasters.Add(um);
                    //if (context.SaveChanges() == 1)
                    //{

                    //context.SaveChanges();
                    if (um.SysUserGroupPermission.Count() > 0)
                    {
                        foreach (var permissions in um.SysUserGroupPermission.Where(p => p.IsGrant == true).ToList())
                        {
                            SysUserPermission userpermissions = new SysUserPermission();

                            userpermissions.EmployeeID = Convert.ToInt32(emp.EmployeeID);
                            userpermissions.EmployeeCode = um.EmployeeCode;
                            userpermissions.EnCode = "";
                            userpermissions.FunctionName = permissions.FunctionName;
                            userpermissions.FunctionDescription = permissions.FunctionDescription;
                            userpermissions.GroupId = um.UserGroupID;
                            context.SysUserPermissions.Add(userpermissions);
                        }
                        context.SaveChanges();
                        dbtransaction.Commit();
                        return true;
                    }
                    else
                    {
                        dbtransaction.Rollback();
                        return false;
                    }
                   

                }
                catch (Exception e)
                {
                    dbtransaction.Rollback();
                    return false;
                }

            }

        }

        //public int UpdateUserMaster(SysUserMaster um)
        //{
        //    try
        //    {

        //        int res = context.SaveChanges();
        //        return res;
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}


        public long DeleteUserPermissionsByEmpCodeGroupId(string empcode, long empid,long groupid)
        {
            var res = 0;
            try
            {
                context.SysUserPermissions.RemoveRange(context.SysUserPermissions.Where(x => x.EmployeeCode == empcode && 
                                                                                        x.EmployeeID == empid &&
                                                                                        x.GroupId==groupid));

                res = context.SaveChanges();


            }
            catch (Exception)
            {

                throw;
            }
            return res;
        }


        public List<SysUserGroupPermission> CompairPermissions(long groupid,string empcode)
        {
            try
            {
                List<SysUserGroupPermission> group = context.SysUserGroupPermissions.Where(u => u.SysUserGroupId == groupid).ToList();
                List<SysUserPermission> user = context.SysUserPermissions.Where(p => p.GroupId == groupid && p.EmployeeCode == empcode).ToList();

                List<SysUserGroupPermission> permissions = new List<SysUserGroupPermission>();
                foreach (var g in group)
                {
                    foreach (var u in user)
                    {
                        if (u.FunctionName==g.FunctionName)
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



    }
}