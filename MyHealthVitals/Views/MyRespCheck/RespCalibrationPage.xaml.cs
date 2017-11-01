using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.Diagnostics;

namespace MyHealthVitals
{
	public partial class RespCalibrationPage : ContentPage,BLEReadingUpdatableSpiroMeter
	{

		ObservableCollection<SpirometerReading> calibratedReadingList = new ObservableCollection<SpirometerReading>();

		//BleManagerSpirometer bleManager = new BleManagerSpirometer();

		public RespCalibrationPage()
		{
			InitializeComponent();

			if (Device.Idiom == TargetIdiom.Tablet)
			{
				layout.Spacing = 24 * Screensize.heightfactor;
				label.Margin = new Thickness(20 * Screensize.widthfactor, 10 * Screensize.heightfactor, 20 * Screensize.widthfactor, 10 * Screensize.heightfactor);
				label.FontSize = 32 * Screensize.heightfactor;
				layoutButton.Spacing = 20 * Screensize.heightfactor;
				button.HeightRequest = 70 * Screensize.heightfactor;
				button.FontSize = 24 * Screensize.heightfactor;
				labelpef.WidthRequest = 175 * Screensize.widthfactor;
				labelfev1.WidthRequest = 175 * Screensize.widthfactor;
				labelpef.FontSize = 30 * Screensize.heightfactor;
				labelfev1.FontSize = 30 * Screensize.heightfactor;
				listView.HeightRequest = 50 * Screensize.heightfactor;
				save.FontSize = 36 * Screensize.heightfactor;
				save.HeightRequest = 90 * Screensize.heightfactor;
                save.Margin = new Thickness(3,4,3,120*Screensize.heightfactor);
			}
            else if (Device.Idiom == TargetIdiom.Phone)
            {
				layout.Spacing *= Screensize.heightfactor;
				label.Margin = new Thickness(10 * Screensize.widthfactor, 5 * Screensize.heightfactor, 10 * Screensize.widthfactor, 5 * Screensize.heightfactor);
				label.FontSize *= Screensize.heightfactor;
				layoutButton.Spacing *= Screensize.heightfactor;
				//button.HeightRequest *= Screensize.heightfactor;
				button.FontSize *= Screensize.heightfactor;
				labelpef.WidthRequest *= Screensize.widthfactor;
				labelfev1.WidthRequest *= Screensize.widthfactor;
				labelpef.FontSize *= Screensize.heightfactor;
				labelfev1.FontSize *= Screensize.heightfactor;
				listView.HeightRequest *= Screensize.heightfactor;
				save.FontSize *= Screensize.heightfactor;
				save.HeightRequest *= Screensize.heightfactor;
            }
		}
		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			BLECentralManager.sharedInstance.spiroServHandler.stopPolling();
		}

