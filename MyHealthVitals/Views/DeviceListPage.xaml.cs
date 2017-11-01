using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public partial class DeviceListPage : ContentPage, IBluetoothCallBackUpdatable
	{

		private String activeDeviceName = "";
		bool isNavigated = false;
		void btnScaleClicked(object sender, System.EventArgs e) 
		{
			activeDeviceName = "eBody-Scale";//"Headset";
            navigateToMainPage();
			/*
			try
			{
				if (BLECentralManager.sharedInstance.scaleServHandle.connectedDevice.State == Plugin.BLE.Abstractions.DeviceState.Connected)
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
			}*/
		}
		void btnPC100Clicked(object sender, System.EventArgs e)
		{
			//BLECentralManager.sharedInstance.connectToDevice("PC-100", this);

			activeDeviceName = "PC-100";
            navigateToMainPage();
			/*
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
			}*/
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
            navigateToMainPage();
			/*
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
			}*/
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

			BLECentralManager.sharedInstance.disConnectAll();


			ParametersPageLocal.allReadings = null;
			logcalParameteritem.localhashmap.Clear();
			logcalParameteritem.localspirometerList.Clear();
			this.Navigation.PopModalAsync(true);
		}

		public DeviceListPage()
		{
			InitializeComponent();

			gifWebView.Source = DependencyService.Get<IBaseUrl>().Get() + "/gifContainer.html";
			if (Device.Idiom == TargetIdiom.Tablet) 
            {
                page.Spacing = 120 * Screensize.heightfactor;
                middle.Spacing = 150 * Screensize.widthfactor;
                btn300.WidthRequest = 300 * Screensize.widthfactor;
                btnweight.WidthRequest = 300 * Screensize.widthfactor;
				btnspi.FontSize = 36 * Screensize.heightfactor;
				btn300.FontSize = 36 * Screensize.heightfactor;
				btn100.FontSize = 36 * Screensize.heightfactor;
				btnweight.FontSize = 36 * Screensize.heightfactor;
				PC100.Image = (FileImageSource)ImageSource.FromFile("PC100MonitorPad.png");
				PC300.Image = (FileImageSource)ImageSource.FromFile("PC300MonitorPad.png");
				weightScales.Image = (FileImageSource)ImageSource.FromFile("WeightScalesPad.png");
				Spiromter.Image = (FileImageSource)ImageSource.FromFile("SpiromterPad.png");
			}
            else if (Device.Idiom == TargetIdiom.Phone)
            {
				page.Spacing *= Screensize.heightfactor;
				middle.Spacing *= Screensize.widthfactor;
				btn300.WidthRequest *= Screensize.widthfactor;
				btnweight.WidthRequest *= Screensize.widthfactor;
				btnspi.FontSize *= Screensize.heightfactor;
				btn300.FontSize *= Screensize.heightfactor;
				btn100.FontSize *= Screensize.heightfactor;
				btnweight.FontSize *= Screensize.heightfactor;
            }
		}

		public void navigateToMainPage() { 
			var newScreen = new MainPage(activeDeviceName);
			newScreen.isFromDeviecList = true;
			newScreen.Title = "Main Screen";
			this.Navigation.PushAsync(newScreen);
		}

		public void ShowMessageOnUI(String message, Boolean isConnected, String title = null ) {


						Debug.WriteLine("ShowMessageOnUI  device list page  :"  );

			layoutLoadingDevice.IsVisible = false;

			if (isConnected)
			{
				isNavigated = true;
				navigateToMainPage();
			}
			else {
				if (!isNavigated)
				{
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
                    {
                        if (Device.Idiom == TargetIdiom.Tablet)
                        {
                            var ret = await DependencyService.Get<IFileHelper>().dispAlert(activeDeviceName, message, true, "OK", null);
                        }
                        else
                        {
                            var ret = await DependencyService.Get<IFileHelper>().dispAlert(activeDeviceName, message, false, "OK", null);
                        }
                        //DisplayAlert(activeDeviceName, message, "OK");
                    }));
				}
			}	
		}
		public void SaveEcgState(int state) { }
		public void SPO2_readingCompleted(int sp02, int bpm, float perfusionIndex) { }
		public void SYS_DIA_BPM_updated(int bpsys, int bpdia, int bpm) { }
		public void updated_Weight(decimal weight) { }
		public void updatingPressureMeanTime(int pressure) { }
		public void updateTemperature(decimal temperature) { }



		//	public void Showupdata(String message, Boolean isConnected) { }

		public void ShowConcetion(String message, Boolean isConnected) { }
		
		public void noticeEndOfReadingSpo2() { }
		public void updateDeviceConnected(String deviceName, bool isConnected) { }

		public void updateGlucoseReading(decimal gluReading, string unit) { }

		public void updateECGPacket(List<int> ecgPacket) { }
		public void updateECGEnded(int bpm, int ecg) { }

		public void resetEcgDisplay() { }
		public void updateBpmWaveform(int bpm) { }
	}
}
