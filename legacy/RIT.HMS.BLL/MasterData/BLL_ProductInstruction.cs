using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
     public class BLL_ProductInstruction
    {
        private readonly UnitOfWork _unitofwork;

        public BLL_ProductInstruction()
        {
            _unitofwork = new UnitOfWork();
        }

        public BLL_ProductInstruction(string connection)
        {
            _unitofwork = new UnitOfWork(connection);
        }
        public IEnumerable<KOTBOTDescription> GetDescription(int companyid)
        {
            try
            {
                IEnumerable<KOTBOTDescription> descriptions = _unitofwork.KOTBOTDescriptionRepository.Get(g => g.IsActive == true && g.CompanyId==companyid).OrderBy(g => g.Description);
                descriptions.ToList().ForEach(d=>{ d.Type = _unitofwork.PrinterTypeRepository.GetById(Convert.ToInt32(d.Type)).PrinterTypeName; });

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

                var prdins = (from pi in _unitofwork.ProductInstructionRepository.Get()
                              join p in _unitofwork.ProductRepository.Get() on pi.ProductId equals p.ProductId
                              where p.IsActive == true && p.IsDelete == false
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


                var prdins = (from pi in _unitofwork.ProductInstructionRepository.Get()
                              join p in _unitofwork.ProductRepository.Get() on pi.ProductId equals p.ProductId
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
                KOTBOTDescription descriptions = _unitofwork.KOTBOTDescriptionRepository.Get(g => g.KOTBOTDescriptionId == id).FirstOrDefault();
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
                _unitofwork.KOTBOTDescriptionRepository.Update(desc);
                int res = _unitofwork.Save();
                return res;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public bool Save(ProductInstruction insheader)
        {

            _unitofwork.CreateTransaction();
         

                try
                {
                    foreach (var detail in insheader.Idetail)
                    {

                        ProductInstruction ins = new ProductInstruction();
                        ins.InstructionList = insheader.InstructionList;
                        ins.ProductId = detail.ProductId;
                        ins.CreateDate = DateTime.Now;
                        ins.ModifiedDate = DateTime.Now;
                        ins.CompanyId = insheader.CompanyId;

                    _unitofwork.ProductInstructionRepository.Insert(ins);
                    _unitofwork.ProductInstructionRepository.DeleteRange(_unitofwork.ProductInstructionRepository.Get(x => x.ProductId == detail.ProductId));

                    }

                _unitofwork.Save();
                _unitofwork.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                _unitofwork.Rollback();
                    return false;

                }
            
        }

        public int UpdateInstruction(ProductInstruction ins)
        {
            try
            {
                if (ins.InstructionList == "")
                {
                    _unitofwork.ProductInstructionRepository.DeleteRange(_unitofwork.ProductInstructionRepository.Get(x => x.ProductId == ins.ProductId));
                    _unitofwork.Save();
                    return 1;
                }
                else
                {
                    _unitofwork.ProductInstructionRepository.DeleteRange(_unitofwork.ProductInstructionRepository.Get(x => x.ProductId == ins.ProductId));
                    ins.CreateDate = DateTime.Now;
                    ins.ModifiedDate = DateTime.Now;
                    _unitofwork.ProductInstructionRepository.Insert(ins);                   
                }
                int res = _unitofwork.Save();
                return res;
            }
            catch (Exception ex)
            {
                return 0;
                //throw;
            }
        }

        public bool SaveDescription(KOTBOTDescription desc)
        {


            _unitofwork.CreateTransaction();


                try
                {
                _unitofwork.KOTBOTDescriptionRepository.Insert(desc);
                _unitofwork.Save();
                _unitofwork.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                _unitofwork.Rollback();
                    return false;

                }
            
        }
        public KOTBOTDescription GetInstructionById(long id)
        {
            try
            {
                var ins = _unitofwork.KOTBOTDescriptionRepository.Get(g => g.KOTBOTDescriptionId == id).FirstOrDefault();
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
                _unitofwork.ProductInstructionRepository.DeleteRange(_unitofwork.ProductInstructionRepository.Get(x => x.ProductId == id && x.ProductInstructionId == pid));
                var res = _unitofwork.Save();
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
                _unitofwork.ProductInstructionRepository.DeleteRange(_unitofwork.ProductInstructionRepository.Get(x => x.ProductInstructionId == id));
                var res = _unitofwork.Save();
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
                var ins = _unitofwork.ProductInstructionRepository.Get().Select(p => new { p.ProductId, p.InstructionList }).Where(p => p.ProductId == id).OrderBy(g => g.ProductId);
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
