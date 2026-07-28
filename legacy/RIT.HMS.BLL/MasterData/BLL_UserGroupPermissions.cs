using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_UserGroupPermissions
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_UserGroupPermissions()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_UserGroupPermissions(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public IEnumerable<SysUserGroupPermission> GetUserGroupPermissions()
        {
            try
            {
                IEnumerable<SysUserGroupPermission> permissions = _unitofwork.UserGroupPermissionRepository.Get().OrderBy(g => g.FunctionName);
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
                IEnumerable<SysUserGroupPermission> permissions = _unitofwork.UserGroupPermissionRepository.Get(g => g.IsDelete == false && g.IsActive == true)
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
                IEnumerable<SysUserGroupPermission> permissions = _unitofwork.UserGroupPermissionRepository.Get(g => g.IsDelete == false && g.IsActive == true)
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

        public IEnumerable<SysUserGroupPermission> GetByGroupId(long id,int companyid)
        {
            try
            {
                IEnumerable<SysUserGroupPermission> permissions = _unitofwork.UserGroupPermissionRepository.Get(g => g.SysUserGroupId == id && g.CompanyID==companyid && g.IsDelete==false)
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

        public List<SysUserFunction> CompairGroupFunctions(long groupid,int companyid)
        {
            try
            {
                List<SysUserGroupPermission> group = _unitofwork.UserGroupPermissionRepository.Get(u => u.SysUserGroupId == groupid && u.CompanyID==companyid && u.IsDelete == false).ToList();
                List<SysUserFunction> functions = _unitofwork.UserFunctionRepository.Get(p => p.IsDelete==false).ToList();

                List<SysUserGroupPermission> permissions = new List<SysUserGroupPermission>();
                foreach (var f in functions)
                {
                    foreach (var g in group)
                    {
                        if (f.FunctionName == g.FunctionName)
                        {
                            f.IsGrant = true;
                        }
                    }
                    // permissions.Add(g);
                }

                return functions;
            }

            catch (Exception ex)
            {

                throw;
            }
        }



        public IEnumerable<CashierGroup> GetByPOSGroupId(long id)
        {
            try
            {
                IEnumerable<CashierGroup> permissions = _unitofwork.CashierGroupRepository.Get(g => g.EmployeeGroupId == id)
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
                IEnumerable<CashierPermission> permissions = _unitofwork.CashierPermissionRepository.Get(g => g.EmployeeId == empid)
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
                _unitofwork.UserGroupPermissionRepository.Insert(per);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }


        public int SaveCashierGroup(CashierGroup per)
        {
            _unitofwork.CreateTransaction();
            try
            {
                _unitofwork.CashierGroupRepository.Insert(per);
                var res=_unitofwork.Save();
                _unitofwork.Commit();
                return res;

            }
            catch (Exception ex)
            {
                _unitofwork.Rollback();
                return 0;
            }
        }

        public int DeletePermissionsByUserGrouypId(long usergroupid,int companyid)
        {
            try
            {

                _unitofwork.UserGroupPermissionRepository.DeleteRange(_unitofwork.UserGroupPermissionRepository.Get(x => x.SysUserGroupId == usergroupid && x.CompanyID==companyid));
                int res = _unitofwork.Save();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public int DeletePermissionsByCashierGroup(long cashiergroupid)
        {
            try
            {

                _unitofwork.CashierGroupRepository.DeleteRange(_unitofwork.CashierGroupRepository.Get(x => x.EmployeeGroupId == cashiergroupid));
                int res = _unitofwork.Save();
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
                _unitofwork.UserGroupPermissionRepository.Update(per);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }



    }
}
