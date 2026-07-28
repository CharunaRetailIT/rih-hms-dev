using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class SupplierGroupService
    {
        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<SupplierGroup> GetSupplierGroups()
        {
            try
            {
                IEnumerable<SupplierGroup> suppliergroups = context.SupplierGroup.Where(g=>g.IsDelete==false).OrderBy(sg => sg.SupplierGroupCode);
                if (suppliergroups != null)
                {
                    return suppliergroups;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<SupplierGroup> GetActiveSupplierGroups()
        {
            try
            {
                IEnumerable<SupplierGroup> suppliergroups = context.SupplierGroup.Where(sg => sg.IsDelete == false).OrderBy(sg => sg.SupplierGroupCode);
                if (suppliergroups != null)
                {
                    return suppliergroups;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public SupplierGroup GetSupplierGroupById(long id)
        {
            try
            {
                SupplierGroup suppliergroups = context.SupplierGroup.Where(sg => sg.SupplierGroupID == id).FirstOrDefault();
                if (suppliergroups != null)
                {
                    return suppliergroups;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveSupplierGroup(SupplierGroup sg)
        {
            try
            {
                context.SupplierGroup.Add(sg);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateSupplierGroup(SupplierGroup sg)
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


        public SupplierGroup GetSupGroupByCode(string code)
        {
            try
            {
                SupplierGroup supgrp = context.SupplierGroup.Where(g => g.SupplierGroupCode == code).FirstOrDefault();
                if (supgrp != null)
                {
                    return supgrp;
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