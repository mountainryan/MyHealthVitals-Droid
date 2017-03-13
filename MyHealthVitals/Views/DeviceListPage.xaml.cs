using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public partial class DeviceListPage : ContentPage, IBluetoothCallBackUpdatable
	{

		private String activeDeviceName = "";
		bool isNavigated = false;

		void btnPC100Clicked(object sender, System.EventArgs e)
		{
			//BLECentralManager.sharedInstance.connectToDevice("PC-100", this);

			activeDeviceName = "PC-100";

			try
			{
				if (BLECentralManager.sharedInstance.pc100ServHandler.connectedDevice.State == Plugin.BLE.Abstractions.DeviceState.Connected)
				{
					navigateToMainPage();
				}
				else {
					layoutLoadingDevice.IsVisible = true;
					BLECentralManager.sharedInstance.connectToDevice(activeDeviceName, this);
				}
			}
			catch
			{
				layoutLoadingDevice.IsVisible = true;
				BLECentralManager.sharedInstance.connectToDevice(activeDeviceName, this);
			}
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			isNavigated = false;
		}

		void btnSpirometerClicked(object sender, System.EventArgs e)
		{
			var newScreen = new RespHomePage();
			newScreen.Title = "Main Screen";
			this.Navigation.PushAsync(newScreen);
		}

		void btnPC300clicked(object sender, System.EventArgs e)
		{
			activeDeviceName = "PC_300SNT";

			try
			{
				if (BLECentralManager.sharedInstance.spotServHandler.connectedDevice.State == Plugin.BLE.Abstractions.DeviceState.Connected)
				{
					navigateToMainPage();
				}
				else {
					layoutLoadingDevice.IsVisible = true;
					BLECentralManager.sharedInstance.connectToDevice(activeDeviceName, this);
				}
			}
			catch { 
				layoutLoadingDevice.IsVisible = true;
				BLECentralManager.sharedInstance.connectToDevice(activeDeviceName, this);
			}
		}

		public void updateControllerWithMessage(String message,bool isConnected) { 
			
		}

		void btnCancelTakeReadingClicked(object sender, System.EventArgs e) {
			layoutLoadingDevice.IsVisible = false;
		}

		void btnLogOutClicked(object sender, System.EventArgs e)
		{
			//Debug.WriteLine(" log out");
			//Demographics.sharedInstance.clearLocalStorageOnLogout();
			//Demographics.sharedInstance.password = "";

			this.Navigation.PopModalAsync(true);
		}

		public DeviceListPage()
		{
			InitializeComponent();
			gifWebView.Source = DependencyService.Get<IBaseUrl>().Get() + "/gifContainer.html";
			//DependencyService.Get<ICBCentralManager>().ConnectToDevice((IBluetoothCallBackUpdatable)this);
		}

		public void navigateToMainPage() { 
			var newScreen = new MainPage(activeDeviceName);
			newScreen.Title = "Main Screeen";
			if(BLECentralManager.sharedInstance.spotServHandler!=null)
				BLECentralManager.sharedInstance.spotServHandler.updateController(newScreen);
			this.Navigation.PushAsync(newScreen);
		}

		public void ShowMessageOnUI(String message, Boolean isConnected) {

			layoutLoadingDevice.IsVisible = false;

			if (isConnected)
			{
				isNavigated = true;
				navigateToMainPage();
			}
			else {
				if (!isNavigated)
				{
					DisplayAlert(activeDeviceName, message, "OK");
				}
			}	
		}

		//void updateDeviceList(string devNama,string message, bool isConnected) { 
			
		//}

		public void SPO2_readingCompleted(int sp02, int bpm, float perfusionIndex) { }
		public void SYS_DIA_BPM_updated(int bpsys, int bpdia, int bpm) { }
		public void updatingPressureMeanTime(int pressure) { }
		public void updateTemperature(decimal temperature, String type) { }

		public void noticeEndOfReadingSpo2() { }
		public void updateDeviceConnected(String deviceName, bool isConnected) { }

		public void updateGlucoseReading(decimal gluReading, string unit) { }

		public void updateECGPacket(List<int> ecgPacket) { }
		public void updateECGEnded(int bpm) { }

		public void resetEcgDisplay() { }
	}
}
