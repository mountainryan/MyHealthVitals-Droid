using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Diagnostics;
using System.IO;

namespace MyHealthVitals
{
	public partial class MainPage : ContentPage, IBluetoothCallBackUpdatable
	{
		private VitalsData vitalsData = new VitalsData();

		public MainPage()
		{
			InitializeComponent();
			// calling to start connecting the device this this should be implemented differently in android because it is calling the native API
			Xamarin.Forms.Device.StartTimer(TimeSpan.FromMilliseconds(250), () =>
			{

				btnFareinheit.TextColor = (Color)App.Current.Resources["colorThemeBlue"];
				btnCelcious.TextColor = Color.Gray;
				isCelcious = false;

				Debug.WriteLine("searcching decice...");
				DependencyService.Get<ICBCentralManager>().ConnectToDevice((IBluetoothCallBackUpdatable)this);
				return false;
			});
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

		//void btnSaveClicked(object sender, System.EventArgs e)
		//{
		//	vitalsData.save();
		//}

		void btnBleClicked(Object sender, System.EventArgs e)
		{
			//Debug.WriteLine(sender.is);
			if (this.btnBle.IsEnabled) { 
				DependencyService.Get<ICBCentralManager>().ConnectToDevice((IBluetoothCallBackUpdatable)this);
			}
		}

		public void ShowMessageOnUI(string message, Boolean isConnected)
		{
			//Debug.WriteLine(message);
			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				//layoutLoading.IsVisible = true;
				lblStatus.Text = message;

				if (isConnected)
				{
					btnBle.Image = "imgDevCon.png";
					btnBle.IsEnabled = false;
				}
				else { 
					btnBle.Image = "imgDevDiscon.png";
					btnBle.IsEnabled = true;
				}
				//hideMessageWthDelay();
			});
		}

		public void noticeEndOfReadingSpo2() {
			//vitalsData.sendToServer_SPO2_PI_BPM();
		}

		public void updateTemperature(decimal temperature, String type) {
			vitalsData.temperature = new Reading("Temperature", temperature, 4);
			vitalsData.sendToServerTemperature();

			Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {

				if (isCelcious)
				{
					lblTemperature.Text = ConvertFahrenheitToCelsius((double)this.vitalsData.temperature.EnglishValue).ToString();
				}
				else {
					lblTemperature.Text = temperature.ToString();
				}
			});
		}

		//public void hideMessageWthDelay()
		//{
		//	Xamarin.Forms.Device.StartTimer(TimeSpan.FromMilliseconds(20000), () => {
		//		Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
		//			{
		//				//layoutLoading.IsVisible = false;
		//			});
		//		return true;
		//	});
		//}

		public void updatingPressureMeanTime(int pressure)
		{
			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				lblPressure.Text = pressure.ToString() + " mmHg";
			});
		}

		public void SPO2_readingCompleted(int sp02, int bpm, float perfusionIndex)
		{
			this.vitalsData.bpDia = new Reading("Oxygen", sp02,2);
			//this.vitalsData.bpSys = new Reading("Perfusion Index", perfusionIndex,2);
			this.vitalsData.bpSys = new Reading("Hearth Rate", bpm,3);

			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				if (bpm == 0)
				{
					lblBpm.Text = "."+lblBpm.Text;
				}
				else { 
					lblBpm.Text = bpm.ToString();	
				}

				if (sp02 == 0)
				{
					lblSpo2.Text = lblSpo2.Text+".";
				}
				else {
					lblSpo2.Text = sp02.ToString();
				}

				if (perfusionIndex > 0)
				{
					lblPerfusionIndex.Text = perfusionIndex.ToString();
				}
				else {
					lblPerfusionIndex.Text = lblPerfusionIndex.Text +".";
				}
			});
		}

		public void SYS_DIA_BPM_updated(int bpsys, int bpdia, int bpm)
		{

			this.vitalsData.bpDia = new Reading("Diastolic", bpdia,1);
			this.vitalsData.bpSys = new Reading("Systolic", bpsys,1);
			this.vitalsData.bpSys = new Reading("Hearth Rate", bpm,3);

			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				
				lblBpm.Text = bpm.ToString();
				lblDia.Text = bpdia.ToString();
				lblSys.Text = bpsys.ToString();

				//layoutLoading.IsVisible = false;
			});
		}

		//public static double ConvertCelsiusToFahrenheit(double c)
		//{
		//	return ((9.0 / 5.0) * c) + 32;
		//}

		public static double ConvertFahrenheitToCelsius(double f)
		{
			return Math.Round((5.0 / 9.0) * (f - 32),1);
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
			var newPage = new ParametersPage();
			newPage.Title = "Parameter List Screen";
			this.Navigation.PushAsync(newPage);
		}

		void btnViewProfileClicked(object sender, System.EventArgs e)
		{
			Debug.WriteLine("code to view profile detail");
		}

		void btnFareinheitClicked(Object sender, System.EventArgs e)
		{
			btnFareinheit.TextColor = (Color)App.Current.Resources["colorThemeBlue"];
			btnCelcious.TextColor = Color.Gray;
			isCelcious = false;

			try
			{
				lblTemperature.Text = this.vitalsData.temperature.EnglishValue.ToString();
			}
			catch (Exception){
				Debug.WriteLine("exception nullpointer");
			}
		}
		bool isCelcious = false;
		void btnCelciusClicked(Object sender, System.EventArgs e)
		{
			isCelcious = true;
			btnFareinheit.TextColor = Color.Gray; 
			btnCelcious.TextColor = (Color)App.Current.Resources["colorThemeBlue"];

			try
			{
				lblTemperature.Text = ConvertFahrenheitToCelsius((double)this.vitalsData.temperature.EnglishValue).ToString();
			}
			catch(Exception) {
				Debug.WriteLine("conversion exception");
			}
		}

		bool isKg = true;
		void btnLbsClicked(Object sender, System.EventArgs e)
		{
			isKg = false;
			btnLbs.TextColor = (Color)App.Current.Resources["colorThemeBlue"];
			btnKgs.TextColor = Color.Gray;
		}

		void btnKgsClicked(Object sender, System.EventArgs e)
		{
			isKg = true;
			btnKgs.TextColor = (Color)App.Current.Resources["colorThemeBlue"];
			btnLbs.TextColor = Color.Gray;
		}
	}
}
