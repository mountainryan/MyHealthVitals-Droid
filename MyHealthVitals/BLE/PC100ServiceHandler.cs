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
	public class PC100ServiceHandler : IBLEDeviceServiceHandler
	{
		public ICharacteristic bmChar;
		public IDevice connectedDevice;
		public IBluetoothCallBackUpdatable uiController;

		public void GetData(Byte[] bytes)
		{ 
			//Debug.WriteLine("Using BLE 2");
            //Debug.WriteLine("Made it into PC100ServiceHandler.GetData()");
			//get the bytes into an int array
			int[] ints = bytes.Select(x => (int)x).ToArray();
            ManipData(ints);
        }

		public void reconnectToDevice(IDevice device)
		{
			//this.uiController = (IBluetoothCallBackUpdatable)controller;
			connectedDevice = device;

			if (connectedDevice.State == Plugin.BLE.Abstractions.DeviceState.Connected)
			{
				//Debug.WriteLine("PC-100 already in connected state.");
			}



			else
			{
				//Debug.WriteLine("reconnectToDevice connectedDevice = " + connectedDevice );

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
		public async void startMeasuringBP()
		{
            glucoseResult = -100;
            if (BLE_val.BLE_value == 1)
            {
                executeWriteCommand(new byte[] { 0xAA, 0x55, 0x40, 0x02, 0x01, 0x29 });
            }else{
				try
				{
					Guid gservice = new Guid("0000fff0-0000-1000-8000-00805f9b34fb");
					Guid gchar = new Guid("0000fff2-0000-1000-8000-00805f9b34fb");
					nexus.protocols.ble.connection.IBleGattServer GattServer = BLEdata.gattserver;
					// The resulting value of the characteristic is returned. In nearly all cases this
					// will be the same value that was provided to the write call (e.g. `byte[]{ 1, 2, 3 }`)
					var value = await GattServer.WriteCharacteristicValue(
					   gservice,
					   gchar,
					   new byte[] { 0xAA, 0x55, 0x40, 0x02, 0x01, 0x29 });
				}
				catch (GattException ex)
				{
					//Debug.WriteLine("starting BP error msg: " + ex.Message);
				}
            }
			
		}

		public async void stoptMeasuringBP()
		{
            glucoseResult = -1;
            if (BLE_val.BLE_value == 1)
            {
                executeWriteCommand(new byte[] { 0xAA, 0x55, 0x40, 0x02, 0x02, 0xCB });
            }else{
				try
				{
					Guid gservice = new Guid("0000fff0-0000-1000-8000-00805f9b34fb");
					Guid gchar = new Guid("0000fff2-0000-1000-8000-00805f9b34fb");
					nexus.protocols.ble.connection.IBleGattServer GattServer = BLEdata.gattserver;
					// The resulting value of the characteristic is returned. In nearly all cases this
					// will be the same value that was provided to the write call (e.g. `byte[]{ 1, 2, 3 }`)
					var value = await GattServer.WriteCharacteristicValue(
					   gservice,
					   gchar,
					   new byte[] { 0xAA, 0x55, 0x40, 0x02, 0x02, 0xCB });
				}
				catch (GattException ex)
				{
					//Debug.WriteLine("stopping BP error msg: " + ex.Message);
				}
            }
		}
		public async void stopMeasuringSpo2()
		{
            glucoseResult = -1;
            if (BLE_val.BLE_value == 1)
            {
                executeWriteCommand(new byte[] { 0xAA, 0x55, 0x50, 0x02, 0x02, 0x85 });
            }else{
				try
				{
					Guid gservice = new Guid("0000fff0-0000-1000-8000-00805f9b34fb");
					Guid gchar = new Guid("0000fff2-0000-1000-8000-00805f9b34fb");
					nexus.protocols.ble.connection.IBleGattServer GattServer = BLEdata.gattserver;
					// The resulting value of the characteristic is returned. In nearly all cases this
					// will be the same value that was provided to the write call (e.g. `byte[]{ 1, 2, 3 }`)
					var value = await GattServer.WriteCharacteristicValue(
					   gservice,
					   gchar,
					   new byte[] { 0xAA, 0x55, 0x50, 0x02, 0x02, 0x85 });
				}
				catch (GattException ex)
				{
					//Debug.WriteLine("stopping sp02 error msg: " + ex.Message);
				}
            }
		}

		public async void getBPreading()
		{
            glucoseResult = -1;
            if (BLE_val.BLE_value == 1)
            {
                //Debug.WriteLine("Executing write command!");
                executeWriteCommand(new byte[] { 0xAA, 0x55, 0x43, 0x02, 0x01, 0xCD });
            }else{
				try
				{
					Guid gservice = new Guid("0000fff0-0000-1000-8000-00805f9b34fb");
					Guid gchar = new Guid("0000fff2-0000-1000-8000-00805f9b34fb");
					nexus.protocols.ble.connection.IBleGattServer GattServer = BLEdata.gattserver;
					// The resulting value of the characteristic is returned. In nearly all cases this
					// will be the same value that was provided to the write call (e.g. `byte[]{ 1, 2, 3 }`)
					var value = await GattServer.WriteCharacteristicValue(
					   gservice,
					   gchar,
					   new byte[] { 0xAA, 0x55, 0x43, 0x02, 0x01, 0xCD });
				}
				catch (GattException ex)
				{
					//Debug.WriteLine("get BP reading error msg: " + ex.Message);
				}
            }
		}

		public async void getBatteryInfo()
		{
            //Debug.WriteLine("Called getBatteryInfo()");
			if (BLE_val.BLE_value == 1)
			{
				executeWriteCommand(new byte[] { 0xAA, 0x55, 0x51, 0x02, 0x01, 0xC8 });
			}
			else
			{
				try
				{
					Guid gservice = new Guid("0000fff0-0000-1000-8000-00805f9b34fb");
					Guid gchar = new Guid("0000fff2-0000-1000-8000-00805f9b34fb");
					nexus.protocols.ble.connection.IBleGattServer GattServer = BLEdata.gattserver;
					// The resulting value of the characteristic is returned. In nearly all cases this
					// will be the same value that was provided to the write call (e.g. `byte[]{ 1, 2, 3 }`)
					var value = await GattServer.WriteCharacteristicValue(
					   gservice,
					   gchar,
					   new byte[] { 0xAA, 0x55, 0x51, 0x02, 0x01, 0xC8 });
				}
				catch (GattException ex)
				{
					//Debug.WriteLine("stopping sp02 error msg: " + ex.Message);
				}
			}
		}

		public async Task discoverServices(IDevice device)
		{
			//Debug.WriteLine("discoverServices");
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
		public async Task diconnectServices(IDevice device)
		{
			//Debug.WriteLine("discoverServices");
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

		private void printUpdatedCharacteristics(int[] vals)
		{
			List<int> values = new List<int>();
			foreach (var b in vals)
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
		int last_status = 0;
		int err_status = 1;

        public void C_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
        {
			//Debug.WriteLine("Using BLE 1");
            var ch = e.Characteristic;
            //Debug.WriteLine("vals[2]" + vals[2]);

            //Debug.WriteLine("vals.Length = "+vals.Length);
            //Debug.WriteLine("vals[2] = "+vals[2]);

            int[] ints = ch.Value.Select(x => (int)x).ToArray();
            //ch.Id
              //Debug.WriteLine("char id = " + ch.Id);
            ManipData(ints);
        }

		public void ManipData(int[] vals)
		{
            
            /*for (int i = 0; i < vals.Length;i++) 
            {
                Debug.WriteLine("vals["+i.ToString()+"]="+vals[i]);
            }*/


            //check battery level
			if (vals.Length > 2)
			{
				if (vals[2] == 81 && vals[3] == 5)
				{
					//just need to compare with last 3 bits
					int batteryLife = vals[7] & 7;
					//Debug.WriteLine("Battery Life = " + batteryLife);
					if (batteryLife <= 1)
					{
						uiController.ShowMessageOnUI("You must plug in your PC-100 device.", true, "Low Battery");
					}
				}
			}

			if ((int)vals[2] == 66)
			{
				glucoseResult = -100;
                //Debug.WriteLine("vals.Length  ===  " + vals.Length);

                if (vals.Length == 19)
                {
                    this.uiController.SYS_DIA_BPM_updated(0, 0, 9999);
                }
				else if (vals.Length >= 9)
				{
					//Debug.WriteLine("vals[5] = " + vals[5]);
					//Debug.WriteLine("vals[6] = " + vals[6]);
					//Debug.WriteLine("vals[7] = " + vals[7]);
					//Debug.WriteLine("vals[8] = " + vals[8]);
					int sys = ((int)vals[5] << 8) + (int)vals[6];
					//Debug.WriteLine("sys" + sys);
					this.uiController.SYS_DIA_BPM_updated(sys, (int)vals[8], 0);

				}
				else if(vals.Length >= 7)
				{
					//Debug.WriteLine("vals[5] = " + vals[5]);
					//Debug.WriteLine("vals[6] = " + vals[6]);
					//Debug.WriteLine("vals[7] = " + vals[7]);
					//Debug.WriteLine("vals[8] = " + vals[8]);
					int sys = ((int)vals[5] << 8) + (int)vals[6];
					//Debug.WriteLine("sys" + sys);
					this.uiController.SYS_DIA_BPM_updated(sys, 0, 0);
						//	Debug.WriteLine("vals[8]" + vals[8]);
				}
			}
		//token in NIBP result in pc-100
			else if ((int)vals[2] == 67)
			{
				glucoseResult = -1;

				//Debug.WriteLine("vals[5] = " + vals[5]);
				//Debug.WriteLine("vals[6] = " + vals[6]);
				//Debug.WriteLine("vals[7] = " + vals[7]);
				//Debug.WriteLine("vals[8] = " + vals[8]);
				// checking length of data
				if ((int)vals[3] == 7)
				{
					// combining byte 6 and byte 7 to read temperature
					// high byte << 8 + low byte
					int sys = ((int)vals[5] << 8) + (int)vals[6];
					//int map = (int)vals[7];
					int dia = (int)vals[8];
					int heartRate = (int)vals[9];

					//Debug.WriteLine(sys);

					this.uiController.SYS_DIA_BPM_updated(sys, dia, heartRate);
				}

				// error
				if ((int)vals[3] == 3)
				{
                    
					var bit7 = (int)(vals[5] >> 7);

					var bit3 = (vals[5] & (1 << 3));
					var bit2 = (vals[5] & (1 << 2));
					var bit1 = (vals[5] & (1 << 1));
					var bit0 = (vals[5] & (1 << 0));

					var bit3_0 = ""+bit3+""+bit2+""+bit1+""+bit0;
					//Debug.WriteLine("vals[5] "+vals[5] );
					var message = "";

					if (bit7==1)
					{
						switch (Convert.ToInt32(bit3_0))
						{
							case 1:
								message = "Pressure did not reach 30mmHg in 7 seconds (cuff placed incorrectly).";
								break;
							case 2:
								message = "Pressure over 295mmHg, device is self-protecting.";
								break;
							case 3:
								message = "Cannot detect pulse.";
								break;
							case 4:
								message = "Too many interference (Movement, Talking...)";
								break;
							case 5:
								message = "Result value incorrect.";
								break;
							case 6:
								message = "Air leakage.";
								break;
							case 15:
								message = "Low battery, measurement stopped.";
								break;
						}

						//Debug.WriteLine("error type 1");
						//Debug.WriteLine(message);
					}
					else {
						
						switch (Convert.ToInt32(bit3_0))
						{
							case 0:
								message = "Cannot detect pulse.";
								break;
							case 1:
								message = "Pressure did not reach 30mmHg in 7 seconds (cuff placed incorrectly).";
								break;
							case 2:
								message = "Result value incorrect.";
								break;
							case 3:
								message = "Pressure over 295mmHg, device is self-protecting.";
								break;
							case 4:
								message = "Too many interference (Movements, Talking...)";
								break;
							case 15:
								message = "Low battery, measurement stopped.";
								break;
						}

						//Debug.WriteLine("error type 0");
						//Debug.WriteLine(message);
					}

					this.uiController.ShowMessageOnUI(message, false, "Blood Pressure Measure Error");
				}
			}

			/// <summary>
			/// Spo2 related parsing
			/// </summary>
			/// 
			/// 
			if ((int)vals[2] == 80 && (int)vals[3] == 3)
			{
				//new reading, rest status, and values
				last_status = 0;
				err_status = 1;
               



                //this.uiController.BeginReadingSpo2();
			}




			//Debug.WriteLine("err_status = " + err_status);
			//Debug.WriteLine("last_status = " + last_status);

			if ((int)vals[2] == 82)
			{
				glucoseResult = -100;
				// when waveform data comes down to 0 then it is end of the spo2 reading
				var waveformData = (int)vals[5];
				//Debug.WriteLine("WaveformData: " + waveformData);
				//printUpdatedCharacteristics(ch);

				this.uiController.updateBpmWaveform((int)vals[6]);

				if (waveformData == 0)
				{
					//Debug.WriteLine("Wave form Data");
					//  this.uiController.noticeEndOfReadingSpo2();
				}
			}

			if ((int)vals[2] == 80 && (int)vals[4] == 2)
			{
				//Debug.WriteLine("err_status = " + err_status);
				//Debug.WriteLine("last_status = " + last_status);
				glucoseResult = -1;
				if (err_status == 1 && last_status != 0)
				{
					//display the last_status message
					string message = "";
					switch (last_status)
					{
						case 1:
							{
								message = "Probe disconnected";
								break;
							}
						case 2:
							{
								message = "Detect probe";
								break;
							}
						case 4:
							{
								message = "Looking for pulse";
								break;
							}
						case 8:
							{
								message = "Search timeout";
								break;
							}
						case 16:
							{
								message = "Detect movement";
								break;
							}
						case 32:
							{
								message = "Low perfusion";
								break;
							}
						default:
							{
								message = "Unknown error";
								break;
							}
					}
					uiController.ShowMessageOnUI(message, true, "SpO2 status");
				}
				this.uiController.noticeEndOfReadingSpo2();
			}
			if ((int)vals[2] == 83 && (int)vals[3] == 7)
			{

				if (vals[5] == 0 || vals[6] == 0)
				{
					//Debug.WriteLine("Invalid readings.");
					//Debug.WriteLine("err_status (should be 1) = " + err_status);
					if (err_status == 1)
					{
						int errshift = vals[9] >> 6;
						int errval = (int)vals[9] - (errshift * (int)Math.Pow(2, 6));
						last_status = errval;
						//Debug.WriteLine("errshift = " + errshift);
						//Debug.WriteLine("last_status = " + last_status);
					}
				}
				else
				{
					int lastSpo2 = (int)vals[5];
					int lastBPM = (int)vals[6];

					err_status = 0;

					//Debug.WriteLine("this.uiController.SPO2_readingCompleted");

					this.uiController.SPO2_readingCompleted(lastSpo2, lastBPM, (float)((int)vals[8]) / 10);
				}
			}
			/*
			Debug.WriteLine("vals[2] = " + vals[2]);
			Debug.WriteLine("vals[3] = " + vals[3]);
			Debug.WriteLine("vals[4] = " + vals[4]);
			Debug.WriteLine("vals[5] = " + vals[5]);
*/
			// this token is for glucose reading
			if ((int)vals[2] == 115) 
			{

                //testings
    			//for (int i = 0; i < vals.Length;i++) 
                //{
                //    Debug.WriteLine("vals["+i.ToString()+"]="+vals[i]);
                //}


				//Debug.WriteLine("glucose");
				// this is needed because device is reading same data more than once 
                // so we are tracking glucose reading stop and sending the last reading
                if (glucoseResult == -1)// || glucoseResult == -100)
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


			
				glucoseResult = ((int)vals[5] >> 3) & 3;
				//Debug.WriteLine("correctResult= " + glucoseResult);
				if (glucoseResult == 0)
				{
					int D0_data1 = (int)vals[5];
					//Debug.WriteLine("((int)vals[6] << 8) + (int)vals[7] = " + ((int)vals[6] << 8) + (int)vals[7]);
					//Debug.WriteLine("(int)vals[6] * 100 + (int)vals[7] = " + (int)vals[6] * 100 + (int)vals[7]);
					if ((D0_data1 & 1) == 1)
					{
						int dataMgDl = ((int)vals[6] << 8) + (int)vals[7];
						glucoseReadingVal = (decimal)dataMgDl;
						gluUnit = "mg/dL";
					}
					else
					{
						int dataMmol = (int)vals[6] * 100 + (int)vals[7];
						glucoseReadingVal = (decimal)dataMmol / 10;
						gluUnit = "mmol/L";
					}

					//Debug.WriteLine("D0_data1 = " + D0_data1);
					//Debug.WriteLine("glucoseReadingVal = " + glucoseReadingVal);
					//Debug.WriteLine("gluUnit = " + gluUnit);

				}
			}
		}
	}
}
