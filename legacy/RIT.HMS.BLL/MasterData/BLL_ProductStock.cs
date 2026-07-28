using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
    public class BLL_ProductStock
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_ProductStock()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_ProductStock(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public List<ProductStockMaster> GetProductStockMasterByProductId(long id)
        {
            try
            {

                List<ProductStockMaster> productstockmaster = _unitofwork.ProductStockMasterRepository.Get(r => r.ProductId == id).
                                                                OrderBy(c => c.LocationId).ToList();
                if (productstockmaster != null)
                {
                    return productstockmaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public ProductStockMaster GetProductStockMasterByProductIdLocid(long productid,int locid)
        {
            try
            {

                ProductStockMaster productstockmaster = _unitofwork.ProductStockMasterRepository.Get(r => r.ProductId == productid && r.LocationId == locid).
                                                                OrderBy(c => c.LocationId).FirstOrDefault();
                if (productstockmaster != null)
                {
                    return productstockmaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


    }
}
