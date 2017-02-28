using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MyHealthVitals
{
	public partial class UserProfile : ContentPage
	{
		void btnSaveClicked(object sender, System.EventArgs e)
		{
			Demographics.sharedInstance.isAutoLogin = switchAutoLogin.IsToggled;
			Demographics.sharedInstance.isRememberUsername = switchRemUsername.IsToggled;
			Demographics.sharedInstance.saveUserDefaults();
			this.Navigation.PopAsync();
		}

		public UserProfile()
		{
			InitializeComponent();
			// initial rendering previously saved data

			switchAutoLogin.IsToggled = Demographics.sharedInstance.isAutoLogin;
			switchRemUsername.IsToggled = Demographics.sharedInstance.isRememberUsername;

			if (Demographics.sharedInstance.Id > 0)
			{
				initialRendering();
			}
			else {
				callAPi();
			}
		}

		private async void callAPi()
		{
			var isSuccess = await Demographics.sharedInstance.getDemographicFromApi();

			if (isSuccess)
			{
				initialRendering();

				this.lblName.Text = Demographics.sharedInstance.getFullName();
				this.lblEmail.Text = Demographics.sharedInstance.Email;

				// calling async to download the image and setting in to the image
				String imageBase64 = await Demographics.sharedInstance.downloadProfilePic();

				if (imageBase64 != null)
				{
					this.imgProfile.Source = Xamarin.Forms.ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(imageBase64)));
				}
			}
		}

		public async void initialRendering() {

			try
			{
				this.lblName.Text = Demographics.sharedInstance.getFullName();
				this.lblEmail.Text = Demographics.sharedInstance.Email;

				lblBirthdate.Text = String.Format("{0:MM/dd/yyyy}", (DateTime)Demographics.sharedInstance.DateOfBirth);

				lblAge.Text = (DateTime.Now.Year - 1 - ((DateTime)Demographics.sharedInstance.DateOfBirth).Year).ToString();
				lblHeight.Text = Demographics.sharedInstance.Height;
				lblWeight.Text = Demographics.sharedInstance.Weight;

				Regex regexObj = new Regex(@"[^\d]");
				string officePhone3 = Demographics.sharedInstance.CellPhone ?? "";

				if (officePhone3.Length == 10)
				{
					string officePhone1 = regexObj.Replace(Demographics.sharedInstance.CellPhone, "");
					string officePhone2 = officePhone1.Insert(officePhone1.Length - 4, "-");
					officePhone3 = officePhone2.Insert(officePhone2.Length - 8, "-");
				}

				lblMobileNo.Text = officePhone3;
			}
			catch(Exception) {
				Debug.WriteLine("exception User profile initial rendering data check on birthday calculation");
			}

			// calling async to download the image and setting in to the image
			String imageBase64 = await Demographics.sharedInstance.downloadProfilePic();

			if (imageBase64 != null)
			{
				this.imgProfile.Source = Xamarin.Forms.ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(imageBase64)));
			}
		}
	}
}
