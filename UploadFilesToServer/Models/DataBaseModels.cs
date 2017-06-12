using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace UploadFilesToServer.Models
{
    //public class UploadContextInitializer : CreateDatabaseIfNotExists<UploadContext>
    //public class UploadContextInitializer : DropCreateDatabaseAlways<UploadContext>
    public class UploadContextInitializer : DropCreateDatabaseIfModelChanges<UploadContext>
    {
        protected override void Seed(UploadContext db)
        {
            Role role1 = new Role { Name = "Admin" };
            Role role2 = new Role { Name = "User" };

            db.Roles.Add(role1);
            db.Roles.Add(role2);

            db.SaveChanges();

            User user1 = new User { Login = "admin", Password = "admin", Role = role1, RoleId = role1.Id };
            User user2 = new User { Login = "user", Password = "user", Role = role2, RoleId = role2.Id };

            db.Users.Add(user1);
            db.Users.Add(user2);

            db.SaveChanges();
        }
    }
    public class UploadContext : DbContext
    {
        static UploadContext()
        {
            Database.SetInitializer<UploadContext>(new UploadContextInitializer());
        }
        public UploadContext()
            : base("UFiles") { }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UploadedFile> UploadedFiles { get; set; }
        public DbSet<OldInformationFile> OldInformationFile { get; set; }
    }
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class User
    {
        // ID 
        public int Id { get; set; }
        // Логин
        [Required]
        [Display(Name = "Логин")]
        [MaxLength(50, ErrorMessage = "Превышена максимальная длина записи")]
        public string Login { get; set; }
        // Пароль
        [Required]
        [Display(Name = "Пароль")]
        [MaxLength(50, ErrorMessage = "Превышена максимальная длина записи")]
        public string Password { get; set; }
        // Статус
        [Required]
        [Display(Name = "Статус")]
        public int RoleId { get; set; }
        public Role Role { get; set; }
        public ICollection<UploadedFile> UploadedFiles { get; set; }


        public User()
        {
            UploadedFiles = new List<UploadedFile>();
        }
    }
    public class UploadedFile
    {
        public int Id { get; set; }

        //content-type
        public string MimeType { get; set; }
        public string FullFileName { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public byte[] FileContent { get; set; }


        public int? UserId { get; set; }
        public User User { get; set; }

        public virtual OldInformationFile OldInformationFile { get; set; }


        public Dictionary<string, string> OldXmlData { get; set; }


        public UploadedFile(){}
        //public UploadedFile(System.Web.HttpPostedFileBase upload, string[] info, byte[] processedFile, User user)
        //{
        //    this.FullFileName = System.IO.Path.GetFileName(upload.FileName);
        //    this.FileExtension = info[1];
        //    this.FileName = System.IO.Path.GetFileNameWithoutExtension(upload.FileName);
        //    this.FileType = info[0];
        //    this.FileContent = processedFile;
        //    this.User = user;
        //    this.UserId = user.Id;
        //}
        //public UploadedFile(string[] fileInfo, byte[] processedFile, User user)
        //{
        //    this.FullFileName = fileInfo[2] + fileInfo[1];
        //    this.FileExtension = fileInfo[1];
        //    this.FileName = fileInfo[2];
        //    this.FileType = fileInfo[0];
        //    this.FileContent = processedFile;
        //    this.User = user;
        //    this.UserId = user.Id;
        //}
        public UploadedFile(string fileName, byte[] arrayBytes, User user)
        {
            this.OldXmlData = new Dictionary<string, string>();
            this.User = user;
            this.UserId = user.Id;

            //DEFAULT UNKNOWN MIME TYPE
            this.MimeType = "application/octet-stream";
            this.FileName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            this.FileExtension = System.IO.Path.GetExtension(fileName);
            this.FileContent = arrayBytes;

            byte[] DOC = { 208, 207, 17, 224, 161, 177, 26, 225, 0, 0, 0, 0, 0, 0 };
            byte[] DOCX = { 80, 75, 3, 4, 20, 0, 6, 0, 8, 0, 0, 0, 33, 0 };
            //Get the MIME Type and Extension
            if (this.FileContent.Take(14).SequenceEqual(DOC))
            {
                this.MimeType = "application/msword";
                this.FileExtension = ".doc";
            }
            else if (this.FileContent.Take(14).SequenceEqual(DOCX))
            {
                this.MimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                this.FileExtension = ".docx";
            }

            this.FullFileName = this.FileName + this.FileExtension;
        }


    }
    public class OldInformationFile
    {
        [Key]
        [ForeignKey("UploadedFile")]
        public int Id { get; set; }
        public virtual UploadedFile UploadedFile { get; set; }


        public string CoreTitle { get; set; }
        public string CoreSubject { get; set; }
        public string CoreCreator { get; set; }
        public string CoreKeywords { get; set; }
        public string CoreDescription { get; set; }
        public string CoreLastModifiedBy { get; set; }
        public int? CoreRevision { get; set; }
        public DateTimeOffset CoreCreated { get; set; }
        public DateTimeOffset CoreModified { get; set; }
        public string AppTemplate { get; set; }
        public int? AppTotalTime { get; set; }
        public int? AppPages { get; set; }
        public int? AppWords { get; set; }
        public int? AppCharacters { get; set; }
        public string AppApplication { get; set; }
        public int? AppDocSecurity { get; set; }
        public int? AppLines { get; set; }
        public int? AppParagraphs { get; set; }
        public bool AppScaleCrop { get; set; }
        public string AppCompany { get; set; }
        public bool AppLinksUpToDate { get; set; }
        public int? AppCharactersWithSpaces { get; set; }
        public bool AppSharedDoc { get; set; }
        public bool AppHyperlinksChanged { get; set; }
        public float? AppAppVersion { get; set; }
        public OldInformationFile()
        {

        }
        public OldInformationFile(Dictionary<string, string> information, UploadedFile uf = null)
        {
            this.UploadedFile = uf;

            InitializationXmlData(information);
        }
        public OldInformationFile(UploadedFile uf)
        {
            this.UploadedFile = uf;

            InitializationXmlData(uf.OldXmlData);
        }

        private void InitializationXmlData(Dictionary<string, string> information)
        {
            if (information.ContainsKey("dc:title"))
                this.CoreTitle = information["dc:title"];
            if (information.ContainsKey("dc:subject"))
                this.CoreSubject = information["dc:subject"];
            if (information.ContainsKey("dc:creator"))
                this.CoreCreator = information["dc:creator"];
            if (information.ContainsKey("cp:keywords"))
                this.CoreKeywords = information["cp:keywords"];
            if (information.ContainsKey("dc:description"))
                this.CoreDescription = information["dc:description"];
            if (information.ContainsKey("cp:lastModifiedBy"))
                this.CoreLastModifiedBy = information["cp:lastModifiedBy"];
            if (information.ContainsKey("cp:revision"))
                this.CoreRevision = int.Parse(information["cp:revision"]);
            if (information.ContainsKey("dcterms:created"))
                this.CoreCreated = DateTimeOffset.ParseExact(information["dcterms:created"].ToString(), "yyyy-MM-dd'T'HH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
            if (information.ContainsKey("dcterms:modified"))
                this.CoreModified = DateTimeOffset.ParseExact(information["dcterms:modified"].ToString(), "yyyy-MM-dd'T'HH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);

            if (information.ContainsKey("Template"))
                this.AppTemplate = information["Template"];
            if (information.ContainsKey("TotalTime"))
                this.AppTotalTime = int.Parse(information["TotalTime"]);
            if (information.ContainsKey("Pages"))
                this.AppPages = int.Parse(information["Pages"]);
            if (information.ContainsKey("Words"))
                this.AppWords = int.Parse(information["Words"]);
            if (information.ContainsKey("Characters"))
                this.AppCharacters = int.Parse(information["Characters"]);
            if (information.ContainsKey("Application"))
                this.AppApplication = information["Application"];
            if (information.ContainsKey("DocSecurity"))
                this.AppDocSecurity = int.Parse(information["DocSecurity"]);
            if (information.ContainsKey("Lines"))
                this.AppLines = int.Parse(information["Lines"]);
            if (information.ContainsKey("Paragraphs"))
                this.AppParagraphs = int.Parse(information["Paragraphs"]);
            if (information.ContainsKey("ScaleCrop"))
                this.AppScaleCrop = bool.Parse(information["ScaleCrop"]);
            if (information.ContainsKey("Company"))
                this.AppCompany = information["Company"];
            if (information.ContainsKey("LinksUpToDate"))
                this.AppLinksUpToDate = bool.Parse(information["LinksUpToDate"]);
            if (information.ContainsKey("CharactersWithSpaces"))
                this.AppCharactersWithSpaces = int.Parse(information["CharactersWithSpaces"]);
            if (information.ContainsKey("SharedDoc"))
                this.AppSharedDoc = bool.Parse(information["SharedDoc"]);
            if (information.ContainsKey("HyperlinksChanged"))
                this.AppHyperlinksChanged = bool.Parse(information["HyperlinksChanged"]);
            if (information.ContainsKey("AppVersion"))
                this.AppAppVersion = float.Parse(information["AppVersion"], System.Globalization.CultureInfo.InvariantCulture);
        }

    }
}