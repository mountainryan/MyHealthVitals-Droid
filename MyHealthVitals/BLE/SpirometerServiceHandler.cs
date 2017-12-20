using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System.Linq;
using nexus.core.logging;
using nexus.protocols.ble;

namespace MyHealthVitals
{
	
	public class SpirometerServiceHandler: IBLEDeviceServiceHandler
	{
		public static ICharacteristic bmChar;
		public IDevice connectedDevice;
		public bool isStopPolling = false;
		public BLEReadingUpdatableSpiroMeter uiController;

		public void GetData(Byte[] bytes)
		{ 
            //Shouldn't ever make it here yet
            //Debug.WriteLine("Made it into SpirometerServiceHandler.GetData()");
			//get the bytes into an int array
			int[] ints = bytes.Select(x => (int)x).ToArray();
            ManipData(ints);
        }

		public async void write6()
		{
			if (BLE_val.BLE_value == 1)
			{
				//executeWriteCommand(new byte[] { 0xAA, 0x55, 0x40, 0x02, 0x01, 0x29 });
			}
			else
			{
				try
				{
					Guid gservice = new Guid("0000fff0-0000-1000-8000-00805f9b34fb");
					Guid gchar = new Guid("0000ff0b-0000-1000-8000-00805f9b34fb");
					nexus.protocols.ble.connection.IBleGattServer GattServer = BLEdata.gattserver;
					// The resulting value of the characteristic is returned. In nearly all cases this
					// will be the same value that was provided to the write call (e.g. `byte[]{ 1, 2, 3 }`)
					var value = await GattServer.WriteCharacteristicValue(
					   gservice,
					   gchar,
					   new byte[] { 0x55, 0x06 });
				}
				catch (GattException ex)
				{
					//Debug.WriteLine("writing 6 error: " + ex.Message);
				}
			}

		}

		public async void write1()
		{
			if (BLE_val.BLE_value == 1)
			{
				//executeWriteCommand(new byte[] { 0xAA, 0x55, 0x40, 0x02, 0x01, 0x29 });
			}
			else
			{
				try
				{
					Guid gservice = new Guid("0000fff0-0000-1000-8000-00805f9b34fb");
					Guid gchar = new Guid("0000ff0b-0000-1000-8000-00805f9b34fb");
					nexus.protocols.ble.connection.IBleGattServer GattServer = BLEdata.gattserver;
					// The resulting value of the characteristic is returned. In nearly all cases this
					// will be the same value that was provided to the write call (e.g. `byte[]{ 1, 2, 3 }`)
					var value = await GattServer.WriteCharacteristicValue(
					   gservice,
					   gchar,
					   new byte[] { 0x55, 0x01 });
				}
				catch (GattException ex)
				{
					//Debug.WriteLine("writing 1 error: " + ex.Message);
				}
			}

		}

		public async void writeData()
		{
			if (BLE_val.BLE_value == 1)
			{
				//executeWriteCommand(new byte[] { 0xAA, 0x55, 0x40, 0x02, 0x01, 0x29 });
			}
			else
			{
				try
				{
					Guid gservice = new Guid("0000fff0-0000-1000-8000-00805f9b34fb");
					Guid gchar = new Guid("0000ff0b-0000-1000-8000-00805f9b34fb");
					nexus.protocols.ble.connection.IBleGattServer GattServer = BLEdata.gattserver;
					// The resulting value of the characteristic is returned. In nearly all cases this
					// will be the same value that was provided to the write call (e.g. `byte[]{ 1, 2, 3 }`)
					var value = await GattServer.WriteCharacteristicValue(
					   gservice,
					   gchar,
					   new byte[] { 0x55, 0x02, 0x01, 0x00 });
				}
				catch (GattException ex)
				{
					//Debug.WriteLine("writing data error: " + ex.Message);
				}
			}

		}

