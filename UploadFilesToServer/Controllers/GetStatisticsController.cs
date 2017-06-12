using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using UploadFilesToServer.Models;

namespace UploadFilesToServer.Controllers
{
    public class AdminData
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string WhoDidFileUpload { get; set; }
        public string CoreCreator { get; set; }
        public int? CoreRevision { get; set; }
        public DateTimeOffset CoreCreated { get; set; }
        public DateTimeOffset CoreModified { get; set; }

    }
    public class GetStatisticsController : ApiController
    {
        // GET api/getstatistics
        [Authorize(Roles = "Admin")]
        public IEnumerable<AdminData> Get()
        {
            IEnumerable<AdminData> entries=null;
            using (UploadContext db = new UploadContext())
            {
                entries = db.UploadedFiles.Select(x => new AdminData { 
                Id = x.Id,
                FileName = x.FullFileName,
                WhoDidFileUpload = x.User.Login,
                CoreCreator = x.OldInformationFile == null ? null : x.OldInformationFile.CoreCreator,
                CoreRevision = x.OldInformationFile.CoreRevision == null ? null: x.OldInformationFile.CoreRevision,
                CoreCreated = x.OldInformationFile == null ? new DateTimeOffset() : x.OldInformationFile.CoreCreated,
                CoreModified = x.OldInformationFile == null ? new DateTimeOffset() : x.OldInformationFile.CoreModified
                }).ToList<AdminData>();
            }
            return entries;
        }
    }
}
