using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_CashierPermission
    {
        private readonly UnitOfWork _unitofwork;

        public BLL_CashierPermission()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_CashierPermission(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public List<string> GetPOSUsers(int companyid)
        {
            try
            {
                //IEnumerable<CashierPermission> posusers = _unitofwork.CashierPermissionRepository.Get(c => c.IsDelete == false).
                //                                                                    OrderBy(c => c.EmployeeId).Distinct().ToList();

                var posusers = (from c in _unitofwork.CashierPermissionRepository.Get(c => c.IsDelete == false).OrderBy(c => c.EmployeeId).Distinct().ToList()
                                join e in _unitofwork.EmployeeRepository.Get(e => e.CompanyID == companyid) on c.EmployeeId equals e.EmployeeID
                                select new
                                {
                                    EmployeeId = e.EmployeeID,
                                    LocationId = c.LocationId
                                }
                                );


                List<string> filteredusers = new List<string>();
                foreach (var u in posusers)
                {
                    if (!filteredusers.Contains(u.EmployeeId + "," + u.LocationId))
                    {
                        filteredusers.Add(u.EmployeeId + "," + u.LocationId);
                    }
                }

                if (filteredusers != null)
                {
                    return filteredusers;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public CashierPermission GetCashierByEmpId(int empid)
        {
            var cashier = _unitofwork.CashierPermissionRepository.GetAsNoTracking(c=>c.EmployeeId==empid).Distinct().FirstOrDefault();
            return cashier;
        }
        public int DeleteCashiers(long id, long locid)
        {
            try
            {
                _unitofwork.CashierPermissionRepository.DeleteRange(_unitofwork.CashierPermissionRepository.Get(x => x.EmployeeId == id && x.LocationId == locid));
                var res = _unitofwork.Save();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public int SaveCashierPermissions(CashierPermission cashierpermissions)
        {
            _unitofwork.CreateTransaction();
            try
            {
                var empname = _unitofwork.EmployeeRepository.Get(u => u.EmployeeID == cashierpermissions.EmployeeId).FirstOrDefault().EmployeeName;

                if (_unitofwork.CashierPermissionRepository.Get(p => p.LocationId == cashierpermissions.LocationId && p.EmployeeId == cashierpermissions.EmployeeId).ToList().Count == 0)
                {
                    var sss = _unitofwork.CashierPermissionRepository.Get(p => p.LocationId == cashierpermissions.LocationId && p.EmployeeId != cashierpermissions.EmployeeId && p.Password == cashierpermissions.Password).ToList();
                    if (_unitofwork.CashierPermissionRepository.Get( p => p.LocationId == cashierpermissions.LocationId && p.EmployeeId != cashierpermissions.EmployeeId && p.Password == cashierpermissions.Password).ToList().Count > 0)
                    {
                        return 4;
                    }
                    var fileredpermissions = cashierpermissions.CashierGroupPermissions.Where(p => p.IsGrant == true);
                    int order = 1;
                    foreach (var permission in fileredpermissions)
                    {
                        CashierPermission newcashierpermissions = new CashierPermission();
                        if (cashierpermissions.IsActive)
                        {
                            permission.MaxValue = 999999;
                            permission.Value = 999999;
                        }
                        newcashierpermissions.LocationId = cashierpermissions.LocationId;
                        newcashierpermissions.CashierId = cashierpermissions.EmployeeId;
                        newcashierpermissions.EmployeeId = cashierpermissions.EmployeeId;
                        newcashierpermissions.Password = cashierpermissions.Password;

                        newcashierpermissions.FunctionName = permission.FunctionName;
                        newcashierpermissions.FunctionDescription = permission.FunctionDescription;
                        newcashierpermissions.Order = order;
                        newcashierpermissions.JournalName = empname;
                        newcashierpermissions.EnCode = "0";
                        newcashierpermissions.Type = "0";
                        newcashierpermissions.TypeID = "0";
                        newcashierpermissions.MaxValue = permission.MaxValue;
                        newcashierpermissions.Value = Convert.ToInt64(permission.Value);
                        newcashierpermissions.IsActive = cashierpermissions.IsActive;
                        newcashierpermissions.IsAccess = true;//permission.IsAccess;
                        newcashierpermissions.Remarks = "N/A";

                        newcashierpermissions.IsDelete = cashierpermissions.IsDelete;
                        newcashierpermissions.IsValue = true;
                        newcashierpermissions.GroupOfCompanyId = cashierpermissions.GroupOfCompanyId;
                        newcashierpermissions.CreatedDate = cashierpermissions.CreatedDate;
                        newcashierpermissions.CreatedUser = cashierpermissions.CreatedUser;
                        newcashierpermissions.ModifiedDate = cashierpermissions.ModifiedDate;
                        newcashierpermissions.ModifiedUser = cashierpermissions.ModifiedUser;
                        newcashierpermissions.DataTransfer = cashierpermissions.DataTransfer;

                        order += 1;

                        _unitofwork.CashierPermissionRepository.Insert(newcashierpermissions);
                    }
                    int res = _unitofwork.Save();
                    _unitofwork.Commit();
                    return res;
                }
                else
                {
                    if (_unitofwork.CashierPermissionRepository.Get(p => p.LocationId == cashierpermissions.LocationId && p.EmployeeId != cashierpermissions.EmployeeId && p.Password == cashierpermissions.Password).ToList().Count > 0)
                    {
                        return 4;
                    }
                    var res = DeleteCashiers(cashierpermissions.EmployeeId, cashierpermissions.LocationId);
                    if (res > 0)
                    {
                        var fileredpermissions = cashierpermissions.CashierGroupPermissions.Where(p => p.IsGrant == true);
                        int order = 1;
                        foreach (var permission in fileredpermissions)
                        {
                            CashierPermission newcashierpermissions = new CashierPermission();
                            if(cashierpermissions.IsActive)
                            {
                                permission.MaxValue=999999;
                                permission.Value = 999999;
                            }
                            newcashierpermissions.LocationId = cashierpermissions.LocationId;
                            newcashierpermissions.CashierId = cashierpermissions.EmployeeId;
                            newcashierpermissions.EmployeeId = cashierpermissions.EmployeeId;
                            newcashierpermissions.Password = cashierpermissions.Password;

                            newcashierpermissions.FunctionName = permission.FunctionName;
                            newcashierpermissions.FunctionDescription = permission.FunctionDescription;
                            newcashierpermissions.Order = order;
                            newcashierpermissions.JournalName = empname;
                            newcashierpermissions.EnCode = "0";
                            newcashierpermissions.Type = "0";
                            newcashierpermissions.TypeID = "0";
                            newcashierpermissions.MaxValue = permission.MaxValue;
                            newcashierpermissions.Value = Convert.ToInt64(permission.Value);
                            newcashierpermissions.IsActive = cashierpermissions.IsActive;
                            newcashierpermissions.IsAccess = true;// permission.IsAccess;
                            newcashierpermissions.Remarks = "N/A";

                            newcashierpermissions.IsDelete = cashierpermissions.IsDelete;
                            newcashierpermissions.IsValue = true;
                            newcashierpermissions.GroupOfCompanyId = cashierpermissions.GroupOfCompanyId;
                            newcashierpermissions.CreatedDate = cashierpermissions.CreatedDate;
                            newcashierpermissions.CreatedUser = cashierpermissions.CreatedUser;
                            newcashierpermissions.ModifiedDate = cashierpermissions.ModifiedDate;
                            newcashierpermissions.ModifiedUser = cashierpermissions.ModifiedUser;
                            newcashierpermissions.DataTransfer = cashierpermissions.DataTransfer;

                            order += 1;

                            _unitofwork.CashierPermissionRepository.Insert(newcashierpermissions);
                        }
                     
                        int res1 = _unitofwork.Save();
                        _unitofwork.Commit();
                        return res1;
                    }
                    else
                    {
                        _unitofwork.Rollback();
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _unitofwork.Rollback();
                return 0;
            }
        }

     


    }
}
