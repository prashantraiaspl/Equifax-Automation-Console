namespace EquifaxRPA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EnumRemoved : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.RequestMasters", "request_status", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.RequestMasters", "request_status", c => c.Int(nullable: false));
        }
    }
}
