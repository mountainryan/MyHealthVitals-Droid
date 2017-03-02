using System;
using Foundation;
using CoreBluetooth;
using System.Diagnostics;
using System.Timers;
using MyHealthVitals;

namespace MyHealthVitals.iOS
{
	public class BLEPeripheralDelSpirometer : CBPeripheralDelegate
	{
		public Timer pollingTimer;
		private byte packets;
		private byte packetCallNumber;
		public CBCharacteristic bmChar;

		public BLEPeripheralDelSpirometer(Object vc)
		{
			pollingTimer = new Timer();
			pollingTimer.Interval = 500;
			pollingTimer.Enabled = false;

			pollingTimer.Elapsed += PollingTimer_Elapsed;
		}

		public void PollingTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (this.bmChar == null)
			{
				pollingTimer.Enabled = false;

			}
			else {
				Debug.WriteLine("polling...");
				BLECentralManagerSpirometer.connectedPeripheral.WriteValue(NSData.FromArray(new byte[] { 0x55, 0x06 }), this.bmChar, CBCharacteristicWriteType.WithResponse);
			}
		}

		private void getPacketData(byte packetCallNumber)
		{
			byte[] bytes = new byte[4];

			bytes[0] = Convert.ToByte("55", 16);
			bytes[1] = Convert.ToByte("02", 16);
			bytes[2] = packetCallNumber;
			bytes[3] = Convert.ToByte("00", 16);

			if (this.bmChar == null)
			{
				pollingTimer.Enabled = false;
				//this.caller.updateCaller(null);
			}
			else {
				BLECentralManagerSpirometer.connectedPeripheral.WriteValue(NSData.FromArray(bytes), this.bmChar, CBCharacteristicWriteType.WithResponse);
			}
		}

		private void clearReadingOnSpirometerDevice(decimal pef,decimal fev1)
		{
			byte[] bytes = new byte[2];

			pollingTimer.Enabled = false;

			bytes[0] = Convert.ToByte("55", 16);
			bytes[1] = Convert.ToByte("03", 16);

			if (this.bmChar != null)
				BLECentralManagerSpirometer.connectedPeripheral.WriteValue(NSData.FromArray(bytes), this.bmChar, CBCharacteristicWriteType.WithResponse);
			
			((BLEReadingUpdatableSpiroMeter)BLECentralManagerSpirometer.caller).updateCaller(pef,fev1);
		}

