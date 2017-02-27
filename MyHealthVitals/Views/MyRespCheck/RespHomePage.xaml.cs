﻿using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public partial class RespHomePage : ContentPage,BLEReadingUpdatableSpiroMeter
	{
		SpirometerReading currReading;
		public RespHomePage()
		{
			InitializeComponent();

			callAPiToDisplayGetDemographics();
		}

		// call back methods
		public void updateCaller(SpirometerReading currReading)
		{

			this.layoutLoading.IsVisible = false;

			System.Diagnostics.Debug.WriteLine(" loaded spirometer reading:" + currReading.pefString);
			this.currReading = currReading;

			Device.BeginInvokeOnMainThread(() =>
			{
				lblPefReading.Text = currReading.pefString;
				lblFevReading.Text = currReading.fev1String;
				lblDateReading.Text = currReading.dateString;

				btnDelete.IsVisible = true;
				btnIndicator.IsVisible = true;

				btnIndicator.BackgroundColor = Color.FromHex(currReading.color);

				layoutLoadingTakeReading.IsVisible = false;

			});
		}

		void btnTakeReadingClicked(object sender, System.EventArgs e)
		{

			layoutLoadingTakeReading.IsVisible = true;

			DependencyService.Get<ICBCentralManagerSpirometer>().connectToSpirometer((BLEReadingUpdatableSpiroMeter)this);
		}

		void btnCancelTakeReadingClicked(object sender, System.EventArgs e)
		{
			layoutLoadingTakeReading.IsVisible = false;
			DependencyService.Get<ICBCentralManagerSpirometer>().StopReadingValue();
		}

		async void btnSaveClicked(object sender, System.EventArgs e)
		{
			layoutLoading.IsVisible = true;

			try
			{
				Reading fevReading = new Reading("FEV1", currReading.Fev1, 9);
				Reading pefReading = new Reading("PEF", currReading.Pef, 9);

				await pefReading.PostReadingToService();
				await fevReading.PostReadingToService();

				clearReadingDisplay();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Exception on saving curr reading: " + ex.Message);
			}
			finally{
				layoutLoading.IsVisible = false;
			}
		}

		void btnDeleteCurrentReadingClicked(object sender, System.EventArgs e)
		{
			clearReadingDisplay();
		}

		private void clearReadingDisplay() { 
			try
			{
				this.currReading.Pef = -1;
				this.currReading.Fev1 = -1;

				lblPefReading.Text = " ";
				lblFevReading.Text = " ";
				lblDateReading.Text = " ";

				btnDelete.IsVisible = false;
				btnIndicator.IsVisible = false;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Exception " + ex.Message);
			}
		}

		void btnViewProfileClicked(object sender, System.EventArgs e)
		{
			var newPage = new UserProfile();
			newPage.Title = "My Account";
			this.Navigation.PushAsync(newPage);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			try
			{
				if (Demographics.sharedInstance.calibratedReading.Pef > 0)
				{
					lblDate.Text = Demographics.sharedInstance.calibratedReading.dateString;
					lblFev1.Text = Demographics.sharedInstance.calibratedReading.fev1String;
					lblPef.Text = Demographics.sharedInstance.calibratedReading.pefString;
				}
				else {
					lblDate.Text = "--";
					lblFev1.Text = "--";
					lblPef.Text = "--";
				}
			}
			catch
			{	
				lblDate.Text = "--";
				lblFev1.Text = "--";
				lblPef.Text = "--";
				System.Diagnostics.Debug.WriteLine("Exception in getting calibrated data");
			}
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

		void btnCalibrateClicked(object sender, System.EventArgs e)
		{
			var newPage = new RespCalibrationPage();
			newPage.Title = "Calibration Screen";
			this.Navigation.PushAsync(newPage);
		}

		void btnViewGraphPageClicked(object sender, System.EventArgs e)
		{
			this.Navigation.PushAsync(new RespGraphPage());
		}

		void btnViewDataListClicked(object sender, System.EventArgs e)
		{
			var newPage = new RespDataListPage();
			newPage.Title = "Data List Screen";
			this.Navigation.PushAsync(newPage);
		}
	}
}