        public void ManipData(int[] vals)
        {
			if (vals.Length > 1)
			{
				timespolled++;
				//Debug.WriteLine("Times polled: " + timespolled);
				//Debug.WriteLine("C_ValueUpdated  vals[0]=" + vals[0] + "   vals[1]=" + vals[1]);
				if (vals[0] == 170 && vals[1] == 3 && isStatusAsked == false)
				{
					//Debug.WriteLine("Got a 3, send a 6");
					//if (bmChar != null)
					//{
					//	Debug.WriteLine("bmchar is not null.");
						Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
						{
							//bmChar.WriteAsync(new byte[] { 0x55, 0x06 });
                            write6();
						});

					//}
					//else
					//{
					//	Debug.WriteLine("bmchar is null.");
					//}
				}
				if (vals[0] == 170 && vals[1] == 6)// && isStatusAsked == false)
				{
					//Debug.WriteLine("Got a 6, send a 1");
					//Debug.WriteLine("bmchar = " + bmChar.ToString());
					isStopPolling = true;
					isStatusAsked = true;

					//if (bmChar != null)
					//{
					//	Debug.WriteLine("bmchar is not null.");
						Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
						{
							//bmChar.WriteAsync(new byte[] { 0x55, 0x01 });
                            write1();
						});


					//}
					//else
					//{
					//	Debug.WriteLine("bmchar is null.");
					//}

				}

