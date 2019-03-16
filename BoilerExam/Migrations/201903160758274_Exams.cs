namespace BoilerExam.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Exams : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExamQuestions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExamId = c.Int(nullable: false),
                        QuestionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Exams", t => t.ExamId, cascadeDelete: true)
                .ForeignKey("dbo.Questions", t => t.QuestionId, cascadeDelete: true)
                .Index(t => t.ExamId)
                .Index(t => t.QuestionId);
            
            CreateTable(
                "dbo.Exams",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Description = c.String(),
                        Time = c.DateTime(nullable: false),
                        SemesterId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Semesters", t => t.SemesterId)
                .Index(t => t.SemesterId);
            
            CreateTable(
                "dbo.Semesters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ExamQuestions", "QuestionId", "dbo.Questions");
            DropForeignKey("dbo.ExamQuestions", "ExamId", "dbo.Exams");
            DropForeignKey("dbo.Exams", "SemesterId", "dbo.Semesters");
            DropIndex("dbo.Exams", new[] { "SemesterId" });
            DropIndex("dbo.ExamQuestions", new[] { "QuestionId" });
            DropIndex("dbo.ExamQuestions", new[] { "ExamId" });
            DropTable("dbo.Semesters");
            DropTable("dbo.Exams");
            DropTable("dbo.ExamQuestions");
        }
    }
}
