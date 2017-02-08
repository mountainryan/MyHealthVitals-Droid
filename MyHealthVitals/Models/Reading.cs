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

		public Reading(String valueType, decimal englishVal,long catId) {

			this.ValueType = valueType;
			this.CategoryId = catId;
			this.EnglishValue = englishVal;

			// this is same for all reading
			this.Source = "Device";
			this.Date = DateTime.Now;
		}

		//public void getCelcious

		public async Task<bool> PostReadingToService()
        {
            //var item = await Client.PostAsync(credential, $"api/v1/Patient/{credential.Mrn}/HomeHealth/Reading", this);
            //Id = item.Id;
            //Abnormal = item.Abnormal;
            //EnglishValue = item.EnglishValue;
            //MetricValue = item.MetricValue;

			HttpClient client = new HttpClient();
			client.MaxResponseContentBufferSize = 256000;
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Credential.sharedInstance.Token}");

			// converting the this reading into string to send it to the service as application/json
			var content = new StringContent(JsonConvert.SerializeObject(this), Encoding.UTF8, "application/json");
			var serviceUri = Credential.BASE_URL_TEST + $"Patient/{Credential.sharedInstance.Mrn}/HomeHealth/Reading";

			var response = await client.PostAsync(serviceUri,content);

			if (response.ReasonPhrase=="OK")
			{
				return true;
			}
			else {
				return false;
			}
        }
    }
}
