using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class CashierPermissionService
    {
        ApplicationDbContext context = new ApplicationDbContext();

        public List<string> GetPOSUsers()
        {
            try
            {
                IEnumerable<CashierPermission> posusers = context.CashierPermission.Where(c => c.IsDelete == false).
                                                                                    OrderBy(c => c.EmployeeId).Distinct().ToList();

                List<string> filteredusers=new List<string>();
                foreach (var u in posusers)
                {                   
                    if (!filteredusers.Contains(u.EmployeeId+","+ u.LocationId))
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

        public int DeleteCashiers(long id,long locid)
        {
            try
            {
                context.CashierPermission.RemoveRange(context.CashierPermission.Where(x => x.EmployeeId == id && x.LocationId==locid));
                var res = context.SaveChanges();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public int SaveCashierPermissions(CashierPermission cashierpermissions)
        {
            try
            {
                var empname = context.Employees.Where(u=>u.EmployeeID==cashierpermissions.EmployeeId).FirstOrDefault().EmployeeName;
                if (context.CashierPermission.Where(p => p.LocationId == cashierpermissions.LocationId && p.EmployeeId == cashierpermissions.EmployeeId).ToList().Count == 0)
                {

                    var fileredpermissions = cashierpermissions.CashierGroupPermissions.Where(p => p.IsGrant == true);
                    int order = 1;
                    foreach (var permission in fileredpermissions)
                    {
                        CashierPermission newcashierpermissions = new CashierPermission();

                        newcashierpermissions.LocationId = cashierpermissions.LocationId;
                        newcashierpermissions.CashierId = cashierpermissions.EmployeeId;
                        newcashierpermissions.EmployeeId = cashierpermissions.EmployeeId;
                        newcashierpermissions.Password = cashierpermissions.Password;

                        newcashierpermissions.FunctionName = permission.FunctionName;
                        newcashierpermissions.FunctionDescription = permission.FunctionDescription;
                        newcashierpermissions.Order = order;
                        newcashierpermissions.JournalName = empname;
                        newcashierpermissions.EnCode = "a";
                        newcashierpermissions.Type = "a";
                        newcashierpermissions.TypeID = "a";
                        newcashierpermissions.MaxValue = permission.Value;
                        newcashierpermissions.Value = Convert.ToInt64(permission.Value);
                        newcashierpermissions.IsActive = permission.IsGrant;
                        newcashierpermissions.IsAccess = permission.IsGrant;
                        newcashierpermissions.Remarks = "a";

                        newcashierpermissions.IsDelete = false;
                        newcashierpermissions.IsValue = true;
                        newcashierpermissions.GroupOfCompanyId = cashierpermissions.GroupOfCompanyId;
                        newcashierpermissions.CreatedDate = cashierpermissions.CreatedDate;
                        newcashierpermissions.CreatedUser = cashierpermissions.CreatedUser;
                        newcashierpermissions.ModifiedDate = cashierpermissions.ModifiedDate;
                        newcashierpermissions.ModifiedUser = "a";
                        newcashierpermissions.DataTransfer = cashierpermissions.DataTransfer;

                        order += 1;

                        context.CashierPermission.Add(newcashierpermissions);
                    }


                    int res = context.SaveChanges();
                    return res;
                }
                else
                {
                    var res = DeleteCashiers(cashierpermissions.EmployeeId,cashierpermissions.LocationId);

                    if (res > 0)
                    {
                        var fileredpermissions = cashierpermissions.CashierGroupPermissions.Where(p => p.IsGrant == true);
                        int order = 1;
                        foreach (var permission in fileredpermissions)
                        {
                            CashierPermission newcashierpermissions = new CashierPermission();

                            newcashierpermissions.LocationId = cashierpermissions.LocationId;
                            newcashierpermissions.CashierId = cashierpermissions.EmployeeId;
                            newcashierpermissions.EmployeeId = cashierpermissions.EmployeeId;
                            newcashierpermissions.Password = cashierpermissions.Password;

                            newcashierpermissions.FunctionName = permission.FunctionName;
                            newcashierpermissions.FunctionDescription = permission.FunctionDescription;
                            newcashierpermissions.Order = order;
                            newcashierpermissions.JournalName = empname;
                            newcashierpermissions.EnCode = "a";
                            newcashierpermissions.Type = "a";
                            newcashierpermissions.TypeID = "a";
                            newcashierpermissions.MaxValue = permission.Value;
                            newcashierpermissions.Value = Convert.ToInt64(permission.Value);
                            newcashierpermissions.IsActive = permission.IsGrant;
                            newcashierpermissions.IsAccess = permission.IsGrant;
                            newcashierpermissions.Remarks = "a";

                            newcashierpermissions.IsDelete = false;
                            newcashierpermissions.IsValue = true;
                            newcashierpermissions.GroupOfCompanyId = cashierpermissions.GroupOfCompanyId;
                            newcashierpermissions.CreatedDate = cashierpermissions.CreatedDate;
                            newcashierpermissions.CreatedUser = cashierpermissions.CreatedUser;
                            newcashierpermissions.ModifiedDate = cashierpermissions.ModifiedDate;
                            newcashierpermissions.ModifiedUser = "a";
                            newcashierpermissions.DataTransfer = cashierpermissions.DataTransfer;

                            order += 1;

                            context.CashierPermission.Add(newcashierpermissions);
                        }


                        int res1 = context.SaveChanges();
                        return res1;
                    }
                    else
                    {
                        return 0;

                    }

                   

                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}