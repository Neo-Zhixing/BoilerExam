namespace BoilerExam.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuestionPoints : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ExamQuestions", "Points", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ExamQuestions", "Points");
        }
    }
}
