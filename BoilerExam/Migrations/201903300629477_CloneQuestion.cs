namespace BoilerExam.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CloneQuestion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Questions", "ParentId", c => c.Int());
            CreateIndex("dbo.Questions", "ParentId");
            AddForeignKey("dbo.Questions", "ParentId", "dbo.Questions", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Questions", "ParentId", "dbo.Questions");
            DropIndex("dbo.Questions", new[] { "ParentId" });
            DropColumn("dbo.Questions", "ParentId");
        }
    }
}
