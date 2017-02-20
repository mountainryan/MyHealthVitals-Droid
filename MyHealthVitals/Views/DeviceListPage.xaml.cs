using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public partial class DeviceListPage : ContentPage,IBluetoothCallBackUpdatable
	{
		void btnPC300clicked(object sender, System.EventArgs e)
		{
			var newScreen = new MainPage();
			newScreen.Title = "Main Screeen";
			this.Navigation.PushAsync(newScreen);

			//btnMyButton.ab
		}

		public DeviceListPage()
		{
			InitializeComponent();

			//DependencyService.Get<ICBCentralManager>().ConnectToDevice((IBluetoothCallBackUpdatable)this);
		}

		// IBluetoothCallBackUpdatable methods

		public void ShowMessageOnUI(String message, Boolean isConnected) { }
		public void SPO2_readingCompleted(int sp02, int bpm, float perfusionIndex) {}
		public void SYS_DIA_BPM_updated(int bpsys, int bpdia, int bpm) { }
		public void updatingPressureMeanTime(int pressure) { }
		public void updateTemperature(decimal temperature, String type) { }
		public void noticeEndOfReadingSpo2() { }
		public void updateDeviceConnected(String deviceName, bool isConnected) {
			System.Diagnostics.Debug.WriteLine("deviceName: " + deviceName);
		}

		public void updateGlucoseReading(decimal gluReading, string unit)
		{
			
		}

		public void updateECGResult(List<int> ecgPacket) { 
			
		}
	}
}
