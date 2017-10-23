using Plugin.BLE;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
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

	//	public HashSet<IDevice> connectedDevices = new HashSet<IDevice>();

		//	public IDevice currentDevice;
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
		//	CrossBluetoothLE.Current.Adapter.DeviceDisconnected += Adapter_DisConnection;
		    
			//connec

			Debug.WriteLine("bluetooth adapter initialized.");

			spiroServHandler = new SpirometerServiceHandler();
			spotServHandler = new SpotCheckServiceHandler();
			pc100ServHandler = new PC100ServiceHandler();
			scaleServHandle = new ScaleServiceHandler();
		}

		public void connectToDevice(String deviceName, object controller)
		{
			Debug.WriteLine("connectToDevice");
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
			else
			{
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

			foreach (var device in CrossBluetoothLE.Current.Adapter.ConnectedDevices )
			{
				if (device.Name == deviceName)
				{
					Debug.WriteLine("getCurrentDevice" + device.Name);
					return device;
				}
			}
			return null;

		}

		public bool checkIfDeviceScanned(string deviceName)
		{
			Debug.WriteLine("checkIfDeviceScanned  IN CONNECTED ARRAY :" + deviceName);

			foreach (var device in CrossBluetoothLE.Current.Adapter.ConnectedDevices)
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
            //scan for devices
			if (e.Device.Name == scanningDeviceName)
			{
				Debug.WriteLine(string.Format("StopScanningForDevicesAsync"));
				
				try
				{
					CrossBluetoothLE.Current.Adapter.StopScanningForDevicesAsync();
				}
				catch (Exception ex)
				{
					Debug.WriteLine("BLE stop scanning ex msg: " + ex.Message);
				}

				Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
				{
					try
					{
						await CrossBluetoothLE.Current.Adapter.ConnectToDeviceAsync(e.Device);
					}
					catch (DeviceConnectionException ex)
					{
						Debug.WriteLine("BLE connect DCE ex msg: " + ex.Message);
                        Debug.WriteLine("e.Device.Name = " + e.Device.Name);
                        //send error message to screen if callback error 133 = "GattCallback error: 133"
                        SendConnError(e.Device.Name);
					}
					catch (Exception ex)
					{
						Debug.WriteLine("BLE connect basic ex msg: " + ex.Message);
					}

				}));
                    
				
				
				


				//CrossBluetoothLE.Current.Adapter.fail
				//CrossBluetoothLE.Current.Adapter.ConnectedDevices
			}
            //var BLE_status = CrossBluetoothLE.Current.Adapter.ConnectToDeviceAsync(e.Device).Status;
            //Debug.WriteLine("BLE status : "+BLE_status.ToString());
            var ble_state = CrossBluetoothLE.Current.State;
            Debug.WriteLine("BLE state : "+ble_state);
		}

        public void SendConnError (string DeviceName)
        {
            switch (DeviceName)
            {
                case "BLE-MSA":
                    {
                        //can't show spirometer message
                        spiroServHandler.uiController.updateDeviceStateOnUI("Failed to connect to Spirometer.", false);
                        break;
                    }
                case "PC_300SNT":
                    {
                        spotServHandler.uiController.ShowConcetion("Failed to connect to PC-300.", false);
                        break;
                    }
                case "PC-100":
                    {
                        pc100ServHandler.uiController.ShowConcetion("Failed to connect to PC-100.", false);
                        break;
                    }
                case "eBody-Scale":
                    {
                        scaleServHandle.uiController.ShowConcetion("Failed to connect to eBody-Scale.", false);
                        break;
                    }
                default:break;
            }
        }

		private void Adapter_DeviceConnected(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{
			Debug.WriteLine("Adapter_DeviceConnected: " + e.Device.Name);
		//	connectedDevices.Add(e.Device);

			switch (e.Device.Name)
			{
				case "BLE-MSA":
					{
						spiroServHandler.discoverServices(e.Device);
						break;
					}

				case "PC_300SNT":
					{
                        Debug.WriteLine("PC_300SNT device id = "+e.Device.Id.ToString());
                             
						spotServHandler.discoverServices(e.Device);
						spotServHandler.uiController.ShowConcetion("Connected.", true);
						break;
					}
				case "PC-100":
					{
                        Debug.WriteLine("PC-100 device id = " + e.Device.Id.ToString());
						pc100ServHandler.discoverServices(e.Device);
						pc100ServHandler.uiController.ShowConcetion("Connected.", true);
						break;
					}
				case "eBody-Scale":
                    {
                        Debug.WriteLine("eBody-Scale device id = " + e.Device.Id.ToString());
						scaleServHandle.discoverServices(e.Device);
						scaleServHandle.uiController.ShowConcetion("Connected.", true);
						break;
                    }
					
				default:
					break;
			}

			//devServiceHandler.discoverServices(e.Device);
		}

		void Adapter_DeviceConnectionLost(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceErrorEventArgs e)
		{
			Debug.WriteLine(e.Device.Name + "  Adapter_DeviceConnectionLost");
			//	if (!connectedDevices.Contains(e.Device)) return;
			//	connectedDevices.Remove(e.Device);
            try
            {
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
            catch (Exception ex)
            {
                Debug.WriteLine("Connection lost exception : "+ex.Message);
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

			else if (scanningDeviceName == "eBody-Scale")
			{
				scaleServHandle.uiController.ShowConcetion("Scanning time out. Please check if Scale is turned on.", false);
			}
			Debug.WriteLine("Adapter_ScanTimeoutElapsed.");
		}


		void Adapter_DisConnection(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{
			Debug.WriteLine(e.Device.Name + "  Adapter_DisConnection");
			if (e.Device.Name == "PC_300SNT")
			{
				spotServHandler.uiController.ShowConcetion("PC-300 DisConnection.", false);
			}
			else if (e.Device.Name == "BLE-MSA")
			{
				spiroServHandler.stopPolling();
				spiroServHandler.uiController.updateDeviceStateOnUI("Spirometer DisConnection.", false);
			}
			else if (e.Device.Name == "PC-100")
			{
				pc100ServHandler.uiController.ShowConcetion("PC-100 DisConnection.", false);
			}
			else if (e.Device.Name == "eBody-Scale")
			{
				scaleServHandle.uiController.ShowConcetion("Scale DisConnection.", false);
			}
		}
		public async void disConnectAll(string exceptDevice = "")
		{
			Debug.WriteLine("================DisconnectDeviceAsync=========================");
			foreach (var device in CrossBluetoothLE.Current.Adapter.ConnectedDevices)
			{
				Debug.WriteLine(device.State);

				if (device.Name == "PC_300SNT")
				{
					await spotServHandler.diconnectServices(device);
				}
				else if (device.Name == "BLE-MSA")
				{
					await spiroServHandler.diconnectServices(device);
				}
				else if (device.Name == "PC-100")
				{
					await pc100ServHandler.diconnectServices(device);
				}
				else if (device.Name == "eBody-Scale")
				{
					await scaleServHandle.diconnectServices(device);
				}

				if (!device.Name.Equals(exceptDevice))
					await CrossBluetoothLE.Current.Adapter.DisconnectDeviceAsync(device);

				Debug.WriteLine("after " + device.State);
			}
			foreach (var device in CrossBluetoothLE.Current.Adapter.ConnectedDevices)
			{
				Debug.WriteLine(device.Name + "  " + device.State);
			}

		}
	}
}
