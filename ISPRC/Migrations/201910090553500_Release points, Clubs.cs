namespace ISPRC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReleasepointsClubs : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Clubs",
                c => new
                    {
                        ClubId = c.Int(nullable: false, identity: true),
                        ClubName = c.String(),
                    })
                .PrimaryKey(t => t.ClubId);
            
            CreateTable(
                "dbo.ReleasePoints",
                c => new
                    {
                        ReleasePointId = c.Int(nullable: false, identity: true),
                        ReleasePointName = c.String(),
                        ReleasePointCoordinates = c.String(),
                        RaceLatitudeCoordinate = c.String(),
                        RaceLongitudeCoordinate = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ReleasePointId);
            
            AddColumn("dbo.AspNetUsers", "Address", c => c.String());
            AddColumn("dbo.AspNetUsers", "MobileNumber", c => c.String());
            AddColumn("dbo.AspNetUsers", "LoftLatitudeCoordinate", c => c.Int());
            AddColumn("dbo.AspNetUsers", "LoftLongitudeCoordinate", c => c.Int());
            AddColumn("dbo.AspNetUsers", "ClubId", c => c.Int());
            AddColumn("dbo.Races", "ReleasePointId", c => c.Int());
            CreateIndex("dbo.AspNetUsers", "ClubId");
            CreateIndex("dbo.Races", "ReleasePointId");
            AddForeignKey("dbo.AspNetUsers", "ClubId", "dbo.Clubs", "ClubId");
            AddForeignKey("dbo.Races", "ReleasePointId", "dbo.ReleasePoints", "ReleasePointId");
            DropColumn("dbo.Races", "RaceLatitudeCoordinate");
            DropColumn("dbo.Races", "RaceLongitudeCoordinate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Races", "RaceLongitudeCoordinate", c => c.String());
            AddColumn("dbo.Races", "RaceLatitudeCoordinate", c => c.String());
            DropForeignKey("dbo.Races", "ReleasePointId", "dbo.ReleasePoints");
            DropForeignKey("dbo.AspNetUsers", "ClubId", "dbo.Clubs");
            DropIndex("dbo.Races", new[] { "ReleasePointId" });
            DropIndex("dbo.AspNetUsers", new[] { "ClubId" });
            DropColumn("dbo.Races", "ReleasePointId");
            DropColumn("dbo.AspNetUsers", "ClubId");
            DropColumn("dbo.AspNetUsers", "LoftLongitudeCoordinate");
            DropColumn("dbo.AspNetUsers", "LoftLatitudeCoordinate");
            DropColumn("dbo.AspNetUsers", "MobileNumber");
            DropColumn("dbo.AspNetUsers", "Address");
            DropTable("dbo.ReleasePoints");
            DropTable("dbo.Clubs");
        }
    }
}
