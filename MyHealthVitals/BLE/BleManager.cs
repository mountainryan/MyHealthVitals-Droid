using Plugin.BLE;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Plugin.BLE.Abstractions.Contracts;

namespace MyHealthVitals
{
	public class BleManager
	{
		//public IAdapter Adapter;
		private IDevice connectedDevice;

		//private UIViewController nextView;
		//private UINavigationController nav;
		//private VitalsData vitalsData;

		private BluetoothCallBackUpdatable uiController;
		private IBluetoothLE ble;
		//private IAdapter adapter;
		private static int countBLEmanagerCreatedNumber = 0;

		//public BleManager() {

		//}
		public void disconnectDevice()
		{
			//if (this.connectedDevice != null)
			//{
			//	CrossBluetoothLE.Current.Adapter.DisconnectDeviceAsync(this.connectedDevice);
			//}
		}

		public void connect(object caller)
		{
			uiController = (BluetoothCallBackUpdatable)caller;

			if (countBLEmanagerCreatedNumber > 0)
			{

				//List<IDevice> devices = new List<IDevice>(CrossBluetoothLE.Current.Adapter.GetSystemConnectedOrPairedDevices());

				//foreach (var device in devices)
				//{
				//	if (device.Name == "PC_300SNT")
				//	{
				//		//CrossBluetoothLE.Current.Adapter.di
				//		if (device.State == Plugin.BLE.Abstractions.DeviceState.Connected)
				//		{
				//			device.GetServicesAsync();
				//		}
				//	}
				//}

				if (this.connectedDevice == null)
				{
					CrossBluetoothLE.Current.Adapter.StartScanningForDevicesAsync();
				}
				else {
					this.connectedDevice.GetServiceAsync(this.connectedDevice.Id);
				}

			}
			else {

				countBLEmanagerCreatedNumber++;
				ble = CrossBluetoothLE.Current;
				ble.StateChanged += Bluetooth_StateChanged; ;
				//this.adapter = CrossBluetoothLE.Current.Adapter;

				CrossBluetoothLE.Current.Adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
				CrossBluetoothLE.Current.Adapter.DeviceConnected += Adapter_DeviceConnected;
				//Adapter.ConnectToKnownDeviceAsync(new Guid("12dcfd17-01b1-8a1b-3e0f-26811f129d4f"))
				CrossBluetoothLE.Current.Adapter.DeviceDisconnected += Adapter_DeviceDisconnected;
				//Adapter.ScanTimeout = 5000
				CrossBluetoothLE.Current.Adapter.DeviceConnectionLost += Adapter_connectionLost;
				CrossBluetoothLE.Current.Adapter.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
				CrossBluetoothLE.Current.Adapter.StartScanningForDevicesAsync();
			}
		}

		private void Bluetooth_StateChanged(object sender, Plugin.BLE.Abstractions.EventArgs.BluetoothStateChangedArgs e)
		{
			Debug.WriteLine("my bluetooth State: " + ble.State);
			this.connectedDevice = null;
			if (ble.State == BluetoothState.On)
			{

			}
			//countBLEmanagerCreatedNumber = 0;
		}

		private void Adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{
			var dev = e.Device.Name;

			if (dev == "PC_300SNT")
			{
				CrossBluetoothLE.Current.Adapter.StopScanningForDevicesAsync();
				CrossBluetoothLE.Current.Adapter.ConnectToDeviceAsync(e.Device);
				this.connectedDevice = e.Device;

				Debug.WriteLine(string.Format("Device Found: {0}", e.Device.Id));

				this.uiController.ShowMessageOnUI("PC300 SNT is connected.");
			}

			//e.Device.
		}

		void Adapter_ScanTimeoutElapsed(object sender, EventArgs e)
		{
			this.uiController.ShowMessageOnUI("time out please try to connect again.");
		}

