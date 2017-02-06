using System.Threading.Tasks;
using System;
using System.Net;
using System.Net.Http;
using System.Diagnostics;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyHealthVitals
{
    public class Category
    {
        public long Id { get; set; }
        public string EnglishExpression { get; set; }
        public string MetricExpression { get; set; }
        public string Mode { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string ValueTypes { get; set; }

		public static async Task<Category[]> callServiceToGetCategories()
        {
			//return await Client.GetAsync<Category[]>(credential, $"api/v1/Patient/{Credential.sharedInstance.Mrn}/HomeHealth/Category");

			HttpClient client = new HttpClient();
			client.MaxResponseContentBufferSize = 256000;
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Credential.sharedInstance.Token}");

			var response = await client.GetAsync(Credential.BASE_URL_TEST + $"Patient/{Credential.sharedInstance.Mrn}/HomeHealth/Category");
			if (response.IsSuccessStatusCode)
			{
				try
				{
					var content = await response.Content.ReadAsStringAsync();
					return JsonConvert.DeserializeObject<Category[]>(content);
				}
				catch (JsonSerializationException ex)
				{
					Debug.WriteLine("parse error: " + ex.Message);
					return null;
				}
			}
			else {
				return null;;
			}
        }

   //     public static async Task<Category> GetAsync( long id)
   //     {
			//return await Client.GetAsync<Category>(credential, $"api/v1/Patient/{Credential.sharedInstance.Mrn}/HomeHealth/Category/{id}");
   //     }
    }
}
