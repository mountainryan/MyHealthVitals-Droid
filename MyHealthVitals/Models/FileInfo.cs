using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
//using Newtonsoft.Json;
using System.IO;
//using Polly;

namespace MyHealthVitals.iOS
{
    public class FileInfo
    {
		//public long? Id { get; set; }
		public string Category { get; set; }
		public byte[] Content { get; set; }
		public string Name { get; set; }
		public DateTime? ServiceDate { get; set; }
		public long Size { get; set; }
		public DateTime? UploadDate { get; set; }

		public void FileUpload(string filepath, string filename)
		{
            //var serviceUri = Credential.BASE_URL_TEST + $"Patient/{Credential.sharedInstance.Mrn}/HomeHealth/Reading";
            Category = "ECG";
            Content = File.ReadAllBytes(filepath);
            Name = filename;
            ServiceDate = Task_vars.ecgdate;
            //FileInfo Finfo = new FileInfo(filepath)
            //Size = Finfo.length;
            UploadDate = DateTime.Now;
			


            FPostAsync(Credential.BASE_URL_TEST + $"api/v1/Patient/{Credential.sharedInstance.Mrn}/File", this);
			//Id = item.Id;
			//Category = item.Category;
			//Content = item.Content;
			//Name = item.Name;
			//ServiceDate = item.ServiceDate;
			//Size = item.Size;
			//UploadDate = item.UploadDate;
		}

		
    }
}
