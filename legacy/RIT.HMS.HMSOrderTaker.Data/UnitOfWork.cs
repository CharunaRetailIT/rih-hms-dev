using RIT.HMS.HMSOrderTaker.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.HMSOrderTaker.Data
{
    public class UnitOfWork<TContext> : IDisposable
      where TContext : DbContext, new()
    {
        private DbContext context;


        ////------------------------------------ STOS_TabOrderHeader --------------------------------------------------------------------------------------

        private IGenericRepository<STOS_TabOrderHeader, DbContext> tbl_TabOrderHeader;
        public IGenericRepository<STOS_TabOrderHeader, DbContext> Tbl_TabOrderHeader
        {
            get
            {
                if (this.tbl_TabOrderHeader == null)
                {
                    this.tbl_TabOrderHeader = new GenericRepository<STOS_TabOrderHeader, DbContext>(context);
                }
                return tbl_TabOrderHeader;
            }
        }

        private IGenericRepository<STOS_TabOrderConfirmationHeader, DbContext> tbl_TabOrderConfirmationHeader;
        public IGenericRepository<STOS_TabOrderConfirmationHeader, DbContext> Tbl_TabOrderConfirmationHeader
        {
            get
            {
                if (this.tbl_TabOrderConfirmationHeader == null)
                {
                    this.tbl_TabOrderConfirmationHeader = new GenericRepository<STOS_TabOrderConfirmationHeader, DbContext>(context);
                }
                return tbl_TabOrderConfirmationHeader;
            }
        }

        ////--------------------------------------------------------------------------------------------------------------------------

        ////------------------------------------ STOS_TabOrderDetail --------------------------------------------------------------------------------------

        private IGenericRepository<STOS_TabOrderDetail, DbContext> tbl_TabOrderDetail;
        public IGenericRepository<STOS_TabOrderDetail, DbContext> Tbl_TabOrderDetail
        {
            get
            {
                if (this.tbl_TabOrderDetail == null)
                {
                    this.tbl_TabOrderDetail = new GenericRepository<STOS_TabOrderDetail, DbContext>(context);
                }
                return tbl_TabOrderDetail;
            }
        }

        private IGenericRepository<STOS_TabOrderConfirmationDetail, DbContext> tbl_TabOrderConfirmationDetail;
        public IGenericRepository<STOS_TabOrderConfirmationDetail, DbContext> Tbl_TabOrderConfirmationDetail
        {
            get
            {
                if (this.tbl_TabOrderConfirmationDetail == null)
                {
                    this.tbl_TabOrderConfirmationDetail = new GenericRepository<STOS_TabOrderConfirmationDetail, DbContext>(context);
                }
                return tbl_TabOrderConfirmationDetail;
            }
        }
        ////--------------------------------------------------------------------------------------------------------------------------

        ////------------------------------------ Tbl Master --------------------------------------------------------------------------------------

        private IGenericRepository<TableMaster, DbContext> tbl_TblMasters;
        public IGenericRepository<TableMaster, DbContext> Tbl_TblMasters
        {
            get
            {
                if (this.tbl_TblMasters == null)
                {
                    this.tbl_TblMasters = new GenericRepository<TableMaster, DbContext>(context);
                }
                return tbl_TblMasters;
            }
        }
        ////--------------------------------------------------------------------------------------------------------------------------

        ////------------------------------------ STOS_SysLocation --------------------------------------------------------------------------------------

        private IGenericRepository<SysLocation, DbContext> tbl_SysLocation;
        public IGenericRepository<SysLocation, DbContext> Tbl_SysLocation
        {
            get
            {
                if (this.tbl_SysLocation == null)
                {
                    this.tbl_SysLocation = new GenericRepository<SysLocation, DbContext>(context);
                }
                return tbl_SysLocation;
            }
        }

        private IGenericRepository<RstDepartment, DbContext> tbl_RstDepartment;
        public IGenericRepository<RstDepartment, DbContext> Tbl_RstDepartment
        {
            get
            {
                if (this.tbl_RstDepartment == null)
                {
                    this.tbl_RstDepartment = new GenericRepository<RstDepartment, DbContext>(context);
                }
                return tbl_RstDepartment;
            }
        }
        private IGenericRepository<RstCategory, DbContext> tbl_RstCategory;
        public IGenericRepository<RstCategory, DbContext> Tbl_RstCategory
        {
            get
            {
                if (this.tbl_RstCategory == null)
                {
                    this.tbl_RstCategory = new GenericRepository<RstCategory, DbContext>(context);
                }
                return tbl_RstCategory;
            }
        }

        private IGenericRepository<Product, DbContext> tbl_Product;
        public IGenericRepository<Product, DbContext> Tbl_Product
        {
            get
            {
                if (this.tbl_Product == null)
                {
                    this.tbl_Product = new GenericRepository<Product, DbContext>(context);
                }
                return tbl_Product;
            }
        }

        private IGenericRepository<CashierPermission, DbContext> tbl_CashierPermission;
        public IGenericRepository<CashierPermission, DbContext> Tbl_CashierPermission
        {
            get
            {
                if (this.tbl_CashierPermission == null)
                {
                    this.tbl_CashierPermission = new GenericRepository<CashierPermission, DbContext>(context);
                }
                return tbl_CashierPermission;
            }
        }
        private IGenericRepository<ProductServingUnit, DbContext> tbl_ProductServingUnit;
        public IGenericRepository<ProductServingUnit, DbContext> Tbl_ProductServingUnit
        {
            get
            {
                if (this.tbl_ProductServingUnit == null)
                {
                    this.tbl_ProductServingUnit = new GenericRepository<ProductServingUnit, DbContext>(context);
                }
                return tbl_ProductServingUnit;
            }
        }

        private IGenericRepository<ProductStockMaster, DbContext> tbl_ProductStockMaster;
        public IGenericRepository<ProductStockMaster, DbContext> Tbl_ProductStockMaster
        {
            get
            {
                if (this.tbl_ProductStockMaster == null)
                {
                    this.tbl_ProductStockMaster = new GenericRepository<ProductStockMaster, DbContext>(context);
                }
                return tbl_ProductStockMaster;
            }
        }

        ////--------------------------------------------------------------------------------------------------------------------------

        public UnitOfWork()
        {
            context = new TContext();
        }

        public int Save()
        {
            return context.SaveChanges();
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
