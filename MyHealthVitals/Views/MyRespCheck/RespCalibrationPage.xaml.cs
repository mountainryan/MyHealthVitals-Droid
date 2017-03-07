using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public partial class RespCalibrationPage : ContentPage,BLEReadingUpdatableSpiroMeter
	{

		ObservableCollection<SpirometerReading> calibratedReadingList = new ObservableCollection<SpirometerReading>();

		//BleManagerSpirometer bleManager = new BleManagerSpirometer();

		public RespCalibrationPage()
		{
			InitializeComponent();

			////calibratedReadingList.
			//var reading0 = new SpirometerReading(DateTime.Now, 456, 3.5m);
			//reading0.index = calibratedReadingList.Count;
			//calibratedReadingList.Add(reading0);

			//var reading1 = new SpirometerReading(DateTime.Now, 456, 3.5m);
			//reading1.index = calibratedReadingList.Count;
			//calibratedReadingList.Add(reading1);

			//var reading2 = new SpirometerReading(DateTime.Now, 456, 3.5m);
			//reading2.index = calibratedReadingList.Count;
			//calibratedReadingList.Add(reading2);

			////calibratedReadingList.CollectionChanged += (sender, e) => { 
				
			////};

			//listView.ItemsSource = calibratedReadingList;
		}

		void btnCalibrateClicked(object sender, System.EventArgs e)
		{
			if (this.calibratedReadingList.Count < 3)
			{
				layoutLoading.IsVisible = true;

				//bleManager.ScanToConnectToSpotCheck(this);
				BLECentralManager.sharedInstance.connectToDevice("BLE-MSA", this);
				//DependencyService.Get<ICBCentralManagerSpirometer>().connectToSpirometer((BLEReadingUpdatableSpiroMeter)this);

				lblLoadingMessage.Text = "Please, take " + (3 - calibratedReadingList.Count) + " more reading.";
			}
			else {
				DisplayAlert("Calibration", "Readings taken are sufficient for calibration. If you want to take more readings, Please, delete the unwanted row and take reading again.", "OK");
			}
		}

		// call back methods
		public void updateCaller(SpirometerReading currReading)
		{

			//var currReading = new SpirometerReading(DateTime.Now, pef, fev1);

			currReading.index = calibratedReadingList.Count;
			calibratedReadingList.Add(currReading);

			System.Diagnostics.Debug.WriteLine("loaded spirometer reading:" + currReading.pefString);

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
		}

		async void btnSaveCLicked(object sender, System.EventArgs e)
		{
			layoutLoading.IsVisible = true;
			lblLoadingMessage.Text = "Saving Calibrated Reading.";

			try
			{
				SpirometerReading highestReading = getHighestReading();

				// saving it to the local storage
				Demographics.sharedInstance.calibratedReading = highestReading;
				Demographics.sharedInstance.saveCalibratedReadig();

				Reading fevReading = new Reading("FEV1", highestReading.Fev1, 9);
				Reading pefReading = new Reading("PEF", highestReading.Pef, 9);

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

			System.Diagnostics.Debug.WriteLine((int)btn.CommandParameter);

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
