using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
   public class BLL_Table
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_Table()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_Table(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public IEnumerable<TableMaster> GetTables(Int32 compid)
        {
            try
            {
                IEnumerable<TableMaster> tablemaster = _unitofwork.TableMasterRepository.Get(tm=>tm.CompanyID==compid).OrderBy(tm => tm.TableCode);
                if (tablemaster != null)
                {
                    return tablemaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<TableMaster> GetActiveTables(Int32 compid)
        {
            try
            {
                IEnumerable<TableMaster> tablemaster = _unitofwork.TableMasterRepository.Get(tm => tm.IsDelete == false && tm.CompanyID==compid).OrderBy(tm => tm.TableCode);
                if (tablemaster != null)
                {
                    return tablemaster;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public TableMaster GetTableById(long id)
        {
            try
            {
                TableMaster tablemaster = _unitofwork.TableMasterRepository.Get(tm => tm.TableMasterID == id).FirstOrDefault();
                if (tablemaster != null)
                {
                    return tablemaster;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public TableMaster GetTableByCode(string code, Int32 compid)
        {
            try
            {
                TableMaster tbl = _unitofwork.TableMasterRepository.Get(g => g.TableCode == code && g.CompanyID==compid).FirstOrDefault();
                if (tbl != null)
                {
                    return tbl;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public int SaveTable(TableMaster tm)
        {
            try
            {
                _unitofwork.TableMasterRepository.Insert(tm);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateTable(TableMaster tm)
        {
            try
            {
                _unitofwork.TableMasterRepository.Update(tm);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        //Add by dilshan 2020/09/15
        public IEnumerable<TableMaster> GetActiveTablesByLocationId(int locationid)
        {
            try
            {
                IEnumerable<TableMaster> tablemaster = _unitofwork.TableMasterRepository.Get(tm => tm.LocationId == locationid && tm.IsDelete == false).OrderBy(tm => tm.TableCode);
                if (tablemaster != null)
                {
                    return tablemaster;

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
