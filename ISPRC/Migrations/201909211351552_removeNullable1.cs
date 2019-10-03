namespace ISPRC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeNullable1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Races", "ForceRaceDone", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Races", "ForceRaceDone", c => c.Boolean());
        }
    }
}
