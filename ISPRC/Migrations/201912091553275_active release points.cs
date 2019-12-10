namespace ISPRC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class activereleasepoints : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReleasePoints", "IsActive", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReleasePoints", "IsActive");
        }
    }
}
