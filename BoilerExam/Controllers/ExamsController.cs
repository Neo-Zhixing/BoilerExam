using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using BoilerExam.Models;

namespace BoilerExam.Controllers
{
  public class ExamsController : ApiController
  {
    private ExamLibraryContext db = new ExamLibraryContext();

    // GET: api/Exams
    public IQueryable<Exam> GetExams()
    {
      return db.Exams;
    }

    // GET: api/Exams/5
    [ResponseType(typeof(Exam))]
    public async Task<IHttpActionResult> GetExam(int id)
    {
      Exam exam = await db.Exams.FindAsync(id);
      if (exam == null)
      {
        return NotFound();
      }
      await db.Entry(exam).Collection(e => e.ExamQuestions).LoadAsync();

      return Ok(exam);
    }

    // PUT: api/Exams/5
    [ResponseType(typeof(void))]
    public async Task<IHttpActionResult> PutExam(int id, Exam exam)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      if (id != exam.Id)
      {
        return BadRequest();
      }
      var oldExamQuestions = await db.ExamQuestions.Where(eq => eq.ExamId == id).ToListAsync();
      var oldExamQuestionTable = oldExamQuestions.ToDictionary(eq => eq.QuestionId, eq => eq);
      HashSet<int> examsToKeep = new HashSet<int>();
      foreach (var examQuestion in exam.ExamQuestions)
      {
        examQuestion.ExamId = id;
        if (oldExamQuestionTable.ContainsKey(examQuestion.QuestionId)) {
          var oldEQ = oldExamQuestionTable[examQuestion.QuestionId];
          oldEQ.Points = examQuestion.Points;
          oldExamQuestionTable.Remove(examQuestion.QuestionId);
        } else {
          db.Entry(examQuestion).State = EntityState.Added;
        }
      }
      var examsToRemove = oldExamQuestionTable.Values;
      db.ExamQuestions.RemoveRange(examsToRemove);

      db.Entry(exam).State = EntityState.Modified;

      try
      {
        await db.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!ExamExists(id))
        {
          return NotFound();
        }
        else
        {
          throw;
        }
      }

      return StatusCode(HttpStatusCode.NoContent);
    }

    // POST: api/Exams
    [ResponseType(typeof(Exam))]
    public async Task<IHttpActionResult> PostExam(Exam exam)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      db.Exams.Add(exam);
      await db.SaveChangesAsync();

      return CreatedAtRoute("DefaultApi", new { id = exam.Id }, exam);
    }

    // DELETE: api/Exams/5
    [ResponseType(typeof(Exam))]
    public async Task<IHttpActionResult> DeleteExam(int id)
    {
      Exam exam = await db.Exams.FindAsync(id);
      if (exam == null)
      {
        return NotFound();
      }

      db.Exams.Remove(exam);
      await db.SaveChangesAsync();

      return Ok(exam);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        db.Dispose();
      }
      base.Dispose(disposing);
    }

    private bool ExamExists(int id)
    {
      return db.Exams.Count(e => e.Id == id) > 0;
    }
  }
}
