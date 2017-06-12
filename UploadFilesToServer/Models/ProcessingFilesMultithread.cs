using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Xml;

namespace UploadFilesToServer.Models
{

    public class MultithreadProcessingFiles
    {
        UploadContext db;

        User user;
        /// <summary>
        /// It is data form user
        /// </summary>
        Dictionary<string, byte[]> dictionary;

        Dictionary<string, string> listUploadFiles = new Dictionary<string, string>();
        List<string> listOfIllegalFiles = new List<string>();
        public List<string> listOfDamagedFiles = new List<string>();

        object lockObject = new object();
        public MultithreadProcessingFiles(User user)
        {
            this.db = new UploadContext();
            this.user = user;
            this.dictionary = new Dictionary<string, byte[]>();
        }
        public void Processing(Dictionary<string, byte[]> dictionary)
        {
            this.dictionary = dictionary;
            ProcessingFiles();
        }
        public void Processing(System.Web.HttpFileCollectionBase collection)
        {
            System.Threading.Tasks.Parallel.ForEach(collection.AllKeys, (file) =>
            {
                System.Web.HttpPostedFileBase upload = collection[file];
                lock (this.lockObject)
                {
                    byte[] arrayBytes = new byte[upload.ContentLength];
                    upload.InputStream.Read(arrayBytes, 0, arrayBytes.Length);
                    this.dictionary.Add(upload.FileName, arrayBytes);
                }
            });

            ProcessingFiles();
        }
        public void ProcessingFiles()
        {

            System.Threading.Tasks.Parallel.ForEach(this.dictionary, (file) =>
            {
                bool flag = true;

                UploadedFile uploadedFile = new UploadedFile(file.Key, file.Value, user);
                if (uploadedFile.MimeType != "application/octet-stream" && uploadedFile.FileExtension == ".docx")
                {
                    flag = ReadAndRewriteXmlData(uploadedFile);
                }

                if (uploadedFile.MimeType != "application/octet-stream" && flag == true)
                    lock (this.lockObject)
                    {
                        db.UploadedFiles.Add(uploadedFile);
                        db.SaveChanges();
                        OldInformationFile oif = new OldInformationFile(uploadedFile);
                        db.OldInformationFile.Add(oif);

                        listUploadFiles.Add(uploadedFile.Id.ToString(), uploadedFile.FullFileName);
                    }

                if (uploadedFile.MimeType != "application/octet-stream" && flag == false && uploadedFile.FileExtension == ".docx")
                {
                    lock (this.lockObject)
                        listOfDamagedFiles.Add(uploadedFile.FullFileName);
                }
                if (uploadedFile.MimeType == "application/octet-stream" && uploadedFile.FileExtension != ".docx" && uploadedFile.FileExtension != ".doc")
                {
                    lock (this.lockObject)
                        listOfIllegalFiles.Add(uploadedFile.FullFileName);
                }
            });

            db.SaveChanges();
        }

