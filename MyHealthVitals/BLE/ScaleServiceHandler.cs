using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System.Linq;

namespace MyHealthVitals
{
	public class ScaleServiceHandler : IBLEDeviceServiceHandler
	{
		public ICharacteristic bmChar;
		public IDevice connectedDevice;
		public IBluetoothCallBackUpdatable uiController;

		public void GetData(Byte[] bytes)
		{ 
            //Debug.WriteLine("Using BLE 2");
			//get the bytes into an int array
			int[] ints = bytes.Select(x => (int)x).ToArray();
            ManipData(ints);
        }

		public void C_ValueUpdated(object sender, CharacteristicUpdatedEventArgs e)
		{
			//Debug.WriteLine("Using BLE 1");
			var ch = e.Characteristic;
			//Debug.WriteLine("C_ValueUpdated CH.VALUE.SIZE == " + ch.Value.Length);
			/*
			foreach (var s in ch.Value)
			{
				Debug.WriteLine("value in ch = " + Int32.Parse(Convert.ToString(s, 2)).ToString("0000 0000"));
			}*/

			//	Debug.WriteLine("ch.Value[0] = " + ch.Value[0]);
			//	Debug.WriteLine("ch.Value[1] = " + ch.Value[1]);
			//Debug.WriteLine("ch.Value[0] = " + ch.Value[0]);
			int[] ints = ch.Value.Select(x => (int)x).ToArray();
			ManipData(ints);
		}

		public void ManipData(int[] vals)
		{
			//token in NIBP result in pc-100
			if ((int)vals[0] == 0xFF)
			{
				Debug.WriteLine("vals[0] = " + vals[0]);
				Debug.WriteLine("vals[1] = " + vals[1]);
				Debug.WriteLine("vals[2] = " + vals[2]);
				int type = (int)vals[1] >> 6;
				//string hex = BitConverter.ToString(vals);
				//Debug.WriteLine("hex = " + hex);
				double weight_adj = (((vals[1] - 128) << 8) + vals[2]) * 2.20462262185 / 10;
				Debug.WriteLine("weight adj = " + weight_adj);
				double weight = ((vals[1] & 0x3F) << 8) | vals[2];
				Debug.WriteLine("weight = " + weight);
				weight = Math.Round(weight * 2.20462262185 / 10, 1);
				Debug.WriteLine("weight = " + weight);

				//weight = Math.Round((weight / 10), 2);
                this.uiController.updated_Weight((decimal)weight);

				// the data is kg no matter the first bits is 01,10 or 11

				/*
				if (type == 1)//KG
				{
					//1 kg = 2.20462262185 lb
					weight = Math.Round(weight *= 2.20462262185, 2);
					Debug.WriteLine("weight = " + weight);
					this.uiController.updated_Weight((decimal)weight);
				}
				else if (type == 2) //LB
				{                  
					Debug.WriteLine("weight = " + weight);
					this.uiController.updated_Weight((decimal)weight);
				}
				else if (type == 3)//ST
				{
					//1 st = 14 lb
					weight = Math.Round(weight *= 14, 2);
					Debug.WriteLine("weight = " + weight);
					this.uiController.updated_Weight((decimal)weight);
				}*/
			}
		}
		public async Task diconnectServices(IDevice device)
		{
			this.connectedDevice = device;
			//this.uiController = (IBluetoothCallBackUpdatable)controller;

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

		public async void discoverServices(IDevice device)
		{
			this.connectedDevice = device;
			//this.uiController = (IBluetoothCallBackUpdatable)controller;

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
		}

		public void reconnectToDevice(IDevice device)
		{
			connectedDevice = device;

			if (connectedDevice.State == Plugin.BLE.Abstractions.DeviceState.Connected)
			{
				Debug.WriteLine("Scales alrady in connected state.");
			}
			else
			{
				CrossBluetoothLE.Current.Adapter.ConnectToDeviceAsync(connectedDevice);
			}
		}
		public void updateController(IBluetoothCallBackUpdatable controller)
		{
			this.uiController = controller;
		}
	}
}
