using RIT.HMS.Data;
using RIT.HMS.Domain.Loyalty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.BLL.Loyalty
{
    public class BLL_CardNumberGeneration
    {
        UnitOfWork _unitofwork;
        public BLL_CardNumberGeneration()
        {
            _unitofwork = new UnitOfWork();
        }
        public BLL_CardNumberGeneration(string connectionname)
        {
            _unitofwork = new UnitOfWork(connectionname);
        }
        public CardGenerationLocationSetting GetParams(int locid,int companyid)
        {
            CardGenerationLocationSetting cardgensettings = new CardGenerationLocationSetting();
            var existlocation = _unitofwork.LocationRepository.GetById(locid);
            if (existlocation != null)
            {
                if (existlocation.LocationPrefixCode != null)
                {
                    cardgensettings.LocationPrefix = existlocation.LocationPrefixCode.ToString();
                }
                var para = _unitofwork.cardGenerationLocationSettingReporsitory.Get(p => p.LocationId == locid && p.CompanyID==companyid).SingleOrDefault();
                if (para != null)
                {

                    cardgensettings.CardStartingNo = para.CardStartingNo;
                    cardgensettings.EncodeStartingNo = para.EncodeStartingNo;
                    cardgensettings.CardNoLength = para.CardNoLength;
                }

            }
            
             return cardgensettings == null ? null : cardgensettings;
        }

        public CardGenerationLocationSetting GetDefaultParams(int companyid)
        {
            CardGenerationLocationSetting defaultsettings = new CardGenerationLocationSetting();      
            var para = _unitofwork.cardGenerationLocationSettingReporsitory.Get(p=>p.CompanyID==companyid).SingleOrDefault();
            if (para != null)
            {

                defaultsettings.CardStartingNo = para.CardStartingNo;
                defaultsettings.EncodeStartingNo = para.EncodeStartingNo;
                defaultsettings.CardNoLength = para.CardNoLength;
            }
            return defaultsettings == null ? null : defaultsettings;
        }
        public List<LoyaltyCardGenerationDetail> GenerateCardNumbers(int qty,int cardstartingno,
                                                                     int encodestartingno,int cardNoLength)
        {
            List<LoyaltyCardGenerationDetail> cardnumbers = new List<LoyaltyCardGenerationDetail>();
            for (int i = 1; i < qty + 1; i++)
            {
                LoyaltyCardGenerationDetail loyaltyCardGenerationDetail = new LoyaltyCardGenerationDetail();

                //loyaltyCardGenerationDetail.CardPrefix = cardFrefix;
                loyaltyCardGenerationDetail.CardPrefix = "";
                loyaltyCardGenerationDetail.CardLength = 0;
                loyaltyCardGenerationDetail.CardStartingNo = cardstartingno;
                loyaltyCardGenerationDetail.EncodeLength = 0;
                loyaltyCardGenerationDetail.EncodeStartingNo = encodestartingno;
                loyaltyCardGenerationDetail.GeneratedDate = DateTime.Now;

                //loyaltyCardGenerationDetail.CardNo = GetCardNo(cardstartingno, cardFrefix, cardNoLength);
                //loyaltyCardGenerationDetail.CardNoWithPrefix = GetCardNo(cardstartingno, cardFrefix, "", "", cardNoLength);
                loyaltyCardGenerationDetail.CardNo = GetCardNo(cardstartingno, "", cardNoLength);
                loyaltyCardGenerationDetail.CardNoWithPrefix = GetCardNo(cardstartingno, "", "", "", cardNoLength);
                loyaltyCardGenerationDetail.EncodeNo = GetEncodeNo(encodestartingno, cardNoLength);
                cardnumbers.Add(loyaltyCardGenerationDetail);

                cardstartingno++;
                encodestartingno++;
            }
            return cardnumbers == null ? null : cardnumbers;
          
        }
        private string GetCardNo(long cardStartingNo, string loyaltyPrefixCode, string encodePrifix, string encodeSuffix, int cardNoLength)
        {
            //  %H/O00002123%
            //cardNoLength += encodePrifix.Length + encodeSuffix.Length;
            string Format = string.Empty;
            int body = 0;
            body = (cardNoLength - loyaltyPrefixCode.Length);
            Format = String.Format("{0}{1}{2}{3}", encodePrifix.Trim(), loyaltyPrefixCode.Trim(), cardStartingNo.ToString().PadLeft(body, '0'), encodeSuffix.Trim());
            return Format;
        }
        private string GetCardNo(long cardStartingNo, string loyaltyPrefixCode, int cardNoLength)
        {
            
            string Format = string.Empty;
            int body = 0;
            body = (cardNoLength - loyaltyPrefixCode.Length);
            Format = String.Format("{0}{1}", loyaltyPrefixCode.Trim(), cardStartingNo.ToString().PadLeft(body, '0'));
            return Format;
        }
        private string GetEncodeNo(long encodeStartingNo, int cardNoLength)
        {
            string Format = string.Empty;          
            Format = String.Format("{0}", encodeStartingNo.ToString().PadLeft(cardNoLength, '0'));

            return Format;
        }
        public bool SaveCardNumbers(LoyaltyCardGenerationHeader header)
        {
            try
            {
                _unitofwork.CreateTransaction();
                header.LoyaltyCardGenerationDetail.ToList().ForEach(
                                                                    z => { z.GeneratedDate = header.GeneratedDate;
                                                                           z.CardGenerationDetailID=z.LoyaltyCardGenerationDetailId;
                                                                          }
                                                                    );


                if (header.Update==false)
                {
                    _unitofwork.loyaltyCardGenerationHeaderReporsitory.Insert(header);
                    var exsettings = _unitofwork.cardGenerationLocationSettingReporsitory.Get(s=>s.CompanyID==header.CompanyID).SingleOrDefault();
                    exsettings.CardStartingNo += header.LoyaltyCardGenerationDetail.Count();
                    exsettings.EncodeStartingNo += header.LoyaltyCardGenerationDetail.Count();
                    _unitofwork.cardGenerationLocationSettingReporsitory.Update(exsettings);

                    // Issue Loyalty Card Numbers - Mearged 2 Forms as per the ERP

                    LoyaltyCardIssueHeader issueheader = new LoyaltyCardIssueHeader();
                    issueheader.CardIssueHeaderID = issueheader.LoyaltyCardIssueHeaderId;
                    issueheader.IssueDate = header.GeneratedDate;
                    issueheader.ToLocationID = header.GenLocationId;
                    issueheader.DocumentNo = header.DocNumber;
                    issueheader.ReferenceNo = "";
                    issueheader.Remark = "Direct";

                    var user = _unitofwork.SysUserMasterRepository.Get(u => u.UserName == header.CreatedUser).SingleOrDefault();
                    var sss = _unitofwork.EmployeeRepository.Get(e => e.EmployeeCode == user.EmployeeCode && e.CompanyID == header.CompanyID);
                    var emp = _unitofwork.EmployeeRepository.Get(e => e.EmployeeCode == user.EmployeeCode 
                                                                        && e.CompanyID==header.CompanyID 
                                                                        && e.IsActive==true).SingleOrDefault();
                    issueheader.EmployeeID = Convert.ToInt16(emp.EmployeeID);
                    issueheader.GroupOfCompanyID = header.GroupOfCompanyID;
                    issueheader.LocationId = header.LocationId;
                    issueheader.CreatedDate = header.CreatedDate;
                    issueheader.CreatedUser = header.CreatedUser;
                    issueheader.ModifiedDate = header.ModifiedDate;
                    issueheader.ModifiedUser = header.ModifiedUser;
                    issueheader.DataTransfer = header.DataTransfer;
                    issueheader.CompanyID = header.CompanyID;
                    _unitofwork.Save();

                    List<LoyaltyCardIssueDetail> issuedetail = new List<LoyaltyCardIssueDetail>();
                    foreach (var d in header.LoyaltyCardGenerationDetail)
                    {
                        LoyaltyCardIssueDetail d1 = new LoyaltyCardIssueDetail();
                        d1.LoyaltyCardIssueHeaderId = issueheader.LoyaltyCardIssueHeaderId;
                        d1.CardIssueDetailID = d1.LoyaltyCardIssueDetailId;
                        d1.ToLocationID = header.LocationId;
                        d1.IssueDate = header.GeneratedDate;
                        d1.CardNo = d.CardNo;
                        d1.EncodeNo = d.EncodeNo;
                        d1.IsIssued = false;
                        d1.IsActive = true;
                        d1.FefCardNo1 = d.CardNo;
                        d1.GroupOfCompanyID = header.GroupOfCompanyID;
                        d1.LocationId = header.LocationId;
                        d1.CompanyID = header.CompanyID;
                        d1.CreatedDate = header.CreatedDate;
                        d1.CreatedUser = header.CreatedUser;
                        d1.ModifiedDate = header.ModifiedDate;
                        d1.ModifiedUser = header.ModifiedUser;
                        d1.DataTransfer = header.DataTransfer;
                        issuedetail.Add(d1);
                    }
                    issueheader.LoyaltyCardIssueDetail = issuedetail;
                    _unitofwork.LoyaltyCardIssueHeaderReporsitory.Insert(issueheader);
                }
                else
                {
                    var issuedetails= _unitofwork.LoyaltyCardIssueDetailReporsitory.Get(c => c.CardNo.CompareTo(header.CardNoFrom) >= 0 &&
                                                                                             c.CardNo.CompareTo(header.CardNoTo) <= 0 && c.IsIssued==false).ToList();
                   issuedetails.ForEach(d=>{
                        d.ToLocationID = header.GenLocationId;
                        d.ModifiedDate = DateTime.Now;
                        d.ModifiedUser = header.CreatedUser;
                    });

                    foreach (var d in issuedetails)
                    {
                        _unitofwork.LoyaltyCardIssueDetailReporsitory.Update(d);
                    }
                   

                }

                // End Issue Loyalty Card Numbers - Mearged 2 Forms as per the ERP



                _unitofwork.Save();
                _unitofwork.Commit();
                return true;
            }
            catch (Exception e)
            {
                _unitofwork.Rollback();
                return false;
            }       
             
        }
        public List<LoyaltyCardGenerationDetail> GetCardNoDetailByHeaderId(int id)
        {        
            var detail = _unitofwork.loyaltyCardGenerationDetailReporsitory.Get(h=>h.LoyaltyCardGenerationHeaderID==id).ToList();
            return detail == null ? null : detail;
        }

        public LoyaltyCustomer GetCardNumberByCustomerId(int id,int companyid)
        {
            var card = _unitofwork.LoyaltyCustomerReporsitory.Get(h => h.CustomerId == id && h.CompanyId== companyid).SingleOrDefault();
            return card == null ? null : card;
        }

        public List<LoyaltyCardIssueDetail> SelectCardNumbers(string cardnofrom, string cardnoto,int locationid,int companyid)
        {
            List<LoyaltyCardIssueDetail> LoyaltyCardIssueDetailList = new List<LoyaltyCardIssueDetail>();
            LoyaltyCardIssueDetailList = _unitofwork.LoyaltyCardIssueDetailReporsitory.Get(c => c.CardNo.CompareTo(cardnofrom) >= 0 &&
                                                                                                c.CardNo.CompareTo(cardnoto) <= 0 && c.IsIssued == false).ToList();
            return LoyaltyCardIssueDetailList == null ? null : LoyaltyCardIssueDetailList;
        }

        public int ValidateCardNumber(string cardnumber)
        {
             int isvalid=0;

            bool isgenerated = _unitofwork.LoyaltyCardIssueDetailReporsitory.Get().Any(c=>c.CardNo==cardnumber);
            bool isissued = _unitofwork.LoyaltyCustomerReporsitory.Get().Any(c => c.CardNo == cardnumber);

            if (isgenerated == false)
            {
                isvalid = 1; // not generated : error
            }
            else if (isgenerated == true && isissued == true)
            {
                isvalid = 2; // generated and already issued : error
            }
            else if (isgenerated == true && isissued == false)
            {
                isvalid = 3; // generated and not issued : success
            }
            return isvalid;
        }
    }
}
