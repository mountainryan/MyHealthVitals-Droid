using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;

namespace MyHealthVitals
{
	public class PC100ServiceHandler : IBLEDeviceServiceHandler
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
				Debug.WriteLine("reconnectToDevice connectedDevice = " + connectedDevice );

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
			executeWriteCommand(new byte[] { 0xAA, 0x55, 0x40, 0x02, 0x02, 0xCB });
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

		//	Debug.WriteLine(string.Format("UUID: {0}  ->{1}", ch.Uuid, sb.ToString()));
		}

		decimal glucoseReadingVal = -1;
		string gluUnit = "";
		int glucoseResult = -1;

		int tempReadingCount = 0;

		public void C_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
		{
			var ch = e.Characteristic;
			Debug.WriteLine("ch.Value[2]" + ch.Value[2]);



			if ((int)ch.Value[2] == 66)
			{
				Debug.WriteLine("ch.Value.Length  ===  " + ch.Value.Length);
				if (ch.Value.Length >= 9)
				{
					int sys = ((int)ch.Value[5] << 8) + (int)ch.Value[6];
					Debug.WriteLine("sys" + sys);
					this.uiController.SYS_DIA_BPM_updated(sys, (int)ch.Value[8], 0);

				}
				else if(ch.Value.Length >= 7)
				{
					int sys = ((int)ch.Value[5] << 8) + (int)ch.Value[6];
					Debug.WriteLine("sys" + sys);
					this.uiController.SYS_DIA_BPM_updated(sys, 0, 0);
						//	Debug.WriteLine("ch.Value[8]" + ch.Value[8]);
				}
			}
		//token in NIBP result in pc-100
			else if ((int)ch.Value[2] == 67)
			{


				// checking length of data
				if ((int)ch.Value[3] == 7)
				{
					// combining byte 6 and byte 7 to read temperature
					// high byte << 8 + low byte
					int sys = ((int)ch.Value[5] << 8) + (int)ch.Value[6];
					//int map = (int)ch.Value[7];
					int dia = (int)ch.Value[8];
					int heartRate = (int)ch.Value[9];

					//Debug.WriteLine(sys);

					this.uiController.SYS_DIA_BPM_updated(sys, dia, heartRate);
				}

				// error
				if ((int)ch.Value[3] == 3)
				{
					var bit7 = (ch.Value[5] & (1 << 7));

					var bit3 = (ch.Value[5] & (1 << 3));
					var bit2 = (ch.Value[5] & (1 << 2));
					var bit1 = (ch.Value[5] & (1 << 1));
					var bit0 = (ch.Value[5] & (1 << 0));

					var bit3_0 = ""+bit3+""+bit2+""+bit1+""+bit0;

					var message = "";

					if (bit7==1)
					{
						switch (Convert.ToInt32(bit3_0))
						{
							case 1:
								message = "Pressure did not reach 30 mmHg in 7 seconds.";
								break;
							case 2:
								message = "Pressure over 295mmHg, device is self - protecting.";
								break;
							case 3:
								message = "Can't detect pulse.";
								break;
							case 4:
								message = "Too many interferennce. Movements, Talking.";
								break;
							case 5:
								message = "Result value incorrect.";
								break;
							case 6:
								message = "Air leakage.";
								break;
							case 15:
								message = "Low Battery, measurement stopped.";
								break;
						}

						Debug.WriteLine("error type 1");
						Debug.WriteLine(message);
					}
					else {
						
						switch (Convert.ToInt32(bit3_0))
						{
							case 0:
								message = "Can't detect pulse.";
								break;
							case 1:
								message = "Pressure did not reach 30 mmHg in 7 seconds.";
								break;
							case 2:
								message = "Result value incorrect.";
								break;
							case 3:
								message = "Pressure over 295mmHg, device is self-protecting.";
								break;
							case 4:
								message = "Too many interferennce. Movements, Talking.";
								break;
							case 15:
								message = "Low Battery, measurement stopped.";
								break;
						}

						Debug.WriteLine("error type 0");
						Debug.WriteLine(message);
					}

					this.uiController.ShowMessageOnUI(message, false, "Blood Pressure Measure Error");
				}
			}

			/// <summary>
			/// Spo2 related parsing
			/// </summary>
			/// 
			/// 
			if ((int)ch.Value[2] == 82)
			{
				// when waveform data comes down to 0 then it is end of the spo2 reading
				var waveformData = (int)ch.Value[5];
				//Debug.WriteLine("WaveformData: " + waveformData);
				//printUpdatedCharacteristics(ch);

				this.uiController.updateBpmWaveform((int)ch.Value[6]);

				if (waveformData == 0) {
					Debug.WriteLine("Wave form Data");
				//	this.uiController.noticeEndOfReadingSpo2();
				}
			}
		

			if ((int)ch.Value[2] == 80 && (int)ch.Value[4] == 2) { 
                    this.uiController.noticeEndOfReadingSpo2();
			}
			if ((int)ch.Value[2] == 83 && (int)ch.Value[3] == 7)
			{
				if (ch.Value[5] == 0 || ch.Value[6] == 0)
				{
					Debug.WriteLine("Invallid readings.");
				}
				else { 
					int lastSpo2 = (int)ch.Value[5];
					int lastBPM = (int)ch.Value[6];

					Debug.WriteLine("this.uiController.SPO2_readingCompleted");

					this.uiController.SPO2_readingCompleted(lastSpo2, lastBPM, (float)((int)ch.Value[8])/10);
				}
			}

			// this token is for GLU data pc-100
			//if ((int)ch.Value[2] == 226)
			//{
			//	var bit5 = (ch.Value[5] & (1 << 5)) != 0;
			//	var bit4 = (ch.Value[5] & (1 << 4)) != 0;

			//	// normal reading
			//	if (bit5 && bit4) {
			//		var dataGlu = (double)((int)ch.Value[6] * 100 + (int)ch.Value[7])/10;
			//		Debug.WriteLine("dataGlu: " + dataGlu);
			//	}
			//}

			// temperature related token with result 0x72 = 114 in pc-100
			if ((int)ch.Value[2] == 114)
			{
				Debug.WriteLine("Temparature related token pc100.");
				//int dataTemp1 = ((int)ch.Value[6] << 8) + (int)ch.Value[7];
				//Debug.WriteLine("datatemp: " + dataTemp1);
				//printUpdatedCharacteristics(ch);

				// from document it is written that the 5 byte is status and D4 is temperature probe is connected
				//Byte temperatureStatus = ;
				// bit 5
				var bit4 = ch.Value[5] & (1 << 4);
				if (bit4 == 1)
				{
					uiController.ShowMessageOnUI("Probe disconnected while measuring.", true);
					return;
				}

				var bit5 = ch.Value[5] & (1 << 5);
				var bit6 = ch.Value[5] & (1 << 6);

				if (bit6 == 1 && bit5 == 1)
				{
					uiController.ShowMessageOnUI("Temperature measuring time out.", true, "Temperature measuring ");
					return;
				}

				// measurement finished normallly
				// combining byte 6 and byte 7 to read temperature
				int dataTemp = ((int)ch.Value[6] << 8) + (int)ch.Value[7];

				if (dataTemp >= 200 && dataTemp <= 1301)
				{
					// this variable is introduced because it is reading temperature twice identical data and we send only last one to server and UI
					tempReadingCount++;

					//uiController.ShowMessageOnUI("Temperature read successfully.", true);
					//decimal temperatureInCelcious = (decimal)((float)633 / 100 + 30);
					//(9.0 / 5.0) * c) +32
					// the device always reads in Celcious even if it is displaying in fareinheit
					if (tempReadingCount == 2)
					{
						var tempC = (double)dataTemp / 100 + 30.0;
						double tempF = Math.Round(((9.0 / 5.0) * tempC) + 32, 1);
						this.uiController.updateTemperature((decimal)tempF);
						tempReadingCount = 0;
					}
				}
				else
				{
					if (dataTemp >= 1312) uiController.ShowMessageOnUI("Too high temperature.", true, "Temperature measuring ");
					if (dataTemp < 200) uiController.ShowMessageOnUI("Too low temperature.", true, "Temperature measuring ");
				}
			}


			Debug.WriteLine("ch.Value[2] = " + ch.Value[2]);
			Debug.WriteLine("ch.Value[3] = " + ch.Value[3]);
			Debug.WriteLine("ch.Value[4] = " + ch.Value[4]);
			Debug.WriteLine("ch.Value[5] = " + ch.Value[5]);

			// this token is for glucose reading
			if ((int)ch.Value[2] == 115) 
			{
				Debug.WriteLine("glucose");
				// this is needed because device is reading same data more than once to we are tracking glucose reading stop and sending the last reading
				if (glucoseResult == -1)
				{
					Xamarin.Forms.Device.StartTimer(TimeSpan.FromMilliseconds(5000), () =>
					{
						if (glucoseResult >= 0)
						{
							if (glucoseResult != 0)
							{
								string title = glucoseResult == 2 ? "Too Low" : "Too High";
								uiController.ShowMessageOnUI("The measurement result is out of range", true, title);							
							}
							else
							{
								uiController.updateGlucoseReading(glucoseReadingVal, gluUnit);
							}
						}

						glucoseResult = -1;
						return false;
					});
				}

				// combining byte 6 and byte 7 to read temperature
				// status bit
			
				glucoseResult = ((int)ch.Value[5] >> 3) & 3;
				Debug.WriteLine("correctResult= " + glucoseResult);
				if (glucoseResult == 0)
				{
					int D0_data1 = (int)ch.Value[5];
					Debug.WriteLine("((int)ch.Value[6] << 8) + (int)ch.Value[7] = " + ((int)ch.Value[6] << 8) + (int)ch.Value[7]);
					Debug.WriteLine("(int)ch.Value[6] * 100 + (int)ch.Value[7] = " + (int)ch.Value[6] * 100 + (int)ch.Value[7]);
					if ((D0_data1 & 1) == 1)
					{
						int dataMgDl = ((int)ch.Value[6] << 8) + (int)ch.Value[7];
						glucoseReadingVal = (decimal)dataMgDl;
						gluUnit = "mg/dL";
					}
					else
					{
						int dataMmol = (int)ch.Value[6] * 100 + (int)ch.Value[7];
						glucoseReadingVal = (decimal)dataMmol / 10;
						gluUnit = "Mmol/L";
					}

					Debug.WriteLine("D0_data1 = " + D0_data1);
					Debug.WriteLine("glucoseReadingVal = " + glucoseReadingVal);
					Debug.WriteLine("gluUnit = " + gluUnit);
				}
			}
		}
	}
}
