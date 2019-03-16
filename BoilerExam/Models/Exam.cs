using System;

namespace BoilerExam.Models
{
  public class Exam
  {
    public int Id { get; set; }
    public String Title { get; set; }
    public String Description { get; set; }

    public DateTime Time { get; set; }

    public int? SemesterId { get; set; }
    public virtual Semester Semester { get; set; }
  }

  public class ExamQuestion
  {
    public int Id { get; set; }

    public int ExamId { get; set; }
    public virtual Exam Exam { get; set; }

    public int QuestionId { get; set; }
    public virtual Question Question { get; set; }
  }

  public class Semester
  {
    public int Id { get; set; }

    public int Title { get; set; }
  }
}
