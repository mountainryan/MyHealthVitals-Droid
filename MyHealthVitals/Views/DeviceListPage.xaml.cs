using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace MyHealthVitals
{
	public partial class DeviceListPage : ContentPage, IBluetoothCallBackUpdatable
	{
        //NavigationPage.SetHasNavigationBar(this, false);
		private String activeDeviceName = "";
		bool isNavigated = false;
		void btnScaleClicked(object sender, System.EventArgs e) 
		{
            NavigationPage.SetHasNavigationBar(this, false);
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
        public void FailedConn(String message, bool isConn, int camefrom){}

		void btnPC100Clicked(object sender, System.EventArgs e)
		{
			//BLECentralManager.sharedInstance.connectToDevice("PC-100", this);
            NavigationPage.SetHasNavigationBar(this, false);
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
            NavigationPage.SetHasNavigationBar(this, false);
			var newScreen = new RespHomePage();
			newScreen.Title = "Main Screen";
			this.Navigation.PushAsync(newScreen);
		}

		void btnPC300clicked(object sender, System.EventArgs e)
		{
            NavigationPage.SetHasNavigationBar(this, false);
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
            NavigationPage.SetHasNavigationBar(this, false);
			//NavigationPage.SetBackButtonTitle(this, "Back");
			InitializeComponent();
			//NavigationPage.SetBackButtonTitle(this, "Back");

			gifWebView.Source = DependencyService.Get<IBaseUrl>().Get() + "/gifContainer.html";

			if (Device.Idiom == TargetIdiom.Tablet) 
            {
                setPortrait();
			}
            else if (Device.Idiom == TargetIdiom.Phone)
            {
				setPortrait();
            }

		}

        /*
		protected override void OnSizeAllocated(double width, double height)
		{
            base.OnSizeAllocated(width, height);

            int pages = Navigation.NavigationStack.Count;
            Debug.WriteLine("Nav stack count = " + pages);

            if (width < height)
            {
                Debug.WriteLine("Application Is in portrait");
                if (pages == 1) { setPortrait(); }

            }
            else
            {
                Debug.WriteLine("Application Is in Landscape");
                if (pages == 1) { setLandscape(); }
            }
		}*/

        public void setLandscape()
        {
            Debug.WriteLine("Called DeviceListPage setLandscape()");
			if (Device.Idiom == TargetIdiom.Tablet)
			{
				FakeToolbar.HeightRequest = 75 * Screensize.heightfactor;
				//titlebtn.FontSize = 36 * Screensize.heightfactor;
				logoutbtn.FontSize = 30 * Screensize.heightfactor;
				btnfirst.Margin = new Thickness(0, 75 * Screensize.heightfactor, 0, 0);

				page.Spacing = 10 * Screensize.heightfactor;
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
				FakeToolbar.HeightRequest = 55 * Screensize.heightfactor;
				//titlebtn.FontSize = 24 * Screensize.heightfactor;
				logoutbtn.FontSize = 16 * Screensize.heightfactor;
				btnfirst.Margin = new Thickness(0, 55 * Screensize.heightfactor, 0, 0);
                page.Spacing = 0;//10 * Screensize.heightfactor;
				middle.Spacing = 150 * Screensize.widthfactor;
				btn300.WidthRequest = 150 * Screensize.widthfactor;
				btnweight.WidthRequest = 150 * Screensize.widthfactor;
				btnspi.FontSize = 16 * Screensize.heightfactor;
				btn300.FontSize = 16 * Screensize.heightfactor;
				btn100.FontSize = 16 * Screensize.heightfactor;
				btnweight.FontSize = 16 * Screensize.heightfactor;
			}
        }

        public void setPortrait()
        {
            Debug.WriteLine("Called DeviceListPage setPortrait()");
			if (Device.Idiom == TargetIdiom.Tablet)
			{
				FakeToolbar.HeightRequest = 75 * Screensize.heightfactor;
				//titlebtn.FontSize = 36 * Screensize.heightfactor;
				logoutbtn.FontSize = 30 * Screensize.heightfactor;
				btnfirst.Margin = new Thickness(0, 40 * Screensize.heightfactor, 0, 0);
				page.Spacing = 90 * Screensize.heightfactor;
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
				FakeToolbar.HeightRequest = 55 * Screensize.heightfactor;
				//titlebtn.FontSize = 24 * Screensize.heightfactor;
				logoutbtn.FontSize = 16 * Screensize.heightfactor;
				btnfirst.Margin = new Thickness(0, 20 * Screensize.heightfactor, 0, 0);
				page.Spacing = 40 * Screensize.heightfactor;
				middle.Spacing = 40 * Screensize.widthfactor;
				btn300.WidthRequest = 150 * Screensize.widthfactor;
				btnweight.WidthRequest = 150 * Screensize.widthfactor;
				btnspi.FontSize = 16 * Screensize.heightfactor;
				btn300.FontSize = 16 * Screensize.heightfactor;
				btn100.FontSize = 16 * Screensize.heightfactor;
				btnweight.FontSize = 16 * Screensize.heightfactor;
			}
        }

		public void navigateToMainPage() { 
            NavigationPage.SetHasNavigationBar(this, false);
			var newScreen = new MainPage(activeDeviceName);
			newScreen.isFromDeviceList = true;
			newScreen.Title = "Main Screen";
			//NavigationPage.SetBackButtonTitle(this, "Main Screen");
			this.Navigation.PushAsync(newScreen);
			//var nav = new NavigationPage(newScreen);
			//this.Navigation.PushModalAsync(nav);
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

        async public Task checkBattery() { }

		//	public void Showupdata(String message, Boolean isConnected) { }

		async public Task ShowConnection(String message, Boolean isConnected) { }
		
		public void noticeEndOfReadingSpo2() { }
		public void updateDeviceConnected(String deviceName, bool isConnected) { }

		public void updateGlucoseReading(decimal gluReading, string unit) { }

		public void updateECGPacket(List<int> ecgPacket) { }
		public void updateECGEnded(int bpm, int ecg) { }

		public void resetEcgDisplay() { }
		public void updateBpmWaveform(int bpm) { }
	}
}
