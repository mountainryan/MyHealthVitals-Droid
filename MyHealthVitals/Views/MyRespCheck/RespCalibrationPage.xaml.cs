﻿using System;
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
			//var reading0 = new SpirometerReading(DateTime.Now, 420, 3.1m);
			//reading0.index = calibratedReadingList.Count;
			//calibratedReadingList.Add(reading0);

			//var reading1 = new SpirometerReading(DateTime.Now, 300, 3.4m);
			//reading1.index = calibratedReadingList.Count;
			//calibratedReadingList.Add(reading1);

			//var reading2 = new SpirometerReading(DateTime.Now, 300, 3.5m);
			//reading2.index = calibratedReadingList.Count;
			//calibratedReadingList.Add(reading2);

			//////calibratedReadingList.CollectionChanged += (sender, e) => { 
			//////};
			//listView.ItemsSource = calibratedReadingList;
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
				string readings = (3 - calibratedReadingList.Count) > 1 ? "more readings." : "more reading.";
				lblLoadingMessage.Text = "Please, take " + (3 - calibratedReadingList.Count) + readings;
			}
			else {
				DisplayAlert("Calibration", "Readings taken are sufficient for calibration. If you want to take more readings, Please, delete the unwanted row and take reading again.", "OK");
			}
		}
		public void testAgainDialog()
		{
           DisplayAlert("Reading", "The FEV value is too low, please take reading again.", "OK");
	//		DisplayAlert("Test Again", "The FEV value is too low, please test again.", "OK");"
			BLECentralManager.sharedInstance.connectToDevice("BLE-MSA", this);
		
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

			DisplayAlert("Spirometer", message, "OK");
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
