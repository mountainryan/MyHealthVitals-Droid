using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;

namespace MyHealthVitals
{
	
	public class SpirometerServiceHandler: IBLEDeviceServiceHandler
	{
		public static ICharacteristic bmChar;
		public IDevice connectedDevice;
		public bool isStopPolling = false;
		public BLEReadingUpdatableSpiroMeter uiController;

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
				Debug.WriteLine("reconnectToDevice connectedDevice = " + connectedDevice );

				CrossBluetoothLE.Current.Adapter.ConnectToDeviceAsync(connectedDevice);
				// after this it will call central manager and when device_connected event of the central manager fires then it will call again this class discoverServices()
			}
		}

		public async void discoverServices(IDevice device)
		{
			//uiController = (BLEReadingUpdatableSpiroMeter)controller;
			connectedDevice = device;

			var services = await connectedDevice.GetServicesAsync();
			foreach (var s in services)
			{
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
			this.isStopPolling = true;
		}

		private void startPolling()
		{
			isStopPolling = false;

			Xamarin.Forms.Device.StartTimer(TimeSpan.FromMilliseconds(250), () =>
			{
				if (isStopPolling == false)
				{
					Debug.WriteLine("polling for data...");
					bmChar.WriteAsync(new byte[] { 0x55, 0x06 });
				}
				return !isStopPolling;
			});
		}

		public void clearReadingOnDevice()
		{
			bmChar.WriteAsync(new byte[] { 0x55, 0x03 });
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

		//int pefReading = -1;

		public void C_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
		{
			//printUpdatedCharacteristics(e.Characteristic);

			var data = e.Characteristic.Value;
			//Debug.WriteLine("C_ValueUpdated  data[0]==" +data[0] +"   data[1]" +data[1]);
			if (data[0] == 170 && data[1] == 6 && isStatusAsked == false)
			{
				isStopPolling = true;
				isStatusAsked = true;

				if (bmChar != null)
					bmChar.WriteAsync(new byte[] { 0x55, 0x01 });
			}

			if (data[0] == 170 && data[1] == 1 && isDataAsked == false)
			{
				isDataAsked = true;

				if (bmChar != null)
					bmChar.WriteAsync(new byte[] { 0x55, 0x02, 0x01, 0x00 });
			}

			// this is data
			if (data[0] == 221)
			{
				// geting int from two byte
				// sometime data is 17 long and sometime itis 9 long
				int dataIndex = data.Length > 9 ? 13 : 5;

				var fev1 = (double)((data[dataIndex + 1] << 8) + data[dataIndex]) / 100;
				int pef = (data[dataIndex + 3] << 8) + data[dataIndex + 2];
				Debug.WriteLine("fev1: " + fev1 + "  " + "pef: " + pef);

				isDataAsked = false;
				isStatusAsked = false;
                clearReadingOnDevice();

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
