using RIT.HMS.Data;
using RIT.HMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.MasterData
{
   public class BLL_MealType
    {
        private readonly UnitOfWork _unitofwork;
        public BLL_MealType()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_MealType(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }
        public IEnumerable<RstMealType> GetMeals(Int32 compid)
        {
            try
            {
                IEnumerable<RstMealType> rstmealtype = _unitofwork.MealTypeRepository.Get(m=>m.CompanyID==compid).OrderBy(m => m.RstMealTypeCode);
                if (rstmealtype != null)
                {
                    return rstmealtype;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<RstMealType> GetActiveMealss(Int32 compid)
        {
            try
            {
                IEnumerable<RstMealType> rstmealtype = _unitofwork.MealTypeRepository.Get(m => m.IsActive == true && m.CompanyID==compid).OrderBy(m => m.RstMealTypeCode);
                if (rstmealtype != null)
                {
                    return rstmealtype;

                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public RstMealType GetMealTypeById(long id)
        {
            try
            {
                RstMealType rstmealtype = _unitofwork.MealTypeRepository.Get(m => m.RstMealTypeId == id).FirstOrDefault();
                if (rstmealtype != null)
                {
                    return rstmealtype;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public RstMealType GetMealTypeByCode(string code, Int32 compid)
        {
            try
            {
                RstMealType rstmealtype = _unitofwork.MealTypeRepository.Get(m => m.RstMealTypeCode == code && m.CompanyID==compid).FirstOrDefault();
                if (rstmealtype != null)
                {
                    return rstmealtype;
                }
                else
                    return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public int SaveMealType(RstMealType m)
        {
            try
            {
                _unitofwork.MealTypeRepository.Insert(m);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateMealType(RstMealType m)
        {
            try
            {
                _unitofwork.MealTypeRepository.Update(m);
                return _unitofwork.Save();

            }
            catch (Exception ex)
            {
                return 0;
            }
        }



    }
}
