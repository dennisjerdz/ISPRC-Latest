namespace ISPRC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ClubIdforRaces : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Races", "ClubId", c => c.Int());
            CreateIndex("dbo.Races", "ClubId");
            AddForeignKey("dbo.Races", "ClubId", "dbo.Clubs", "ClubId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Races", "ClubId", "dbo.Clubs");
            DropIndex("dbo.Races", new[] { "ClubId" });
            DropColumn("dbo.Races", "ClubId");
        }
    }
}
