namespace TravelJournal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Entries",
                c => new
                    {
                        EntryId = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Content = c.String(),
                        Location = c.String(),
                        Latitude = c.Decimal(precision: 18, scale: 2),
                        Longitude = c.Decimal(precision: 18, scale: 2),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                        JournalId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.EntryId)
                .ForeignKey("dbo.Journals", t => t.JournalId, cascadeDelete: true)
                .Index(t => t.JournalId);
            
            CreateTable(
                "dbo.Journals",
                c => new
                    {
                        JournalId = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Description = c.String(),
                        IsPublic = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.JournalId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        Username = c.String(),
                        Email = c.String(),
                        DisplayName = c.String(),
                        PasswordHash = c.String(),
                        Role = c.String(),
                        SubscriptionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.Subscriptions", t => t.SubscriptionId, cascadeDelete: true)
                .Index(t => t.SubscriptionId);
            
            CreateTable(
                "dbo.Subscriptions",
                c => new
                    {
                        SubscriptionId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        StorageLimitMB = c.Int(nullable: false),
                        EntryLimit = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.SubscriptionId);
            
            CreateTable(
                "dbo.Media",
                c => new
                    {
                        MediaId = c.Int(nullable: false, identity: true),
                        FileName = c.String(),
                        FileType = c.String(),
                        FileSize = c.Int(nullable: false),
                        Url = c.String(),
                        UploadedAt = c.DateTime(nullable: false),
                        EntryId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MediaId)
                .ForeignKey("dbo.Entries", t => t.EntryId, cascadeDelete: true)
                .Index(t => t.EntryId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Media", "EntryId", "dbo.Entries");
            DropForeignKey("dbo.Users", "SubscriptionId", "dbo.Subscriptions");
            DropForeignKey("dbo.Journals", "UserId", "dbo.Users");
            DropForeignKey("dbo.Entries", "JournalId", "dbo.Journals");
            DropIndex("dbo.Media", new[] { "EntryId" });
            DropIndex("dbo.Users", new[] { "SubscriptionId" });
            DropIndex("dbo.Journals", new[] { "UserId" });
            DropIndex("dbo.Entries", new[] { "JournalId" });
            DropTable("dbo.Media");
            DropTable("dbo.Subscriptions");
            DropTable("dbo.Users");
            DropTable("dbo.Journals");
            DropTable("dbo.Entries");
        }
    }
}
