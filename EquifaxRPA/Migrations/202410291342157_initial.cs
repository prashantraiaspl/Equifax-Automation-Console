namespace EquifaxRPA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RequestMasters",
                c => new
                    {
                        RequestId = c.Int(nullable: false, identity: true),
                        user_name = c.String(),
                        user_password = c.String(),
                        client_id = c.Int(nullable: false),
                        dispute_type = c.String(),
                        credit_repair_id = c.Int(nullable: false),
                        creditor_name = c.String(),
                        account_number = c.String(),
                        credit_balance = c.String(),
                        open_date = c.String(),
                        creditor = c.String(),
                        ownership = c.String(),
                        accuracy = c.String(),
                        comment = c.String(),
                        file_number = c.String(),
                        estimated_completion_date = c.DateTime(nullable: false),
                        submitted_date = c.DateTime(nullable: false),
                        request_status = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.RequestId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.RequestMasters");
        }
    }
}
