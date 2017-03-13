using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public partial class RespHomePage : ContentPage, BLEReadingUpdatableSpiroMeter
	{
		SpirometerReading currReading;
		//BleManagerSpirometer bleManager = new BleManagerSpirometer();

		public RespHomePage()
		{
			InitializeComponent();

			var tapGestureRecognizer = new TapGestureRecognizer();
			tapGestureRecognizer.Tapped += (s, e) =>
			{
				var newPage = new UserProfile();
				newPage.Title = "My Account";
				this.Navigation.PushAsync(newPage);
			};

			imgProfile.GestureRecognizers.Add(tapGestureRecognizer);

			gifWebView.Source = DependencyService.Get<IBaseUrl>().Get() + "/gifContainer.html";

			callAPiToDisplayGetDemographics();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			BLECentralManager.sharedInstance.spiroServHandler.stopPolling();
		}

		// call back methods
		public void updateCaller(SpirometerReading reading)
		{
			currReading = reading;

			if (reading !=null)
			{
				this.layoutLoading.IsVisible = false;
				//this.currReading = new SpirometerReading(DateTime.Now, pef, fev1);

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
		}

		public void updateDeviceStateOnUI(String message, bool isConnected) {

			Device.BeginInvokeOnMainThread(() =>
				{
				layoutLoadingTakeReading.IsVisible = false;
				});

			DisplayAlert("Spirometer", message, "OK");
		}

		void btnTakeReadingClicked(object sender, System.EventArgs e)
		{

			//updateCaller(new SpirometerReading(DateTime.Now, 680, 3.5m));

			try
			{
				layoutLoadingTakeReading.IsVisible = true;
				BLECentralManager.sharedInstance.connectToDevice("BLE-MSA", this);
			}
			catch { 
				layoutLoadingTakeReading.IsVisible = true;
			}

			//bleManager.ScanToConnectToSpotCheck((BLEReadingUpdatableSpiroMeter)this);
			//DependencyService.Get<ICBCentralManagerSpirometer>().connectToSpirometer((BLEReadingUpdatableSpiroMeter)this);
		}

		void btnCancelTakeReadingClicked(object sender, System.EventArgs e)
		{
			layoutLoadingTakeReading.IsVisible = false;
			BLECentralManager.sharedInstance.spiroServHandler.stopPolling();
			//((SpirometerServiceHandler)BLECentralManager.sharedInstance.devServiceHandler).stopPolling();
			//DependencyService.Get<ICBCentralManagerSpirometer>().StopReadingValue();
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
			finally
			{
				layoutLoading.IsVisible = false;
			}
		}

		void btnDeleteCurrentReadingClicked(object sender, System.EventArgs e)
		{
			clearReadingDisplay();
		}

		private void clearReadingDisplay()
		{
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

		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			try
			{
				var latestCalibratedReading = Demographics.sharedInstance.getLatestCalibratedReading();

				System.Diagnostics.Debug.WriteLine(latestCalibratedReading.dateString);

				//latestCalibratedReading.Date.

				//lblDate.Text = "03/10/2017 08:25 PM    ";
				lblDate.Text = latestCalibratedReading.dateString;
				//lblDate.BackgroundColor = Color.Red;

				lblFev1.Text = latestCalibratedReading.fev1String;
				lblPef.Text = latestCalibratedReading.pefString;

				//lblDate.Text = latestCalibratedReading.dateString +" AM";

				System.Diagnostics.Debug.WriteLine("date:" + lblDate.Text);
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

			lblClickMessage.IsVisible = true;

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
			var newPage = new RespGraphPage();
			newPage.Title = "Data Graph Screen";
			this.Navigation.PushAsync(newPage);
		}

		void btnViewDataListClicked(object sender, System.EventArgs e)
		{
			var newPage = new RespDataListPage();
			newPage.Title = "Data List Screen";
			this.Navigation.PushAsync(newPage);
		}
	}
}
