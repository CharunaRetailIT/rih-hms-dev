using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace RIT.HMS.BLL
{
    public class BLL_ProductKitchenMapper
    {


        private readonly UnitOfWork _unitofwork;
        public BLL_ProductKitchenMapper()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_ProductKitchenMapper(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }

        public List<ProductKitchenMapper> GetActiveAll(Int32 compid)
        {
            try
            {
                List<ProductKitchenMapper> sm = _unitofwork.ProductKitchenMapperRepository.Get(g => g.IsActive == true && g.CompanyID == compid).ToList();
                if (sm != null)
                {
                    return sm;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                return null;
            }
        }
        public List<ProductKitchenMapper> GetAllByProductId(Int32 compid, int productId)
        {
            var result = new List<ProductKitchenMapper>();
            try
            {
                result = _unitofwork.ProductKitchenMapperRepository.Get(g => g.CompanyID == compid && g.ProductId == productId && g.IsActive == true).ToList();
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }
        public List<SysLocation> MapperdLocationSelect(ProductMapperToKitchen productMapperToKitchen)
        {
            var result = productMapperToKitchen.KitchenLocationList;
            try
            {
                foreach (var row in productMapperToKitchen.ProductKitchenMapper)
                {
                    var getResult = result.Where(o => o.SysLocationID == row.SubLocationId).FirstOrDefault();
                    getResult.IsSelectLocation = true;
                    getResult.IsSelectLocationIsActive = row.IsActive;
                }

            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        public int SaveSubLocation(ProductMapperToKitchen productMapperToKitchen)
        {
            var result = 0;

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Required))
            {
                try
                {

                    //_unitofwork.ProductKitchenMapperRepository.DeleteRange(productMapperToKitchen.ProductKitchenMapper);
                    List<ProductKitchenMapper> ProductKitchenMapperList = new List<ProductKitchenMapper>();
                    foreach (var r in productMapperToKitchen.KitchenLocationList)
                    {

                        var isValidateRow = _unitofwork.ProductKitchenMapperRepository.Get(g => g.CompanyID == productMapperToKitchen.Product.CompanyID && g.ProductId == productMapperToKitchen.Product.ProductId && g.SubLocationId == r.SysLocationID).FirstOrDefault();

                        if (isValidateRow == null)
                        {
                            if (r.IsSelectLocation)
                            {
                                var productKitchenMapper = new ProductKitchenMapper();
                                productKitchenMapper.CompanyID = productMapperToKitchen.Product.CompanyID;
                                productKitchenMapper.GroupOfCompanyID = productMapperToKitchen.Product.GroupOfCompanyID;
                                productKitchenMapper.SubLocationId = r.SysLocationID;
                                productKitchenMapper.ProductId = productMapperToKitchen.Product.ProductId;
                                productKitchenMapper.CreatedUser = productMapperToKitchen.CreatedUser;
                                productKitchenMapper.CreatedDate = productMapperToKitchen.CreatedDate;
                                productKitchenMapper.ModifiedUser = productMapperToKitchen.ModifiedUser;
                                productKitchenMapper.ModifiedDate = productMapperToKitchen.ModifiedDate;
                                productKitchenMapper.DataTransfer = 0;
                                productKitchenMapper.IsActive = true;
                                ProductKitchenMapperList.Add(productKitchenMapper);
                                _unitofwork.Save();
                            }
                        }
                        else
                        {
                            isValidateRow.CreatedUser = productMapperToKitchen.CreatedUser;
                            isValidateRow.CreatedDate = productMapperToKitchen.CreatedDate;
                            isValidateRow.ModifiedUser = productMapperToKitchen.ModifiedUser;
                            isValidateRow.ModifiedDate = productMapperToKitchen.ModifiedDate;

                            if (r.IsSelectLocation)
                            {
                                isValidateRow.DataTransfer = 0;
                                isValidateRow.IsActive = true;
                                _unitofwork.ProductKitchenMapperRepository.Update(isValidateRow);
                                _unitofwork.Save();
                            }
                            else
                            {
                                isValidateRow.DataTransfer = 0;
                                isValidateRow.IsActive = false;
                                _unitofwork.ProductKitchenMapperRepository.Update(isValidateRow);
                                _unitofwork.Save();
                            }
                        }


                    }
                    _unitofwork.ProductKitchenMapperRepository.BulkInsert(ProductKitchenMapperList);
                    _unitofwork.Save();
                    transactionScope.Complete();
                    result = 1;


                }
                catch (Exception)
                {
                    transactionScope.Dispose();
                    throw;
                }
            }

            return result;
        }
    }
}
