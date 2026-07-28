using RIT.HMS.Data;
using RIT.HMS.Domain;
using RIT.HMS.Domain.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_UnitConversion
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_UnitConversion()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_UnitConversion(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public IEnumerable<UnitConversion> GetUnitConversions(Int32 compid)
        {
            try
            {
                IEnumerable<UnitConversion> unitconversions = _unitofwork.UnitConversionRepository.Get(c=>c.CompanyID==compid).OrderBy(c => c.UnitConversionId);
                if (unitconversions != null)
                {
                    return unitconversions;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<UnitConversion> GetConversionById(long id)
        {
            try
            {
                IEnumerable<UnitConversion> unitConversions = _unitofwork.UnitConversionRepository.Get(g => g.UnitConversionId == id).
                                                                                                OrderBy(g => g.UnitConversionId);
                return unitConversions ?? null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public UnitConversion GetConversionByCode(string code,int uomid, int companyid)
        {
            try
            {
                UnitConversion unitConversions = _unitofwork.UnitConversionRepository.Get(g => g.SubUnit == code && g.UnitOfMeasureId==uomid &&g.CompanyID==companyid).FirstOrDefault();
                                                                                               
                return unitConversions ?? null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IEnumerable<UnitConversion> GetConversionByMeasurementId(long id,int companyid)
        {
            try
            {
                IEnumerable<UnitConversion> unitConversions = _unitofwork.UnitConversionRepository.Get(g => g.UnitOfMeasureId == id && g.CompanyID==companyid);

                return unitConversions ?? null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int SaveUnitConversions(List<UnitConversion> conversions, string loggeduser,Int32 compid,Int32 locid)
        {
            var res = 0;

            _unitofwork.CreateTransaction();


             //  DeleteConversionsByUnitOfMeasureId(conversions.FirstOrDefault().UnitOfMeasureId);


                try
                {

                    foreach (var unitConversion in conversions)
                    {
                        unitConversion.CreatedUser = loggeduser;
                        unitConversion.CompanyID = compid;
                        unitConversion.LocationId = locid;
                        unitConversion.GroupOfCompanyID = 0;

                        var exists = _unitofwork.UnitConversionRepository.GetById(unitConversion.UnitConversionId);
                        if (exists != null)
                        {
                       
                                var cc = _unitofwork.UnitConversionRepository.GetById(exists.UnitConversionId);
                                cc.ModifiedDate = DateTime.Now;
                                cc.ModifiedUser = loggeduser;
                                _unitofwork.UnitConversionRepository.Update(cc);
                        

                        }
                        else
                        {
                            _unitofwork.UnitConversionRepository.Insert(unitConversion);
                            _unitofwork.Save();
                            LOGUnitConversion lgconversion = new LOGUnitConversion();
                            var mapped = Common.HMSExtensions.MatchAndMap(unitConversion, lgconversion);
                            mapped.SourceId = Convert.ToInt32(unitConversion.UnitConversionId);
                            mapped.CreatedUser = loggeduser;
                            mapped.CreatedDate = DateTime.Now;
                            mapped.Action = "Added";
                            _unitofwork.LOGUnitConversion.Insert(mapped);
                          //  _unitofwork.Save();

                         }

                        res = _unitofwork.Save();
                        if (res != 0) continue;
                        _unitofwork.Rollback();
                        return res;

                    }


                _unitofwork.Commit();

                }
                catch (Exception ex)
                {
                _unitofwork.Rollback();
                    throw;
                }
            

            return res;
        }
        public int DeleteConversionsByUnitOfMeasureId(long id)
        {
            try
            {
                _unitofwork.UnitConversionRepository.DeleteRange(_unitofwork.UnitConversionRepository.Get(x => x.UnitOfMeasureId == id));
                int res = _unitofwork.Save();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }
        public int UpdateUnitConversion(UnitConversion conversion)
        {
            try
            {

                //  ..context.SysGroupOfCompanys.Add(goc);
                _unitofwork.UnitConversionRepository.Update(conversion);
                var res = _unitofwork.Save();

                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public int GetProductsByConversionId(long id,string user)
        {
            try
            {
                IEnumerable<Product> products = _unitofwork.ProductRepository.Get(g => g.WeightPerUnit == id);
                if (products.Count() != 0)
                {
                    return products.Count();
                }
                else
                {
                    if (id == 0) { return 0; }
                    var exists = _unitofwork.UnitConversionRepository.GetById(id);
                    _unitofwork.UnitConversionRepository.Delete(exists);

                    LOGUnitConversion lgconversion = new LOGUnitConversion();
                    var mapped = Common.HMSExtensions.MatchAndMap(exists, lgconversion);
                    mapped.SourceId =Convert.ToInt32(exists.UnitConversionId);
                    mapped.ModifiedUser = user;
                    mapped.ModifiedDate = DateTime.Now;
                    mapped.Action = "Removed";
                    _unitofwork.LOGUnitConversion.Insert(mapped);

                    _unitofwork.Save();
                    return 0;
                }

            }
            catch (Exception ex)
            {

                throw;
            }
        }

    }
}
