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
            NavigationPage.SetHasNavigationBar(this, false);
			InitializeComponent();
			FakeToolbar.Children.Add(
			backarrow,
			// Adds the Button on the top left corner, with 10% of the navbar's width and 100% height
			new Rectangle(0, 0.5, 0.1, 1),
			// The proportional flags tell the layout to scale the value using [0, 1] -> [0%, 100%]
			AbsoluteLayoutFlags.HeightProportional | AbsoluteLayoutFlags.WidthProportional
			);

			FakeToolbar.Children.Add(
				backbtn,
				// Using 0.5 will center it and the layout takes the size of the element into account
				// 0.5 will center, 1 will right align
				// Adds in the center, with 90% of the navbar's width and 100% of height
				new Rectangle(0.1, 0.5, 0.3, 1),
				AbsoluteLayoutFlags.All
			);
			FakeToolbar.Children.Add(
				titlebtn,
				// Using 0.5 will center it and the layout takes the size of the element into account
				// 0.5 will center, 1 will right align
				// Adds in the center, with 90% of the navbar's width and 100% of height
				new Rectangle(0.5, 0.5, 0.5, 1),
				AbsoluteLayoutFlags.All
			);
			// initial rendering previously saved data
			if (Device.Idiom == TargetIdiom.Tablet)
			{
				FakeToolbar.HeightRequest = 75 * Screensize.heightfactor;
				backbtn.FontSize = 30 * Screensize.heightfactor;
				titlebtn.FontSize = 30 * Screensize.heightfactor;

                layout.Spacing = (360 * Screensize.heightfactor)/9;
                layout.Padding = new Thickness((135 * Screensize.heightfactor)/9);
				imgProfile.WidthRequest = 160 * Screensize.widthfactor;
				imgProfile.HeightRequest = 192 * Screensize.heightfactor;
				lblName.FontSize = 32 * Screensize.heightfactor;
				lblEmail.FontSize = 32 * Screensize.heightfactor;
				lblGender.FontSize = 32 * Screensize.heightfactor;
				lblBirthdate.FontSize = 32 * Screensize.heightfactor;
				lblAge.FontSize = 32 * Screensize.heightfactor;
				lblHeight.FontSize = 32 * Screensize.heightfactor;
				lblWeight.FontSize = 32 * Screensize.heightfactor;
				lblMobileNo.FontSize = 32 * Screensize.heightfactor;
				gender.FontSize = 32 * Screensize.heightfactor;
				bid.FontSize = 32 * Screensize.heightfactor;
				age.FontSize = 32 * Screensize.heightfactor;
				height.FontSize = 32 * Screensize.heightfactor;
				weight.FontSize = 32 * Screensize.heightfactor;
				mobileno.FontSize = 32 * Screensize.heightfactor;
				rem.FontSize = 32 * Screensize.heightfactor;
				auto.FontSize = 32 * Screensize.heightfactor;
				save.FontSize = 32 * Screensize.heightfactor;
                save.HeightRequest = 100 * Screensize.heightfactor;
                //savecont.Margin = new Thickness(5*Screensize.widthfactor, -245*Screensize.heightfactor, 5*Screensize.widthfactor, 0);
                //savecont.HeightRequest = 180 * Screensize.heightfactor;
                //savecont.Spacing = 9 * Screensize.heightfactor;
				save.Margin = new Thickness(50 * Screensize.widthfactor, 0);
                //Debug.WriteLine("Height of savecont = " + savecont.HeightRequest);
                //Debug.WriteLine("Height of cpyrt = " + cpyrt.HeightRequest
			}
            else if (Device.Idiom == TargetIdiom.Phone)
            {
				FakeToolbar.HeightRequest = 55 * Screensize.heightfactor;
				titlebtn.FontSize = 16 * Screensize.heightfactor;
                backbtn.FontSize = 16 * Screensize.heightfactor;

                layout.Spacing = (225 * Screensize.heightfactor) / 9;
                layout.Padding = new Thickness((135 * Screensize.heightfactor)/9);
				imgProfile.WidthRequest = 100 * Screensize.widthfactor;
				imgProfile.HeightRequest = 120 * Screensize.heightfactor;
				lblName.FontSize = 16 * Screensize.heightfactor;
				lblEmail.FontSize = 15 * Screensize.heightfactor;
				lblGender.FontSize = 16 * Screensize.heightfactor;
				lblBirthdate.FontSize = 16 * Screensize.heightfactor;
				lblAge.FontSize = 16 * Screensize.heightfactor;
				lblHeight.FontSize = 16 * Screensize.heightfactor;
				lblWeight.FontSize = 16 * Screensize.heightfactor;
				lblMobileNo.FontSize = 16 * Screensize.heightfactor;
				gender.FontSize = 16 * Screensize.heightfactor;
				bid.FontSize = 16 * Screensize.heightfactor;
				age.FontSize = 16 * Screensize.heightfactor;
				height.FontSize = 16 * Screensize.heightfactor;
				weight.FontSize = 16 * Screensize.heightfactor;
				mobileno.FontSize = 16 * Screensize.heightfactor;
				rem.FontSize = 16 * Screensize.heightfactor;
				auto.FontSize = 16 * Screensize.heightfactor;
				save.FontSize = 16 * Screensize.heightfactor;
            }
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

		void btnPrevClicked(object sender, System.EventArgs e)
		{
			Navigation.PopAsync();
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

				var genValue = 0;

				if (Demographics.sharedInstance.Sex.ToString() == "Female") {
					genValue = 1;
				}
				lblGender.Text = Demographics.sharedInstance.Sex.ToString();
				//SegControl.SelectedSegment = genValue;

				lblBirthdate.Text = String.Format("{0:MM/dd/yyyy}", (DateTime)Demographics.sharedInstance.DateOfBirth);

				lblAge.Text = (DateTime.Now.Year - 1 - ((DateTime)Demographics.sharedInstance.DateOfBirth).Year).ToString();
				lblHeight.Text = Demographics.sharedInstance.Height.Split('/')[0];
				lblWeight.Text = Demographics.sharedInstance.Weight.Split('/')[0];

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
				//Debug.WriteLine("exception User profile initial rendering data check on birthday calculation");
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
