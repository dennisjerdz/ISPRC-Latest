namespace ISPRC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class requiredadjustments : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Clubs", "ClubName", c => c.String(nullable: false));
            AlterColumn("dbo.ReleasePoints", "ReleasePointName", c => c.String(nullable: false));
            AlterColumn("dbo.ReleasePoints", "RaceLatitudeCoordinate", c => c.String(nullable: false));
            AlterColumn("dbo.ReleasePoints", "RaceLongitudeCoordinate", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ReleasePoints", "RaceLongitudeCoordinate", c => c.String());
            AlterColumn("dbo.ReleasePoints", "RaceLatitudeCoordinate", c => c.String());
            AlterColumn("dbo.ReleasePoints", "ReleasePointName", c => c.String());
            AlterColumn("dbo.Clubs", "ClubName", c => c.String());
        }
    }
}
