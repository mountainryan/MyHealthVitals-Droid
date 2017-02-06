using System;
using Foundation;
using CoreBluetooth;
using System.Diagnostics;
using System.Timers;
using System.Collections.Generic;
using System.Text;

namespace SLxaml
{
	interface BlueToothReadingUpdatable
	{
		void updateCaller(String message);
	}

	public class BluetoothPeripheralDelegate : CBPeripheralDelegate
	{
		public CBCharacteristic bmChar;

		public void StartBloodPressure()
		{
			byte[] bytes = new byte[] { 0xaa, 0x55, 0x40, 0x02, 0x01, 0x29 };
			BluetoothCentralManager.connectedPeripheral.WriteValue(NSData.FromArray(bytes), this.bmChar, CBCharacteristicWriteType.WithResponse);
		}

		public override void DiscoveredService(CBPeripheral peripheral, NSError error)
		{
			BluetoothCentralManager.connectedPeripheral = peripheral;

			foreach (var service in peripheral.Services)
			{
				peripheral.DiscoverCharacteristics(service);
			}
		}

		public override void DiscoveredCharacteristic(CBPeripheral peripheral, CBService service, NSError error)
		{
			Debug.WriteLine("discovered DiscoveredCharacteristic");
			foreach (var c in service.Characteristics)
			{
				peripheral.SetNotifyValue(true, c); //subscribe the characteristic   
				this.bmChar = c;
			}
		}

		public override void UpdatedCharacterteristicValue(CBPeripheral peripheral, CBCharacteristic ch, NSError error)
		{

			BlueToothReadingUpdatable uiController = (BlueToothReadingUpdatable)BluetoothCentralManager.uiController;

			if ((int)ch.Value[2] > 63 && (int)ch.Value[2] < 68)
			{
				Debug.WriteLine("NIBP related token.");

				uiController.updateCaller("Measuring the Blood pressure...");

				//vitalsData.Bpm = (int)ch.Value[9];
				//vitalsData.BPSys = (int)ch.Value[6];
				//vitalsData.BPDia = (int)ch.Value[8];

				if (ch.Value.Length > 21)
				{
					//uiController.SYS_DIA_BPM_updated((int)ch.Value[6], (int)ch.Value[8], (int)ch.Value[9]);
					//uiController.ShowMessageOnUI("Boood pressure read succesfully.");
					//uiController.updateCaller( (int)ch.Value[6]

					Debug.WriteLine("BPsys: " + (int)ch.Value[6] + " " + " bpdia: " + (int)ch.Value[8] + " " + (int)ch.Value[9]);
				}

				if (ch.Value.Length == 8)
				{
					//uiController.updatingPressureMeanTime((int)ch.Value[6]);
					Debug.WriteLine("real time pressure: " + ((int)ch.Value[6]));
				}
			}

			//if ((int)ch.Value[2] > 111 && (int)ch.Value[2] < 115)
			//{
			//	Debug.WriteLine("Temparature related token.");
			//}

			if ((int)ch.Value[2] > 80 && (int)ch.Value[2] < 84)
			{
				Debug.WriteLine(" Spo2 related token");

				if (ch.Value.Length == 19)
				{
					//vitalsData.Bpm = (int)ch.Value[10];
					//vitalsData.SpO2 = (int)ch.Value[5];
					//vitalsData.Bpm = (int)ch.Value[6];

					//uiController.SPO2_readingCompleted((int)ch.Value[5], (int)ch.Value[6]);

					Debug.WriteLine("Spo2: " + (int)ch.Value[5] + " " + "bpm: " + (int)ch.Value[6]);

				}
			}

			//if (ch.Value.Length > 10 && ch.Value.Length < 22)
			//{
			//	//Debug.WriteLine("spo2: " + ch.Value[5] + " BPM: " + ch.Value[6]);
			//}

			//if (ch.Value.Length == 19)
			//{
			//	//Debug.WriteLine("spo2: " + ch.Value[5] + " BPM: " + ch.Value[6]);
			//}

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

				Debug.WriteLine(message);
			}

			//// sys , dia and bpm is available in spot check monitor
			//if (ch.Value.Length > 21)
			//{
			//	//vitalsData.Bpm = (int)ch.Value[9];
			//	//vitalsData.BPSys = (int)ch.Value[6];
			//	//vitalsData.BPDia = (int)ch.Value[8];
			//	Debug.WriteLine("inside 22 byte: " + ch.Value[6]);
			//	Debug.WriteLine("check content of error byete when there is no error: " + (27 & (int)ch.Value[5]));
			//	uiController.SYS_DIA_BPM_updated();
			//}

			//if (ch.Value.Length == 9)
			//{
			//	//vitalsData.Temp = 98;
			//}

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

			Debug.WriteLine(string.Format("UUID: {0}  ->{1}", ch.UUID, sb.ToString()));

		}
	}
}
