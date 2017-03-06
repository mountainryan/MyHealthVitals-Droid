using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;

namespace MyHealthVitals
{
	public class SpotCheckServiceHandler: IBLEDeviceServiceHandler
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
				uiController.ShowMessageOnUI("Connected.", true);
			}
			else
			{
				uiController.ShowMessageOnUI("Searching device...", false);
				CrossBluetoothLE.Current.Adapter.ConnectToDeviceAsync(connectedDevice);
				// after this it will call central manager and when device_connected event of the central manager fires then it will call again this class discoverServices()
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

		public void startMeasuringBP()
		{
			executeWriteCommand(new byte[] { 0xaa, 0x55, 0x40, 0x02, 0x01, 0x29 });
		}

		public void stopReadingECG()
		{
			executeWriteCommand(new byte[] { 0xaa, 0x55, 0x30, 0x02, 0x02, 0x2 });
		}

		public void startEcgMeasuring()
		{
			executeWriteCommand(new byte[] { 0xaa, 0x55, 0x31, 0x02, 0x02, 0xC6 });
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
			//Debug.WriteLine("guid:D "+e.Characteristic.Id);
			//printUpdatedCharacteristics(e.Characteristic);

			var ch = e.Characteristic;
			// sys , dia and bpm is available in spot check monitor
			if ((int)ch.Value[2] > 63 && (int)ch.Value[2] < 68)
			{
				Debug.WriteLine("NIBP related token.");

				if (countMeasuringPressure == 0)
				{
					uiController.ShowMessageOnUI("Measuring the Blood pressure...", true);
					countMeasuringPressure++;
				}

				if (ch.Value.Length > 21)
				{
					uiController.SYS_DIA_BPM_updated((int)ch.Value[6], (int)ch.Value[8], (int)ch.Value[9]);
					uiController.ShowMessageOnUI("Boood pressure read succesfully.", true);
					countMeasuringPressure = 0;
				}

				if (ch.Value.Length == 8) uiController.updatingPressureMeanTime((int)ch.Value[6]);
			}

			if ((int)ch.Value[2] >= 48 && (int)ch.Value[2] <= 51)
			{
				Debug.WriteLine("this is ECG data:");

				var token = (int)ch.Value[2];
				// this is result of query working state
				if (token == 49)
				{
					var d7_bit = (ch.Value[5] & (1 << 7)) != 0;

					if (d7_bit)
					{
						Debug.WriteLine("measuring state");
					}
					else {
						Debug.WriteLine("standby");
						// command to  start measuring the ecg;
						bmChar.WriteAsync(new byte[] { 0xaa, 0x55, 0x30, 0x02, 0x01, 0xC6 });
					}
				}

				if (token == 50)
				{
					// this is to measure time duration taken by ecg to get its result by itself.
					if (isEcgStarted == false)
					{
						uiController.resetEcgDisplay();

						isEcgStarted = true;
						t1 = DateTime.Now;
						uiController.ShowMessageOnUI("Reading ECG...", true);
					}

					List<int> values = new List<int>();

					for (int i = 6; i < 25 * 2 + 6; i = i + 2)
					{
						var data = ch.Value[i];

						var D7_data = (data & (1 << 7)) != 0;
						if (D7_data == false)
						{
							// the first byte does not contain data out of two byte
							//Debug.Write(" 8bit: " + (int)ch.Value[i + 1]);
							values.Add((int)ch.Value[i + 1]);
						}
						else {
							// the first byte contains status bit and MSB ( 4 bits ) of the ECG waveform data;
							var maskedFoutBit = (int)ch.Value[i] & 15;
							var data1 = (maskedFoutBit >> 8) + (int)ch.Value[i + 1];

							values.Add(data1);
							//Debug.Write("12 bit: " + data1);
						}
					}

					//countOfEcgdataIn30Sec = countOfEcgdataIn30Sec + values.Count;
					//Debug.WriteLine(countOfEcgdataIn30Sec);

					uiController.updateECGPacket(values);
				}

				// end of the ecg reading
				if (token == 51)
				{
					Debug.WriteLine("total duration of ECG measurment: " + (t1.Subtract(DateTime.Now)).TotalMilliseconds);
					isEcgStarted = false;

					var data1 = (int)ch.Value[5];
					switch (data1)
					{
						case 0:
							{
								uiController.ShowMessageOnUI("No irregularity found.", true);
								break;
							}
						case 1:
							{
								uiController.ShowMessageOnUI("Suspected a little fast beat.", true);
								break;
							}
						case 2:
							{
								uiController.ShowMessageOnUI("Suspected fast beat.", true);
								break;
							}
						case 3:
							{
								uiController.ShowMessageOnUI("Suspected short run of fast beat.", true);
								break;
							}
						case 4:
							{
								uiController.ShowMessageOnUI("Suspected a little slow beat.", true);
								break;
							}
						case 5:
							{
								uiController.ShowMessageOnUI("Suspected occasional short beat interval.", true);
								break;
							}
						case 6:
							{
								uiController.ShowMessageOnUI("Suspected occasional short beat interval.", true);
								break;
							}
						case 7:
							{
								uiController.ShowMessageOnUI("Suspected irregular beat interval.", true);
								break;
							}
						case 8:
							{
								uiController.ShowMessageOnUI("Suspected fast beat with short beat interval.", true);
								break;
							}
						case 9:
							{
								uiController.ShowMessageOnUI("Suspected slow beat with short beat interva.", true);
								break;
							}
						case 10:
							{
								uiController.ShowMessageOnUI("Suspected slow beat with irregular beat interval.", true);
								break;
							}
						case 11:
							{
								uiController.ShowMessageOnUI("Waveform baseline wander.", true);
								break;
							}
						case 12:
							{
								uiController.ShowMessageOnUI("Suspected fast beat with baseline wander.", true);
								break;
							}
						case 13:
							{
								uiController.ShowMessageOnUI("Suspected slow beat with baseline wander.", true);
								break;
							}
						case 14:
							{
								uiController.ShowMessageOnUI("Suspected occasional short beat interval with baseline wander.", true);
								break;
							}
						case 15:
							{
								uiController.ShowMessageOnUI("Suspected irregular beat interval with baseline wander.", true);
								break;
							}
						case 16:
							{
								uiController.ShowMessageOnUI("Poor Signal, measure again.", true);
								break;
							}
						default:
							{
								uiController.ShowMessageOnUI("No Result Found.", true);
								break;
							}
					}

					//Debug.WriteLine("bpm reslt of ecg: " + );
					uiController.updateECGEnded((int)ch.Value[7]);
				}

				//printUpdatedCharacteristics(ch);
			}

			if ((int)ch.Value[2] == 115) // this token is for glucose reading
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

			if ((int)ch.Value[2] > 111 && (int)ch.Value[2] < 115)
			{
				Debug.WriteLine("Temparature related token.");

				// from document it is written that the 5 byte is status and D4 is temperature probe is connected
				//Byte temperatureStatus = ;
				// bit 5
				var D4 = (ch.Value[5] & (1 << 4)) != 0;

				// if D4=0 means temperature reading is completed
				if (D4 == false)
				{
					// has the temperature reading
					// combining byte 6 and byte 7 to read temperature
					int data = ((int)ch.Value[6] << 8) + (int)ch.Value[7];
					if (data <= 3) uiController.ShowMessageOnUI("Failed to read temperature.", true);

					if (data >= 1312) uiController.ShowMessageOnUI("Too high temperature.", true);

					// if this condition saisfies the reading is measured in celcious
					if (data >= 200 && data <= 1301)
					{
						uiController.ShowMessageOnUI("Temperature read successfully.", true);
						//decimal temperatureInCelcious = (decimal)((float)633 / 100 + 30);
						//(9.0 / 5.0) * c) +32
						// the device always reads in Celcious even if it is displaying in fareinheit
						var tempC = (double)data / 100 + 30.0;
						double tempF = Math.Round(((9.0 / 5.0) * tempC) + 32, 1);

						uiController.updateTemperature((decimal)tempF, "Celcious");
					}
				}
			}

			//// spo2 , PI and bpm is available in spot check monitor
			if ((int)ch.Value[2] > 80 && (int)ch.Value[2] < 84)
			{
				if (ch.Value[5] == 0 || ch.Value[6] == 0)
				{
					Debug.WriteLine("either start or end of the spo2 reading.");

					if (isSpo2ReadingStarted) { 
						uiController.noticeEndOfReadingSpo2();	
					}

					isSpo2ReadingStarted = false;
				}
				else if(ch.Value.Length == 11) {

					isSpo2ReadingStarted = true;

					int lastSpo2 = (int)ch.Value[5];
					int lastBPM = (int)ch.Value[6];
					uiController.SPO2_readingCompleted(lastSpo2, lastBPM, (float)((int)ch.Value[8]) / 100);
				}
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
		}
	}
}
