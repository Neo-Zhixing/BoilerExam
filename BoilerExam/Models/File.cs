using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BoilerExam.Models
{
  public class File
  {
    public int Id { get; set; }

    public int QuestionId { get; set; }
    public Question Question { get; set; }

    public string Path { get; set; }
    public string MediaType { get; set; }
    public string FileName { get; set; }
  }
}
