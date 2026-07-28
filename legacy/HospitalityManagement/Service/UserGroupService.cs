using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class UserGroupService
    {


        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<SysUserGroup> GetUserGroups()
        {
            try
            {
                IEnumerable<SysUserGroup> sysusergroup = context.SysUserGroups.OrderBy(ug => ug.UserGroupCode);
                if (sysusergroup != null)
                {
                    return sysusergroup;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<SysUserFunction> GetUserFunctions()
        {
            try
            {
                IEnumerable<SysUserFunction> functions = context.SysUserFunctions.OrderBy(ug => ug.FunctionName);
                if (functions != null)
                {
                    return functions;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public SysUserFunction GetUserFunction(long id)
        {
            try
            {
               SysUserFunction function = context.SysUserFunctions.Where(f=>f.SysUserFunctionID==id).FirstOrDefault();
                if (function != null)
                {
                    return function;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<SysUserGroup> GetActiveUserGroups()
        {
            try
            {
                IEnumerable<SysUserGroup> sysusergroup = context.SysUserGroups.Where(ug => ug.IsDelete == false).OrderBy(ug => ug.UserGroupCode);
                if (sysusergroup != null)
                {
                    return sysusergroup;
                   
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<POSUserGroup> GetActivePOSUserGroups()
        {
            try
            {
                IEnumerable<POSUserGroup> posusergroup = context.POSUserGroup.Where(ug => ug.IsDelete == false && ug.IsActive==true).OrderBy(ug => ug.POSUserGroupName);
                if (posusergroup != null)
                {
                    return posusergroup;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public SysUserGroup GetUserGroupById(long id)
        {
            try
            {
                SysUserGroup sysusergroup = context.SysUserGroups.Where(ug => ug.SysUserGroupID == id).FirstOrDefault();
                if (sysusergroup != null)
                {
                    return sysusergroup;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveUserGroup(SysUserGroup ug)
        {
            try
            {
                context.SysUserGroups.Add(ug);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateUserGroup(SysUserGroup ug)
        {
            try
            {

                //  ..context.SysGroupOfCompanys.Add(goc);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public SysUserGroup GetUserGroupByCode(string code)
        {
            try
            {
                SysUserGroup usergroup = context.SysUserGroups.Where(g => g.UserGroupCode == code).FirstOrDefault();
                if (usergroup != null)
                {
                    return usergroup;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}