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
using System.Configuration;
using BoilerExam.Models;
using System.Net.Http.Headers;

namespace BoilerExam.Controllers
{
  public class FilesController : ApiController
  {
    private ExamLibraryContext db = new ExamLibraryContext();

    // GET: api/Files/5
    [Route("api/Files/{id}")]
    public async Task<IHttpActionResult> GetFile(int id)
    {
      var file = await this.db.Files.FindAsync(id);
      if (file == null) {
        return NotFound();
      }
      var path = ConfigurationManager.AppSettings["fileStorage"] + "\\storage\\" + file.Path;
      if (!System.IO.File.Exists(path)) {
        return NotFound();
      }

      var stream = new System.IO.FileStream(path, System.IO.FileMode.Open);

      var result = new HttpResponseMessage(HttpStatusCode.OK)
      {
        Content = new StreamContent(stream)
      };
      result.Content.Headers.ContentType = new MediaTypeHeaderValue(file.MediaType);
      result.Content.Headers.ContentLength = stream.Length;
      result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
      result.Content.Headers.ContentDisposition.FileName = file.FileName;
      return this.ResponseMessage(result);
    }

    [Route("api/Files/{id}/meta")]
    public async Task<IHttpActionResult> GetFileMeta(int id)
    {
      var file = await this.db.Files.FindAsync(id);
      if (file == null)
      {
        return NotFound();
      }
      await db.Entry(file).Reference(f => f.Question).LoadAsync();
      return Ok(file);
    }

    // PUT: api/Files/5
    [Route("api/Files/{id}")]
    public async Task<IHttpActionResult> PutFile(int id)
    {
      var file = await this.db.Files.FindAsync(id);
      if (file == null)
      {
        return NotFound();
      }
      var newfile = await this.handleFileUpload(file.QuestionId);

      var path = ConfigurationManager.AppSettings["fileStorage"] + "\\storage\\" + file.Path;
      System.IO.File.Delete(path);
      file.Path = newfile.Path;
      file.MediaType = newfile.MediaType;
      file.FileName = newfile.FileName;
      await this.db.SaveChangesAsync();


      return StatusCode(HttpStatusCode.NoContent);
    }

    [Route("api/Questions/{questionId}/Attachments")]
    [HttpGet]
    public IHttpActionResult ListFile(int questionId)
    {
      if (!this.db.Questions.Any(o => o.Id == questionId))
      {
        return NotFound();
      }
      var files = this.db.Files.Where(f => f.QuestionId == questionId);
      return Ok(files);
    }

    // POST: api/Files
    [Route("api/Questions/{questionId}/Attachments")]
    [HttpPost]
    public async Task<IHttpActionResult> UploadFile(int questionId)
    {
      if (!this.db.Questions.Any(o => o.Id == questionId))
      {
        return NotFound();
      }
      var file = await this.handleFileUpload(questionId);
      this.db.Files.Add(file);
      await this.db.SaveChangesAsync();

      return Created("/files/{id}", file);
    }

    async Task<File> handleFileUpload(int questionId) {
      var rootPath = ConfigurationManager.AppSettings["fileStorage"];
      var tempath = rootPath + "\\temp";
      if (!System.IO.Directory.Exists(tempath))
      {
        System.IO.Directory.CreateDirectory(tempath);
      }
      var provider = new MultipartFileStreamProvider(tempath);
      if (!Request.Content.IsMimeMultipartContent())
      {
        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Content MIME is not multipart"));
      }
      var result = await Request.Content.ReadAsMultipartAsync(provider);


      var tempFile = result.FileData.First();
      var tempFilePath = tempFile.LocalFileName;

      var filename = this.generateRandomString();
      var storagePath = rootPath + "\\storage\\";
      var absolutePath = storagePath + filename;
      if (!System.IO.Directory.Exists(storagePath))
      {
        System.IO.Directory.CreateDirectory(storagePath);
      }
      System.IO.File.Move(tempFilePath, absolutePath);

      return new File
      {
        QuestionId = questionId,
        Path = filename,
        MediaType = tempFile.Headers.ContentType.MediaType,
        FileName = tempFile.Headers.ContentDisposition.FileName
      };
    }

    // DELETE: api/Files/5
    [Route("api/Files/{id}")]
    public async Task<IHttpActionResult> DeleteFile(int id)
    {
      File file = await db.Files.FindAsync(id);
      if (file == null)
      {
        return NotFound();
      }

      var path = ConfigurationManager.AppSettings["fileStorage"] + "\\storage\\" + file.Path;
      System.IO.File.Delete(path);

      db.Files.Remove(file);
      await db.SaveChangesAsync();

      return Ok(file);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        db.Dispose();
      }
      base.Dispose(disposing);
    }

    private bool FileExists(int id)
    {
      return db.Files.Count(e => e.Id == id) > 0;
    }

    private String generateRandomString() {
      Guid guid = Guid.NewGuid();
      return guid.ToString();
    }
  }
}
