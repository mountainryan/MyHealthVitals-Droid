
using System;
using Plugin.BLE;
using System.Diagnostics;
using Plugin.BLE.Abstractions.Contracts;
using System.Collections.Generic;
using System.Text;

namespace MyHealthVitals
{
	public class BleManagerSpirometer
	{
		public static ICharacteristic bmChar;
		public static IDevice connectedSpotCheck;

		public static IAdapter Adapter;
		public static BLEReadingUpdatableSpiroMeter uiController;

		public void ScanToConnectToSpotCheck(BLEReadingUpdatableSpiroMeter uiController)
		{
			BleManagerSpirometer.uiController = uiController;

			isStopPolling = false;

			if (Adapter == null)
			{
				Adapter = CrossBluetoothLE.Current.Adapter;
				Adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
				Adapter.DeviceConnected += Adapter_DeviceConnected;
				Adapter.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
				Adapter.DeviceConnectionLost += Adapter_DeviceConnectionLost;
				Adapter.StartScanningForDevicesAsync();
			}
			else {
				if (connectedSpotCheck != null && connectedSpotCheck.State == Plugin.BLE.Abstractions.DeviceState.Connected)
				{
					startPolling();
				}
				else
				{
					if (connectedSpotCheck != null)
					{
						Adapter.ConnectToDeviceAsync(connectedSpotCheck);
					}
					else {
						Adapter.StartScanningForDevicesAsync();
					}
				}
			}
		}

		public void clearReadingOnDevice() {
			bmChar.WriteAsync(new byte[] { 0x55, 0x03 });
		}

		public void stopPolling() {
			isStopPolling = true;
		}

		private void Adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{
			Debug.WriteLine(string.Format("Device Found: {0}", e.Device.Name));

			if (e.Device.Name == "BLE-MSA")
			{
				Adapter.StopScanningForDevicesAsync();
				Adapter.ConnectToDeviceAsync(e.Device);
			}
		}

		private void Adapter_DeviceConnected(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{
			Debug.WriteLine("Adapter_DeviceConnected.");
			connectedSpotCheck = e.Device;
			discoverServices();
		}

		private async void discoverServices()
		{
			var services = await connectedSpotCheck.GetServicesAsync();
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

			//connectedPeripheral.WriteValue(NSData.FromArray(new byte[] { 0x55, 0x06 }), this.bmChar, CBCharacteristicWriteType.WithResponse);
		}

		private void startPolling() { 
			Xamarin.Forms.Device.StartTimer(TimeSpan.FromMilliseconds(50), () =>
			{
				if (isStopPolling == false)
				{
					Debug.WriteLine("polling...");
					bmChar.WriteAsync(new byte[] { 0x55, 0x06 });
				}
				return !isStopPolling;
			});
		}

		void Adapter_DeviceConnectionLost(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceErrorEventArgs e)
		{
			Debug.WriteLine("PC_300SNT just disconnected");
		}

		void Adapter_ScanTimeoutElapsed(object sender, EventArgs e)
		{
			Debug.WriteLine("Adapter_ScanTimeoutElapsed.");
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

			Debug.WriteLine(string.Format("UUID: {0}  ->{1}", ch.Uuid, sb.ToString()));
		}

		bool isStopPolling = false;
		bool isStatusAsked = false;
		bool isDataAsked = false;

		//int pefReading = -1;

		private void C_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
		{
			printUpdatedCharacteristics(e.Characteristic);

			var data = e.Characteristic.Value;

			if (data[0] == 170 &&  ( data[1] == 1 || data[1] == 2 ) && isStatusAsked == false)
			{
				isStopPolling = true;
				isStatusAsked = true;

				if (bmChar != null)
					bmChar.WriteAsync(new byte[] { 0x55, 0x01 });
			}

			if (data[0] == 170 && data[1] == 6 && isDataAsked == false)
			{
				isStopPolling = true;
				isDataAsked = true;

				if (bmChar != null)
					bmChar.WriteAsync(new byte[] { 0x55, 0x02, 0x06, 0x00 });
			}

			// this is data
			if (data[0] == 221)
			{
				// geting int from two byte
				var fev1 = (double)((data[14] << 8) + data[13])/100;
				int pef = (data[16] << 8) + data[15];

				//var fev11 = (double)fev1 / 100.0;

				isDataAsked = false;
				isStatusAsked = false;

				uiController.updateCaller(pef, (decimal)fev1);
				clearReadingOnDevice();
				Debug.WriteLine("fev1: " + fev1 + "  " + "pef: " + pef);
			}
		}
	}
}