		void btnCalibrateClicked(object sender, System.EventArgs e)
		{
			if (this.calibratedReadingList.Count < 3)
			{
				layoutLoading.IsVisible = true;

				//bleManager.ScanToConnectToSpotCheck(this);
				BLECentralManager.sharedInstance.connectToDevice("BLE-MSA", this);
				//DependencyService.Get<ICBCentralManagerSpirometer>().connectToSpirometer((BLEReadingUpdatableSpiroMeter)this);
				string readings = (3 - calibratedReadingList.Count) > 1 ? " more readings." : " more reading.";
				lblLoadingMessage.Text = "Please, take " + (3 - calibratedReadingList.Count) + readings;
			}
			else {
				Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
				{
					if (Device.Idiom == TargetIdiom.Tablet)
					{
						var ret = await DependencyService.Get<IFileHelper>().dispAlert("Calibration", "Readings taken are sufficient for calibration. If you want to take more readings, Please, delete the unwanted row and take reading again.", true, "OK", null);
					}
					else
					{
						var ret = await DependencyService.Get<IFileHelper>().dispAlert("Calibration", "Readings taken are sufficient for calibration. If you want to take more readings, Please, delete the unwanted row and take reading again.", false, "OK", null);
					}
				}));
				//DisplayAlert("Calibration", "Readings taken are sufficient for calibration. If you want to take more readings, Please, delete the unwanted row and take reading again.", "OK");
			}
		}
		public void testAgainDialog()
		{
			Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
			{
				if (Device.Idiom == TargetIdiom.Tablet)
				{
					var ret = await DependencyService.Get<IFileHelper>().dispAlert("Reading", "The FEV value is too low, please take reading again.", true, "OK", null);
				}
				else
				{
					var ret = await DependencyService.Get<IFileHelper>().dispAlert("Reading", "The FEV value is too low, please take reading again.", false, "OK", null);
				}
			}));
           //DisplayAlert("Reading", "The FEV value is too low, please take reading again.", "OK");
	//		DisplayAlert("Test Again", "The FEV value is too low, please test again.", "OK");"
			BLECentralManager.sharedInstance.connectToDevice("BLE-MSA", this);
		
		}
		// call back methods
		public void updateCaller(SpirometerReading currReading)
		{
            Debug.WriteLine("Calibration reading.");
			//var currReading = new SpirometerReading(DateTime.Now, pef, fev1);

			currReading.fontsize = 15 * Screensize.heightfactor;
            currReading.spacing = 10 * Screensize.heightfactor;
            currReading.stackheight = 80 * Screensize.heightfactor;
            currReading.imagepng = "deleteicon.png";

			if (Device.Idiom == TargetIdiom.Tablet)
			{
				currReading.fontsize = 30 * Screensize.heightfactor;
                currReading.spacing = 20 * Screensize.heightfactor;
                currReading.stackheight = 120 * Screensize.heightfactor;
                currReading.imagepng = "deleteicon_tab.png";
			}

			currReading.index = calibratedReadingList.Count;
			calibratedReadingList.Add(currReading);

			Debug.WriteLine("loaded spirometer reading:" + currReading.pefString);

			Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
			{

				if (this.calibratedReadingList.Count < 3)
				{
					lblLoadingMessage.Text = "Please, take " + (3 - calibratedReadingList.Count) + " more reading.";
					//bleManager.ScanToConnectToSpotCheck(this);
					//DependencyService.Get<ICBCentralManagerSpirometer>().connectToSpirometer((BLEReadingUpdatableSpiroMeter)this);
					BLECentralManager.sharedInstance.connectToDevice("BLE-MSA", this);
				}
				else {
					layoutLoading.IsVisible = false;
				}

				listView.ItemsSource = calibratedReadingList;
			}));
		}

		public void updateDeviceStateOnUI(String message, bool isConnected)
		{
			try
			{
				Device.BeginInvokeOnMainThread(() =>
					{
						layoutLoading.IsVisible = false;
					});

			}
            catch { 
			
			}
			Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
			{
				if (Device.Idiom == TargetIdiom.Tablet)
				{
					var ret = await DependencyService.Get<IFileHelper>().dispAlert("Spirometer", message, true, "OK", null);
				}
				else
				{
					var ret = await DependencyService.Get<IFileHelper>().dispAlert("Spirometer", message, false, "OK", null);
				}
			}));
			//DisplayAlert("Spirometer", message, "OK");
		}

		async void btnSaveCLicked(object sender, System.EventArgs e)
		{
			layoutLoading.IsVisible = true;
			lblLoadingMessage.Text = "Saving Calibrated Readings.";

			try
			{
				SpirometerReading highestReading = getHighestReading();

				// saving it to the local storage
				//Demographics.sharedInstance.calibratedReading = highestReading;
				//Demographics.sharedInstance.saveCalibratedReadig();

				Demographics.sharedInstance.saveCalibratedReadig(highestReading);

				Reading fevReading = new Reading("FEV1", highestReading.Fev1, 9, false, null, null);
				Reading pefReading = new Reading("PEF", highestReading.Pef, 9, false, null, null);


				logcalParameteritem.localspirometerList.Insert(0, new SpirometerReading(fevReading.Date, highestReading.Pef, highestReading.Fev1));
				await pefReading.PostReadingToService();
				await fevReading.PostReadingToService();
				await this.Navigation.PopAsync();
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Exception occured while savind the pef and fev to server");
			}
			finally
			{
				layoutLoading.IsVisible = false;
			}
		}

		void DeleteClicked(object sender, System.EventArgs e)
		{
			var btn = (Xamarin.Forms.Button)sender;

			System.Diagnostics.Debug.WriteLine("Remove calibrated item "+(int)btn.CommandParameter);

			calibratedReadingList.RemoveAt((int)btn.CommandParameter);

			// update the index value for reading display on left of each row
			int count = 0;
			foreach (var rdn in calibratedReadingList) {
				rdn.index = count++; 
			}

			if (this.calibratedReadingList.Count < 3)
			{
				layoutLoading.IsVisible = true;
				//bleManager.ScanToConnectToSpotCheck(this);
				//DependencyService.Get<ICBCentralManagerSpirometer>().connectToSpirometer(this);
				BLECentralManager.sharedInstance.connectToDevice("BLE-MSA", this);
				lblLoadingMessage.Text = "Please, take " + (3 - calibratedReadingList.Count) + " more reading.";
			}
		}

		private SpirometerReading getHighestReading() {


			SpirometerReading highestReading = calibratedReadingList[0];

			foreach (var redn in calibratedReadingList) {
				if (highestReading.Pef < redn.Pef) {
					highestReading = redn;
				}
			}

			return highestReading;
		}
	}
}
