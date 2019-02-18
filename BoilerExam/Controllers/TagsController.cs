using System;
using System.Collections.Generic;
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
    public class TagsController : ApiController
    {
        private ExamLibraryContext db = new ExamLibraryContext();

        // GET: api/Tags
        async public Task<IHttpActionResult> GetTags([FromUri] string search = null)
        {
            if (search == null)
            {
                var result = await db.Tags.ToListAsync();
                return Ok(result);
            }
            else
            {
                var result = await db.Tags
                    .Where(x => x.Title.Contains(search))
                    .ToListAsync();
                return Ok(result);
            }
        }

        // GET: api/Tags/5
        public class TagDetailModel
        {
            public int Id;
            public string Title;
            public IEnumerable<Question> Questions;
        }

        // GET: api/Tags/5
        [ResponseType(typeof(Tag))]
        public async Task<IHttpActionResult> GetTag(int id)
        {
            var tag = await db.Tags.FindAsync(id);

            if (tag == null)
            {
                return NotFound();
            }
            await db.Entry(tag)
                .Collection(t => t.QuestionTags)
                .Query()
                .Include(qt => qt.Question)
                .LoadAsync();

            return Ok(new TagDetailModel
            {
              Id = tag.Id,
              Title = tag.Title,
              Questions = tag.QuestionTags.Select(qt => qt.Question)
            });
        }

        // PUT: api/Tags/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutTag(int id, Tag tag)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (tag.Id != 0 && id != tag.Id)
            {
                return BadRequest();
            }
            tag.Id = id;

            db.Entry(tag).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TagExists(id))
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

        // POST: api/Tags
        [ResponseType(typeof(Tag))]
        public async Task<IHttpActionResult> PostTag(Tag tag)
        {
            if (!ModelState.IsValid)
                  {
                      return BadRequest(ModelState);
                  }

            tag.Id = 0;
            db.Tags.Add(tag);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = tag.Id }, tag);
        }

        // DELETE: api/Tags/5
        [ResponseType(typeof(Tag))]
        public async Task<IHttpActionResult> DeleteTag(int id)
        {
            Tag tag = await db.Tags.FindAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            db.Tags.Remove(tag);
            await db.SaveChangesAsync();

            return Ok(tag);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TagExists(int id)
        {
            return db.Tags.Count(e => e.Id == id) > 0;
        }
    }
}
