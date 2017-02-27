﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public partial class RespCalibrationPage : ContentPage,BLEReadingUpdatableSpiroMeter
	{

		ObservableCollection<SpirometerReading> calibratedReadingList = new ObservableCollection<SpirometerReading>();
		public RespCalibrationPage()
		{
			InitializeComponent();
		}

		void btnCalibrateClicked(object sender, System.EventArgs e)
		{
			if (this.calibratedReadingList.Count < 3)
			{
				layoutLoading.IsVisible = true;
				DependencyService.Get<ICBCentralManagerSpirometer>().connectToSpirometer((BLEReadingUpdatableSpiroMeter)this);

				lblLoadingMessage.Text = "Please, take " + (3 - calibratedReadingList.Count) + " more reading.";
			}
			else {
				DisplayAlert("Calibration", "Readings taken are sufficient for calibration. If you want to take more readings, Please, delete the unwanted row and take reading again.", "OK");
			}
		}

		// call back methods
		public void updateCaller(SpirometerReading currReading)
		{
			currReading.index = calibratedReadingList.Count;
			calibratedReadingList.Add(currReading);

			System.Diagnostics.Debug.WriteLine("loaded spirometer reading:" + currReading.pefString);

			if (this.calibratedReadingList.Count < 3)
			{
				lblLoadingMessage.Text = "Please, take " + (3 - calibratedReadingList.Count) + " more reading.";
				DependencyService.Get<ICBCentralManagerSpirometer>().connectToSpirometer((BLEReadingUpdatableSpiroMeter)this);
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
			calibratedReadingList.RemoveAt((int)btn.CommandParameter);

			if (this.calibratedReadingList.Count < 3)
			{
				layoutLoading.IsVisible = false;
				DependencyService.Get<ICBCentralManagerSpirometer>().connectToSpirometer(this);

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
