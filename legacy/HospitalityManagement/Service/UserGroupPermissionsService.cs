using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class UserGroupPermissionsService
    {
        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<SysUserGroupPermission> GetUserGroupPermissions()
        {
            try
            {
                IEnumerable<SysUserGroupPermission> permissions = context.SysUserGroupPermissions.OrderBy(g => g.FunctionName);
                if (permissions != null)
                {
                    return permissions;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<SysUserGroupPermission> GetById(long id)
        {
            try
            {
                IEnumerable<SysUserGroupPermission> permissions = context.SysUserGroupPermissions.Where(g => g.IsDelete == false && g.IsActive==true)
                                                                  .OrderBy(g => g.FunctionName);
                if (permissions != null)
                {
                    return permissions;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<SysUserGroupPermission> GetActivePermissions()
        {
            try
            {
                IEnumerable<SysUserGroupPermission> permissions = context.SysUserGroupPermissions.Where(g => g.IsDelete == false && g.IsActive==true)
                                                                   .OrderBy(g => g.FunctionName);
                if (permissions != null)
                {
                    return permissions;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<SysUserGroupPermission> GetByGroupId(long id)
        {
            try
            {
                IEnumerable<SysUserGroupPermission> permissions = context.SysUserGroupPermissions.Where(g=>g.SysUserGroupId==id)
                                                                                   .OrderBy(k => k.FunctionName);
                if (permissions != null)
                {
                    return permissions;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<CashierGroup> GetByPOSGroupId(long id)
        {
            try
            {
                IEnumerable<CashierGroup> permissions = context.CashierGroup.Where(g => g.EmployeeGroupId == id)
                                                                                   .OrderBy(k => k.FunctionName);
                if (permissions != null)
                {
                    return permissions;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public IEnumerable<CashierPermission> GetByEmpId(long empid)
        {
            try
            {
                IEnumerable<CashierPermission> permissions = context.CashierPermission.Where(g => g.EmployeeId == empid)
                                                                                   .OrderBy(k => k.FunctionName);
                if (permissions != null)
                {
                    return permissions;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public int SavePermissions(SysUserGroupPermission per)
        {
            try
            {
                context.SysUserGroupPermissions.Add(per);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public int DeletePermissionsByUserGrouypId(long usergroupid)
        {
            try
            {
                

                //var permissionentity = context.SysUserGroupPermissions.Where(s => s.SysUserGroupId == usergroupid).FirstOrDefault();
                //    context.Entry(permissionentity).State = System.Data.Entity.EntityState.Deleted;
                context.SysUserGroupPermissions.RemoveRange(context.SysUserGroupPermissions.Where(x => x.SysUserGroupId == usergroupid));            
                int res = context.SaveChanges();
                return res;
                
            }
            catch (Exception)
            {

                throw;
            }
        }


        public int UpdatePermissions(SysUserGroupPermission per)
        {
            try
            {

                //  ..context.SysGroupOfCompanys.Add(goc);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}