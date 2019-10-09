namespace ISPRC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RaceLoadingdateandDescription : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Races", "RaceLoadingDate", c => c.DateTime());
            AddColumn("dbo.Races", "RaceDescription", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Races", "RaceDescription");
            DropColumn("dbo.Races", "RaceLoadingDate");
        }
    }
}
