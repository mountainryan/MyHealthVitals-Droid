using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;

namespace MyHealthVitals
{
	public class PC100ServiceHandler:IBLEDeviceServiceHandler
	{
		public ICharacteristic bmChar;
		public IDevice connectedDevice;
		public IBluetoothCallBackUpdatable uiController;

		public void reconnectToDevice(IDevice device)
		{
			//this.uiController = (IBluetoothCallBackUpdatable)controller;
			connectedDevice = device;

			if (connectedDevice.State == Plugin.BLE.Abstractions.DeviceState.Connected)
			{
				Debug.WriteLine("PC-100 alrady in connected state.");
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

		public void executeWriteCommand(byte[] byteCommand)
		{
			if (connectedDevice != null && connectedDevice.State == Plugin.BLE.Abstractions.DeviceState.Connected)
			{
				bmChar.WriteAsync(byteCommand);
			}
			else {
				uiController.ShowMessageOnUI("Device is not connected. Please connect and try again.", false);
			}
		}

		//NIBP commands
		public void startMeasuringBP()
		{
			executeWriteCommand(new byte[] { 0xAA, 0x55, 0x40, 0x02, 0x01, 0x29 });
		}

		public void stoptMeasuringBP()
		{
			executeWriteCommand(new byte[] { 0xAA, 0x55, 0x40, 0x02, 0x02, 0x3D });
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


		int countMeasuringPressure = 0;
		//int lastSpo2 = 0;
		//int lastBPM = 0;
		//bool isReadingGlucose = false;
		decimal glucoseReadingVal = -1;
		string gluUnit = "";

		//int countOfEcgdataIn30Sec = 0;
		bool isEcgStarted = false;
		DateTime t1;
		//DateTime t2;
		bool isSpo2ReadingStarted = false;
		public void C_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
		{
			var ch = e.Characteristic;

			// sys , dia and bpm is available in spot check monitor
			if ((int)ch.Value[2] > 63 && (int)ch.Value[2] < 68)
			{
				Debug.WriteLine("NIBP related token.");

				if (countMeasuringPressure == 0)
				{
					//uiController.ShowMessageOnUI("Measuring the Blood pressure...", true);
					countMeasuringPressure++;
				}

				if (ch.Value.Length > 21)
				{
					uiController.SYS_DIA_BPM_updated((int)ch.Value[6], (int)ch.Value[8], (int)ch.Value[9]);
					//uiController.ShowMessageOnUI("Boood pressure read succesfully.", true);
					countMeasuringPressure = 0;
				}

				//if (ch.Value.Length == 8) uiController.updatingPressureMeanTime((int)ch.Value[6]);
			}

			// error in blood pressure reading
			if (ch.Value.Length == 14)
			{
				String message = "";
				switch (127 & (int)ch.Value[5])
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

				uiController.ShowMessageOnUI(message, true);
			}

			//// spo2 , PI and bpm is available in spot check monitor
			if ((int)ch.Value[2] > 80 && (int)ch.Value[2] < 84)
			{
				printUpdatedCharacteristics(e.Characteristic);

				if (ch.Value[5] == 0 || ch.Value[6] == 0)
				{
					Debug.WriteLine("either start or end of the spo2 reading.");

					if (isSpo2ReadingStarted)
					{
						uiController.noticeEndOfReadingSpo2();
					}

					isSpo2ReadingStarted = false;
				}
				else if (ch.Value.Length > 8)
				{

					isSpo2ReadingStarted = true;

					int lastSpo2 = (int)ch.Value[5];
					int lastBPM = (int)ch.Value[6];
					uiController.SPO2_readingCompleted(lastSpo2, lastBPM, (float)((int)ch.Value[8]) / 100);
				}
			}

			// this token is for glucose reading
			if ((int)ch.Value[2] == 115)
			{
				// this is needed because device is reading same data more than once to we are tracking glucose reading stop and sending the last reading
				if (glucoseReadingVal == -1)
				{
					Xamarin.Forms.Device.StartTimer(TimeSpan.FromMilliseconds(10000), () =>
					{
						if (glucoseReadingVal > 0)
						{
							uiController.updateGlucoseReading(glucoseReadingVal, gluUnit);
						}

						glucoseReadingVal = -1;
						return false;
					});
				}

				// combining byte 6 and byte 7 to read temperature
				int data = ((int)ch.Value[6] << 8) + (int)ch.Value[7];

				// status bit
				var D0_data1 = ((int)ch.Value[5] & (1 << 0)) != 0;

				if (D0_data1)
				{
					glucoseReadingVal = (decimal)data;
					gluUnit = "mg/dL";
				}
				else
				{
					glucoseReadingVal = (decimal)data / 10;
					gluUnit = "Mmol/L";
				}
			}
		}
	}
}
