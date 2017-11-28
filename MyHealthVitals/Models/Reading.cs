using System.Threading.Tasks;
using System;
using System.Net;
using System.Net.Http;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        public string MrnFileId { get; set; } //need to handle this being a string in the emhr api


		public Reading(String valueType, decimal englishVal, long catId, bool Abn, string Narr, string FileId)
		{

			this.ValueType = valueType;
			this.CategoryId = catId;
			this.EnglishValue = englishVal;

			// this is same for all reading
			this.Source = "Device";
			this.Date = DateTime.Now;

            //for ecg readings
            this.Abnormal = Abn;
            this.Narrative = Narr;
            this.MrnFileId = FileId;
		}

		//public void getCelcious

		public async Task<bool> PostReadingToService()
		{
            //this.Narrative = Task_vars.ecgmessage;
            Debug.WriteLine("ecgmessage: " + Task_vars.ecgmessage);
            Debug.WriteLine("ecgmessage sent: "+this.Narrative);
			//var item = await Client.PostAsync(credential, $"api/v1/Patient/{credential.Mrn}/HomeHealth/Reading", this);
			//Id = item.Id;
			//Abnormal = item.Abnormal;
			//EnglishValue = item.EnglishValue;
			Debug.WriteLine("PostReadingToService");
			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Credential.sharedInstance.Token}");

			// converting the this reading into string to send it to the service as application/json
			var content = new StringContent(JsonConvert.SerializeObject(this), Encoding.UTF8, "application/json");
            Debug.WriteLine("json stuff: "+ content.ToString());

			var serviceUri = Credential.BASE_URL + $"Patient/{Credential.sharedInstance.Mrn}/HomeHealth/Reading";
            Debug.WriteLine("sent to :"+serviceUri.ToString());

			var response = await client.PostAsync(serviceUri, content);

			if (response.ReasonPhrase == "OK")
			{
				return true;
			}
			else {
				return false;
			}
		}

		public static async Task<Reading[]> GetAllReadingsFromService()
		{

			HttpClient client = new HttpClient();
			//client.MaxResponseContentBufferSize = 256000;
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Credential.sharedInstance.Token}");
			var serviceUri = Credential.BASE_URL + $"Patient/{Credential.sharedInstance.Mrn}/HomeHealth/Reading";
	
			Debug.WriteLine("serviceUri = " +serviceUri);

			try
			{
				var response = await client.GetAsync(serviceUri);
				Debug.WriteLine("GetAllReadingsFromService response.IsSuccessStatusCode= "+response.IsSuccessStatusCode);
				if (response.IsSuccessStatusCode)
				{
                    Debug.WriteLine("Got readings successfully.");
					var content = await response.Content.ReadAsStringAsync();
					Debug.WriteLine(JsonConvert.DeserializeObject<Reading[]>(content));
					return JsonConvert.DeserializeObject<Reading[]>(content);
				}
				else {
                    Debug.WriteLine("Failed to get readings.");
					return null; ;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("parse error: " + ex.Message);
				return null;
			}
		}
		
	}
	

	
}
