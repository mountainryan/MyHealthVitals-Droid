using Plugin.BLE;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using System.Threading;
using System.Threading.Tasks;
using nexus.core;
using nexus.core.logging;
using nexus.protocols.ble;
using nexus.protocols.ble.connection;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public interface IBLEDeviceServiceHandler
	{
		void C_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e);
		Task discoverServices(IDevice device);
		void reconnectToDevice(IDevice device);
        void GetData(Byte[] bytes);

		//object uiController;

	}
    public class BLEdata
    {
        public static nexus.protocols.ble.connection.IBleGattServer gattserver;

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

        IBluetoothLowEnergyAdapter adapter = BLEadapter.adapter;


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



        public async Task ConnectToDevice2(string deviceName, object controller)
        {
            Debug.WriteLine("connectToDevice2");
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
                    {
                        scaleServHandle.uiController = (IBluetoothCallBackUpdatable)controller;
                        break;
                    }
				default: break;
			}


			var val = checkBLEState(deviceName);
            if (val)
            {
                Guid deviceID = new Guid("00000000-0000-0000-0000-000000000000");

                Debug.WriteLine("result is null");

                IBlePeripheral found = null;
				//scan for the named device and attempt to connect to it

				if (BLEdata.gattserver != null)
				{
					BLEdata.gattserver.Dispose();
				}

                await adapter.ScanForBroadcasts(
                    new ScanFilter.Factory()
                        .SetAdvertisedDeviceName(deviceName)
                        .SetIgnoreRepeatBroadcasts(true),
                    p => { found = p; });

                if (found != null)
                {
                    Debug.WriteLine("Device guid = " + found.DeviceId.ToString());
                    deviceID = found.DeviceId;
                }

				if (deviceID.ToString() != "00000000-0000-0000-0000-000000000000")
				{
					//connect to the device
                    var connection = await adapter.ConnectToDevice(deviceID, TimeSpan.FromSeconds(15));
					if (connection.IsSuccessful())
					{
                        BLE_val.BLE_value = 2;
                        //write to BLElog.txt
                        DependencyService.Get<IFileHelper>().saveBLEinfo(deviceName, BLE_val.BLE_value, deviceID);

                        var gattServer = connection.GattServer;
                        BLEdata.gattserver = gattServer;
						// do things with gattServer here... (see further examples...)
						Debug.WriteLine("Successfully connected!!!!");
						//show that we are connected
						await DeviceConnected(deviceName);
						

						foreach (var guid in await gattServer.ListAllServices())
						{
							Debug.WriteLine($"service: {guid}");
							foreach (var guid2 in await gattServer.ListServiceCharacteristics(guid))
							{
								Debug.WriteLine($"  characteristic: {guid2}");
							}
						}

						if (deviceName == "BLE-MSA")
						{
							//run discoverservices2
							spiroServHandler.discoverServices2(deviceID);
						}

						gattServer.Subscribe(
            			   c =>
            			   {
            				   if (c == ConnectionState.Disconnected)
            				   {
            					   //m_dialogManager.Toast("Device disconnected");

            					   CloseConnection(deviceName);
            				   }
            				   //Connection = c.ToString();
            			   });

						NotifyChar(gattServer, deviceName);

					}
					else
					{
						// Do something to inform user or otherwise handle unsuccessful connection.
						Debug.WriteLine("Error connecting to device. result={0:g}", connection.ConnectionResult);
						// e.g., "Error connecting to device. result=ConnectionAttemptCancelled"
						//send the error message to screen
						SendConnError(deviceName, 2);//, connection.ConnectionResult.ToString());
					}
                }else{
                    //device not found
                    Debug.WriteLine("Device not found");
                }
            }

        }

		public void CloseConnection(string deviceName)
		{
			if (BLEdata.gattserver != null)
			{
				//Log.Trace("Closing connection to GATT Server. state={0:g}", m_gattServer?.State);
				BLEdata.gattserver.Dispose();
			}

			try
			{
				if (deviceName == "PC_300SNT")
				{
					spotServHandler.uiController.ShowConnection("PC-300 Connection Lost.", false);
				}
				else if (deviceName == "BLE-MSA")
				{
					spiroServHandler.stopPolling();
					spiroServHandler.uiController.updateDeviceStateOnUI("Spirometer Connection Lost.", false);
				}
				else if (deviceName == "PC-100")
				{
					pc100ServHandler.uiController.ShowConnection("PC-100 Connection Lost.", false);
				}
				else if (deviceName == "eBody-Scale")
				{
					scaleServHandle.uiController.ShowConnection("Scale Connection Lost.", false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Connection lost exception : " + ex.Message);
			}
			//Services.Clear();
			//IsBusy = false;
		}

        public void NotifyChar(nexus.protocols.ble.connection.IBleGattServer gattServer, string deviceName)
        {						
			switch (deviceName)
			{
				case "BLE-MSA":
					{
						//listen for Characteristic
						Guid gservice = new Guid("0000fff0-0000-1000-8000-00805f9b34fb");
						Guid gchar = new Guid("0000ff0a-0000-1000-8000-00805f9b34fb");
						try
						{
							// will stop listening when gattServer is disconnected
							//Byte[] charbytes;
							gattServer.NotifyCharacteristicValue(
							   gservice,
							   gchar,
								spotServHandler.GetData,
							   // provide IObserver<Tuple<Guid, Byte[]>> or IObserver<Byte[]>
							   // There are several extension methods to assist in creating the obvserver...
							   bytes => {


							   });
						}
						catch (GattException ex)
						{
							Debug.WriteLine(ex.ToString());
						}
						break;
					}

				case "PC_300SNT":
					{
						Guid gservice = new Guid("0000fff0-0000-1000-8000-00805f9b34fb");
						Guid gchar = new Guid("0000fff1-0000-1000-8000-00805f9b34fb");
						try
						{
							// will stop listening when gattServer is disconnected
							//Byte[] charbytes;
							gattServer.NotifyCharacteristicValue(
							   gservice,
							   gchar,
								spotServHandler.GetData,
							   // provide IObserver<Tuple<Guid, Byte[]>> or IObserver<Byte[]>
							   // There are several extension methods to assist in creating the obvserver...
							   bytes => {


							   });
						}
						catch (GattException ex)
						{
							Debug.WriteLine(ex.ToString());
						}
						break;
					}
				case "PC-100":
					{
						Guid gservice = new Guid("0000fff0-0000-1000-8000-00805f9b34fb");
						Guid gchar = new Guid("0000fff1-0000-1000-8000-00805f9b34fb");
						try
						{
							// will stop listening when gattServer is disconnected
							//Byte[] charbytes;
							gattServer.NotifyCharacteristicValue(
							   gservice,
							   gchar,
								pc100ServHandler.GetData,
							   // provide IObserver<Tuple<Guid, Byte[]>> or IObserver<Byte[]>
							   // There are several extension methods to assist in creating the obvserver...
							   bytes => {


							   });
						}
						catch (GattException ex)
						{
							Debug.WriteLine(ex.ToString());
						}
						break;
					}
				case "eBody-Scale":
                    {
						Guid gservice = new Guid("0000fff0-0000-1000-8000-00805f9b34fb");
						Guid gchar = new Guid("0000fff4-0000-1000-8000-00805f9b34fb");
                        try
                        {
                            // will stop listening when gattServer is disconnected
                            //Byte[] charbytes;
                            gattServer.NotifyCharacteristicValue(
                               gservice,
                               gchar,
                                scaleServHandle.GetData,
                               // provide IObserver<Tuple<Guid, Byte[]>> or IObserver<Byte[]>
                               // There are several extension methods to assist in creating the obvserver...
                               bytes =>
                               {

                               });
                        }
                        catch (GattException ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }
                        break;
                    }
				default:
					break;
			}
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

            var val = checkBLEState(deviceName);
            if (val)
            {
            
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

		}

		private IDevice getCurrentDevice(String deviceName)
		{
            Debug.WriteLine("calling getCurrentDevice()");
			foreach (var device in CrossBluetoothLE.Current.Adapter.ConnectedDevices )
			{
				if (device.Name == deviceName)
				{
					Debug.WriteLine("getCurrentDevice = " + device.Name);
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
					Debug.WriteLine(deviceName + " is already connected");
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
						Debug.WriteLine("attempting to connect...");
                        await CrossBluetoothLE.Current.Adapter.ConnectToDeviceAsync(e.Device);

					}
					catch (DeviceConnectionException ex)
					{
						Debug.WriteLine("BLE connect DCE ex msg: " + ex.Message);
                        Debug.WriteLine("e.Device.Name = " + e.Device.Name);
                        //send error message to screen if callback error 133 = "GattCallback error: 133"
                        SendConnError(e.Device.Name, 1);
					}
					catch (Exception ex)
					{
						Debug.WriteLine("BLE connect basic ex msg: " + ex.Message);
					}

				}));			
				


				//CrossBluetoothLE.Current.Adapter.fail
				//CrossBluetoothLE.Current.Adapter.ConnectedDevices
            }else{
                Debug.WriteLine("wrong device = "+e.Device.Name);
            }
            //var BLE_status = CrossBluetoothLE.Current.Adapter.ConnectToDeviceAsync(e.Device).Status;
            //Debug.WriteLine("BLE status : "+BLE_status.ToString());

		}

        public void SendConnError (string DeviceName, int camefrom)
        {
            switch (DeviceName)
            {
                case "BLE-MSA":
                    {
                        //can't show spirometer message
                        spiroServHandler.uiController.FailedConn("Failed to connect to Spirometer. Try again?", false, camefrom);
                        break;
                    }
                case "PC_300SNT":
                    {
                        spotServHandler.uiController.FailedConn("Failed to connect to PC-300. Try again?", false, camefrom);
                        break;
                    }
                case "PC-100":
                    {
                        pc100ServHandler.uiController.FailedConn("Failed to connect to PC-100. Try again?", false, camefrom);

                        break;
                    }
                case "eBody-Scale":
                    {
                        scaleServHandle.uiController.FailedConn("Failed to connect to eBody-Scale. Try again?", false, camefrom);
                        break;
                    }
                default:break;
            }
        }
        /*
        public void Conn_Error(string deviceName)//, string message)
        {
			switch (deviceName)
			{
				case "BLE-MSA":
					{
						//can't show spirometer message
						spiroServHandler.uiController.updateDeviceStateOnUI("Failed to connect to Spirometer.", false);
						break;
					}
				case "PC_300SNT":
					{
						spotServHandler.uiController.ShowConnection("Failed to connect to PC-300.", false);
						break;
					}
				case "PC-100":
					{
						pc100ServHandler.uiController.ShowConnection("Failed to connect to PC-100.", false);
						break;
					}
				case "eBody-Scale":
					{
						scaleServHandle.uiController.ShowConnection("Failed to connect to eBody-Scale.", false);
						break;
					}
				default: break;
			}
        }*/

        public async Task<bool> ConnectKnownDevice2(Guid deviceID, string deviceName, object controller)
        {
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
					{
						scaleServHandle.uiController = (IBluetoothCallBackUpdatable)controller;
						break;
					}
				default: break;
			}

            //may not need this line
			if (BLEdata.gattserver != null)
			{
				BLEdata.gattserver.Dispose();
			}

            Debug.WriteLine("called ConnectKnownDevice2");
			var connection = await adapter.ConnectToDevice(deviceID, TimeSpan.FromSeconds(5));
            bool connected = false;
			if (connection.IsSuccessful())
			{
                connected = true;
				BLE_val.BLE_value = 2;
				//write to BLElog.txt
				DependencyService.Get<IFileHelper>().saveBLEinfo(deviceName, BLE_val.BLE_value, deviceID);

				var gattServer = connection.GattServer;
                BLEdata.gattserver = gattServer;
				// do things with gattServer here... (see further examples...)
				Debug.WriteLine("Successfully connected!!!!");
				//show that we are connected
				await DeviceConnected(deviceName);
                if (deviceName == "BLE-MSA")
                {
                    //run discoverservices2
                    spiroServHandler.discoverServices2(deviceID);
                }

				foreach (var guid in await gattServer.ListAllServices())
				{
					Debug.WriteLine($"service: {guid}");
					foreach (var guid2 in await gattServer.ListServiceCharacteristics(guid))
					{
						Debug.WriteLine($"  characteristic: {guid2}");
					}
				}

				gattServer.Subscribe(
				   c =>
				   {
					   if (c == ConnectionState.Disconnected)
					   {
						   //m_dialogManager.Toast("Device disconnected");

						   CloseConnection(deviceName);
					   }
					   //Connection = c.ToString();
				   });

				NotifyChar(gattServer, deviceName);

			}
			else
			{
				// Do something to inform user or otherwise handle unsuccessful connection.
				Debug.WriteLine("Error connecting to device. result={0:g}", connection.ConnectionResult);
				// e.g., "Error connecting to device. result=ConnectionAttemptCancelled"
				//send the error message to screen
				//Conn_Error(deviceName);//, connection.ConnectionResult.ToString());
			}
            return connected;
        }

        public async Task<bool> ConnectKnownDevice(Guid deviceID, string deviceName, object controller)
        {
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
					{
						scaleServHandle.uiController = (IBluetoothCallBackUpdatable)controller;
						break;
					}
				default: break;
			}
            bool connected = false;
            if (!checkIfDeviceScanned(deviceName))
            {
                Debug.WriteLine("called ConnectKnownDevice");

                try
                {
                    //CancellationTokenSource cts = new CancellationTokenSource();
                    //var token = cts.Token;
                    await CrossBluetoothLE.Current.Adapter.ConnectToKnownDeviceAsync(deviceID);

                    connected = true;
                }
                catch (DeviceConnectionException e)
                {
                    Debug.WriteLine("connectToKnownDeviceAsync error msg: " + e.Message);
                }
            }else{
                //Debug.WriteLine("skipped redundant scan!");
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
                connected = true;
            }
            return connected;
        }

        public async Task DeviceConnected(string deviceName)
        {
			switch (deviceName)
			{
				case "BLE-MSA":
					{
						//don't do anything!
                        //start discoverservices2 maybe?
						break;
					}

				case "PC_300SNT":
					{
						//Debug.WriteLine("PC_300SNT device id = " + e.Device.Id.ToString());

						//spotServHandler.discoverServices(e.Device);
						await spotServHandler.uiController.ShowConnection("Connected.", true);
                        //await spotServHandler.uiController.checkBattery();
						break;
					}
				case "PC-100":
					{
						//Debug.WriteLine("PC-100 device id = " + e.Device.Id.ToString());
						//pc100ServHandler.discoverServices(e.Device);
						await pc100ServHandler.uiController.ShowConnection("Connected.", true);
                        //await pc100ServHandler.uiController.checkBattery();
						break;
					}
				case "eBody-Scale":
					{
						//Debug.WriteLine("eBody-Scale device id = " + e.Device.Id.ToString());
						//scaleServHandle.discoverServices(e.Device);
						await scaleServHandle.uiController.ShowConnection("Connected.", true);
						break;
					}

				default:
					break;
			}
        }

		private async void Adapter_DeviceConnected(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{
			Debug.WriteLine("Adapter_DeviceConnected: " + e.Device.Name);
			//	connectedDevices.Add(e.Device);
			Guid deviceID = e.Device.Id;
			BLE_val.BLE_value = 1;
			DependencyService.Get<IFileHelper>().saveBLEinfo(e.Device.Name, BLE_val.BLE_value, deviceID);

			switch (e.Device.Name)
			{
				case "BLE-MSA":
					{
						await spiroServHandler.discoverServices(e.Device);
						break;
					}

				case "PC_300SNT":
					{
                        Debug.WriteLine("PC_300SNT device id = "+e.Device.Id.ToString());
                             
						await spotServHandler.discoverServices(e.Device);
						await spotServHandler.uiController.ShowConnection("Connected.", true);
                        //await spotServHandler.uiController.checkBattery();
						break;
					}
				case "PC-100":
					{
                        Debug.WriteLine("PC-100 device id = " + e.Device.Id.ToString());
						await pc100ServHandler.discoverServices(e.Device);
						await pc100ServHandler.uiController.ShowConnection("Connected.", true);
                        //await pc100ServHandler.uiController.checkBattery();
						break;
					}
				case "eBody-Scale":
                    {
                        Debug.WriteLine("eBody-Scale device id = " + e.Device.Id.ToString());
						await scaleServHandle.discoverServices(e.Device);
						await scaleServHandle.uiController.ShowConnection("Connected.", true);
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
					spotServHandler.uiController.ShowConnection("PC-300 Connection Lost.", false);
				}
				else if (e.Device.Name == "BLE-MSA")
				{
					spiroServHandler.stopPolling();
					spiroServHandler.uiController.updateDeviceStateOnUI("Spirometer Connection Lost.", false);
				}
				else if (e.Device.Name == "PC-100")
				{
					pc100ServHandler.uiController.ShowConnection("PC-100 Connection Lost.", false);
				}
				else if (e.Device.Name == "eBody-Scale")
				{
					scaleServHandle.uiController.ShowConnection("Scale Connection Lost.", false);
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
				spotServHandler.uiController.ShowConnection("Scanning time out. Please, check if Spot Check Monitor is turned on.", false);
			}

			else if (scanningDeviceName == "BLE-MSA")
			{
				spiroServHandler.stopPolling();
				spiroServHandler.uiController.updateDeviceStateOnUI("Scanning time out. Please, check if Spirometer is turned on.", false);
			}

			else if (scanningDeviceName == "PC-100")
			{
				pc100ServHandler.uiController.ShowConnection("Scanning time out. Please, check if Spot Check Monitor is turned on.", false);
			}

			else if (scanningDeviceName == "eBody-Scale")
			{
				scaleServHandle.uiController.ShowConnection("Scanning time out. Please check if Scale is turned on.", false);
			}
			Debug.WriteLine("Adapter_ScanTimeoutElapsed.");
		}


		void Adapter_DisConnection(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{
			Debug.WriteLine(e.Device.Name + "  Adapter_DisConnection");
			if (e.Device.Name == "PC_300SNT")
			{
				spotServHandler.uiController.ShowConnection("PC-300 DisConnection.", false);
			}
			else if (e.Device.Name == "BLE-MSA")
			{
				spiroServHandler.stopPolling();
				spiroServHandler.uiController.updateDeviceStateOnUI("Spirometer DisConnection.", false);
			}
			else if (e.Device.Name == "PC-100")
			{
				pc100ServHandler.uiController.ShowConnection("PC-100 DisConnection.", false);
			}
			else if (e.Device.Name == "eBody-Scale")
			{
				scaleServHandle.uiController.ShowConnection("Scale DisConnection.", false);
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
		public bool checkBLEState(string deviceName)
		{
			var ble_state = CrossBluetoothLE.Current.State;
			Debug.WriteLine("BLE state : " + ble_state);
			string BLEstate = ble_state.ToString();

			if (BLEstate == "Off")
			{
				//throw a msg to turn on Bluetooth
				Debug.WriteLine("Your Bluetooth is off!");
				switch (deviceName)
				{
					case "BLE-MSA":
						{
							//can't show spirometer message
							spiroServHandler.uiController.updateDeviceStateOnUI("Bluetooth is turned off.", false);
							break;
						}
					case "PC_300SNT":
						{
							spotServHandler.uiController.ShowConnection("Bluetooth is turned off.", false);
							break;
						}
					case "PC-100":
						{
							pc100ServHandler.uiController.ShowConnection("Bluetooth is turned off.", false);
							break;
						}
					case "eBody-Scale":
						{
							scaleServHandle.uiController.ShowConnection("Bluetooth is turned off.", false);
							break;
						}
					default: break;
				}
				return false;
			}
			else
			{
				return true;
			}
		}
	}
}
