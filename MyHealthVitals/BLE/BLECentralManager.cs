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
/*	public class ListItemManager {
		public static ListItemManager sharedInstance = new ListItemManager();
		public ListCellTwoItem listcellTwoItem;
		private ListItemManager()r
		{
			listcellTwoItem = new void ListCellTwoItem();
		}
	}
*/
	public class BLECentralManager
	{
		//public static IAdapter Adapter = new IAdapter;

		public static BLECentralManager sharedInstance = new BLECentralManager();

		public List<IDevice> connectedDevices = new List<IDevice>();
		//public IDevice currentDevice;
		public string scanningDeviceName;
		//public IBLEDeviceServiceHandler devServiceHandler;
		public SpirometerServiceHandler spiroServHandler;
		public SpotCheckServiceHandler spotServHandler;
		public PC100ServiceHandler pc100ServHandler;
		public ScaleServiceHandler scaleServHandle;
		string ScaleName = "eBody-Scale";

		//public DeviceListPage deviceListPage;

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
			pc100ServHandler = new PC100ServiceHandler();
			scaleServHandle = new ScaleServiceHandler();
		}

		public void connectToDevice(String deviceName, object controller)
		{
			scanningDeviceName = deviceName;

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
				case "PC-100":
					{
						pc100ServHandler.uiController = (IBluetoothCallBackUpdatable)controller;
						break;
					}
				case "eBody-Scale":
						scaleServHandle.uiController = (IBluetoothCallBackUpdatable)controller;
						break;
				default:
					break;
			}

			if (!checkIfDeviceScanned(deviceName))
			{
				// the device is not in the scanned list now scan to find the desired device and then connnect
				Debug.WriteLine("StartScanningForDevicesAsync : " + deviceName);
				CrossBluetoothLE.Current.Adapter.StartScanningForDevicesAsync();

				//if (deviceName == "PC_300SNT") spotServHandler.uiController.ShowMessageOnUI("Searching device...", false);

			}
			else {
				Debug.WriteLine("reconnectToDevice : " + deviceName);

				switch (deviceName)
				{
					case "BLE-MSA":
						{
							spiroServHandler.reconnectToDevice(getCurrentDevice(deviceName));
							break;
						}

					case "PC_300SNT":
						{
							spotServHandler.reconnectToDevice(getCurrentDevice(deviceName));
							break;
						}

					case "PC-100":
						{
							pc100ServHandler.reconnectToDevice(getCurrentDevice(deviceName));
							break;
						}
					case "eBody-Scale":
						scaleServHandle.reconnectToDevice(getCurrentDevice(deviceName));
						break;
					default:
						break;
				}
			}
		}

		private IDevice getCurrentDevice(String deviceName)
		{
			foreach (var device in connectedDevices)
			{
				if (device.Name == deviceName)
				{
					return device;
				}
			}
			return null;
		}

		public bool checkIfDeviceScanned(string deviceName)
		{
			Debug.WriteLine("checkIfDeviceScanned  IN CONNECTED ARRAY :"+ deviceName);

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

			if (e.Device.Name == scanningDeviceName)
			{
				Debug.WriteLine(string.Format("StopScanningForDevicesAsync"));

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
						spotServHandler.uiController.ShowConcetion("Connected.", true);
						break;
					}
				case "PC-100":
					{
						pc100ServHandler.discoverServices(e.Device);
						pc100ServHandler.uiController.ShowConcetion("Connected.", true);
						break;
					}
				case "eBody-Scale":
					scaleServHandle.discoverServices(e.Device);
					scaleServHandle.uiController.ShowConcetion("Connected.", true);
					break;
				default:
					break;
			}

			//devServiceHandler.discoverServices(e.Device);
		}

		void Adapter_DeviceConnectionLost(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceErrorEventArgs e)
		{
			Debug.WriteLine(e.Device.Name + " just Adapter_DeviceConnectionLost");
			connectedDevices.Remove(e.Device);
			if (e.Device.Name == "PC_300SNT")
			{
				spotServHandler.uiController.ShowConcetion("PC-300 Connection Lost.", false);
			}
			else if (e.Device.Name == "BLE-MSA")
			{
				spiroServHandler.stopPolling();
				spiroServHandler.uiController.updateDeviceStateOnUI("Spirometer Connection Lost.", false);
			}
			else if (e.Device.Name == "PC-100")
			{
				pc100ServHandler.uiController.ShowConcetion("PC-100 Connection Lost.", false);
			}
			else if (e.Device.Name == "eBody-Scale")
			{
				scaleServHandle.uiController.ShowConcetion("Scale Connection Lost.", false);
			}
		}

		void Adapter_ScanTimeoutElapsed(object sender, EventArgs e)
		{
			if (scanningDeviceName == "PC_300SNT")
			{
				spotServHandler.uiController.ShowConcetion("Scanning time out. Please, check if Spot Check Monitor is turned on.", false);
			}

			else if (scanningDeviceName == "BLE-MSA")
			{
				spiroServHandler.stopPolling();
				spiroServHandler.uiController.updateDeviceStateOnUI("Scanning time out. Please, check if Spirometer is turned on.", false);
			}

			else if (scanningDeviceName == "PC-100")
			{
				pc100ServHandler.uiController.ShowConcetion("Scanning time out. Please, check if Spot Check Monitor is turned on.", false);
			}

			else if (scanningDeviceName == "eBody-Scale") {
				scaleServHandle.uiController.ShowConcetion("Scanning time out. Please check if Scale is turned on.", false);
			}
			Debug.WriteLine("Adapter_ScanTimeoutElapsed.");
		}
	}
}
