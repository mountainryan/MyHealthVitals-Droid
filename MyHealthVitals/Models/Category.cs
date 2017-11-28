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
			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Credential.sharedInstance.Token}");

			var response = await client.GetAsync(Credential.BASE_URL + $"Patient/{Credential.sharedInstance.Mrn}/HomeHealth/Category");

			if (response.IsSuccessStatusCode)
			{
				try
				{
				//	string content = "[{\"id\":1,\"englishExpression\":null,\"metricExpression\":null,\"mode\":\"d\",\"name\":\"Blood Pressure\",\"unit\":\"mmHg\",\"valueTypes\":\"Systolic\\r\\nDiastolic\"},{\"id\":2,\"englishExpression\":null,\"metricExpression\":null,\"mode\":\"d\",\"name\":\"SPo2\",\"unit\":\"%\",\"valueTypes\":null},{\"id\":3,\"englishExpression\":null,\"metricExpression\":null,\"mode\":\"d\",\"name\":\"Heart Rate\",\"unit\":\"beats/min\",\"valueTypes\":null},{\"id\":4,\"englishExpression\":\"x 9 5 / * 32 +\",\"metricExpression\":\"x 32 - 5 9 / *\",\"mode\":\"d\",\"name\":\"Temperature\",\"unit\":\"F/C\",\"valueTypes\":null},{\"id\":5,\"englishExpression\":\"x 0.45359237 /\",\"metricExpression\":\"x 0.45359237 *\",\"mode\":\"d\",\"name\":\"Weight\",\"unit\":\"lbs/kg\",\"valueTypes\":null},{\"id\":6,\"englishExpression\":\"x 2.54 /\",\"metricExpression\":\"x 2.54 *\",\"mode\":\"d\",\"name\":\"Height\",\"unit\":\"in/cm\",\"valueTypes\":null},{\"id\":7,\"englishExpression\":null,\"metricExpression\":null,\"mode\":\"c\",\"name\":\"BMI\",\"unit\":\"kg/m2\",\"valueTypes\":null},{\"id\":8,\"englishExpression\":null,\"metricExpression\":null,\"mode\":\"d\",\"name\":\"Glucose\",\"unit\":null,\"valueTypes\":null},{\"id\":9,\"englishExpression\":null,\"metricExpression\":null,\"mode\":\"d\",\"name\":\"Spirometer\",\"unit\":\"L/min\",\"valueTypes\":\"PEF\\r\\nFEV1\"},{\"id\":10,\"englishExpression\":null,\"metricExpression\":null,\"mode\":\"b\",\"name\":\"ECG\",\"unit\":null,\"valueTypes\":null},{\"id\":11,\"englishExpression\":null,\"metricExpression\":null,\"mode\":\"b\",\"name\":\"Fall\",\"unit\":null,\"valueTypes\":null}]";
					var content = await response.Content.ReadAsStringAsync();;
					Debug.WriteLine("content :" + content);
						Debug.WriteLine("JsonConvert.DeserializeObject<Category[]>(content) :" + JsonConvert.DeserializeObject<Category[]>(content));
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
