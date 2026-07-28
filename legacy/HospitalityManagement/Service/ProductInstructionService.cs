using HospitalityManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Service
{
    public class ProductInstructionService
    {
        ApplicationDbContext context = new ApplicationDbContext();
        public IEnumerable<KOTBOTDescription> GetDescription()
        {
            try
            {
                IEnumerable<KOTBOTDescription> descriptions = context.KOTBOTDescription.Where(g => g.IsActive == true ).OrderBy(g => g.Description);
                if (descriptions != null)
                {
                    return descriptions;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<ProductInstruction> GetInstructions()
        {
            try
            {
               // IEnumerable<ProductInstruction> ins = context.ProductInstruction.OrderBy(g => g.ProductInstructionId);


                var prdins = (from pi in context.ProductInstruction                              
                              join p in context.Product on pi.ProductId equals p.ProductId                          
                              orderby p.ProductName
                              select new
                              {
                                  ProductId = pi.ProductId,
                                  ProductName = p.ProductName,
                                  InsList = pi.InstructionList
                                 
                              }
                       ).ToList();
                List<ProductInstruction> ins = new List<ProductInstruction>();
                foreach (var item in prdins) 
                {
                    ProductInstruction pins = new ProductInstruction();
                    pins.ProductId = item.ProductId;
                    pins.ProductName = item.ProductName;
                    pins.InstructionList = item.InsList;
                    ins.Add(pins);
                }



                if (ins != null)
                {
                    return ins;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<ProductInstruction> GetInstructionsById(long id)
        {
            try
            {
                // IEnumerable<ProductInstruction> ins = context.ProductInstruction.OrderBy(g => g.ProductInstructionId);


                var prdins = (from pi in context.ProductInstruction
                              join p in context.Product on pi.ProductId equals p.ProductId
                              where pi.ProductId == id
                              orderby p.ProductName
                             
                              select new
                              {
                                  ProductId = pi.ProductId,
                                  ProductName = p.ProductName,
                                  InsList = pi.InstructionList

                              }
                       ).ToList();
                List<ProductInstruction> ins = new List<ProductInstruction>();
                foreach (var item in prdins)
                {
                    ProductInstruction pins = new ProductInstruction();
                    pins.ProductId = item.ProductId;
                    pins.ProductName = item.ProductName;
                    pins.InstructionList = item.InsList;
                    ins.Add(pins);
                }



                if (ins != null)
                {
                    return ins;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public KOTBOTDescription GetDescriptionsById(long id)
        {
            try
            {
                KOTBOTDescription descriptions = context.KOTBOTDescription.Where(g => g.KOTBOTDescriptionId == id).FirstOrDefault();
                if (descriptions != null)
                {
                    return descriptions;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public int UpdateDescription(KOTBOTDescription desc)
        {
            try
            {

                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public bool Save(ProductInstruction insheader)
        {


            using (var dbtransaction = context.Database.BeginTransaction())
            {

                try
                {
                    foreach (var detail in insheader.Idetail)
                    {

                        ProductInstruction ins = new ProductInstruction();
                        ins.InstructionList = insheader.InstructionList;
                        ins.ProductId = detail.ProductId;
                        ins.CreateDate = DateTime.Now;
                        ins.ModifiedDate = DateTime.Now;
                        context.ProductInstruction.Add(ins);

                        context.ProductInstruction.RemoveRange(context.ProductInstruction.Where(x => x.ProductId == detail.ProductId));

                    }

                    context.SaveChanges();
                        
                    dbtransaction.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    dbtransaction.Rollback();
                    return false;

                }
            }
        }

        public int UpdateInstruction(ProductInstruction ins)
        {
            try
            {


                if (ins.InstructionList == "")
                {
                    context.ProductInstruction.RemoveRange(context.ProductInstruction.Where(x => x.ProductId == ins.ProductId));
                    return 1;
                }
                else
                {
                    context.ProductInstruction.RemoveRange(context.ProductInstruction.Where(x => x.ProductId == ins.ProductId));
                    ins.CreateDate = DateTime.Now;
                    context.ProductInstruction.Add(ins);
                  
                  // context.SaveChanges();

                }
                int res = context.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public bool SaveDescription(KOTBOTDescription desc)
        {


            using (var dbtransaction = context.Database.BeginTransaction())
            {

                try
                {
                    context.KOTBOTDescription.Add(desc);
                    context.SaveChanges();
                    dbtransaction.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    dbtransaction.Rollback();
                    return false;

                }
            }
        }
        public KOTBOTDescription GetInstructionById(long id)
        {
            try
            {
                var ins = context.KOTBOTDescription.FirstOrDefault(g => g.KOTBOTDescriptionId == id);
                return ins ?? null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public int RemovePInstructions(long id, long pid)
        {
            try
            {
                context.ProductInstruction.RemoveRange(context.ProductInstruction.Where(x => x.ProductId == id && x.ProductInstructionId == pid));
                var res = context.SaveChanges();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }
        public int RemovePInstructionsbyId(long id)
        {
            try
            {
                context.ProductInstruction.RemoveRange(context.ProductInstruction.Where(x => x.ProductInstructionId == id));
                var res = context.SaveChanges();
                return res;

            }
            catch (Exception)
            {

                throw;
            }
        }
        public IEnumerable<ProductInstruction> GetProductInsByProductId(long id)
        {
            try
            {
                var ins = context.ProductInstruction.Select(p => new { p.ProductId, p.InstructionList }).Where(p => p.ProductId == id).OrderBy(g => g.ProductId);
                List<ProductInstruction> prdins = new List<ProductInstruction>();
                foreach (var p in ins)
                {
                    ProductInstruction pins = new ProductInstruction();
                    pins.ProductId = p.ProductId;
                    pins.InstructionList = p.InstructionList;
                    prdins.Add(pins);
                }

                if (prdins != null)
                {
                    return prdins;
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