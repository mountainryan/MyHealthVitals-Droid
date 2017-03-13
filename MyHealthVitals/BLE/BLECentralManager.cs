using Plugin.BLE;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Plugin.BLE.Abstractions.Contracts;
namespace MyHealthVitals
{

	public interface IBLEDeviceServiceHandler
	{
		void C_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e);
		void discoverServices(IDevice device);
		void reconnectToDevice(IDevice device);

		//object uiController;
	}

	public class BLECentralManager
	{
		//public static IAdapter Adapter = new IAdapter;

		public static BLECentralManager sharedInstance = new BLECentralManager();

		public List<IDevice> connectedDevices = new List<IDevice>();
		//public IDevice currentDevice;
		public string currentDeviceName;
		//public IBLEDeviceServiceHandler devServiceHandler;
		public SpirometerServiceHandler spiroServHandler;
		public SpotCheckServiceHandler spotServHandler;
		public DeviceListPage deviceListPage;

		//public get

		private BLECentralManager()
		{
			//CrossBluetoothLE.Current.Adapter = ;
			CrossBluetoothLE.Current.Adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
			CrossBluetoothLE.Current.Adapter.DeviceConnected += Adapter_DeviceConnected;
			CrossBluetoothLE.Current.Adapter.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
			CrossBluetoothLE.Current.Adapter.DeviceConnectionLost += Adapter_DeviceConnectionLost;
			//CrossBluetoothLE.Current.s

			//connec

			Debug.WriteLine("bluetooth adapter initialized.");

			spiroServHandler = new SpirometerServiceHandler();
			spotServHandler = new SpotCheckServiceHandler();
		}

		public void connectToDevice(String deviceName, object controller)
		{
			currentDeviceName = deviceName;

			switch (deviceName)
			{
				case "BLE-MSA":
					{
						spiroServHandler.uiController = (BLEReadingUpdatableSpiroMeter)controller;
						break;
					}

				case "PC_300SNT":
					{
						spotServHandler.uiController = (IBluetoothCallBackUpdatable)controller;
						break;
					}
			}

			if (!checkIfDeviceScanned(deviceName))
			{
				// the device is not in the scanned list now scan to find the desired device and then connnect
				CrossBluetoothLE.Current.Adapter.StartScanningForDevicesAsync();

				//if (deviceName == "PC_300SNT") spotServHandler.uiController.ShowMessageOnUI("Searching device...", false);

			}
			else {
				
				switch (deviceName)
				{
					case "BLE-MSA":
						{
							spiroServHandler.reconnectToDevice(getCurrentDevice());
							break;
						}

					case "PC_300SNT":
						{
							spotServHandler.reconnectToDevice(getCurrentDevice());
							break;
						}
				}
			}
		}

		private IDevice getCurrentDevice()
		{
			foreach (var device in connectedDevices)
			{
				if (device.Name == this.currentDeviceName)
				{
					return device;
				}
			}
			return null;
		}

		private bool checkIfDeviceScanned(string deviceName)
		{
			foreach (var device in connectedDevices)
			{
				if (device.Name == deviceName)
				{
					// already scanned device
					Debug.WriteLine(deviceName + " is already conneced");
					return true;
				}
			}
			return false;
		}

		private void Adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{
			Debug.WriteLine(string.Format("Device Found : {0}", e.Device.Name));

			if (e.Device.Name == currentDeviceName)
			{
				CrossBluetoothLE.Current.Adapter.StopScanningForDevicesAsync();
				CrossBluetoothLE.Current.Adapter.ConnectToDeviceAsync(e.Device);

				//CrossBluetoothLE.Current.Adapter.fail

				//CrossBluetoothLE.Current.Adapter.ConnectedDevices
			}
		}

		private void Adapter_DeviceConnected(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{
			Debug.WriteLine("Adapter_DeviceConnected: " + e.Device.Name);
			connectedDevices.Add(e.Device);
			switch (e.Device.Name)
			{
				case "BLE-MSA":
					{
						spiroServHandler.discoverServices(e.Device);
						break;
					}

				case "PC_300SNT":
					{
						spotServHandler.discoverServices(e.Device);
						//spotServHandler.uiController.ShowMessageOnUI("Connected.", true);
						break;
					}
			}

			//devServiceHandler.discoverServices(e.Device);
		}

		void Adapter_DeviceConnectionLost(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceErrorEventArgs e)
		{
			Debug.WriteLine(e.Device.Name + " just Adapter_DeviceConnectionLost");

			if (e.Device.Name == "PC_300SNT") { 
				spotServHandler.uiController.ShowMessageOnUI("Spot Check Monitor Connection Lost.", false);
			}

			if (currentDeviceName == "BLE-MSA")
			{
				spiroServHandler.stopPolling();
				spiroServHandler.uiController.updateDeviceStateOnUI("Spirometer Connection Lost.", false);
			}
		}

		void Adapter_ScanTimeoutElapsed(object sender, EventArgs e)
		{
			if (currentDeviceName == "PC_300SNT")
			{
				spotServHandler.uiController.ShowMessageOnUI("Scanning time out. Please, check if Spot Check Monitor is turned on.", false);
			}

			if (currentDeviceName == "BLE-MSA")
			{
				spiroServHandler.stopPolling();
				spiroServHandler.uiController.updateDeviceStateOnUI("Scanning time out. Please, check if Spirometer is turned on.", false);
			}

			Debug.WriteLine("Adapter_ScanTimeoutElapsed.");
		}
	}
}