		public override void DiscoveredService(CBPeripheral peripheral, NSError error)
		{
			BLECentralManagerSpirometer.connectedPeripheral = peripheral;

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
				if (c.UUID == CBUUID.FromString("FF0B"))
				{
					peripheral.SetNotifyValue(true, c); //subscribe the characteristic   
					this.bmChar = c;
					pollingTimer.Enabled = true;
				}
				else
				{
					//this.bmChar = c;
					peripheral.SetNotifyValue(true, c); //subscribe the characteristic   
					//this.bmChar = c;
					//pollingTimer.Enabled = true;  
				}
			}
		}

		public bool OnlyHexInString(string test)
		{
			// For C-style hex notation (0xFF) you can use @"\A\b(0[xX])?[0-9a-fA-F]+\b\Z"
			return System.Text.RegularExpressions.Regex.IsMatch(test, @"\A\b[0-9a-fA-F]+\b\Z");
		}

		public override void UpdatedCharacterteristicValue(CBPeripheral peripheral, CBCharacteristic characteristic, NSError error)
		{

			if (error != null) {
				Console.WriteLine("error: " + error.Description);
			}

			//Console.WriteLine("peripheral name: " + peripheral.Name);

			string valueString = characteristic.Value.ToString();

			Console.WriteLine(valueString);

			//SpirometerReading currReading;

			if (string.IsNullOrEmpty(valueString.Trim()))
				return;
			if (valueString.Length < 8)
				return;

			string firstFourChar = valueString.Substring(1, 4);
			string firstTwoChar = valueString.Substring(1, 2);
			//Debug.WriteLine("firstFourChar= " + firstFourChar);
			//Debug.WriteLine("firstTwoChar= " + firstTwoChar);


			// sometime it reads some unusual character to prevent it
			//String withoutangles

			if (this.OnlyHexInString(valueString.Replace("<", "").Replace(">", "").Replace(" ", "")))
			{

			}
			else {
				pollingTimer.Enabled = true;
				return;
			}

			if (firstFourChar == "aa06")
			{
				pollingTimer.Enabled = false;

				if (this.bmChar != null)
					BLECentralManagerSpirometer.connectedPeripheral.WriteValue(NSData.FromArray(new byte[] { 0x55, 0x01 }), this.bmChar, CBCharacteristicWriteType.WithResponse);
				else 
					((BLEReadingUpdatableSpiroMeter)BLECentralManagerSpirometer.caller).updateCaller(0,0);

			}
			else if (firstFourChar == "aa01") //this is packets number 
			{
				packets = Convert.ToByte(valueString.Substring(7, 2), 16);

				if (packets > 0)
				{
					packetCallNumber++;
					this.getPacketData(packetCallNumber);
				}
			}
			else if (firstTwoChar == "dd") //this is packets data
			{
				//deal with the data

				//byte[] timeBytes1 = new byte[4];
				//timeBytes1[0] = Convert.ToByte(valueString.Substring(3, 2), 16);
				//timeBytes1[1] = Convert.ToByte(valueString.Substring(5, 2), 16);
				//timeBytes1[2] = Convert.ToByte(valueString.Substring(7, 2), 16);
				//timeBytes1[3] = Convert.ToByte(valueString.Substring(10, 2), 16);
				//var seconds1 = BitConverter.ToInt32(timeBytes1, 0);
				//DateTime temp = new DateTime(1970, 1, 1);
				//DateTime dateTime1 = temp.AddSeconds(seconds1);
				//Console.WriteLine (dateTime1.ToString());

				byte[] fev1Bytes1 = new byte[2];
				fev1Bytes1[0] = Convert.ToByte(valueString.Substring(12, 2), 16);
				fev1Bytes1[1] = Convert.ToByte(valueString.Substring(14, 2), 16);
				double fev11 = (double)BitConverter.ToInt16(fev1Bytes1, 0)/100;

				//var fevDec = (double)fev11 / 100;

				//Console.WriteLine(fev11);

				byte[] pefBytes1 = new byte[2];
				pefBytes1[0] = Convert.ToByte(valueString.Substring(16, 2), 16);
				pefBytes1[1] = Convert.ToByte(valueString.Substring(19, 2), 16);
				var pef1 = Convert.ToDouble(BitConverter.ToInt16(pefBytes1, 0));
				//Console.WriteLine(pef1);


				//currReading = new SpirometerReading(dateTime1, (decimal)pef1, (decimal)fev11);

				//currReading.Date = dateTime1;
				//currReading.Pef = pef1;
				//currReading.Fev1 = fev11;

				if (valueString.Length > 22) // means there are two readings in this packet
				{
					//byte[] timeBytes2 = new byte[4];
					//timeBytes2[0] = Convert.ToByte(valueString.Substring(21, 2), 16);
					//timeBytes2[1] = Convert.ToByte(valueString.Substring(23, 2), 16);
					//timeBytes2[2] = Convert.ToByte(valueString.Substring(25, 2), 16);
					//timeBytes2[3] = Convert.ToByte(valueString.Substring(28, 2), 16);
					//var seconds2 = BitConverter.ToInt32(timeBytes2, 0);

					//DateTime temp2 = new DateTime(1970, 1, 1);
					//DateTime dateTime2 = temp2.AddSeconds(seconds2);
					//Console.WriteLine (dateTime2.ToString());


					byte[] fev1Bytes2 = new byte[2];
					fev1Bytes2[0] = Convert.ToByte(valueString.Substring(30, 2), 16);
					fev1Bytes2[1] = Convert.ToByte(valueString.Substring(32, 2), 16);
					//fev11 = Convert.ToDouble() / 100;

					fev11 = (double)BitConverter.ToInt16(fev1Bytes2, 0) / 100;

					//Console.WriteLine(fev12);

					byte[] pefBytes2 = new byte[2];
					pefBytes2[0] = Convert.ToByte(valueString.Substring(34, 2), 16);
					pefBytes2[1] = Convert.ToByte(valueString.Substring(37, 2), 16);
					pef1 = Convert.ToDouble(BitConverter.ToInt16(pefBytes2, 0));
					//Console.WriteLine(pef2);

					//currReading.Date = dateTime2;
					//currReading.Pef = pef2;
					//currReading.Fev1 = fev12;

					//currReading = new SpirometerReading(dateTime2, (decimal)pef2, (decimal)fev12);
				}

				this.clearReadingOnSpirometerDevice((decimal)pef1,(decimal)fev11);
			}

			if (packetCallNumber < packets)
			{
				packetCallNumber++;
				this.getPacketData(packetCallNumber);
			}
			else if (packets > 0)
			{
				//this.clearReadingOnSpirometerDevice(currReading);
			}
		}
	}
}
