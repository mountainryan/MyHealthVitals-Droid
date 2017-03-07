using System;
using System.Net;
using System.Net.Http;
using System.Diagnostics;

using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public class Demographics
	{
		public long Id { get; set; }
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }
		public string Address { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Zip { get; set; }
		public string Country { get; set; }
		public string HomePhone { get; set; }
		public string CellPhone { get; set; }
		public string CellCarrier { get; set; }
		public string Email { get; set; }
		public string TextEmail { get; set; }
		public string WorkStatus { get; set; }
		public string Occupation { get; set; }
		public string EmployerSchool { get; set; }
		public string EmployerSchoolPhone { get; set; }
		public string PreferredPharmacy { get; set; }
		public string CommunicationPreference { get; set; }
		public string Sex { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public string MaritalStatus { get; set; }
		public string Race { get; set; }
		public string Ethnicity { get; set; }
		public string Language { get; set; }
		public string Height { get; set; }
		public string Weight { get; set; }
		public string PrimaryPhysicianName { get; set; }
		public string PrimaryPhysicianOrganization { get; set; }
		public string PrimaryPhysicianPhone { get; set; }
		public string BloodType { get; set; }
		public string SubscriptionName { get; set; }
		public int SubscriptionPlan { get; set; }

		public string EmergencyFirstName { get; set; }
		public string EmergencyMiddleName { get; set; }
		public string EmergencyLastName { get; set; }
		public string EmergencyRelationship { get; set; }
		public string EmergencyAddress { get; set; }
		public string EmergencyCity { get; set; }
		public string EmergencyState { get; set; }
		public string EmergencyZip { get; set; }
		public string EmergencyCountry { get; set; }
		public string EmergencyPhone { get; set; }
		public string EmergencyAlternatePhone { get; set; }

		public string InsurancePrimaryCarrier { get; set; }
		public string InsurancePrimaryAddress { get; set; }
		public string InsurancePrimaryCity { get; set; }
		public string InsurancePrimaryState { get; set; }
		public string InsurancePrimaryZip { get; set; }
		public string InsurancePrimaryCountry { get; set; }
		public string InsurancePrimaryPhone { get; set; }
		public string InsurancePrimaryFacsimile { get; set; }
		public string InsurancePrimaryPlan { get; set; }
		public string InsurancePrimaryExpiry { get; set; }
		public string InsurancePrimaryIdNumber { get; set; }
		public string InsurancePrimaryPolicyNumber { get; set; }
		public string InsurancePrimaryCopay { get; set; }
		public string InsurancePrimarySubscriber { get; set; }
		public string InsuranceSecondaryCarrier { get; set; }
		public string InsuranceSecondaryAddress { get; set; }
		public string InsuranceSecondaryCity { get; set; }
		public string InsuranceSecondaryState { get; set; }
		public string InsuranceSecondaryZip { get; set; }
		public string InsuranceSecondaryCountry { get; set; }
		public string InsuranceSecondaryPhone { get; set; }
		public string InsuranceSecondaryFacsimile { get; set; }
		public string InsuranceSecondaryPlan { get; set; }
		public string InsuranceSecondaryExpiry { get; set; }
		public string InsuranceSecondaryIdNumber { get; set; }
		public string InsuranceSecondaryPolicyNumber { get; set; }
		public string InsuranceSecondaryCopay { get; set; }
		public string InsuranceSecondarySubscriber { get; set; }
		public string InsuranceGuarantorName { get; set; }
		public string InsuranceGuarantorRelation { get; set; }
		public string InsuranceGuarantorPhone { get; set; }

		public bool MilitaryDisabled { get; set; }
		public string MilitaryId { get; set; }
		public string MilitaryRank { get; set; }
		public string MilitaryMos { get; set; }
		public bool MilitaryStatus { get; set; }
		public string MilitaryBranch { get; set; }

		public static Demographics sharedInstance = new Demographics();


		// userdefaults
		public bool isAutoLogin;
		public bool isRememberUsername;
		public string username;
		public string password;

		public SpirometerReading calibratedReading;

		private Demographics()
		{
			//var curr_username = "";
			if (Application.Current.Properties.ContainsKey("_username"))
			{
				username = (string)Application.Current.Properties["_username"];
			}

			if (Application.Current.Properties.ContainsKey(username+"_isAutoLogin"))
			{
				isAutoLogin = (bool)Application.Current.Properties[username+"_isAutoLogin"];
			}

			if (Application.Current.Properties.ContainsKey(username + "_isRememberUsername"))
			{
				isRememberUsername = (bool)Application.Current.Properties[username + "_isRememberUsername"];
			}

			if (Application.Current.Properties.ContainsKey(username + "_password"))
			{
				password = (string)Application.Current.Properties[username + "_password"];
			}

			// local storage of calibrated value
			try
			{
				var pef = Convert.ToDecimal(Application.Current.Properties[username + "_pef"]);
				var fev = Convert.ToDecimal(Application.Current.Properties[username + "_fev1"]);
				var date = Convert.ToDateTime(Application.Current.Properties[username + "_savedDate"]);

				this.calibratedReading = new SpirometerReading(date, pef, fev);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Exception: " + ex.Message);
			}
		}

		public void saveCalibratedReadig() { 
			Application.Current.Properties[username+"_pef"] = calibratedReading.pefString;
			Application.Current.Properties[username + "_fev1"] = calibratedReading.fev1String;
			Application.Current.Properties[username + "_savedDate"] = calibratedReading.Date.ToString();
			Application.Current.SavePropertiesAsync();
		}

		public void saveUserDefaults()
		{
			Application.Current.Properties["_username"] = username;
			Application.Current.Properties[username + "_password"] = password;
			Application.Current.Properties[username + "_isAutoLogin"] = isAutoLogin;
			Application.Current.Properties[username + "_isRememberUsername"] = isRememberUsername;
			Application.Current.SavePropertiesAsync();
		}

		public void clearLocalStorageOnLogout()
		{
			//Application.Current.Properties["username"] = "";
			//Application.Current.Properties[username + "_password"] = "";

			//Application.Current.Properties[username + "_isAutoLogin"] = false;
			//Application.Current.Properties["isRememberUsername"] = isRememberUsername;
			//Application.Current.SavePropertiesAsync();

			Demographics.sharedInstance.password = "";
		}

		// calling web service to get the json
		public async Task<bool> getDemographicFromApi()
		{
			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Credential.sharedInstance.Token}");

			var response = await client.GetAsync(Credential.BASE_URL_TEST + $"Patient/{Credential.sharedInstance.Mrn}/Demographics");
			if (response.IsSuccessStatusCode)
			{
				try
				{
					var content = await response.Content.ReadAsStringAsync();
					var demoGraphics = JsonConvert.DeserializeObject<Demographics>(content);

					// transfering local values to the shared instance of demographics after deserialize
					demoGraphics.username = Demographics.sharedInstance.username;
					demoGraphics.password = Demographics.sharedInstance.password;
					demoGraphics.isAutoLogin = Demographics.sharedInstance.isAutoLogin;
					demoGraphics.isRememberUsername = Demographics.sharedInstance.isRememberUsername;
					Demographics.sharedInstance = demoGraphics;

					Debug.WriteLine(Demographics.sharedInstance.getFullName());
				}
				catch (JsonSerializationException ex)
				{
					Debug.WriteLine("parse error: " + ex.Message);
				}

				return true;

			}
			else {
				return false;
				//throw new HttpStatusException(response.StatusCode,"Network Error.");
			}
		}

		public async Task<String> downloadProfilePic()
		{

			HttpClient client = new HttpClient();
			//client.MaxResponseContentBufferSize = 256000;
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Credential.sharedInstance.Token}");

			var response = await client.GetAsync(Credential.BASE_URL_TEST + $"Patient/{Credential.sharedInstance.Mrn}/File/Photo");
			if (response.IsSuccessStatusCode)
			{
				try
				{
					var content = await response.Content.ReadAsStringAsync();

					JToken obj = JObject.Parse(content);
					String base64img = (String)obj.SelectToken("content");

					return base64img;
				}
				catch (JsonSerializationException ex)
				{
					Debug.WriteLine("parse error image: " + ex.Message);
					return null;
				}
			}
			else {
				return null;
			}
		}

		//public async

		public String getFullName()
		{
			if (this.MiddleName.Length > 0)
			{
				return FirstName + " " + MiddleName + " " + LastName;
			}
			else {
				return FirstName + " " + LastName;
			}
		}
	}
}
