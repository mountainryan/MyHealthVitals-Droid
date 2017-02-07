using System;
using System.Collections.Generic;

using Xamarin.Forms;
using System.Diagnostics;
using System.IO;

namespace MyHealthVitals
{
	public interface BluetoothCallBackUpdatable
	{
		void ShowMessageOnUI(String message);
		void SPO2_readingCompleted(int sp02, int bpm, int perfusionIndex);
		void SYS_DIA_BPM_updated(int bpsys, int bpdia, int bpm);
		void updatingPressureMeanTime(int pressure);
	}

	public partial class MainPage : ContentPage, BluetoothCallBackUpdatable
	{
		void btnBleClicked(Object sender, System.EventArgs e)
		{
			//Debug.WriteLine(sender.is);
			if (this.btnBle.IsEnabled) { 
				DependencyService.Get<ICBCentralManager>().ConnectToDevice((BluetoothCallBackUpdatable)this);
			}
		}

		public void ShowMessageOnUI(string message)
		{
			//Debug.WriteLine(message);
			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				//layoutLoading.IsVisible = true;
				lblStatus.Text = message;

				if (message == "Connected.")
				{
					this.btnBle.Image = "imgBluetooth.png";
					this.btnBle.IsEnabled = false;
				}
				else {
					this.btnBle.Image = "imgBleDisconnected.png";
					this.btnBle.IsEnabled = true;
				}
				//hideMessageWthDelay();
			});
		}

		public void hideMessageWthDelay()
		{
			Xamarin.Forms.Device.StartTimer(TimeSpan.FromMilliseconds(20000), () => {
				Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
					{
						//layoutLoading.IsVisible = false;
					});
				return true;
			});
		}

		public void updatingPressureMeanTime(int pressure)
		{
			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				lblPressure.Text = pressure.ToString() + " mmHg";
			});
		}

		public void SPO2_readingCompleted(int sp02, int bpm, int perfusionIndex)
		{
			//Debug.WriteLine("sp02: " + sp02 + " bpm: " + bpm);

			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				if (bpm == 0)
				{
					lblBpm.Text = "...";
				}
				else { 
					lblBpm.Text = bpm.ToString();	
				}

				if (sp02 == 0)
				{
					lblSpo2.Text = "...";
				}
				else {
					lblSpo2.Text = sp02.ToString();
				}

				if (perfusionIndex == 0)
				{
					lblPerfusionIndex.Text = "...";
				}
				else {
					lblPerfusionIndex.Text = perfusionIndex.ToString();
				}
			});
		}

		public void SYS_DIA_BPM_updated(int bpsys, int bpdia, int bpm)
		{
			Debug.WriteLine("sys: " + bpsys + " dia: " + bpdia + " bpm: " + bpm);

			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				
				lblBpm.Text = bpm.ToString();
				lblDia.Text = bpdia.ToString();
				lblSys.Text = bpsys.ToString();

				//layoutLoading.IsVisible = false;
			});
		}

		//private BleManager bleManager;
		//private VitalsData vitalsData;

		public MainPage()
		{
			InitializeComponent();
			//this.layoutLoading.IsVisible = false;

			//bleManager = new BleManager();
			//bleManager.connect(this);

			DependencyService.Get<ICBCentralManager>().ConnectToDevice((BluetoothCallBackUpdatable)this);
			//this.layoutLoading.IsVisible = true;
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			//this.bleManager.disconnectDevice();
			//this.bleManager.Adapter = null;
			//this.bleManager.Adapter.DisconnectDeviceAsync(this.bleManager.Adapter.);
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

			// bluetooth wor

			//bleManager.OnDeviceConnected += BleManager_OnDeviceConnected;
			//vitalsData = new VitalsData();
			//vitalsData.OnBmpChange += Instance_OnBmpChange;
			//vitalsData.OnSpO2Change += Instance_OnSpO2Change;
			//vitalsData.OnBPSysChange += VitalsData_OnBPSysChange;
			//vitalsData.OnBPDiaChange += VitalsData_OnBPDiaChange;
			//vitalsData.OnTempChange += VitalsData_OnTempChange;

		}

		// 
		void BleManager_OnDeviceConnected()
		{
			//if(this.bleManager.Adapter.up
			Debug.WriteLine("custom on device connected");
		}

		void btnLogOutClicked(object sender, System.EventArgs e)
		{
			Debug.WriteLine(" log out");
			this.Navigation.PopModalAsync(true);
		}
		void btnNIBPStartClicked(object sender, System.EventArgs e)
		{
			DependencyService.Get<ICBCentralManager>().startMeasuringBP();
		}

		void btnListClicked(object sender, System.EventArgs e)
		{
			var newPage = new ParameterListPage();
			newPage.Title = "Parameter List Screen";
			this.Navigation.PushAsync(newPage);
		}

		void btnViewProfileClicked(object sender, System.EventArgs e)
		{
			Debug.WriteLine("code to view profile detail");
		}

		void btnFareinheitClicked(Object sender, System.EventArgs e)
		{
			Debug.WriteLine("F");
			this.btnFareinheit.BackgroundColor = Color.White;
			this.btnCelcious.BackgroundColor = (Color)App.Current.Resources["colorThemeBlue"];
		}

		void btnCelciusClicked(Object sender, System.EventArgs e)
		{
			this.btnCelcious.BackgroundColor = Color.White;
			this.btnFareinheit.BackgroundColor = (Color)App.Current.Resources["colorThemeBlue"];
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

		//#region Update Vitals
		//private void VitalsData_OnTempChange(int val)
		//{

		//	Debug.WriteLine(string.Format("BPDia was updated to: {0}", val));
		//	//Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
		//	//{
		//	//	lblSys.Text = val.ToString();
		//	//	layoutLoading.IsVisible = false;
		//	//});

		//	//var reading = new Reading();
		//	//reading.CategoryId = 4;
		//	//reading.Date = vitalsData.Date;
		//	//reading.Source = "Device";
		//	//reading.EnglishValue = val;

		//	//await reading.PostAsync(LoginViewController.credential);
		//}

		//private void VitalsData_OnBPDiaChange(int val)
		//{
		//	Debug.WriteLine(string.Format("BPDia was updated to: {0}", val));

		//	Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
		//	{

		//		Debug.WriteLine("in main tread");

		//		lblDia.Text = val.ToString();
		//		layoutLoading.IsVisible = false;
		//		lblBpm.Text = this.vitalsData.Bpm.ToString();
		//	});

		//	//InvokeOnMainThread(() =>
		//	//{
		//	//	// manipulate UI controls
		//	//	lblBPDia.Text = val.ToString();
		//	//});

		//	//this.ma

		//	//var reading = new Reading();
		//	//reading.CategoryId = 1;
		//	//reading.Date = vitalsData.Date;
		//	//reading.Source = "Device";
		//	//reading.EnglishValue = val;
		//	//reading.ValueType = "Diastolic";

		//	//await reading.PostAsync(LoginViewController.credential);
		//}

		//private void VitalsData_OnBPSysChange(int val)
		//{
		//	Debug.WriteLine(string.Format("BPSys was updated to: {0}", val));

		//	Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
		//	{
		//		Debug.WriteLine("in main tread");

		//		lblSys.Text = val.ToString();
		//		layoutLoading.IsVisible = false;
		//		lblBpm.Text = this.vitalsData.Bpm.ToString();
		//	});

		//	//InvokeOnMainThread(() =>
		//	//{
		//	//	// manipulate UI controls
		//	//	lblBPSys.Text = val.ToString();
		//	//});

		//	//var reading = new Reading();
		//	//reading.CategoryId = 1;
		//	//reading.Date = vitalsData.Date;
		//	//reading.Source = "Device";
		//	//reading.EnglishValue = val;
		//	//reading.ValueType = "Systolic";

		//	//await reading.PostAsync(LoginViewController.credential);
		//}

		//private DateTime lastSpO2Reading = DateTime.MinValue;
		//private void Instance_OnSpO2Change(int val)
		//{
		//	Debug.WriteLine(string.Format("SpO2 was updated to: {0}", val));
		//	//InvokeOnMainThread(() =>
		//	//{
		//	//	// manipulate UI controls
		//	//	lblSpO2.Text = val.ToString();
		//	//});

		//	//TimeSpan span = lastSpO2Reading.Subtract(DateTime.Now);
		//	//if (span.Minutes < -1)
		//	//{
		//	//	lastSpO2Reading = DateTime.Now;
		//	//	var reading = new Reading();
		//	//	reading.CategoryId = 2;
		//	//	reading.Date = vitalsData.Date;
		//	//	reading.Source = "Device";
		//	//	reading.EnglishValue = val;

		//	//	//await reading.PostAsync(LoginViewController.credential);
		//	//}


		//}

		//private DateTime lastBpReading = DateTime.MinValue;
		//private void Instance_OnBmpChange(int val)
		//{
		//	Debug.WriteLine(string.Format("BPM was updated to: {0}", val));


		//	Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
		//	{
		//		//Debug.WriteLine("in main tread");

		//		//lblSys.Text = val.ToString();
		//		layoutLoading.IsVisible = false;
		//		lblBpm.Text = val.ToString();
		//	});

		//	//InvokeOnMainThread(() =>
		//	//{
		//	//	// manipulate UI controls
		//	//	lblBpm.Text = val.ToString();
		//	//});

		//	//TimeSpan span = lastBpReading.Subtract(DateTime.Now);
		//	//if (span.Minutes < -1)
		//	//{
		//	//	lastBpReading = DateTime.Now;
		//	//	var reading = new Reading();
		//	//	reading.CategoryId = 3;
		//	//	reading.Date = vitalsData.Date;
		//	//	reading.Source = "Device";
		//	//	reading.EnglishValue = val;

		//	//	//await reading.PostAsync(LoginViewController.credential);
		//	//}
		//}


		//#endregion
	}
}
