using System;
using Foundation;
using CoreBluetooth;
using System.Diagnostics;
using System.Timers;
using System.Collections.Generic;
using System.Text;

namespace MyHealthVitals.iOS
{
	public class BluetoothPeripheralDelegate : CBPeripheralDelegate
	{
		public CBCharacteristic bmChar;

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

		private void printUpdatedCharacteristics(CBCharacteristic ch) { 
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

		int countMeasuringPressure = 0;
		public override void UpdatedCharacterteristicValue(CBPeripheral peripheral, CBCharacteristic ch, NSError error)
		{

			IBluetoothCallBackUpdatable uiController = (IBluetoothCallBackUpdatable)BluetoothCentralManager.uiController;

			// sys , dia and bpm is available in spot check monitor
			if ((int)ch.Value[2] > 63 && (int)ch.Value[2] < 68)
			{
				Debug.WriteLine("NIBP related token.");

				if (countMeasuringPressure == 0)
				{
					uiController.ShowMessageOnUI("Measuring the Blood pressure...", false);
					countMeasuringPressure++;
				}

				if (ch.Value.Length > 21)
				{
					uiController.SYS_DIA_BPM_updated((int)ch.Value[6], (int)ch.Value[8], (int)ch.Value[9]);
					uiController.ShowMessageOnUI("Boood pressure read succesfully.",false);
					countMeasuringPressure = 0;
				}

				if (ch.Value.Length == 8)
				{
					uiController.updatingPressureMeanTime((int)ch.Value[6]);
				}
			}

			//if ((int)ch.Value[2] > 111 && (int)ch.Value[2] < 115)
			//{
			//	Debug.WriteLine("Temparature related token.");
			//}

			//// spo2 , PI and bpm is available in spot check monitor
			if ((int)ch.Value[2] > 80 && (int)ch.Value[2] < 84)
			{
				if (ch.Value.Length == 19)
				{
					uiController.SPO2_readingCompleted((int)ch.Value[5], (int)ch.Value[6], (int)ch.Value[8]);
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

				uiController.ShowMessageOnUI(message,true);
			}

			//printUpdatedCharacteristics(ch);
		}
	}
}
