namespace TravelJournal.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsDeletedToEntry : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Entries", "IsDeleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Entries", "IsDeleted");
        }
    }
}
