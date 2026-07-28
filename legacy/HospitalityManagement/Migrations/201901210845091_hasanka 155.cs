namespace HospitalityManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hasanka155 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CashierFunctions",
                c => new
                    {
                        CashierFunctionId = c.Long(nullable: false, identity: true),
                        FunctionName = c.String(nullable: false),
                        FunctionDescription = c.String(nullable: false),
                        Order = c.Long(nullable: false),
                        TypeID = c.Int(nullable: false),
                        IsDelete = c.Boolean(nullable: false),
                        IsValue = c.Boolean(nullable: false),
                        GroupOfCompanyID = c.Int(nullable: false),
                        CreatedUser = c.String(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.CashierFunctionId);
            
            CreateTable(
                "dbo.CashierGroups",
                c => new
                    {
                        CashierGroupId = c.Int(nullable: false, identity: true),
                        EmployeegroupId = c.Int(nullable: false),
                        FunctionName = c.String(nullable: false),
                        FunctionDescription = c.String(nullable: false),
                        Order = c.Int(nullable: false),
                        Value = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Type = c.String(nullable: false),
                        TypeId = c.Int(nullable: false),
                        IsAccess = c.Boolean(nullable: false),
                        IsValue = c.Boolean(nullable: false),
                        GroupOfCompanyId = c.Int(nullable: false),
                        CreatedUser = c.String(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CashierGroupId);
            
            CreateTable(
                "dbo.CashierPermissions",
                c => new
                    {
                        CashierPermissionId = c.Long(nullable: false, identity: true),
                        LocationId = c.Long(nullable: false),
                        CashierId = c.Long(nullable: false),
                        EmployeeId = c.Long(nullable: false),
                        Password = c.String(nullable: false),
                        JournalName = c.String(nullable: false),
                        EnCode = c.String(nullable: false),
                        FunctionName = c.String(nullable: false),
                        FunctionDescription = c.String(nullable: false),
                        Order = c.Long(nullable: false),
                        Value = c.Long(nullable: false),
                        MaxValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Type = c.String(nullable: false),
                        TypeID = c.String(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        IsAccess = c.Boolean(nullable: false),
                        Remarks = c.String(nullable: false),
                        IsDelete = c.Boolean(nullable: false),
                        IsValue = c.Boolean(nullable: false),
                        GroupOfCompanyId = c.Int(nullable: false),
                        CreatedUser = c.String(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedUser = c.String(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                        DataTransfer = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CashierPermissionId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CashierPermissions");
            DropTable("dbo.CashierGroups");
            DropTable("dbo.CashierFunctions");
        }
    }
}
