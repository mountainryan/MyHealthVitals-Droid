using Plugin.BLE;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Plugin.BLE.Abstractions.Contracts;
//using UIKit;

namespace MyHealthVitals
{
	public class BleManager
	{

		public BleManager()
		{
		}

		public IAdapter Adapter;
		//private UIViewController nextView;
		//private UINavigationController nav;

		private VitalsData vitalsData;
		public void Connect(VitalsData data)
		{
			vitalsData = data;
			//var ble = CrossBluetoothLE.Current;
			Adapter = CrossBluetoothLE.Current.Adapter;
			//var state = ble.State;


			Adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
			Adapter.DeviceConnected += Adapter_DeviceConnected;

			Adapter.StartScanningForDevicesAsync();

		}

		private async void Adapter_DeviceConnected(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{
			var services = await e.Device.GetServicesAsync();
			foreach (var s in services)
			{
				var characteristics = await s.GetCharacteristicsAsync();
				foreach (var c in characteristics)
				{
					Debug.WriteLine(string.Format("Char UUID: {0}  Value: {1}", c.Uuid, c.Value));
					if (c.CanUpdate)
					{
						c.ValueUpdated += C_ValueUpdated;
						c.StartUpdatesAsync();
					}
				}
			}
			if (OnDeviceConnected != null)
				OnDeviceConnected();

		}


		private void C_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
		{

			if (e.Characteristic.Value.Length > 10 && e.Characteristic.Value.Length < 22)
			{
				//vitalsData.Bpm = (int)e.Characteristic.Value[10];
				//vitalsData.Bpm = (int)e.Characteristic.Value[9];
				vitalsData.SpO2 = (int)e.Characteristic.Value[5];
			}

			else if (e.Characteristic.Value.Length > 21)
			{
				vitalsData.Bpm = (int)e.Characteristic.Value[9];
				vitalsData.BPSys = (int)e.Characteristic.Value[6];
				vitalsData.BPDia = (int)e.Characteristic.Value[8];

			}

			if (e.Characteristic.Value.Length == 9)
			{
				vitalsData.Temp = 98;
			}

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

		private void Adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{
			var dev = e.Device.Name;
			Debug.WriteLine(string.Format("Device Found: {0}", dev));
			if (dev == "PC_300SNT")
			{
				Adapter.StopScanningForDevicesAsync();
				Adapter.ConnectToDeviceAsync(e.Device);
			}
		}

		public delegate void DeviceConnectedEventHandler();
		public event DeviceConnectedEventHandler OnDeviceConnected;
	}
}
