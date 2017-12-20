using System.Threading.Tasks;
using System;
using System.Net;
using System.Net.Http;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace MyHealthVitals
{
    public class Reading
    {
        public long Id { get; set; }
        public bool Abnormal { get; set; }
        public long CategoryId { get; set; }
        public DateTime Date { get; set; }
        public Guid? DeviceId { get; set; }
        public string Source { get; set; }
        public decimal? EnglishValue { get; set; }
        public decimal? MetricValue { get; set; }
        public string ValueType { get; set; }
        //added 2 new fields
        public string Narrative { get; set; }
        public long FileId { get; set; }
        //public string Status { get; set; }


        public Reading(String valueType, decimal englishVal, long catId, bool Abn, string Narr, long? FileId, DateTime? date = null, long? id = null)
        {

            this.ValueType = valueType;
            this.CategoryId = catId;
            this.EnglishValue = englishVal;

            // this is same for all reading
            this.Source = "Device";

            if (date != null)
            {
                this.Date = (DateTime)date;
            }
            else
            {
                this.Date = DateTime.Now;
            }

            if (id != null)
            {
                this.Id = Convert.ToInt64(id);
            }

            //if (status != null)
            //{
            //    this.Status = status;
            // }

            //for ecg readings
            this.Abnormal = Abn;
            this.Narrative = Narr;
            if (FileId != null)
            {
                this.FileId = Convert.ToInt64(FileId);
            }
        }

        public async Task<bool> UpdateReadingToService()
        {
            //Debug.WriteLine("UpdateReadingToService");
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Credential.sharedInstance.Token}");

            // converting the this reading into string to send it to the service as application/json
            var content = new StringContent(JsonConvert.SerializeObject(this), Encoding.UTF8, "application/json");

            var serviceUri = Credential.BASE_URL + $"Patient/{Credential.sharedInstance.Mrn}/HomeHealth/Reading/{Id}";

            try
            {
				var response = await client.PutAsync(serviceUri, content);

				if (response.ReasonPhrase == "OK")
				{
					//Debug.WriteLine("successfully updated record.");
					return true;
				}
				else
				{
					//Debug.WriteLine("unsuccessful record update.");
                    await DependencyService.Get<IFileHelper>().offlineSave(this, "put");
					return false;
				}
            }
            catch (Exception ex)
            {
                await DependencyService.Get<IFileHelper>().offlineSave(this, "put");
                return false;
            }

        }

        public async Task<Reading> PostReadingToService()
        {
            //Debug.WriteLine("PostReadingToService");
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Credential.sharedInstance.Token}");

            // converting the this reading into string to send it to the service as application/json
            var content = new StringContent(JsonConvert.SerializeObject(this), Encoding.UTF8, "application/json");

            var serviceUri = Credential.BASE_URL + $"Patient/{Credential.sharedInstance.Mrn}/HomeHealth/Reading";

			//testing***********************************************************
			//var val = await DependencyService.Get<IFileHelper>().offlineSave(this, "post");
            //return null;                                                   
			//testing***********************************************************

			try
            {
				var response = await client.PostAsync(serviceUri, content);

				if (response.ReasonPhrase == "OK")
				{
					var json = await response.Content.ReadAsStringAsync();
					var obj = JsonConvert.DeserializeObject<Reading>(json);
					return obj;
				}
				else
				{
					await DependencyService.Get<IFileHelper>().offlineSave(this, "post");
					return null;
				}
            }
            catch (Exception ex)
            {
                await DependencyService.Get<IFileHelper>().offlineSave(this, "post");
                return null;
            }
        }

        public static async Task<Reading[]> GetCategoryReadingsFromService(string catid)
        {
            HttpClient client = new HttpClient();
            //client.MaxResponseContentBufferSize = 256000;
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Credential.sharedInstance.Token}");
            var serviceUri = Credential.BASE_URL + $"Patient/{Credential.sharedInstance.Mrn}/HomeHealth/Reading/Category/{catid}";

           // Debug.WriteLine("serviceUri = " + serviceUri);

            try
            {
                var response = await client.GetAsync(serviceUri);
                //Debug.WriteLine("GetAllReadingsFromService response.IsSuccessStatusCode= " + response.IsSuccessStatusCode);
                if (response.IsSuccessStatusCode)
                {
                    //Debug.WriteLine("Got readings successfully.");
                    var content = await response.Content.ReadAsStringAsync();
                    //Debug.WriteLine(JsonConvert.DeserializeObject<Reading[]>(content));
                    return JsonConvert.DeserializeObject<Reading[]>(content);
                }
                else
                {
                    //Debug.WriteLine("Failed to get readings.");
                    return null; ;
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine("parse error: " + ex.Message);
                return null;
            }
        }

        public static async Task<Reading[]> GetAllReadingsFromService()
        {

            HttpClient client = new HttpClient();
            //client.MaxResponseContentBufferSize = 256000;
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Credential.sharedInstance.Token}");
            var serviceUri = Credential.BASE_URL + $"Patient/{Credential.sharedInstance.Mrn}/HomeHealth/Reading";

            //Debug.WriteLine("serviceUri = " + serviceUri);

            try
            {
                var response = await client.GetAsync(serviceUri);
                //Debug.WriteLine("GetAllReadingsFromService response.IsSuccessStatusCode= " + response.IsSuccessStatusCode);
                if (response.IsSuccessStatusCode)
                {
                    //Debug.WriteLine("Got readings successfully.");
                    var content = await response.Content.ReadAsStringAsync();
                    //Debug.WriteLine(JsonConvert.DeserializeObject<Reading[]>(content));
                    return JsonConvert.DeserializeObject<Reading[]>(content);
                }
                else
                {
                    //Debug.WriteLine("Failed to get readings.");
                    return null; ;
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine("parse error: " + ex.Message);
                return null;
            }
        }

        public static async Task<Reading> GetSingleReadingFromService(long readId)
        {

            HttpClient client = new HttpClient();
            //client.MaxResponseContentBufferSize = 256000;
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Credential.sharedInstance.Token}");
            var serviceUri = Credential.BASE_URL + $"Patient/{Credential.sharedInstance.Mrn}/HomeHealth/Reading/{readId}";

            //Debug.WriteLine("serviceUri = " + serviceUri);

            try
            {
                var response = await client.GetAsync(serviceUri);
                //Debug.WriteLine("GetSingleReadingFromService response.IsSuccessStatusCode= " + response.IsSuccessStatusCode);
                if (response.IsSuccessStatusCode)
                {
                    //Debug.WriteLine("Got reading successfully.");
                    var content = await response.Content.ReadAsStringAsync();
                    //Debug.WriteLine(JsonConvert.DeserializeObject<Reading>(content));
                    return JsonConvert.DeserializeObject<Reading>(content);
                }
                else
                {
                    //Debug.WriteLine("Failed to get readings.");
                    return null; ;
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine("parse error: " + ex.Message);
                return null;
            }
        }

        public static async Task<FileData> GetFileFromService(long fileId)
        {

            HttpClient client = new HttpClient();
            //client.MaxResponseContentBufferSize = 256000;
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Credential.sharedInstance.Token}");
            var serviceUri = Credential.BASE_URL + $"Patient/{Credential.sharedInstance.Mrn}/File/{fileId}";

            //Debug.WriteLine("serviceUri = " + serviceUri);

            try
            {
                var response = await client.GetAsync(serviceUri);
                //Debug.WriteLine("GetAllReadingsFromService response.IsSuccessStatusCode= " + response.IsSuccessStatusCode);
                if (response.IsSuccessStatusCode)
                {
                    //Debug.WriteLine("Got readings successfully.");
                    var content = await response.Content.ReadAsStringAsync();
                    //Debug.WriteLine(JsonConvert.DeserializeObject<FileData>(content));
                    //return JsonConvert.DeserializeObject<Reading[]>(content);
                    var val = JsonConvert.DeserializeObject<FileData>(content);
                    return val;
                }
                else
                {
                    //Debug.WriteLine("Failed to get readings.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine("parse error: " + ex.Message);
                return null;
            }
        }
       

    }
    public class FileData
    {
        public long? Id { get; set; }
        public string Category { get; set; }
        public byte[] Content { get; set; }
        public string Name { get; set; }
        public DateTime? ServiceDate { get; set; }
        public long Size { get; set; }
        public DateTime? UploadDate { get; set; }
    }
}
