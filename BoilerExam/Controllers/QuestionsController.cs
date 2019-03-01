using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using BoilerExam.Models;

namespace BoilerExam.Controllers
{
  public class QuestionsController : ApiController
  {
    private ExamLibraryContext db = new ExamLibraryContext();

    // GET: api/Questions
    public async Task<IHttpActionResult> GetQuestions(
        [FromUri]int? page = null,
        [FromUri] int? pageSize = null,
        [FromUri] string tags = null
      )
    {

      IQueryable<Question> query = db.Questions
          .Include(question => question.QuestionTags.Select(qt => qt.Tag));
      if (tags != null)
      {
        var tagIds = tags
            .Split(',')
            .Select(t =>
            {
              int theOutput;
              if (Int32.TryParse(t, out theOutput))
                return (int?)theOutput;
              else
                return null;
            });
        query = query
            .Where(question => (db.QuestionTag
                .Where(qt => tagIds.Contains(qt.TagId))
                .GroupBy(qt => qt.QuestionId)
                .Where(group => group.Count() == tagIds.Count())
                .Select(group => group.Key)
                .Contains(question.Id)));
      }
      if (page is int pageNum)
      {
        var totalEntries = await query.CountAsync();
        var size = pageSize ?? 20;
        var totalPages = (int)Math.Ceiling(totalEntries / (float)size);
        //Response.Headers.Add("X-Total-Page-Count", totalPages.ToString());
        //Response.Headers.Add("X-Total-Count", totalEntries.ToString());
        if (totalEntries == 0)
          return Ok(new List<Question>());
        if (pageNum <= 0 || pageNum > totalPages)
          return BadRequest();
        query = query
            .OrderBy(a => a.Id)
            .Skip((pageNum - 1) * size)
            .Take(size);
      }
      var theList = await query.ToListAsync();
      return Ok(theList);
    }

    // GET: api/Questions/5
    [ResponseType(typeof(Question))]
    public async Task<IHttpActionResult> GetQuestion(int id)
    {
      var question = await db.Questions.FindAsync(id);

      if (question == null)
      {
        return NotFound();
      }

      await db.Entry(question)
          .Collection(q => q.QuestionTags)
          .Query()
          .Include(qc => qc.Tag)
          .LoadAsync();
      return Ok(question);
    }

    public class QuestionViewModel
    {
      public int? Id;
      public string Content;
      public IEnumerable<int> Tags;
      public ICollection<string> Options;
      public ICollection<IEnumerable<int>> Combinations;
      public int? Answer;
    }

    // PUT: api/Questions/5
    [ResponseType(typeof(void))]
    public async Task<IHttpActionResult> PutQuestion(int id, QuestionViewModel questionModel)
    {
      if (questionModel.Id != null && id != questionModel.Id)
      {
        return BadRequest();
      }
      var question = new Question
      {
        Id = id,
        Content = questionModel.Content ?? "",
        Answer = questionModel.Answer ?? 0,
        Combinations = questionModel.Combinations,
        Options = questionModel.Options
      };
      if (!question.Verify())
      {
        return BadRequest();
      }

      db.Entry(question).State = EntityState.Modified;

      await db.Entry(question)
          .Collection(q => q.QuestionTags)
          .LoadAsync();
      DiffTags(question.QuestionTags, questionModel.Tags);
      try
      {
        await db.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!QuestionExists(id))
        {
          return NotFound();
        }
        else if (!this.AllTagsExist(questionModel.Tags))
        {
          return BadRequest();
        }
        else
        {
          throw;
        }
      }
      return StatusCode(HttpStatusCode.NoContent);
    }

    // POST: api/Questions
    [ResponseType(typeof(Question))]
    public async Task<IHttpActionResult> PostQuestion([FromBody]QuestionViewModel vm)
    {
      System.Diagnostics.Debug.WriteLine(vm.Content);
      var question = CreateQuestionFromViewModel(vm);
      if (!question.Verify())
      {
        return BadRequest();
      }
      db.Questions.Add(question);
      try
      {
        await db.SaveChangesAsync();
      }
      catch (DbEntityValidationException ex)
      {
        if (!this.AllTagsExist(vm.Tags))
        {
          return BadRequest();
        }
        else
        {
          var errorMessages = ex.EntityValidationErrors
            .SelectMany(x => x.ValidationErrors)
            .Select(x => x.ErrorMessage);

          // Join the list to a single string.
          var fullErrorMessage = string.Join("; ", errorMessages);

          // Combine the original exception message with the new one.
          var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);
          throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
        }
      }
      await db.Entry(question)
          .Collection(q => q.QuestionTags)
          .Query()
          .Include(qc => qc.Tag)
          .LoadAsync();
      return CreatedAtRoute("DefaultApi", new { id = question.Id }, question);
    }

    // DELETE: api/Questions/5
    [ResponseType(typeof(Question))]
    public async Task<IHttpActionResult> DeleteQuestion(int id)
    {
      Question question = await db.Questions.FindAsync(id);
      if (question == null)
      {
        return NotFound();
      }

      db.Questions.Remove(question);
      await db.SaveChangesAsync();

      return Ok(question);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        db.Dispose();
      }
      base.Dispose(disposing);
    }

    private bool QuestionExists(int id)
    {
      return db.Questions.Count(e => e.Id == id) > 0;
    }

    private void DiffTags(ICollection<QuestionTag> qts, IEnumerable<int> updatedTagIdsE)
    {

      var existingTagIds = new HashSet<int>(qts.Select(q => q.TagId));
      var updatedTagIds = updatedTagIdsE == null ?
          new HashSet<int>() :
          new HashSet<int>(updatedTagIdsE);
      var tagIdsToBeAdded = updatedTagIds.Except(existingTagIds); // Tags to be Added
      var tagIdsToBeRemoved = existingTagIds.Except(updatedTagIds);

      foreach (QuestionTag questionTag in qts)
      {
        if (tagIdsToBeRemoved.Contains(questionTag.TagId))
        {
          db.Entry(questionTag).State = EntityState.Deleted;
        }
      }

      foreach (int tagId in tagIdsToBeAdded)
      {
        qts.Add(new QuestionTag
        {
          TagId = tagId
        });
      }
    }

    private bool AllTagsExist(IEnumerable<int> tagIds)
    {
      if (tagIds == null) return true;
      return tagIds
          .All(tagId => db.Tags.Any(tag => tag.Id == tagId));
    }
    private Question CreateQuestionFromViewModel(QuestionViewModel vm)
    {
      return new Question
      {
        Content = vm.Content ?? "",
        QuestionTags = vm.Tags == null ? null : vm.Tags
                .Select(tagId => new QuestionTag()
                {
                  TagId = tagId
                })
                .ToList(),
        Answer = vm.Answer ?? 0,
        Combinations = vm.Combinations,
        Options = vm.Options
      };
    }
  }
}
