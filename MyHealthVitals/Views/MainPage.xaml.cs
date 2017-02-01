using System;
using System.Collections.Generic;

using Xamarin.Forms;
using System.Diagnostics;
using System.IO;

namespace MyHealthVitals
{
	public partial class MainPage : ContentPage
	{
		void Handle_Clicked1(object sender, System.EventArgs e)
		{
			Debug.WriteLine("handle log out");
			//this.dis
			//Remo
			this.Navigation.PopModalAsync(true);
		}

		public MainPage()
		{
			InitializeComponent();
			this.layoutLoading.IsVisible = false;
		}

		protected async override void OnAppearing()
		{
			base.OnAppearing();

			// calling async to get all demographics user details 
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

		private BleManager bleManager;
		private VitalsData vitalsData;

		void Handle_Clicked(object sender, System.EventArgs e)
		{
			bleManager = new BleManager();
			vitalsData = new VitalsData();
			vitalsData.OnBmpChange += Instance_OnBmpChange;
			vitalsData.OnSpO2Change += Instance_OnSpO2Change;
			vitalsData.OnBPSysChange += VitalsData_OnBPSysChange;
			vitalsData.OnBPDiaChange += VitalsData_OnBPDiaChange;
			vitalsData.OnTempChange += VitalsData_OnTempChange;
			bleManager.Connect(vitalsData);

			this.layoutLoading.IsVisible = true;
		}


		void btnFareinheitClicked(Object sender, System.EventArgs e)
		{
			Debug.WriteLine("F");

			this.btnFareinheit.BackgroundColor = Color.White;
			this.btnCelcious.BackgroundColor = Color.Blue;
		}

		void btnCelciusClicked(Object sender, System.EventArgs e)
		{
			this.btnCelcious.BackgroundColor = Color.White;
			this.btnFareinheit.BackgroundColor = Color.Blue;
		}

		void btnLbsClicked(Object sender, System.EventArgs e)
		{
			this.btnCelcious.BackgroundColor = Color.White;
			this.btnFareinheit.BackgroundColor = Color.Blue;
		}

		void btnKgsClicked(Object sender, System.EventArgs e)
		{
			this.btnCelcious.BackgroundColor = Color.White;
			this.btnFareinheit.BackgroundColor = Color.Blue;
		}


		#region Update Vitals
		private void VitalsData_OnTempChange(int val)
		{

			Debug.WriteLine(string.Format("BPDia was updated to: {0}", val));
			//Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			//{
			//	lblSys.Text = val.ToString();
			//	layoutLoading.IsVisible = false;
			//});

			//var reading = new Reading();
			//reading.CategoryId = 4;
			//reading.Date = vitalsData.Date;
			//reading.Source = "Device";
			//reading.EnglishValue = val;

			//await reading.PostAsync(LoginViewController.credential);
		}

		private void VitalsData_OnBPDiaChange(int val)
		{
			Debug.WriteLine(string.Format("BPDia was updated to: {0}", val));

			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{

				Debug.WriteLine("in main tread");

				lblDia.Text = val.ToString();
				layoutLoading.IsVisible = false;
				lblBpm.Text = this.vitalsData.Bpm.ToString();
			});

			//InvokeOnMainThread(() =>
			//{
			//	// manipulate UI controls
			//	lblBPDia.Text = val.ToString();
			//});

			//this.ma

			//var reading = new Reading();
			//reading.CategoryId = 1;
			//reading.Date = vitalsData.Date;
			//reading.Source = "Device";
			//reading.EnglishValue = val;
			//reading.ValueType = "Diastolic";

			//await reading.PostAsync(LoginViewController.credential);
		}

		private void VitalsData_OnBPSysChange(int val)
		{
			Debug.WriteLine(string.Format("BPSys was updated to: {0}", val));

			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				Debug.WriteLine("in main tread");

				lblSys.Text = val.ToString();
				layoutLoading.IsVisible = false;
				lblBpm.Text = this.vitalsData.Bpm.ToString();
			});

			//InvokeOnMainThread(() =>
			//{
			//	// manipulate UI controls
			//	lblBPSys.Text = val.ToString();
			//});

			//var reading = new Reading();
			//reading.CategoryId = 1;
			//reading.Date = vitalsData.Date;
			//reading.Source = "Device";
			//reading.EnglishValue = val;
			//reading.ValueType = "Systolic";

			//await reading.PostAsync(LoginViewController.credential);
		}

		private DateTime lastSpO2Reading = DateTime.MinValue;
		private void Instance_OnSpO2Change(int val)
		{
			Debug.WriteLine(string.Format("SpO2 was updated to: {0}", val));
			//InvokeOnMainThread(() =>
			//{
			//	// manipulate UI controls
			//	lblSpO2.Text = val.ToString();
			//});

			//TimeSpan span = lastSpO2Reading.Subtract(DateTime.Now);
			//if (span.Minutes < -1)
			//{
			//	lastSpO2Reading = DateTime.Now;
			//	var reading = new Reading();
			//	reading.CategoryId = 2;
			//	reading.Date = vitalsData.Date;
			//	reading.Source = "Device";
			//	reading.EnglishValue = val;

			//	//await reading.PostAsync(LoginViewController.credential);
			//}


		}

		private DateTime lastBpReading = DateTime.MinValue;
		private void Instance_OnBmpChange(int val)
		{
			Debug.WriteLine(string.Format("BPM was updated to: {0}", val));


			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				//Debug.WriteLine("in main tread");

				//lblSys.Text = val.ToString();
				layoutLoading.IsVisible = false;
				lblBpm.Text = val.ToString();
			});

			//InvokeOnMainThread(() =>
			//{
			//	// manipulate UI controls
			//	lblBpm.Text = val.ToString();
			//});

			//TimeSpan span = lastBpReading.Subtract(DateTime.Now);
			//if (span.Minutes < -1)
			//{
			//	lastBpReading = DateTime.Now;
			//	var reading = new Reading();
			//	reading.CategoryId = 3;
			//	reading.Date = vitalsData.Date;
			//	reading.Source = "Device";
			//	reading.EnglishValue = val;

			//	//await reading.PostAsync(LoginViewController.credential);
			//}
		}
		#endregion
	}
}
