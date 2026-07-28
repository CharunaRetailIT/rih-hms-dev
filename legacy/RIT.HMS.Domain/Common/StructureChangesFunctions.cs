using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIT.HMS.Domain.Common
{
    public class StructureChangesFunctions
    {
        string tableName = "";
        string query = "";
        string spName = "";
        string ViewName = "";
        string ColumnName = "";
        public bool status = true;
        string Stringsqlconnection = "";
        string functionName = "";
        string functionquery = "";
        string CheckfunctionName = "";

        private string CheckFunctions(string Function, string query)
        {
            var UDQuery = string.Format(@"IF EXISTS (
                                        SELECT * FROM sysobjects WHERE id = object_id(N'function_name') 
                                        AND xtype IN (N'FN', N'IF', N'TF')
                                    )
                                        DROP FUNCTION function_name
                                    GO", Function, query);
            return UDQuery;
        }
   
        public void ExecuteFunction(string con)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(con))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(functionquery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {

            }

        }


        public void RunFunction(string connectionString)
        {
            Stringsqlconnection = connectionString;

            #region GetDailySalesReportValues
            functionName = "GetDailySalesReportValues";
            functionquery = @" 
 ALTER FUNCTION [dbo].[GetDailySalesReportValues] --2026-01-20 Add Uber/Pickme/Online/Visacard/Amexcard/Debitcard/Credit
(
@ReceiptNo varchar(50)='',
@Zno varchar(10)='',
@UnitNo int=0,
@LocationId int=0,
@ValueType char(2),
@Date datetime=''
)
RETURNS decimal(18,2)
AS
BEGIN
declare @val decimal(18,2)=0,@discval decimal(18,2)=0
	IF(@ValueType='D')
	begin
	
	set @val=(
	select SUM(SDiscount)  AS BINNDISC from TransactionDets 
	 WHERE documentid=6 and Receipt=@ReceiptNo And ZNo=@Zno and UnitNo=@UnitNo)
	 
	 set @discval=isnull(@val,0)
	 
	 if (@discval=0)
		 begin
			set @val=(
			select SUM(IDiscount1)  AS BINNDISC from TransactionDets 
			WHERE Receipt=@ReceiptNo And ZNo=@Zno and UnitNo=@UnitNo) 
		 end
	 else
		 begin
			set @val=@val+(
			select SUM(IDiscount1)  AS BINNDISC from TransactionDets 
			WHERE  Receipt=@ReceiptNo And ZNo=@Zno and UnitNo=@UnitNo) 
		 end
	 
	--RETURN isnull(@val,0)
	end
	else
	if (@ValueType='FS')
	begin
		set @val=(
		select sum(case when td.DocumentID in (1,3) then td.Nett when td.DocumentID in (2,4)  then td.Nett *-1 end) as Net		
		from TransactionDets td
		Inner Join Products p on p.ProductId=td.ProductID
		Inner Join RstDepartments d on p.DepartmentId=d.RstDepartmentID
		where td.DocumentID in (1,3,2,4) and
		td.Receipt=@ReceiptNo And td.ZNo=@Zno and td.UnitNo=@UnitNo 
		and D.DepartmentName like '%food%' and d.IsActive=1
	  --	and p.DepartmentId=4
		)
				
	end

	else
	if (@ValueType='DS')
	begin
		set @val=(
		select sum(case when td.DocumentID in (1,3) then td.Nett when td.DocumentID in (2,4)  then td.Nett *-1 end) as Net		
		from TransactionDets td
		Inner Join Products p on p.ProductId=td.ProductID
		Inner Join RstDepartments d on p.DepartmentId=d.RstDepartmentID
		where td.DocumentID in (1,3,2,4) and
		td.Receipt=@ReceiptNo And td.ZNo=@Zno and td.UnitNo=@UnitNo 
		and D.DepartmentName like '%Dessert%'  and d.IsActive=1
		)
				
	end
	 
	
	if (@ValueType='BS')
	begin
		set @val=(
		select sum(case when td.DocumentID in (1,3) then td.Nett when td.DocumentID in (2,4)  then td.Nett *-1 end) as Net		
		from TransactionDets td
		Inner Join Products p on p.ProductId=td.ProductID
		Inner Join RstDepartments d on p.DepartmentId=d.RstDepartmentID
		where
		 td.DocumentID in (1,3,2,4)  and
		td.Receipt=@ReceiptNo And td.ZNo=@Zno and td.UnitNo=@UnitNo 
and D.DepartmentName like '%BEVARAGE%'  and d.IsActive=1
--and p.DepartmentId=8
		)

	end
	if (@ValueType='NS')
	begin
		set @val=(
		select sum(case when td.DocumentID in (1,3) then td.Nett when td.DocumentID in (2,4)  then td.Nett *-1 end) as Net		
		from TransactionDets td
		Inner Join Products p on p.ProductId=td.ProductID
		Inner Join RstDepartments d on p.DepartmentId=d.RstDepartmentID
		where
		 td.DocumentID in (1,3,2,4)  and
		td.Receipt=@ReceiptNo And td.ZNo=@Zno and td.UnitNo=@UnitNo 
    and D.DepartmentName Not like '%BEVARAGE%'  and D.DepartmentName not like '%food%'  and d.IsActive=1
--and p.DepartmentId=8
		)

	end







	if (@ValueType='CH') -- Cash Amount to each reciept
	begin
		set @val=
		(		
		select sum(case when pd.Balance<=pd.Amount then pd.Balance else pd.Amount end) as Net		
		from PaymentDets pd --inner join PayTypes pt on pd.PayTypeID=pt.PaymentID
		where pd.PayTypeID=1 and
		pd.Receipt=@ReceiptNo And pd.ZNo=@Zno and pd.UnitNo=@UnitNo 
		)
			
	end
	if (@ValueType='CD') -- Card Amount to each reciept
	begin
		set @val=
		(		
		select sum(case when pd.Balance<=pd.Amount then pd.Balance else pd.Amount end) as Net		
		from PaymentDets pd 
		where (pd.PayTypeID=2) and
		pd.Receipt=@ReceiptNo And pd.ZNo=@Zno and pd.UnitNo=@UnitNo 
		)
			
	end

		if (@ValueType='VD') -- VISA Amount to each reciept
	begin
		set @val=
		(		
		select sum(case when pd.Balance<=pd.Amount then pd.Balance else pd.Amount end) as Net		
		from PaymentDets pd 
		where (pd.PayTypeID=3) and
		pd.Receipt=@ReceiptNo And pd.ZNo=@Zno and pd.UnitNo=@UnitNo 
		)
			
	end

	if (@ValueType='AX') -- VISA Amount to each reciept
	begin
		set @val=
		(		
		select sum(case when pd.Balance<=pd.Amount then pd.Balance else pd.Amount end) as Net		
		from PaymentDets pd 
		where (pd.PayTypeID=4) and
		pd.Receipt=@ReceiptNo And pd.ZNo=@Zno and pd.UnitNo=@UnitNo 
		)
			
	end

	if (@ValueType='DD') -- VISA Amount to each reciept
	begin
		set @val=
		(		
		select sum(case when pd.Balance<=pd.Amount then pd.Balance else pd.Amount end) as Net		
		from PaymentDets pd 
		where (pd.PayTypeID=7) and
		pd.Receipt=@ReceiptNo And pd.ZNo=@Zno and pd.UnitNo=@UnitNo 
		)
			
	end

		if (@ValueType='CI') -- VISA Amount to each reciept
	begin
		set @val=
		(		
		select sum(case when pd.Balance<=pd.Amount then pd.Balance else pd.Amount end) as Net		
		from PaymentDets pd 
		where (pd.PayTypeID=12) and
		pd.Receipt=@ReceiptNo And pd.ZNo=@Zno and pd.UnitNo=@UnitNo 
		)
			
	end
	
		if (@ValueType='CR') -- credit amount
	begin
		set @val=
		(		
		select sum(pd.Amount) as Net		
		from PaymentDets pd 
		where (pd.PayTypeID=12) and
		pd.Receipt=@ReceiptNo And pd.ZNo=@Zno and pd.UnitNo=@UnitNo 
		)
			
	end

if (@ValueType='Ol') -- Online
	begin
		set @val=
		(		
		select sum(pd.Balance) as Net		
		from PaymentDets pd 
		where (pd.PayTypeID = 58)  and
		pd.Receipt=@ReceiptNo And pd.ZNo=@Zno and pd.UnitNo=@UnitNo 
		)
			
	end

	if (@ValueType='UB') -- UBER
	begin
		set @val=
		(		
		select sum(pd.Balance) as Net		
		from PaymentDets pd 
		where (pd.PayTypeID = 59)  and
		pd.Receipt=@ReceiptNo And pd.ZNo=@Zno and pd.UnitNo=@UnitNo 
		)
			
	end

		if (@ValueType='PM') -- PICKME
	begin
		set @val=
		(		
		select sum(pd.Balance) as Net		
		from PaymentDets pd 
		where pd.PayTypeID = (select PaymentID from PayTypes where Descrip like '%PickMe%')  and
		pd.Receipt=@ReceiptNo And pd.ZNo=@Zno and pd.UnitNo=@UnitNo 
		)
			
	end
	
	--if (@ValueType='OT') -- Other Types Payments
	--begin
	--	set @val=
	--	(		
	--	select sum(pd.Balance) as Net		
	--	from PaymentDets pd 
	--	where (pd.PayTypeID!=1 and pd.PayTypeID!=2 and pd.PayTypeID!=3 and pd.PayTypeID!=12 and 
	--	pd.PayTypeID!=4 and pd.PayTypeID!=7)  and
	--	pd.Receipt=@ReceiptNo And pd.ZNo=@Zno and pd.UnitNo=@UnitNo 
	--	)
			
	--end
	
	if (@ValueType='SC') -- Service Charge
	begin
		set @val=
		(		
		SELECT sum(TempItemTaxes.TaxAmount)
		FROM  Taxes INNER JOIN
		TempItemTaxes ON Taxes.TaxID = TempItemTaxes.TaxId
		WHERE     (TempItemTaxes.Receipt = @ReceiptNo) 
		AND (TempItemTaxes.TaxId = 1) and (TempItemTaxes.ZNo=@Zno)
		group by TempItemTaxes.TaxID
		)
			
	end
	
	if (@ValueType='VT') -- VAT
	begin
		set @val=
		(		
		SELECT sum(TempItemTaxes.TaxAmount)
		FROM  Taxes INNER JOIN
		TempItemTaxes ON Taxes.TaxID = TempItemTaxes.TaxId
		WHERE     (TempItemTaxes.Receipt = @ReceiptNo) AND 
		(TempItemTaxes.TDate = CONVERT(DATETIME,@Date, 102)) 
		AND (TempItemTaxes.TaxId = 4) and (TempItemTaxes.ZNo=@Zno)
		group by TempItemTaxes.TaxID 
		)
					
	end
	
	if (@ValueType='NB') -- NBT
	begin
		set @val=
		(		
		SELECT sum(TempItemTaxes.TaxAmount)
		FROM  Taxes INNER JOIN
		TempItemTaxes ON Taxes.TaxID = TempItemTaxes.TaxId
		WHERE     (TempItemTaxes.Receipt = @ReceiptNo) AND 
		(TempItemTaxes.TDate = CONVERT(DATETIME,@Date, 102)) 
		AND (TempItemTaxes.TaxId = 2) and (TempItemTaxes.ZNo=@Zno)
		group by TempItemTaxes.TaxID 
		)
					
	end
	
	if (@ValueType='TD') -- TDL
	begin
		set @val=
		(	
		SELECT sum(TempItemTaxes.TaxAmount)
		FROM  Taxes INNER JOIN
		TempItemTaxes ON Taxes.TaxID = TempItemTaxes.TaxId
		WHERE     (TempItemTaxes.Receipt = @ReceiptNo) AND 
		(TempItemTaxes.TDate = CONVERT(DATETIME,@Date, 102)) 
		AND (TempItemTaxes.TaxId = 3) and (TempItemTaxes.ZNo=@Zno)
		group by TempItemTaxes.TaxID 
		)
					
	end
	
	if (@ValueType='TD') -- Gross
	begin
		set @val=
		(		
		SELECT sum(TempItemTaxes.TaxAmount)
		FROM  Taxes INNER JOIN
		TempItemTaxes ON Taxes.TaxID = TempItemTaxes.TaxId
		WHERE     (TempItemTaxes.Receipt = @ReceiptNo) AND 
		(TempItemTaxes.TDate = CONVERT(DATETIME,@Date, 102)) 
		AND (TempItemTaxes.TaxId = 3) and (TempItemTaxes.ZNo=@Zno)
		group by TempItemTaxes.TaxID 
		)
					
	end
	if (@ValueType='GR') -- gross
	begin
		set @val=(
		select sum(case when td.DocumentID in (1,3) then td.Amount  when td.DocumentID in (2,4)  then td.Amount  *-1 end   ) as Gross		
		from TransactionDets td
		Inner Join Products p on p.ProductId=td.ProductID
		Inner Join RstDepartments d on p.DepartmentId=d.RstDepartmentID
		where  td.DocumentID in (1,3,2,4) and td.Status=1 and
		td.Receipt=@ReceiptNo And td.ZNo=@Zno and td.UnitNo=@UnitNo 
		
		)
				
	end
	
	if (@ValueType='NT') -- Net
	begin
		set @val=((
		select sum(case when td.DocumentID in (1,3) then td.Amount  when td.DocumentID in (2,4)  then td.Amount  *-1 end ) as TNet		
		from TransactionDets td
		Inner Join Products p on p.ProductId=td.ProductID
		Inner Join RstDepartments d on p.DepartmentId=d.RstDepartmentID
		where  td.DocumentID in (1,3,2,4) and td.Status=1 and
		td.Receipt=@ReceiptNo And td.ZNo=@Zno and td.UnitNo=@UnitNo 		
		)
	)
				
	end

return isnull(@val,0)

END    ";

            CheckFunctions(functionName, functionquery);
            ExecuteFunction(Stringsqlconnection);

            #endregion GetDailySalesReportValues

        }
    }
}
