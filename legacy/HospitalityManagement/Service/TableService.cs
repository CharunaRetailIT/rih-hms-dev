using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class TableService
    {


        ApplicationDbContext context = new ApplicationDbContext();

        public IEnumerable<TableMaster> GetTables()
        {
            try
            {
                IEnumerable<TableMaster> tablemaster = context.TableMasters.OrderBy(tm => tm.TableCode);
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

        public IEnumerable<TableMaster> GetActiveTables()
        {
            try
            {
                IEnumerable<TableMaster> tablemaster = context.TableMasters.Where(tm => tm.IsDelete == false).OrderBy(tm => tm.TableCode);
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
                TableMaster tablemaster = context.TableMasters.Where(tm => tm.TableMasterID == id).FirstOrDefault();
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

        public int SaveTable(TableMaster tm)
        {
            try
            {
                tm.TableState = "Empty";
                context.TableMasters.Add(tm);
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateTable(TableMaster tm)
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

        public TableMaster GetTableByCode(string code)
        {
            try
            {
                TableMaster tbl = context.TableMasters.Where(g => g.TableCode == code).FirstOrDefault();
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


    }
}