using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;
using System.Threading.Tasks;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;

namespace MyHealthVitals
{
	public class SpotCheckServiceHandler : IBLEDeviceServiceHandler
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
				//uiController.ShowMessageOnUI("Connected.", true);
			}
			else
			{
				Debug.WriteLine("reconnectToDevice connectedDevice = " + connectedDevice);
				CrossBluetoothLE.Current.Adapter.ConnectToDeviceAsync(connectedDevice);
				// after this it will call central manager and when device_connected event of the central manager fires then it will call again this class discoverServices()
			}
		}

		public void updateController(IBluetoothCallBackUpdatable controller)
		{
			this.uiController = controller;
		}
		public async Task diconnectServices(IDevice device)
		{
			Contract.Ensures(Contract.Result<Task>() != null);
			this.connectedDevice = device;
			//this.uiController = (IBluetoothCallBackUpdatable)controller;

			var services = await connectedDevice.GetServicesAsync();
			Debug.WriteLine("services ==== " + services.Count);

			foreach (var s in services)
			{
				Debug.WriteLine("diconnect  Services====" + s);
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
		public async void discoverServices(IDevice device)
		{
			this.connectedDevice = device;
			//this.uiController = (IBluetoothCallBackUpdatable)controller;

			var services = await connectedDevice.GetServicesAsync();
			foreach (var s in services)
			{
				Debug.WriteLine("discoverServices====" + s.Device.Name);
				var characteristics = await s.GetCharacteristicsAsync();
				foreach (var c in characteristics)
				{
					Debug.WriteLine(string.Format("Char UUID: {0}  Value: {1}", c.Uuid, c.Value));
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
			else
			{
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

		//ECG commands
		public void startEcgMeasuring()
		{
			executeWriteCommand(new byte[] { 0xAA, 0x55, 0x31, 0x02, 0x02, 0x8F });
		}

		public void stopReadingECG()
		{
			executeWriteCommand(new byte[] { 0xAA, 0x55, 0x30, 0x02, 0x02, 0x24 });
		}

		public void stopMeasuringSpo2()
		{
			executeWriteCommand(new byte[] { 0xAA, 0x55, 0x50, 0x02, 0x02, 0x85 });
		}

		int pretoken = 0;
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

			//  Debug.WriteLine(string.Format("UUID: {0}  ->{1}", ch.Uuid, sb.ToString()));
		}

		int countMeasuringPressure = 0;
		int preBMP = 0;
		decimal glucoseReadingVal = -1;
		int glucoseResult = -1;
		string gluUnit = "";

		//int countOfEcgdataIn30Sec = 0;
		bool isEcgStarted = false;
		bool EcgBegin = false;
		List<int> values = new List<int>();
		int currEcgBytes = 0;
		int last_maskedFoutBit;
		bool last_D7_data;
		DateTime t1;
		//DateTime t2;
		public void C_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
		{
			//Debug.WriteLine("guid:D "+e.Characteristic.Id);
			//printUpdatedCharacteristics(e.Characteristic);

			var ch = e.Characteristic;
			//  Debug.WriteLine("ch.Value[0]"+ch.Value[0]);
			//  Debug.WriteLine("ch.Value[1]"+ch.Value[1]);
			//Debug.WriteLine("-----ch.Value.Length="+ch.Value.Length);
			/*
            for (int i = 0; i < ch.Value.Length; i++)
            {
                Debug.WriteLine(" ch.value["+i+"] = "+ch.Value[i]);
            }
            */

			//for Android, BLE packets only arrive 20 bytes at a time
			if (ch.Value.Length > 0)
			{
				if (isEcgStarted)
				{
					if (currEcgBytes != 0 && currEcgBytes != 59)
					{
						//get the current length
						int currlen = ch.Value.Length;
						//determine whether we're on an even or odd byte count
						bool bitpar = currEcgBytes % 2 == 0; //if 1 it's even, if 0 it's odd
						currEcgBytes += currlen;
						if (bitpar)
						{
							//even bitpar
							if (currEcgBytes == 59)
							{
								//end of the packet
								for (int i = 0; i < currlen - 3; i = i + 2)
								{
									var data = ch.Value[i];
									//Debug.WriteLine("-ch.Value[" + i+ "] = " + (int)ch.Value[i]);
									var D7_data = (data & (1 << 7)) != 0;
									if (D7_data == false)
									{
										// the first byte does not contain data out of two byte
										//Debug.Write(" 8bit: " + (int)ch.Value[i + 1]);
										//Debug.WriteLine("ECG data1 = "+(int)ch.Value[i + 1]);
										values.Add((int)ch.Value[i + 1]);
									}
									else
									{
										// the first byte contains status bit and MSB ( 4 bits ) of the ECG waveform data;
										var maskedFoutBit = (int)ch.Value[i] & 15;
										var data1 = (maskedFoutBit >> 8) + (int)ch.Value[i + 1];
										//Debug.WriteLine("ECG data2 = " + data1);
										values.Add(data1);
										//Debug.Write("12 bit: " + data1);
									}
								}
								uiController.updateECGPacket(values); //send packet values
							}
							else
							{

								if (currlen == 1)
								{
									//only 1 byte in the data array, eval it
									var lastdata = ch.Value[0];
									last_D7_data = (lastdata & (1 << 7)) != 0;
									if (last_D7_data)
									{
										last_maskedFoutBit = (int)lastdata & 15;
									}
								}
								else
								{
									if (currlen % 2 == 0)
									{
										//currlen is even, run as normal
										for (int i = 0; i < currlen; i = i + 2)
										{
											var data = ch.Value[i];

											var D7_data = (data & (1 << 7)) != 0;
											if (D7_data == false)
											{
												// the first byte does not contain data out of two byte
												//Debug.Write(" 8bit: " + (int)ch.Value[i + 1]);
												//Debug.WriteLine("ECG data3 = " + (int)ch.Value[i+1]);
												values.Add((int)ch.Value[i + 1]);
											}
											else
											{
												// the first byte contains status bit and MSB ( 4 bits ) of the ECG waveform data;
												var maskedFoutBit = (int)ch.Value[i] & 15;
												var data1 = (maskedFoutBit >> 8) + (int)ch.Value[i + 1];
												//Debug.WriteLine("ECG data4 = " + data1);
												values.Add(data1);
												//Debug.Write("12 bit: " + data1);
											}
										}
									}
									else
									{
										//currlen is odd, run for <currlen-1 and eval last byte
										for (int i = 0; i < currlen - 1; i = i + 2)
										{
											var data = ch.Value[i];

											var D7_data = (data & (1 << 7)) != 0;
											if (D7_data == false)
											{
												// the first byte does not contain data out of two byte
												//Debug.Write(" 8bit: " + (int)ch.Value[i + 1]);
												//Debug.WriteLine("ECG data5 = " + (int)ch.Value[i + 1]);
												values.Add((int)ch.Value[i + 1]);
											}
											else
											{
												// the first byte contains status bit and MSB ( 4 bits ) of the ECG waveform data;
												var maskedFoutBit = (int)ch.Value[i] & 15;
												var data1 = (maskedFoutBit >> 8) + (int)ch.Value[i + 1];
												//Debug.WriteLine("ECG data6 = " + data1);
												values.Add(data1);
												//Debug.Write("12 bit: " + data1);
											}
										}
										var lastdata = ch.Value[currlen - 1];
										last_D7_data = (lastdata & (1 << 7)) != 0;
										if (last_D7_data)
										{
											last_maskedFoutBit = (int)lastdata & 15;
										}
									}
								}
							}
						}
						else
						{
							//odd bitpar, grab the first byte use the last byte from prev array
							var new_data = ch.Value[0];
							if (last_D7_data == false)
							{
								// the first byte does not contain data out of two byte
								//Debug.Write(" 8bit: " + (int)ch.Value[i + 1]);
								//Debug.WriteLine("ECG data7 = " + (int)ch.Value[0]);
								values.Add((int)ch.Value[0]);
							}
							else
							{
								// the first byte contains status bit and MSB ( 4 bits ) of the ECG waveform data;
								//var maskedFoutBit = (int)ch.Value[i] & 15;
								var data1 = (last_maskedFoutBit >> 8) + (int)ch.Value[0];
								//Debug.WriteLine("ECG data8 = " + data1);
								values.Add(data1);
								//Debug.Write("12 bit: " + data1);
							}
							//get the rest of the bytes in the array
							if (currEcgBytes == 59)
							{
								//end of the packet
								for (int i = 1; i < currlen - 3; i = i + 2)
								{
									var data = ch.Value[i];
									//Debug.WriteLine("-ch.Value[" + i + "] = " + (int)ch.Value[i]);
									var D7_data = (data & (1 << 7)) != 0;
									if (D7_data == false)
									{
										// the first byte does not contain data out of two byte
										//Debug.Write(" 8bit: " + (int)ch.Value[i + 1]);
										//Debug.WriteLine("ECG data9 = " + (int)ch.Value[i + 1]);
										values.Add((int)ch.Value[i + 1]);
									}
									else
									{
										// the first byte contains status bit and MSB ( 4 bits ) of the ECG waveform data;
										var maskedFoutBit = (int)ch.Value[i] & 15;
										var data1 = (maskedFoutBit >> 8) + (int)ch.Value[i + 1];
										//Debug.WriteLine("ECG dataA = " + data1);
										values.Add(data1);
										//Debug.Write("12 bit: " + data1);
									}
								}
								uiController.updateECGPacket(values); //send packet values
							}
							else
							{
								//not end of packet
								if (currlen % 2 == 0)
								{
									//currlen is even, run from 1 to <currlen-1, then eval last byte
									for (int i = 1; i < currlen - 1; i = i + 2)
									{
										var data = ch.Value[i];

										var D7_data = (data & (1 << 7)) != 0;
										if (D7_data == false)
										{
											// the first byte does not contain data out of two byte
											//Debug.Write(" 8bit: " + (int)ch.Value[i + 1]);
											//Debug.WriteLine("ECG dataB = " + (int)ch.Value[i + 1]);
											values.Add((int)ch.Value[i + 1]);
										}
										else
										{
											// the first byte contains status bit and MSB ( 4 bits ) of the ECG waveform data;
											var maskedFoutBit = (int)ch.Value[i] & 15;
											var data1 = (maskedFoutBit >> 8) + (int)ch.Value[i + 1];
											//Debug.WriteLine("ECG dataC = " + data1);
											values.Add(data1);
											//Debug.Write("12 bit: " + data1);
										}
									}
									var lastdata = ch.Value[currlen - 1];
									last_D7_data = (lastdata & (1 << 7)) != 0;
									if (last_D7_data)
									{
										last_maskedFoutBit = (int)lastdata & 15;
									}
								}
								else
								{
									//currlen is odd, shouldn't EVER happen, but just in case it does
									for (int i = 1; i < currlen; i = i + 2)
									{
										var data = ch.Value[i];

										var D7_data = (data & (1 << 7)) != 0;
										if (D7_data == false)
										{
											// the first byte does not contain data out of two byte
											//Debug.Write(" 8bit: " + (int)ch.Value[i + 1]);
											//Debug.WriteLine("ECG dataD = " + (int)ch.Value[i + 1]);
											values.Add((int)ch.Value[i + 1]);
										}
										else
										{
											// the first byte contains status bit and MSB ( 4 bits ) of the ECG waveform data;
											var maskedFoutBit = (int)ch.Value[i] & 15;
											var data1 = (maskedFoutBit >> 8) + (int)ch.Value[i + 1];
											//Debug.WriteLine("ECG dataE = " + data1);
											values.Add(data1);
											//Debug.Write("12 bit: " + data1);
										}
									}
								}
							}

						}
					}
					if (ch.Value.Length > 2)
					{
						if (currEcgBytes == 0 || currEcgBytes == 59)
						{
							//Debug.WriteLine("--ch.Value.Length = " + ch.Value.Length);

							var token = (int)ch.Value[2];

							//Debug.WriteLine("-Token = " + token);

							currEcgBytes = 0;
							values = null;
							values = new List<int>();
							//Debug.WriteLine("ECG token - Ch.Value.Length==" + ch.Value.Length);
							if (ch.Value.Length == 20 && token == 50) //should always be 20
							{
								currEcgBytes = 20;
								//for (int i = 6; i < 25 * 2 + 6; i = i + 2)    
								for (int i = 6; i < 20; i = i + 2)
								{
									var data = ch.Value[i];

									var D7_data = (data & (1 << 7)) != 0;
									if (D7_data == false)
									{
										// the first byte does not contain data out of two byte
										//Debug.Write(" 8bit: " + (int)ch.Value[i + 1]);
										//Debug.WriteLine("ECG dataF = " + (int)ch.Value[i + 1]);
										values.Add((int)ch.Value[i + 1]);
									}
									else
									{
										// the first byte contains status bit and MSB ( 4 bits ) of the ECG waveform data;
										var maskedFoutBit = (int)ch.Value[i] & 15;
										var data1 = (maskedFoutBit >> 8) + (int)ch.Value[i + 1];
										//Debug.WriteLine("ECG dataG = " + data1);
										values.Add(data1);
										//Debug.Write("12 bit: " + data1);
									}
								}

								//countOfEcgdataIn30Sec = countOfEcgdataIn30Sec + values.Count;
								//Debug.WriteLine(countOfEcgdataIn30Sec);

								//wait to do this until an entire packet is finished
								//uiController.updateECGPacket(values);
							}

							if (token == 51)
							{
								//  Debug.WriteLine("total duration of ECG measurment: " + (t1.Subtract(DateTime.Now)).TotalMilliseconds);
								isEcgStarted = false;

								var data1 = (int)ch.Value[5];
								uiController.SaveEcgState(data1);
								uiController.updateECGEnded((int)ch.Value[7], data1);
								//string ecgmessage = "";
								switch (data1)
								{
									case 0:
										{
											Task_vars.ecgmessage = "No irregularity found.";
											uiController.ShowMessageOnUI("No irregularity found.", true, "Normal");
											break;
										}
									case 1:
										{
											Task_vars.ecgmessage = "Suspected a little fast beat.";
											uiController.ShowMessageOnUI("Suspected a little fast beat.", true, "Abnormal");
											break;
										}
									case 2:
										{
											Task_vars.ecgmessage = "Suspected fast beat.";
											uiController.ShowMessageOnUI("Suspected fast beat.", true, "Abnormal");
											break;
										}
									case 3:
										{
											Task_vars.ecgmessage = "Suspected short run of fast beat.";
											uiController.ShowMessageOnUI("Suspected short run of fast beat.", true, "Abnormal");
											break;
										}
									case 4:
										{
											Task_vars.ecgmessage = "Suspected a little slow beat.";
											uiController.ShowMessageOnUI("Suspected a little slow beat.", true, "Abnormal");
											break;
										}
									case 5:
										{
											Task_vars.ecgmessage = "Suspected occasional short beat interval.";
											uiController.ShowMessageOnUI("Suspected occasional short beat interval.", true, "Abnormal");
											break;
										}
									case 6:
										{
											Task_vars.ecgmessage = "Suspected occasional short beat interval.";
											uiController.ShowMessageOnUI("Suspected occasional short beat interval.", true, "Abnormal");
											break;
										}
									case 7:
										{
											Task_vars.ecgmessage = "Suspected irregular beat interval.";
											uiController.ShowMessageOnUI("Suspected irregular beat interval.", true, "Abnormal");
											break;
										}
									case 8:
										{
											Task_vars.ecgmessage = "Suspected fast beat with short beat interval.";
											uiController.ShowMessageOnUI("Suspected fast beat with short beat interval.", true, "Abnormal");
											break;
										}
									case 9:
										{
											Task_vars.ecgmessage = "Suspected slow beat with short beat interval.";
											uiController.ShowMessageOnUI("Suspected slow beat with short beat interval.", true, "Abnormal");
											break;
										}
									case 10:
										{
											Task_vars.ecgmessage = "Suspected slow beat with irregular beat interval.";
											uiController.ShowMessageOnUI("Suspected slow beat with irregular beat interval.", true, "Abnormal");
											break;
										}
									case 11:
										{
											Task_vars.ecgmessage = "Waveform baseline wander.";
											uiController.ShowMessageOnUI("Waveform baseline wander.", true, "Abnormal");
											break;
										}
									case 12:
										{
											Task_vars.ecgmessage = "Suspected fast beat with baseline wander.";
											uiController.ShowMessageOnUI("Suspected fast beat with baseline wander.", true, "Abnormal");
											break;
										}
									case 13:
										{
											Task_vars.ecgmessage = "Suspected slow beat with baseline wander.";
											uiController.ShowMessageOnUI("Suspected slow beat with baseline wander.", true, "Abnormal");
											break;
										}
									case 14:
										{
											Task_vars.ecgmessage = "Suspected occasional short beat interval with baseline wander.";
											uiController.ShowMessageOnUI("Suspected occasional short beat interval with baseline wander.", true, "Abnormal");
											break;
										}
									case 15:
										{
											Task_vars.ecgmessage = "Suspected irregular beat interval with baseline wander.";
											uiController.ShowMessageOnUI("Suspected irregular beat interval with baseline wander.", true, "Abnormal");
											break;
										}
									case 255:
										{
											Task_vars.ecgmessage = "Poor signal, measure again.";
											//needs to be yellow error box in iOS
											uiController.ShowMessageOnUI("Poor signal, measure again.", true, "Poor Signal");
											break;
										}
									default:
										{
											Task_vars.ecgmessage = "";
											uiController.ShowMessageOnUI("No Result Found.", true, "Abnormal");
											break;
										}
								}
								//uiController.ShowMessageOnUI(Task_vars.ecgmessage, true, "Abnormal");
								//Debug.WriteLine("bpm reslt of ecg: " + );  bpm  , ecg

							}
						}
					}
					else
					{
						Debug.WriteLine("--ch.Value.Length = " + ch.Value.Length);
						//loop through ch.Value and print them out
						for (int i = 0; i < ch.Value.Length; i++)
						{
							Debug.WriteLine("-ch.Value[" + i + "] = " + (int)ch.Value[i]);
						}
					}


				}
				else
				{

					//begin ECG
					//ECG started
					if ((int)ch.Value[2] >= 48 && (int)ch.Value[2] <= 51)
					{
						//Debug.WriteLine("this is ECG data:");

						printUpdatedCharacteristics(ch);

						var token = (int)ch.Value[2];

						// this is result of query working state
						if (token == 49)
						{
							//next packet should be token=50 and starting ecg reading
							EcgBegin = true;
							var d7_bit = (ch.Value[5] & (1 << 7)) != 0;

							if (d7_bit)
							{
								Debug.WriteLine("measuring state");
							}
							else
							{
								Debug.WriteLine("standby");
								// command to  start measuring the ecg;
								bmChar.WriteAsync(new byte[] { 0xAA, 0x55, 0x30, 0x02, 0x01, 0x29 });
							}
						}

						if (token == 50)
						{
							// this is to measure time duration taken by ecg to get its result by itself.
							if (isEcgStarted == false)
							{
								//uiController.resetEcgDisplay();

								isEcgStarted = true;
								t1 = DateTime.Now;
								//uiController.ShowMessageOnUI("Reading ECG...", true);
							}

							values = new List<int>();
							//Debug.WriteLine("ECG token - Ch.Value.Length==" + ch.Value.Length);
							if (ch.Value.Length == 20) //should always be 20
							{
								currEcgBytes = 20;
								//for (int i = 6; i < 25 * 2 + 6; i = i + 2)    
								for (int i = 6; i < 20; i = i + 2)
								{
									var data = ch.Value[i];

									var D7_data = (data & (1 << 7)) != 0;
									if (D7_data == false)
									{
										// the first byte does not contain data out of two byte
										//Debug.Write(" 8bit: " + (int)ch.Value[i + 1]);
										//Debug.WriteLine("ECG data00 = " + (int)ch.Value[i + 1]);
										values.Add((int)ch.Value[i + 1]);
									}
									else
									{
										// the first byte contains status bit and MSB ( 4 bits ) of the ECG waveform data;
										var maskedFoutBit = (int)ch.Value[i] & 15;
										var data1 = (maskedFoutBit >> 8) + (int)ch.Value[i + 1];
										//Debug.WriteLine("ECG data00a = " + data1);
										values.Add(data1);
										//Debug.Write("12 bit: " + data1);
									}
								}

								//countOfEcgdataIn30Sec = countOfEcgdataIn30Sec + values.Count;
								//Debug.WriteLine(countOfEcgdataIn30Sec);

								//wait to do this until an entire packet is finished
								uiController.updateECGPacket(values);
							}
						}
						//      Debug.WriteLine("token = " + token);

						if (token == 48 && pretoken == 50)
						{
							Debug.WriteLine("stop response may be token=" + token + "   pretoken " + pretoken);
							Debug.WriteLine("ch.Value[3] :" + ch.Value[3] + "ch.Value[4] " + ch.Value[4] + "ch.Value[5] :" + ch.Value[5]);
							if (ch.Value[4] == 2 && ch.Value[5] == 36)
							{
								uiController.ShowMessageOnUI("You have not finished your ECG measure.", true, "Measure Interrupted");
							}
						}

						// end of the ecg reading


						//printUpdatedCharacteristics(ch);
					}
					// sys , dia and bpm is available in spot check monitor
					if ((int)ch.Value[2] > 63 && (int)ch.Value[2] < 68)
					{
						//Debug.WriteLine("NIBP related token. ch.Value.Length  = " + ch.Value.Length );
						//Debug.WriteLine("ch.Value[2] = " + ch.Value[2]);
						if (ch.Value.Length >= 9)
						{
							//Debug.WriteLine("ch.Value[6] = " + ch.Value[6]);

							//Debug.WriteLine("ch.Value[8] = " + ch.Value[8]);

							uiController.SYS_DIA_BPM_updated((int)ch.Value[6], (int)ch.Value[8], 0);

						}
						else if (ch.Value.Length >= 7)
						{
							//Debug.WriteLine("ch.Value[6]" + ch.Value[6]);
							uiController.SYS_DIA_BPM_updated((int)ch.Value[6], 0, 0);
						}


						if (countMeasuringPressure == 0)
						{
							//uiController.ShowMessageOnUI("Measuring the Blood pressure...", true);
							countMeasuringPressure++;
						}

						if (ch.Value.Length > 21)
						{
							//Debug.WriteLine("ch.Value[4] = " + ch.Value[4]);
							//Debug.WriteLine("ch.Value[6] = " + ch.Value[6]);

							//Debug.WriteLine("ch.Value[8] = " + ch.Value[8]);
							//Debug.WriteLine("ch.Value[9] = " + ch.Value[9]);


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
						//Debug.WriteLine("ch.Value[5]" + ch.Value[5]);
						//Debug.WriteLine("127 & (int)ch.Value[5]" + (127 & (int)ch.Value[5]));
						if ((int)ch.Value[5] >> 7 == 0)
						{
							switch ((int)ch.Value[5])
							{
								case 0:
									message = "No pulse detected.";
									break;
								case 1:
									message = "The cuff pressure failed to reach 30mmHg within 7 seconds (check that cuff is not wrapped well).";
									break;
								case 2:
									message = "Invalid measurement result is obtained.";
									break;
								case 3:
									message = "Overpressure (>295mmHg) protection occurs.";
									break;
								case 4:
									message = "Too much motion (caused by moving, talking etc. during measurement).";
									break;
								default:
									break;
							}
						}
						else if ((int)ch.Value[5] >> 7 == 1)
						{

							switch ((int)ch.Value[5] - 128)
							{
								case 1:
									message = "The cuff pressure failed to reach 30mmHg within 7 seconds (check that cuff is not wrapped well).";
									break;
								case 2:
									message = "Overpressure (>295mmHg) protection occurs.";
									break;
								case 3:
									message = "No pulse detected.";
									break;
								case 4:
									message = "Too much motion (caused by moving, talking etc. during measurement).";
									break;
								case 5:
									message = "Invalid result is obtained.";
									break;
								case 6:
									message = "Air leakage occurred.";
									break;
								case 7:
									//message = "Self - checking failed, probably transducer or A/ D sampling error.";
									break;
								case 8:
									//message = "Pressure error, probably valve can't open normally.";
									break;
								case 9:
									//message = "Signal saturation, caused by movement or other reason yielding too big signal amplitude.";
									break;
								case 10:
									//message = "Air leakage in airway leakage checking.";
									break;
								case 11:
									message = "Hardware or software fault.";
									break;
								case 12:
									//message = "measurement exceeds the specified time limits, 120s for adults with cuff pressure over 200 mmHg, 90s for adults with cuff pressure under 200 mmhg; 90s for neonate.";
									break;
								default:
									return;
									//message = "Unknow error.";
									//break;
							}
						}
						if (message != "")
						{
							uiController.ShowMessageOnUI(message, true, "Blood Pressure Measure Error");
						}

					}


					/// <summary>
					/// Spo2 related parsing
					/// </summary>
					//if ((int)ch.Value[2] == 82)
					//{
					//  // when waveform data comes down to 0 then it is end of the spo2 reading
					//  var waveformData = (int)ch.Value[5];
					//  //Debug.WriteLine("WaveformData: " + waveformData);
					//  //printUpdatedCharacteristics(ch);

					//  this.uiController.updateBpmWaveform((int)ch.Value[6]);

					//  if (waveformData == 0)
					//  {
					//      this.uiController.noticeEndOfReadingSpo2();
					//  }
					//}

					//if ((int)ch.Value[2] == 83 && (int)ch.Value[3] == 7)
					//{
					//  if (ch.Value[5] == 0 || ch.Value[6] == 0)
					//  {
					//      Debug.WriteLine("Invallid readings.");
					//  }
					//  else {
					//      int lastSpo2 = (int)ch.Value[5];
					//      int lastBPM = (int)ch.Value[6];
					//      this.uiController.SPO2_readingCompleted(lastSpo2, lastBPM, (float)((int)ch.Value[8]) / 100);
					//  }
					//}

					//// spo2 , PI and bpm is available in spot check monitor
					if ((int)ch.Value[2] >= 80 && (int)ch.Value[2] < 84)
					{
						//Debug.WriteLine("SpO2...");
						//Debug.WriteLine("ch.Value[2] = " + ch.Value[2]);
						//Debug.WriteLine("ch.Value[3] = " + ch.Value[3]);
						//Debug.WriteLine("ch.Value[4] = " + ch.Value[4]);
						//Debug.WriteLine("ch.Value[5] = " + ch.Value[5]);
						//Debug.WriteLine("ch.Value[6] = " + ch.Value[6]);
						if ((int)ch.Value[2] == 82)
						{
							var waveformBpm = (int)ch.Value[6];
							if (waveformBpm != 0)
								this.uiController.updateBpmWaveform(waveformBpm);
						}

						var token = (int)ch.Value[2];
						var length = (int)ch.Value[3];

						//Debug.WriteLine("getting waveform...");

						if (token == 83 && length == 7)
						{
							//int rshift = ch.Value[6] >> 2;
							//Debug.WriteLine("ch.Value[5] = " + ch.Value[5]);
							//Debug.WriteLine("ch.Value[6] = " + ch.Value[6]);
							//Debug.WriteLine("ch.Value[6]>>2 = " + rshift);
							//Debug.WriteLine("ch.Value[7] = " + ch.Value[7]);
							//Debug.WriteLine("ch.Value[8] = " + ch.Value[8]);
							//Debug.WriteLine("ch.Value[9] = " + ch.Value[9]);
							//Debug.WriteLine("ch.Value[10] = " + ch.Value[10]);


							//ch.Value[9] holds the status info
							//D0 - probe disconnected (reserved)
							//D1 - Probe off (probe disconnected or finger is out of probe
							//D2 - Pulse Searching
							//D3 - Searching too long (reserved)
							//D4 - Motion detected (reserved)
							//D5 - Low perfusion (reserved)
							//D7-D6 - 00 Adult Mode, 01 Neonate Mode, 10 Veterinary Mode (reserved)

							var status_bit1 = ((int)ch.Value[9] & (1 << 1)) != 0;
							if (status_bit1)
							{
								Debug.WriteLine("end of the spo2 reading preBMP=" + preBMP);
								//display the error message on the screen
								int errshift = ch.Value[9] >> 6;
								int errval = (int)ch.Value[9] - (errshift * (int)Math.Pow(2, 6));
								if ((ch.Value[5] == 0 || ch.Value[6] == 0) && errval != 2)
								{

									//Debug.WriteLine("errshift = " + errshift);
									//Debug.WriteLine("errval = " + errval);
									string message = "";
									switch (errval)
									{
										case 1:
											{
												message = "Probe disconnected";
												break;
											}
										case 2:
											{
												message = "Probe off (probe disconnected or finger is out of probe)";
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
												message = "Motion detected";
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
									if (message != "")
									{
										uiController.ShowMessageOnUI(message, true, "SpO2 status");

									}

								}



								if (preBMP != 0)
								{
									uiController.noticeEndOfReadingSpo2();
									preBMP = 0;
									return;
								}
							}
							preBMP = (int)ch.Value[6];
							if (ch.Value[5] == 0 || ch.Value[6] == 0)
							{

								Debug.WriteLine("Invalid readings.");
							}
							else
							{
								int lastSpo2 = (int)ch.Value[5];
								int lastBPM = (int)ch.Value[6];

								uiController.SPO2_readingCompleted(lastSpo2, lastBPM, (float)((int)ch.Value[8]) / 10);
							}
						}
						/*
						if ((int)ch.Value[2] == 0x50 && (int)ch.Value[4] == 2)
						{
							int lastSpo2 = (int)ch.Value[5];
							int lastBPM = (int)ch.Value[6];
							uiController.SPO2_readingCompleted(lastSpo2, lastBPM, (float)((int)ch.Value[8]) / 100);

						}*/
					}

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



					if ((int)ch.Value[2] > 111 && (int)ch.Value[2] < 115)
					{
						Debug.WriteLine("Temperature related token.");

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
							if (data <= 3) uiController.ShowMessageOnUI("Failed to read temperature.", true, "Temperature measuring ");

							if (data >= 1312) uiController.ShowMessageOnUI("Too high temperature.", true, "Temperature measuring ");

							// if this condition saisfies the reading is measured in celcious
							if (data >= 200 && data <= 1301)
							{
								//uiController.ShowMessageOnUI("Temperature read successfully.", true);
								//decimal temperatureInCelcious = (decimal)((float)633 / 100 + 30);
								//(9.0 / 5.0) * c) +32
								// the device always reads in Celcious even if it is displaying in fareinheit
								var tempC = (double)data / 100 + 30.0;
								double tempF = Math.Round(((9.0 / 5.0) * tempC) + 32, 1);

								uiController.updateTemperature((decimal)tempF);
							}
						}
					}
					pretoken = (int)ch.Value[2];
				}
			}

		}

	}
}
