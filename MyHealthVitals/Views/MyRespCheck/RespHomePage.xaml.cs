using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public partial class RespHomePage : ContentPage
	{
		public RespHomePage()
		{
			InitializeComponent();

			callAPiToDisplayGetDemographics();
		}

		void btnViewProfileClicked(object sender, System.EventArgs e)
		{
			var newPage = new UserProfile();
			newPage.Title = "My Account";
			this.Navigation.PushAsync(newPage);
		}

		private async void callAPiToDisplayGetDemographics()
		{
			var isSuccess = await Demographics.sharedInstance.getDemographicFromApi();

			if (isSuccess)
			{
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

		void btnTakeReadingClicked(object sender, System.EventArgs e)
		{
			//this.Navigation.PushAsync(new LoadingPage());
		}

		void btnCalibrateClicked(object sender, System.EventArgs e)
		{
			//this.Navigation.PushAsync(new Loa());
		}

		void btnViewGraphPageClicked(object sender, System.EventArgs e)
		{
			this.Navigation.PushAsync(new RespGraphPage());
		}

		void btnViewDataListClicked(object sender, System.EventArgs e)
		{
			//btnViewDataListClicked

			var newPage = new RespDataListPage();
			newPage.Title = "Data List Screen";
			this.Navigation.PushAsync(newPage);
		}
	}
}
