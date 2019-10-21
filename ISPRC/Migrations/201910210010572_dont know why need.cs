namespace ISPRC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dontknowwhyneed : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Races", "ReleasePointId", "dbo.ReleasePoints");
            DropIndex("dbo.Races", new[] { "ReleasePointId" });
            AlterColumn("dbo.Birds", "BirdName", c => c.String(nullable: false));
            AlterColumn("dbo.Races", "RaceName", c => c.String(nullable: false));
            AlterColumn("dbo.Races", "RaceLoadingDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Races", "ReleasePointId", c => c.Int(nullable: false));
            CreateIndex("dbo.Races", "ReleasePointId");
            AddForeignKey("dbo.Races", "ReleasePointId", "dbo.ReleasePoints", "ReleasePointId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Races", "ReleasePointId", "dbo.ReleasePoints");
            DropIndex("dbo.Races", new[] { "ReleasePointId" });
            AlterColumn("dbo.Races", "ReleasePointId", c => c.Int());
            AlterColumn("dbo.Races", "RaceLoadingDate", c => c.DateTime());
            AlterColumn("dbo.Races", "RaceName", c => c.String());
            AlterColumn("dbo.Birds", "BirdName", c => c.String());
            CreateIndex("dbo.Races", "ReleasePointId");
            AddForeignKey("dbo.Races", "ReleasePointId", "dbo.ReleasePoints", "ReleasePointId");
        }
    }
}
