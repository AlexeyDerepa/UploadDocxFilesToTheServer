using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UploadFilesToServer.Models;
using System.Data.Entity;

namespace UploadFilesToServer.Controllers
{
    public class HomeController : Controller
    {
        
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Statistics()
        {
            return View();
        }


        [Authorize]
        public FileResult GetFile(long Id)
        {
            UploadedFile uf;
            using (UploadContext db = new UploadContext())
            {
                uf = db.UploadedFiles.Where(x => x.Id == Id).FirstOrDefault();
            }
            return File(uf.FileContent, uf.MimeType, uf.FullFileName);
        }
        [HttpGet]
        [Authorize]
        public ActionResult UploadFiles()
        {
            return View();
        }


            object lockObject = new object();

        [HttpPost]
        [Authorize]
        public JsonResult Upload()
        {
            //Dictionary<string, byte[]> dictionary = new Dictionary<string, byte[]>();
            //System.Threading.Tasks.Parallel.ForEach(Request.Files.AllKeys, (file) =>
            //{
            //    System.Web.HttpPostedFileBase upload = Request.Files[file];
            //    //считаем загруженный файл в массив
            //    byte[] arrayBytes = null;
            //    lock (this.lockObject)
            //    {
            //        arrayBytes = new byte[upload.ContentLength];
            //        upload.InputStream.Read(arrayBytes, 0, arrayBytes.Length);
            //        dictionary.Add(upload.FileName, arrayBytes);
            //    }
            //});



            User user =new UploadContext().Users.Where(m => m.Login == HttpContext.User.Identity.Name).FirstOrDefault();
            MultithreadProcessingFiles pr = new MultithreadProcessingFiles(user);

            pr.Processing(Request.Files);

            return Json(pr.GetListUploadedFiles_());
        }
    }
}