				if (vals[0] == 170 && vals[1] == 1)// && isDataAsked == false)
				{
					//Debug.WriteLine("Got a 1, send for data");
					//Debug.WriteLine("bmchar = " + bmChar.ToString());
					isDataAsked = true;

					//if (bmChar != null)
					//{
					//	Debug.WriteLine("bmchar is not null.");
						Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
						{
							//bmChar.WriteAsync(new byte[] { 0x55, 0x02, 0x01, 0x00 });
                            writeData();
						});
					//}
					//else
					//{
					//	Debug.WriteLine("bmchar is null.");
					//}

				}
				if (timespolled > 25 && isDataAsked == false)
				{
					//stuck in a loop
					//try to ask for the data
					isDataAsked = true;

					//if (bmChar != null)
					//{
					//	Debug.WriteLine("bmchar is not null.");
						Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
						{
							//bmChar.WriteAsync(new byte[] { 0x55, 0x02, 0x01, 0x00 });
                            writeData();
						});
					//}
					//else
					//{
					//	Debug.WriteLine("bmchar is null.");
					//}

				}
				if (timespolled > 35 && isDataAsked)
				{
					//can't get the reading for some reason!
					//try over and over
					Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
					{
						//bmChar.WriteAsync(new byte[] { 0x55, 0x02, 0x01, 0x00 });
                        writeData();
					});
				}

				// this is vals
				if (vals[0] == 221)
				{
					// geting int from two byte
					// sometime vals is 17 long and sometime itis 9 long
					int valsIndex = vals.Length > 9 ? 13 : 5;

					var fev1 = (double)((vals[valsIndex + 1] << 8) + vals[valsIndex]) / 100;
					int pef = (vals[valsIndex + 3] << 8) + vals[valsIndex + 2];
					//Debug.WriteLine("fev1: " + fev1 + "  " + "pef: " + pef);

					isDataAsked = false;
					isStatusAsked = false;
					clearReadingOnDevice();
					//sometimes it doesn't clear so run it again
					//clearReadingOnDevice();
					timespolled = 0;

					if (pef <= 200)
					{
						uiController.testAgainDialog();
					}
					else
					{

						var reading = new SpirometerReading(DateTime.Now, (decimal)pef, (decimal)fev1);
						uiController.updateCaller(reading);
					}

				}
			}
        }

		public void reconnectToDevice(IDevice device)
		{
			//uiController = (BLEReadingUpdatableSpiroMeter)controller;
			connectedDevice = device;
			if (connectedDevice.State == Plugin.BLE.Abstractions.DeviceState.Connected)
			{
				startPolling();
			}
			else
			{
				//shouldn't have to do this, but maybe necessary!
                //Debug.WriteLine("reconnectToDevice connectedDevice = " + connectedDevice );

                //CrossBluetoothLE.Current.Adapter.ConnectToKnownDeviceAsync(device.Id);

				CrossBluetoothLE.Current.Adapter.ConnectToDeviceAsync(connectedDevice);
				// after this it will call central manager and when device_connected event of the central manager fires then it will call again this class discoverServices()
			}
		}
		public async Task diconnectServices(IDevice device)
		{
			//uiController = (BLEReadingUpdatableSpiroMeter)controller;
			connectedDevice = device;
			stopPolling();
			var services = await connectedDevice.GetServicesAsync();
			foreach (var s in services)
			{
				var characteristics = await s.GetCharacteristicsAsync();
				foreach (var c in characteristics)
				{
					if (c.CanUpdate)
					{
						c.ValueUpdated -= C_ValueUpdated;
						await c.StopUpdatesAsync();
					}

					if (c.CanWrite)
					{
						bmChar = c;
					}
				}
			}

		}

        public async void discoverServices2(Guid deviceID)
        {
            startPolling2();
        }

		public async Task discoverServices(IDevice device)
		{
			//uiController = (BLEReadingUpdatableSpiroMeter)controller;
			connectedDevice = device;

			var services = await connectedDevice.GetServicesAsync();
			foreach (var s in services)
			{
                //Debug.WriteLine("service guid = " + s.Id.ToString());
				var characteristics = await s.GetCharacteristicsAsync();
				foreach (var c in characteristics)
				{
					//Debug.WriteLine(string.Format("Char UUID: {0}  Value: {1}", c.Uuid, c.Value));
					//Debug.WriteLine("uuid: " + c.Uuid);

					if (c.CanUpdate)
					{
						c.ValueUpdated += C_ValueUpdated;
						await c.StartUpdatesAsync();
					}

					if (c.CanWrite)
					{
						bmChar = c;
					}
				}
			}

			startPolling();
		}

		public void stopPolling() {
			//Debug.WriteLine("Stop polling!");
			this.isStopPolling = true;
		}

		private void startPolling2()
		{
			isStopPolling = false;
			//Debug.WriteLine("Start polling2!");

			Xamarin.Forms.Device.StartTimer(TimeSpan.FromMilliseconds(25), () =>
			{
				if (isStopPolling == false)
				{
					//Debug.WriteLine("polling for data...");
					Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
					{
                        //write6();
						//bmChar.WriteAsync(new byte[] { 0x55, 0x06 });
                        writeData();
					});
				}
				return !isStopPolling;
			});
		}

		private void startPolling()
		{
			isStopPolling = false;
			//Debug.WriteLine("Start polling!");

			Xamarin.Forms.Device.StartTimer(TimeSpan.FromMilliseconds(5), () =>
			{
				if (isStopPolling == false)
				{
                    //Debug.WriteLine("polling for data...");
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                    {
                        //Debug.WriteLine("bmChar guid = "+bmChar.Id.ToString());
                        bmChar.WriteAsync(new byte[] { 0x55, 0x06 });
                    });
				}
				return !isStopPolling;
			});
		}

		public void clearReadingOnDevice()
		{
            //Debug.WriteLine("clearReadingOnDevice");
            //Task.Delay(1).ContinueWith(_ =>
            //{
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                //Debug.WriteLine("bmChar guid = " + bmChar.Id.ToString());
                bmChar.WriteAsync(new byte[] { 0x55, 0x03 });
                //Debug.WriteLine("Spirometer reading cleared.");
            });
			//});

			//wait for 1 millisecond to make sure it clears
            //Task.Delay(1).ContinueWith(_ =>
            //{
                //just wait for 1 millisecond
                //Debug.WriteLine("Waiting...");
                //bmChar.WriteAsync(new byte[] { 0x55, 0x03 });
            //});
			
                                             
		}

		private void printUpdatedCharacteristics(ICharacteristic ch)
		{
			List<int> values = new List<int>();
			foreach (var b in ch.Value)
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

		//	Debug.WriteLine(string.Format("UUID: {0}  ->{1}", ch.Uuid, sb.ToString()));
		}


		bool isStatusAsked = false;
		bool isDataAsked = false;
        int timespolled = 0;

		//int pefReading = -1;

		public void C_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
		{
            //printUpdatedCharacteristics(e.Characteristic);
            //Debug.WriteLine("bmChar guid = " + bmChar.Id.ToString());

			var data = e.Characteristic.Value;
           
            //Debug.WriteLine("reading char id = "+e.Characteristic.Id.ToString());
			
            //Debug.WriteLine("data length= "+data.Length.ToString());
            //Debug.WriteLine("C_ValueUpdated  data[0]=" +data[0] +"   data[1]=" +data[1]);

            if (data.Length > 1)
            {
                timespolled++;
                //Debug.WriteLine("Times polled: "+timespolled);
                //Debug.WriteLine("C_ValueUpdated  data[0]=" + data[0] + "   data[1]=" + data[1]);
				if (data[0] == 170 && data[1] == 3 && isStatusAsked == false)
				{
                    //Debug.WriteLine("Got a 3, send a 6");
					if (bmChar != null)
					{
						//Debug.WriteLine("bmchar is not null.");
                        Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                        {
                           bmChar.WriteAsync(new byte[] { 0x55, 0x06 });
                        });

					}
					else
					{
						//Debug.WriteLine("bmchar is null.");
					}						
				}
				if (data[0] == 170 && data[1] == 6)// && isStatusAsked == false)
                {
                    //Debug.WriteLine("Got a 6, send a 1");
                    //Debug.WriteLine("bmchar = " + bmChar.ToString());
                    isStopPolling = true;
                    isStatusAsked = true;

                    if (bmChar != null)
                    {
                        //Debug.WriteLine("bmchar is not null.");
                        Xamarin.Forms.Device.BeginInvokeOnMainThread( () =>
                        {
                            bmChar.WriteAsync(new byte[] { 0x55, 0x01 });
                        });
						

                    }else{
                        //Debug.WriteLine("bmchar is null.");
                    }
                        
                }

				if (data[0] == 170 && data[1] == 1)// && isDataAsked == false)
                {
                    //Debug.WriteLine("Got a 1, send for data");
                    //Debug.WriteLine("bmchar = "+bmChar.ToString());
                    isDataAsked = true;

					if (bmChar != null)
					{
						//Debug.WriteLine("bmchar is not null.");
                        Xamarin.Forms.Device.BeginInvokeOnMainThread( () =>
                        {
                            bmChar.WriteAsync(new byte[] { 0x55, 0x02, 0x01, 0x00 });
                        });
					}
					else
					{
						//Debug.WriteLine("bmchar is null.");
					}
                        
                }
                if (timespolled>25 && isDataAsked == false)
                {
					//stuck in a loop
					//try to ask for the data
					isDataAsked = true;

					if (bmChar != null)
					{
						//Debug.WriteLine("bmchar is not null.");
                        Xamarin.Forms.Device.BeginInvokeOnMainThread( () =>
                        {
                            bmChar.WriteAsync(new byte[] { 0x55, 0x02, 0x01, 0x00 });
                        });
					}
					else
					{
						//Debug.WriteLine("bmchar is null.");
					}
					
                }
				if (timespolled > 35 && isDataAsked)
				{
                    //can't get the reading for some reason!
                    //try over and over
                    Xamarin.Forms.Device.BeginInvokeOnMainThread( () =>
                    {
                        bmChar.WriteAsync(new byte[] { 0x55, 0x02, 0x01, 0x00 });
                    });
				}

                // this is data
                if (data[0] == 221)
                {
                    // geting int from two byte
                    // sometime data is 17 long and sometime itis 9 long
                    int dataIndex = data.Length > 9 ? 13 : 5;

                    var fev1 = (double)((data[dataIndex + 1] << 8) + data[dataIndex]) / 100;
                    int pef = (data[dataIndex + 3] << 8) + data[dataIndex + 2];
                    //Debug.WriteLine("fev1: " + fev1 + "  " + "pef: " + pef);

                    isDataAsked = false;
                    isStatusAsked = false;
                    clearReadingOnDevice();
                    //sometimes it doesn't clear so run it again
                    //clearReadingOnDevice();
                    timespolled = 0;

                    if (pef <= 200)
                    {
                        uiController.testAgainDialog();
                    }
                    else
                    {
						
						var reading = new SpirometerReading(DateTime.Now, (decimal)pef, (decimal)fev1);
                        uiController.updateCaller(reading);
                    }

                }
            }
		}
	}
}
