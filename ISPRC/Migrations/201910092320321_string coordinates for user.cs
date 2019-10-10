namespace ISPRC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class stringcoordinatesforuser : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AspNetUsers", "LoftLatitudeCoordinate", c => c.String());
            AlterColumn("dbo.AspNetUsers", "LoftLongitudeCoordinate", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AspNetUsers", "LoftLongitudeCoordinate", c => c.Int());
            AlterColumn("dbo.AspNetUsers", "LoftLatitudeCoordinate", c => c.Int());
        }
    }
}
