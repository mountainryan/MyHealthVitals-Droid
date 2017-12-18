using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace MyHealthVitals
{
    public class Credential
    {
       	//public string Hostname { get; }
		public long Mrn { get; set; }
		public string Token { get; set; }
		public string username { get; set; }

		public static String BASE_URL_LIVE = "https://www.myemhr.com/api/v1/";
		public static String BASE_URL_DEV = "http://testmyemhr.local/api/v1/";
		public static String BASE_URL_DEMO = "https://demo.myemhr.com/api/v1/";
		public static String BASE_URL_TEST = "https://test.myemhr.com/api/v1/";

        public static String BASE_URL = BASE_URL_TEST;//"https://test.myemhr.com/api/v1/";

		public static Credential sharedInstance = new Credential();

		private Credential() 
		{ 

		}

		public async Task<Credential> CallApiForLogin(string username, string password)
		{
			
		//	var client = new HttpClient { BaseAddress = new Uri("https://myemhr.com") };
			var client = new HttpClient { BaseAddress = new Uri(BASE_URL) };
			Debug.WriteLine("Client = " + client);
			client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ':' + password)));
			//issuer = Uri.EscapeUriString(issuer);
			var response = await client.PostAsync($"Authorize?expiration={Credential.sharedInstance.Mrn}?issuer=Mobile", null);
		//	var response = await client.PostAsync($"api/v1/Authorize?expiration={Credential.sharedInstance.Mrn}?issuer=Mobile", null);

			//Debug.WriteLine("response: " + response.Content.ReadAsStringAsync());

			var accessToken = await response.Content.ReadAsStringAsync();

			Credential.sharedInstance.username = username;
			Credential.sharedInstance.Token = accessToken.Trim('"');

			//var accessToken = await response.Content.ReadAsStringAsync();
			response.Content?.Dispose();

			//Debug.WriteLine("message: " + message);

			if (response.IsSuccessStatusCode)
			{
				return Credential.sharedInstance;
			}
			else { 
				throw new HttpStatusException(response.StatusCode, Credential.sharedInstance.Token);
			}
		}
    }
}