		private void Adapter_connectionLost(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{

			//this.disconnectDevice();
			this.uiController.ShowMessageOnUI("we lost connection to the device please connect again to take reading." + e.Device.Name);

			//this.uiController.ShowMessageOnUI("we lost connection to the device please connect again to take reading");
		}

		private void Adapter_DeviceDisconnected(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{
			this.uiController.ShowMessageOnUI("Please turn on the device to reading: " + e.Device.Name);

			//this.uiController.ShowMessageOnUI("Please turn on the device to reading: " + e.Device.Name);
		}

		private async void Adapter_DeviceConnected(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{

			if (e.Device.Name == "PC_300SNT") {
				this.uiController.ShowMessageOnUI("PC_300SNT Connected.");
			}

			var services = await e.Device.GetServicesAsync();

			try
			{
				foreach (var s in services)
				{
					var characteristics = await s.GetCharacteristicsAsync();
					foreach (var c in characteristics)
					{
						//Debug.WriteLine(string.Format("Char UUID: {0}  Value: {1}", c.Uuid, c.Value));
						if (c.CanUpdate)
						{
							c.ValueUpdated += C_ValueUpdated;
							c.StartUpdatesAsync();
						}
					}
				}
			}
			catch (InvalidOperationException ex)
			{
				Debug.WriteLine("invallid operation exception: " + ex.Message);
			}
			catch (NullReferenceException ex)
			{
				Debug.WriteLine("NullReferenceException: " + ex.Message);
			}

			if (OnDeviceConnected != null)
				OnDeviceConnected();

		}

		private void C_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
		{


			//if(e.Characteristic.Value.Length == 

			//e.Characteristic.w

			//this.Adapter.s

			//token

			//Debug.WriteLine("token: " + e.Characteristic.Value[2]);

			if ((int)e.Characteristic.Value[2] > 63 && (int)e.Characteristic.Value[2] < 68)
			{
				Debug.WriteLine("NIBP related token.");

				this.uiController.ShowMessageOnUI("Measuring the Blood pressure...");

				//vitalsData.Bpm = (int)e.Characteristic.Value[9];
				//vitalsData.BPSys = (int)e.Characteristic.Value[6];
				//vitalsData.BPDia = (int)e.Characteristic.Value[8];

				if (e.Characteristic.Value.Length > 21) { 
					this.uiController.SYS_DIA_BPM_updated((int)e.Characteristic.Value[6], (int)e.Characteristic.Value[8], (int)e.Characteristic.Value[9]);
					this.uiController.ShowMessageOnUI("Boood pressure read succesfully.");
				}

				if (e.Characteristic.Value.Length == 8) {
					this.uiController.updatingPressureMeanTime((int)e.Characteristic.Value[6]);
				}
			}

			//if ((int)e.Characteristic.Value[2] > 111 && (int)e.Characteristic.Value[2] < 115)
			//{
			//	Debug.WriteLine("Temparature related token.");
			//}

			if ((int)e.Characteristic.Value[2] > 80 && (int)e.Characteristic.Value[2] < 84)
			{
				//Debug.WriteLine(" Spo2 related token");

				if (e.Characteristic.Value.Length == 19)
				{
					//vitalsData.Bpm = (int)e.Characteristic.Value[10];
					//vitalsData.SpO2 = (int)e.Characteristic.Value[5];
					//vitalsData.Bpm = (int)e.Characteristic.Value[6];


					this.uiController.SPO2_readingCompleted((int)e.Characteristic.Value[5], (int)e.Characteristic.Value[6]);

					//this.uiController.
				}
			}

			if (e.Characteristic.Value.Length > 10 && e.Characteristic.Value.Length < 22)
			{
				//Debug.WriteLine("spo2: " + e.Characteristic.Value[5] + " BPM: " + e.Characteristic.Value[6]);
			}

			if (e.Characteristic.Value.Length == 19)
			{
				//Debug.WriteLine("spo2: " + e.Characteristic.Value[5] + " BPM: " + e.Characteristic.Value[6]);
			}

			if (e.Characteristic.Value.Length == 14) { 
				String message = "";
				switch (127 & (int)e.Characteristic.Value[5])
				{
					case 0:
						message = "NO pulse is detected.";
						break;
					case 1:
						message = "the cuff pressure does not reach 30 mmhg within 7 seconds. Probably the cuff is not wrapped well.";
						break;
					case 2:
						message = "Over pressure";
						break;
					case 3:
						message = "no pulse detected";
						break;
					case 4:
						message = "Too much motion artifects.";
						break;
					case 5:
						message = "Invallid result is obtained.";
						break;
					case 6:
						message = "Air leakage occured.";
						break;
					case 7:
						message = "Self - checking failed, probably transducer or A/ D sampling error. ";
						break;
					case 8:
						message = "Pressure error, probably valve can't open normally.";
						break;
					case 9:
						message = "signal saturation, caused by movement or other reason yielding too big signal amplitude";
						break;
					case 10:
						message = "Air leakage in airway leakage checking.";
						break;
					case 11:
						message = "Hardware or software fault.";
						break;
					case 12:
						message = "measurement exceeds the specified time limits, 120s for adults with cuff pressure over 200 mmHg, 90s for adults with cuff pressure under 200 mmhg; 90s for neonate";
						break;
				}

				this.uiController.ShowMessageOnUI(message);
			}

			//// sys , dia and bpm is available in spot check monitor
			//if (e.Characteristic.Value.Length > 21)
			//{
			//	//vitalsData.Bpm = (int)e.Characteristic.Value[9];
			//	//vitalsData.BPSys = (int)e.Characteristic.Value[6];
			//	//vitalsData.BPDia = (int)e.Characteristic.Value[8];
			//	Debug.WriteLine("inside 22 byte: " + e.Characteristic.Value[6]);
			//	Debug.WriteLine("check content of error byete when there is no error: " + (27 & (int)e.Characteristic.Value[5]));
			//	this.uiController.SYS_DIA_BPM_updated();
			//}

			//if (e.Characteristic.Value.Length == 9)
			//{
			//	//vitalsData.Temp = 98;
			//}

			List<int> values = new List<int>();
			foreach (var b in e.Characteristic.Value)
			{
				values.Add(b);
			}

			int count = 0;
			var sb = new StringBuilder();
			foreach (var itm in values)
			{
				//if (count >= values.Count - 3)
				//{
				sb.Append(itm).Append(",");
				//}
				count++;
			}

			Debug.WriteLine(string.Format("UUID: {0}  ->{1}", e.Characteristic.Uuid, sb.ToString()));
		}

		public delegate void DeviceConnectedEventHandler();
		public event DeviceConnectedEventHandler OnDeviceConnected;
	}
}
