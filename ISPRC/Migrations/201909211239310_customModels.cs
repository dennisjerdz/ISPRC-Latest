namespace ISPRC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class customModels : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Birds",
                c => new
                    {
                        BirdId = c.Int(nullable: false, identity: true),
                        BirdName = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        OwnerId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.BirdId)
                .ForeignKey("dbo.AspNetUsers", t => t.OwnerId)
                .Index(t => t.OwnerId);
            
            CreateTable(
                "dbo.BirdRaces",
                c => new
                    {
                        BirdRaceId = c.Int(nullable: false, identity: true),
                        BirdId = c.Int(nullable: false),
                        RaceId = c.Int(nullable: false),
                        EndLatitude = c.String(),
                        EndLongitude = c.String(),
                        ReleaseDate = c.DateTime(),
                        ArrivalDate = c.DateTime(),
                        Speed = c.Double(),
                        Distance = c.Double(),
                        BirdCode = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.BirdRaceId)
                .ForeignKey("dbo.Birds", t => t.BirdId, cascadeDelete: true)
                .ForeignKey("dbo.Races", t => t.RaceId, cascadeDelete: true)
                .Index(t => t.BirdId)
                .Index(t => t.RaceId);
            
            CreateTable(
                "dbo.Races",
                c => new
                    {
                        RaceId = c.Int(nullable: false, identity: true),
                        RaceName = c.String(),
                        RaceStartTime = c.DateTime(nullable: false),
                        RaceCutOff = c.DateTime(nullable: false),
                        RaceEnded = c.DateTime(),
                        DateCreated = c.DateTime(nullable: false),
                        RaceLatitudeCoordinate = c.String(),
                        RaceLongitudeCoordinate = c.String(),
                        ForceRaceDone = c.Boolean(),
                    })
                .PrimaryKey(t => t.RaceId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BirdRaces", "RaceId", "dbo.Races");
            DropForeignKey("dbo.BirdRaces", "BirdId", "dbo.Birds");
            DropForeignKey("dbo.Birds", "OwnerId", "dbo.AspNetUsers");
            DropIndex("dbo.BirdRaces", new[] { "RaceId" });
            DropIndex("dbo.BirdRaces", new[] { "BirdId" });
            DropIndex("dbo.Birds", new[] { "OwnerId" });
            DropTable("dbo.Races");
            DropTable("dbo.BirdRaces");
            DropTable("dbo.Birds");
        }
    }
}
