namespace TravelJournal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixSchema : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Photos",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FilePath = c.String(),
                        EntryId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Entries", t => t.EntryId, cascadeDelete: true)
                .Index(t => t.EntryId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Photos", "EntryId", "dbo.Entries");
            DropIndex("dbo.Photos", new[] { "EntryId" });
            DropTable("dbo.Photos");
        }
    }
}
