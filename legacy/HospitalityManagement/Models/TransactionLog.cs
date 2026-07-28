using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalityManagement.Models
{
    public class TransactionLog
    {
        public int  TransactionLogID  { get; set; }
	    public string  TransactionDocumentNo  { get; set; }
	    public int  TransactionDocumentId  { get; set; }
	    public string  FormName  { get; set; }
	    public DateTime  TransactionDate  { get; set; }
	    public DateTime  AuditDate  { get; set; }
	    public string  LoggedLocation  { get; set; }
	    public string  ReferenceNo  { get; set; }
	    public string  ComputerName  { get; set; }
	    public int  GroupOfCompanyID  { get; set; }
	    public string  CreatedUser  { get; set; }
	    public DateTime  CreatedDate  { get; set; }
	    public string  ModifiedUser  { get; set; }
	    public DateTime  ModifiedDate  { get; set; }
	    public int  DataTransfer  { get; set; }
    }
}