using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations.Schema;
using BoilerExam.Models;
using System.ComponentModel.DataAnnotations;

using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace BoilerExam.Models
{
  public class ExamLibraryContext : DbContext
  {
    public ExamLibraryContext() : base(Environment.GetEnvironmentVariable("TIER") ?? "test")
    {
    }

    public DbSet<Question> Questions { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<QuestionTag> QuestionTag { get; set; }

    public DbSet<Exam> Exams { get; set; }
    public DbSet<ExamQuestion> ExamQuestions { get; set; }

    public DbSet<File> Files { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
    }
  }
}