        private bool ReadAndRewriteXmlData(UploadedFile uploadedFile)
        {
            XmlDocument doc = new XmlDocument();

            try
            {
                using (MemoryStream zipToOpen = new MemoryStream(uploadedFile.FileContent))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                    {
                        //////////////////////////app.xml////////////////////////////////////////
                        //берем конкретный файл который нужно считать
                        ZipArchiveEntry readmeEntry = archive.GetEntry("docProps/app.xml");

                        //считвываем поток из конкретного файла
                        System.IO.Stream entryStream = readmeEntry.Open();
                        doc = new XmlDocument();
                        //передаем паток для обработки xml файлов
                        doc.Load(entryStream);

                        // получим корневой элемент
                        System.Xml.XmlElement root = doc.DocumentElement;

                        // обход всех узлов в корневом элементе до изменения данных
                        foreach (System.Xml.XmlNode childnode in root.ChildNodes)
                        {
                            uploadedFile.OldXmlData.Add(childnode.Name, childnode.InnerText);
                        }
                        /////////////////////////////core.xml/////////////////////////////////////

                        //берем конкретный файл который нужно считать и модефицировать
                        readmeEntry = archive.GetEntry("docProps/core.xml");

                        //считвываем поток из конкретного файла
                        entryStream = readmeEntry.Open();
                        //передаем паток для обработки xml файлов
                        doc.Load(entryStream);

                        //создаем менеджер пространств имен и описываем его
                        XmlNamespaceManager xs = new XmlNamespaceManager(doc.NameTable);
                        xs.AddNamespace("cp", "http://schemas.openxmlformats.org/package/2006/metadata/core-properties");
                        xs.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
                        xs.AddNamespace("dcterms", "http://purl.org/dc/terms/");
                        xs.AddNamespace("dcmitype", "http://purl.org/dc/dcmitype/");
                        xs.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");

                        // получим корневой элемент
                        root = doc.DocumentElement;

                        // обход всех узлов в корневом элементе до изменения данных
                        foreach (System.Xml.XmlNode childnode in root.ChildNodes)
                        {
                            uploadedFile.OldXmlData.Add(childnode.Name, childnode.InnerText);
                        }

                        //////////////////////МОДЕФИЦИРУЕМ УЗЛЫ///////////////////
                        //the tag - dc:title
                        XmlNode myNode = root.SelectSingleNode("dc:title", xs);
                        if (myNode != null)
                            myNode.InnerText = "The best title";

                        //the tag - dc:subject
                        myNode = root.SelectSingleNode("dc:subject", xs);
                        if (myNode != null)
                            myNode.InnerText = "Some a new subject of the article";

                        //the tag - dc:creator
                        myNode = root.SelectSingleNode("dc:creator", xs);
                        if (myNode != null)
                            myNode.InnerText = "The king of rings";

                        //the tag - cp:keywords
                        myNode = root.SelectSingleNode("cp:keywords", xs);
                        if (myNode != null)
                            myNode.InnerText = "blablabla";

                        //the tag - dc:description
                        myNode = root.SelectSingleNode("dc:description", xs);
                        if (myNode != null)
                            myNode.InnerText = "A new description this document";

                        //the tag - cp:lastModifiedBy
                        myNode = root.SelectSingleNode("cp:lastModifiedBy", xs);
                        if (myNode != null)
                            myNode.InnerText = "It is Alex!!!!!!!!!!!!!!!!";

                        //the tag - cp:revision
                        myNode = root.SelectSingleNode("cp:revision", xs);
                        if (myNode != null)
                            myNode.InnerText = new Random().Next(1, 1000).ToString();

                        //the tag - dcterms:created
                        myNode = root.SelectSingleNode("dcterms:created", xs);
                        DateTimeOffset dateTimeOffset;
                        if (myNode != null)
                        {
                            dateTimeOffset = DateTimeOffset.ParseExact(myNode.InnerText.ToString(), "yyyy-MM-dd'T'HH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
                            myNode.InnerText = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssZ");
                        }

                        //the tag - dcterms:modified
                        myNode = root.SelectSingleNode("dcterms:modified", xs);
                        if (myNode != null)
                        {
                            dateTimeOffset = DateTimeOffset.ParseExact(myNode.InnerText.ToString(), "yyyy-MM-dd'T'HH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
                            myNode.InnerText = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssZ");
                        }
                    }
                }

                using (MemoryStream zipStream = new MemoryStream())
                {
                    //считали данные в поток
                    zipStream.Write(uploadedFile.FileContent, 0, uploadedFile.FileContent.Length);

                    using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Update))
                    {
                        archive.GetEntry("docProps/core.xml").Delete();

                        ZipArchiveEntry readmeEntry = archive.CreateEntry("docProps/core.xml");

                        string strFromDoc = doc.InnerXml.ToString();
                        Console.WriteLine(strFromDoc);
                        using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                        {
                            writer.WriteLine(strFromDoc);
                        }

                    }
                    uploadedFile.FileContent = zipStream.ToArray();
                }

            }
            catch (InvalidDataException ex)
            {
                return false;
            } 
            
            return true;
        }

        public DC GetListUploadedFiles_()
        {
            List<string[]> arrayForJSON = new List<string[]>(listUploadFiles.Count);
            foreach (var item in listUploadFiles)
            {
                string[] m = { item.Key.ToString(), item.Value.ToString() };
                arrayForJSON.Add(m);
            }
            DC dc = new DC { listOfIllegalFiles = listOfIllegalFiles, listUploadFiles = arrayForJSON , listOfDamagedFiles = listOfDamagedFiles};
            return dc;

        }

    }
    public class DC
    {
        public List<string[]> listUploadFiles { get; set; }
        public List<string> listOfIllegalFiles { get; set; }
        public List<string> listOfDamagedFiles { get; set; }
    }
}