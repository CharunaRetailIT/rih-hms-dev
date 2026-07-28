using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_SupplierGroup
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_SupplierGroup()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_SupplierGroup(string connectionstring)
        {
            _unitofwork = new UnitOfWork(connectionstring);
        }
        public IEnumerable<SupplierGroup> GetSupplierGroups(Int32 compid)
        {
            try
            {
                IEnumerable<SupplierGroup> suppliergroups = _unitofwork.SuplierGroupRepository.Get(g => g.IsDelete == false && g.CompanyID==compid).OrderBy(sg => sg.SupplierGroupCode);
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

        public IEnumerable<SupplierGroup> GetActiveSupplierGroups(Int32 compid)
        {
            try
            {
                IEnumerable<SupplierGroup> suppliergroups = _unitofwork.SuplierGroupRepository.Get(sg => sg.IsDelete == false && sg.CompanyID==compid).OrderBy(sg => sg.SupplierGroupCode);
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
                SupplierGroup suppliergroups = _unitofwork.SuplierGroupRepository.Get(sg => sg.SupplierGroupID == id).FirstOrDefault();
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
                _unitofwork.SuplierGroupRepository.Insert(sg);          
                int res =_unitofwork.Save();
                return res;
            }
            catch (Exception ex)
            {
               
                throw ex;
            }
        }

        public int UpdateSupplierGroup(SupplierGroup sg)
        {
            try
            {

                _unitofwork.SuplierGroupRepository.Update(sg);
                int res = _unitofwork.Save();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public SupplierGroup GetSupGroupByCode(string code,Int32 compid)
        {
            try
            {
                SupplierGroup supgrp = _unitofwork.SuplierGroupRepository.Get(g => g.SupplierGroupCode == code && g.CompanyID==compid).FirstOrDefault();
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


        public bool SupplierGroupIsUsing(int suppliergroupid)
        {
            try
            {

                return _unitofwork.SuplierRepository.Get().Any(s=>s.SupplierGroupID==suppliergroupid);            
               
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
