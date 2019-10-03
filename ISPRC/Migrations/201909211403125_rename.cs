namespace ISPRC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rename : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Races", "RaceStartDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Races", "RaceCutOffDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Races", "RaceEndedDate", c => c.DateTime());
            DropColumn("dbo.Races", "RaceStartTime");
            DropColumn("dbo.Races", "RaceCutOff");
            DropColumn("dbo.Races", "RaceEnded");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Races", "RaceEnded", c => c.DateTime());
            AddColumn("dbo.Races", "RaceCutOff", c => c.DateTime(nullable: false));
            AddColumn("dbo.Races", "RaceStartTime", c => c.DateTime(nullable: false));
            DropColumn("dbo.Races", "RaceEndedDate");
            DropColumn("dbo.Races", "RaceCutOffDate");
            DropColumn("dbo.Races", "RaceStartDate");
        }
    }
}
