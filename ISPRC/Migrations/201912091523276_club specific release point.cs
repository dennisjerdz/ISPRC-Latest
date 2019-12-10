namespace ISPRC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class clubspecificreleasepoint : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ReleasePoints", "ClubId", c => c.Int());
            CreateIndex("dbo.ReleasePoints", "ClubId");
            AddForeignKey("dbo.ReleasePoints", "ClubId", "dbo.Clubs", "ClubId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ReleasePoints", "ClubId", "dbo.Clubs");
            DropIndex("dbo.ReleasePoints", new[] { "ClubId" });
            DropColumn("dbo.ReleasePoints", "ClubId");
        }
    }
}
