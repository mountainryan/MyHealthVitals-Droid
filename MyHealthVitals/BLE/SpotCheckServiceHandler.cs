using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;
using System.Threading.Tasks;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System.Linq;
using nexus.core.logging;
using nexus.protocols.ble;

namespace MyHealthVitals
{
    public class SpotCheckServiceHandler : IBLEDeviceServiceHandler
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
                //Debug.WriteLine("reconnectToDevice connectedDevice = " + connectedDevice);
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
            //Debug.WriteLine("services ==== " + services.Count);

            foreach (var s in services)
            {
                //Debug.WriteLine("diconnect  Services====" + s);
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
        public async Task discoverServices(IDevice device)
        {
            this.connectedDevice = device;
            //this.uiController = (IBluetoothCallBackUpdatable)controller;
            //Debug.WriteLine("discoverServices====" + device.Name);
            var services = await connectedDevice.GetServicesAsync();
            foreach (var s in services)
            {
                //Debug.WriteLine("service guid = " + s.Id.ToString());
                var characteristics = await s.GetCharacteristicsAsync();
                foreach (var c in characteristics)
                {
                    //Debug.WriteLine(string.Format("Char UUID: {0}  Value: {1}", c.Uuid, c.Value));
                    //Debug.WriteLine("uuid: " + c.Uuid);

                    if (c.CanUpdate)
                    {
                        c.ValueUpdated += C_ValueUpdated;
                        await c.StartUpdatesAsync();
                        //problem here on Android
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
                //Debug.WriteLine("guid of char = " + bmChar.Id.ToString());
               // Debug.WriteLine("guid (UUID) of char = " + bmChar.Uuid);
                bmChar.WriteAsync(byteCommand);
            }
            else
            {
                uiController.ShowMessageOnUI("Device is not connected. Please connect and try again.", false);
            }
        }

        //NIBP commands
        public async void startMeasuringBP()
        {
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
                    //Debug.WriteLine("starting BP error msg: "+ex.Message);
				}
            }

        }

        public async void stoptMeasuringBP()
        {
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

        //ECG commands
        public async void startEcgMeasuring()
        {
            if (BLE_val.BLE_value == 1)
            {
                executeWriteCommand(new byte[] { 0xAA, 0x55, 0x31, 0x02, 0x02, 0x8F });
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
					   new byte[] { 0xAA, 0x55, 0x31, 0x02, 0x02, 0x8F });
				}
				catch (GattException ex)
				{
					//Debug.WriteLine("starting ECG error msg: " + ex.Message);
				}
            }
        }

        public async void stopReadingECG()
        {
            if (BLE_val.BLE_value == 1)
            {
                executeWriteCommand(new byte[] { 0xAA, 0x55, 0x30, 0x02, 0x02, 0x24 });
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
					   new byte[] { 0xAA, 0x55, 0x30, 0x02, 0x02, 0x24 });
				}
				catch (GattException ex)
				{
					//Debug.WriteLine("stopping ECG error msg: " + ex.Message);
				}
            }
        }

        public async void stopMeasuringSpo2()
        {
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

		public async void getBatteryInfo()
		{
            //Debug.WriteLine("Called getBatteryInfo()");
			if (BLE_val.BLE_value == 1)
			{
				executeWriteCommand(new byte[] { 0xAA, 0x55, 0xFF, 0x02, 0x02, 0x28 });
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
					   new byte[] { 0xAA, 0x55, 0xFF, 0x02, 0x02, 0x28 });
				}
				catch (GattException ex)
				{
					//Debug.WriteLine("getting battery info error msg: " + ex.Message);
				}
			}
		}



        int pretoken = 0;
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
			//Debug.WriteLine("Using BLE 1");
            //Debug.WriteLine("guid:D "+e.Characteristic.Id);
            //printUpdatedCharacteristics(e.Characteristic);

            var ch = e.Characteristic;
            //  Debug.WriteLine("vals[0]"+vals[0]);
            //  Debug.WriteLine("vals[1]"+vals[1]);
            //Debug.WriteLine("-----vals.Length="+vals.Length);
            /*
            for (int i = 0; i < vals.Length; i++)
            {
                Debug.WriteLine(" ch.value["+i+"] = "+vals[i]);
            }
            */

            //for Android, BLE packets only arrive 20 bytes at a time


            /*
            if (token == 48 && pretoken == 50)
            {
                Debug.WriteLine("stop response may be token=" + token + "   pretoken " + pretoken);
                Debug.WriteLine("vals[3] :" + vals[3] + "vals[4] " + vals[4] + "vals[5] :" + vals[5]);
                if (vals[4] == 2 && vals[5] == 36)
                {
                    uiController.ShowMessageOnUI("You have not finished your ECG measure.", true, "Measure Interrupted");
                }
            }
            */
            int[] ints = ch.Value.Select(x => (int)x).ToArray();
            ManipData(ints);
        }
        public void ManipData(int[] vals)
        {
            /*
			for (int i = 0; i < vals.Length; i++)
			{
				Debug.WriteLine("vals[" + i.ToString() + "]=" + vals[i]);
			}*/

			//battery life check
			if (vals.Length > 2)
			{
				if (vals[2] == 255 && vals[3] == 5)
				{
					//just check last 3 bits
					int batteryLife = vals[7] & 7;
					//Debug.WriteLine("Battery Life = " + batteryLife);
					if (batteryLife <= 1)
					{
						uiController.ShowMessageOnUI("You must plug in your PC-300 device.", true, "Low Battery");
					}
				}
			}

            if (vals.Length > 0)
            {
                if (isEcgStarted)
                {
                    if (vals.Length > 5)
                    {
                        if (vals[2]==48 && vals[4]==2 && vals[5]==36)
                        {
                            uiController.ShowMessageOnUI("You have not finished your ECG measure.", true, "Measure Interrupted");
                            //start over
                            isEcgStarted = false;
                        }
                    }

                    if (currEcgBytes != 0 && currEcgBytes != 59)
                    {
                        //get the current length
                        int currlen = vals.Length;
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
                                    var data = vals[i];
                                    //Debug.WriteLine("-vals[" + i+ "] = " + (int)vals[i]);
                                    var D7_data = (data & (1 << 7)) != 0;
                                    if (D7_data == false)
                                    {
                                        // the first byte does not contain data out of two byte
                                        //Debug.Write(" 8bit: " + (int)vals[i + 1]);
                                        //Debug.WriteLine("ECG data1 = "+(int)vals[i + 1]);
                                        values.Add((int)vals[i + 1]);
                                    }
                                    else
                                    {
                                        // the first byte contains status bit and MSB ( 4 bits ) of the ECG waveform data;
                                        var maskedFoutBit = (int)vals[i] & 15;
                                        var data1 = (maskedFoutBit >> 8) + (int)vals[i + 1];
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
                                    var lastdata = vals[0];
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
                                            var data = vals[i];

                                            var D7_data = (data & (1 << 7)) != 0;
                                            if (D7_data == false)
                                            {
                                                // the first byte does not contain data out of two byte
                                                //Debug.Write(" 8bit: " + (int)vals[i + 1]);
                                                //Debug.WriteLine("ECG data3 = " + (int)vals[i+1]);
                                                values.Add((int)vals[i + 1]);
                                            }
                                            else
                                            {
                                                // the first byte contains status bit and MSB ( 4 bits ) of the ECG waveform data;
                                                var maskedFoutBit = (int)vals[i] & 15;
                                                var data1 = (maskedFoutBit >> 8) + (int)vals[i + 1];
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
                                            var data = vals[i];

                                            var D7_data = (data & (1 << 7)) != 0;
                                            if (D7_data == false)
                                            {
                                                // the first byte does not contain data out of two byte
                                                //Debug.Write(" 8bit: " + (int)vals[i + 1]);
                                                //Debug.WriteLine("ECG data5 = " + (int)vals[i + 1]);
                                                values.Add((int)vals[i + 1]);
                                            }
                                            else
                                            {
                                                // the first byte contains status bit and MSB ( 4 bits ) of the ECG waveform data;
                                                var maskedFoutBit = (int)vals[i] & 15;
                                                var data1 = (maskedFoutBit >> 8) + (int)vals[i + 1];
                                                //Debug.WriteLine("ECG data6 = " + data1);
                                                values.Add(data1);
                                                //Debug.Write("12 bit: " + data1);
                                            }
                                        }
                                        var lastdata = vals[currlen - 1];
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
                            var new_data = vals[0];
                            if (last_D7_data == false)
                            {
                                // the first byte does not contain data out of two byte
                                //Debug.Write(" 8bit: " + (int)vals[i + 1]);
                                //Debug.WriteLine("ECG data7 = " + (int)vals[0]);
                                values.Add((int)vals[0]);
                            }
                            else
                            {
                                // the first byte contains status bit and MSB ( 4 bits ) of the ECG waveform data;
                                //var maskedFoutBit = (int)vals[i] & 15;
                                var data1 = (last_maskedFoutBit >> 8) + (int)vals[0];
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
                                    var data = vals[i];
                                    //Debug.WriteLine("-vals[" + i + "] = " + (int)vals[i]);
                                    var D7_data = (data & (1 << 7)) != 0;
                                    if (D7_data == false)
                                    {
                                        // the first byte does not contain data out of two byte
                                        //Debug.Write(" 8bit: " + (int)vals[i + 1]);
                                        //Debug.WriteLine("ECG data9 = " + (int)vals[i + 1]);
                                        values.Add((int)vals[i + 1]);
                                    }
                                    else
                                    {
                                        // the first byte contains status bit and MSB ( 4 bits ) of the ECG waveform data;
                                        var maskedFoutBit = (int)vals[i] & 15;
                                        var data1 = (maskedFoutBit >> 8) + (int)vals[i + 1];
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
                                        var data = vals[i];

                                        var D7_data = (data & (1 << 7)) != 0;
                                        if (D7_data == false)
                                        {
                                            // the first byte does not contain data out of two byte
                                            //Debug.Write(" 8bit: " + (int)vals[i + 1]);
                                            //Debug.WriteLine("ECG dataB = " + (int)vals[i + 1]);
                                            values.Add((int)vals[i + 1]);
                                        }
                                        else
                                        {
                                            // the first byte contains status bit and MSB ( 4 bits ) of the ECG waveform data;
                                            var maskedFoutBit = (int)vals[i] & 15;
                                            var data1 = (maskedFoutBit >> 8) + (int)vals[i + 1];
                                            //Debug.WriteLine("ECG dataC = " + data1);
                                            values.Add(data1);
                                            //Debug.Write("12 bit: " + data1);
                                        }
                                    }
                                    var lastdata = vals[currlen - 1];
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
                                        var data = vals[i];

                                        var D7_data = (data & (1 << 7)) != 0;
                                        if (D7_data == false)
                                        {
                                            // the first byte does not contain data out of two byte
                                            //Debug.Write(" 8bit: " + (int)vals[i + 1]);
                                            //Debug.WriteLine("ECG dataD = " + (int)vals[i + 1]);
                                            values.Add((int)vals[i + 1]);
                                        }
                                        else
                                        {
                                            // the first byte contains status bit and MSB ( 4 bits ) of the ECG waveform data;
                                            var maskedFoutBit = (int)vals[i] & 15;
                                            var data1 = (maskedFoutBit >> 8) + (int)vals[i + 1];
                                            //Debug.WriteLine("ECG dataE = " + data1);
                                            values.Add(data1);
                                            //Debug.Write("12 bit: " + data1);
                                        }
                                    }
                                }
                            }

                        }
                    }
                    if (vals.Length > 2)
                    {
                        if (currEcgBytes == 0 || currEcgBytes == 59)
                        {
                            //Debug.WriteLine("--vals.Length = " + vals.Length);

                            var token = (int)vals[2];

                            //Debug.WriteLine("-Token = " + token);

                            currEcgBytes = 0;
                            values = null;
                            values = new List<int>();
                            //Debug.WriteLine("ECG token - Ch.Value.Length==" + vals.Length);
                            if (vals.Length == 20 && token == 50) //should always be 20
                            {
                                currEcgBytes = 20;
                                //for (int i = 6; i < 25 * 2 + 6; i = i + 2)    
                                for (int i = 6; i < 20; i = i + 2)
                                {
                                    var data = vals[i];

                                    var D7_data = (data & (1 << 7)) != 0;
                                    if (D7_data == false)
                                    {
                                        // the first byte does not contain data out of two byte
                                        //Debug.Write(" 8bit: " + (int)vals[i + 1]);
                                        //Debug.WriteLine("ECG dataF = " + (int)vals[i + 1]);
                                        values.Add((int)vals[i + 1]);
                                    }
                                    else
                                    {
                                        // the first byte contains status bit and MSB ( 4 bits ) of the ECG waveform data;
                                        var maskedFoutBit = (int)vals[i] & 15;
                                        var data1 = (maskedFoutBit >> 8) + (int)vals[i + 1];
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

                                var data1 = (int)vals[5];
                                uiController.SaveEcgState(data1);
                                uiController.updateECGEnded((int)vals[7], data1);
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
                        //Debug.WriteLine("--vals.Length = " + vals.Length);
                        //loop through vals and print them out
                        for (int i = 0; i < vals.Length; i++)
                        {
                            //Debug.WriteLine("-vals[" + i + "] = " + (int)vals[i]);
                        }
                    }


                }
                else
                {

                    if (vals.Length > 2)
                    {
                        if ((int)vals[2] >= 48 && (int)vals[2] <= 51)
                        {
                            //Debug.WriteLine("this is ECG data:");

                            printUpdatedCharacteristics(vals);

                            var token = (int)vals[2];

                            // this is result of query working state
                            if (token == 49)
                            {
                                //next packet should be token=50 and starting ecg reading
                                EcgBegin = true;
                                var d7_bit = (vals[5] & (1 << 7)) != 0;

                                if (d7_bit)
                                {
                                   // Debug.WriteLine("measuring state");
                                }
                                else
                                {
                                    //Debug.WriteLine("standby");
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
                                //Debug.WriteLine("ECG token - Ch.Value.Length==" + vals.Length);
                                if (vals.Length == 20) //should always be 20
                                {
                                    currEcgBytes = 20;
                                    //for (int i = 6; i < 25 * 2 + 6; i = i + 2)    
                                    for (int i = 6; i < 20; i = i + 2)
                                    {
                                        var data = vals[i];

                                        var D7_data = (data & (1 << 7)) != 0;
                                        if (D7_data == false)
                                        {
                                            // the first byte does not contain data out of two byte
                                            //Debug.Write(" 8bit: " + (int)vals[i + 1]);
                                            //Debug.WriteLine("ECG data00 = " + (int)vals[i + 1]);
                                            values.Add((int)vals[i + 1]);
                                        }
                                        else
                                        {
                                            // the first byte contains status bit and MSB ( 4 bits ) of the ECG waveform data;
                                            var maskedFoutBit = (int)vals[i] & 15;
                                            var data1 = (maskedFoutBit >> 8) + (int)vals[i + 1];
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
                                //Debug.WriteLine("stop response may be token=" + token + "   pretoken " + pretoken);
                                //Debug.WriteLine("vals[3] :" + vals[3] + "vals[4] " + vals[4] + "vals[5] :" + vals[5]);
                                if (vals[4] == 2 && vals[5] == 36)
                                {
                                    uiController.ShowMessageOnUI("You have not finished your ECG measure.", true, "Measure Interrupted");
                                }
                            }

                            // end of the ecg reading


                            //printUpdatedCharacteristics(ch);
                        }
                        // sys , dia and bpm is available in spot check monitor
                        if ((int)vals[2] > 63 && (int)vals[2] < 68)
                        {
                            //Debug.WriteLine("NIBP related token. vals.Length  = " + vals.Length );
                            //Debug.WriteLine("vals[2] = " + vals[2]);
                            if (vals.Length >= 9)
                            {
                                //Debug.WriteLine("vals[6] = " + vals[6]);

                                //Debug.WriteLine("vals[8] = " + vals[8]);

                                uiController.SYS_DIA_BPM_updated((int)vals[6], (int)vals[8], 0);

                            }
                            else if (vals.Length >= 7)
                            {
                                //Debug.WriteLine("vals[6]" + vals[6]);
                                uiController.SYS_DIA_BPM_updated((int)vals[6], 0, 0);
                            }


                            if (countMeasuringPressure == 0)
                            {
                                //uiController.ShowMessageOnUI("Measuring the Blood pressure...", true);
                                countMeasuringPressure++;
                            }

                            if (vals.Length == 20)
                            {
                                //Debug.WriteLine("vals[4] = " + vals[4]);
                                //Debug.WriteLine("vals[6] = " + vals[6]);

                                //Debug.WriteLine("vals[8] = " + vals[8]);
                                //Debug.WriteLine("vals[9] = " + vals[9]);


                                uiController.SYS_DIA_BPM_updated((int)vals[6], (int)vals[8], (int)vals[9]);
                                //uiController.ShowMessageOnUI("Boood pressure read succesfully.", true);
                                countMeasuringPressure = 0;
                            }
                            //if (vals.Length == 8) uiController.updatingPressureMeanTime((int)vals[6]);
                        }
                        // error in blood pressure reading
                        if (vals.Length == 14)
                        {
                            String message = "";
                            //Debug.WriteLine("vals[5]" + vals[5]);
                            //Debug.WriteLine("127 & (int)vals[5]" + (127 & (int)vals[5]));
                            if ((int)vals[5] >> 7 == 0)
                            {
                                switch ((int)vals[5])
                                {
                                    case 0:
                                        message = "No pulse detected.";
                                        break;
                                    case 1:
                                        message = "The cuff pressure failed to reach 30mmHg within 7 seconds (check that cuff is wrapped well).";
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
                            else if ((int)vals[5] >> 7 == 1)
                            {

                                switch ((int)vals[5] - 128)
                                {
                                    case 1:
                                        message = "The cuff pressure failed to reach 30mmHg within 7 seconds (check that cuff is wrapped well).";
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
                        //if ((int)vals[2] == 82)
                        //{
                        //  // when waveform data comes down to 0 then it is end of the spo2 reading
                        //  var waveformData = (int)vals[5];
                        //  //Debug.WriteLine("WaveformData: " + waveformData);
                        //  //printUpdatedCharacteristics(ch);

                        //  this.uiController.updateBpmWaveform((int)vals[6]);

                        //  if (waveformData == 0)
                        //  {
                        //      this.uiController.noticeEndOfReadingSpo2();
                        //  }
                        //}

                        //if ((int)vals[2] == 83 && (int)vals[3] == 7)
                        //{
                        //  if (vals[5] == 0 || vals[6] == 0)
                        //  {
                        //      Debug.WriteLine("Invallid readings.");
                        //  }
                        //  else {
                        //      int lastSpo2 = (int)vals[5];
                        //      int lastBPM = (int)vals[6];
                        //      this.uiController.SPO2_readingCompleted(lastSpo2, lastBPM, (float)((int)vals[8]) / 100);
                        //  }
                        //}

                        //// spo2 , PI and bpm is available in spot check monitor
                        if ((int)vals[2] >= 80 && (int)vals[2] < 84)
                        {
                            if ((int)vals[2] == 82)
                            {
                                var waveformBpm = (int)vals[6];
                                if (waveformBpm != 0)
                                    this.uiController.updateBpmWaveform(waveformBpm);
                            }

                            var token = (int)vals[2];
                            var length = (int)vals[3];

                            if (token == 83 && length == 7)
                            {
                                var status_bit1 = ((int)vals[9] & (1 << 1)) != 0;
                                if (status_bit1)
                                {
                                    //Debug.WriteLine("end of the spo2 reading preBMP=" + preBMP);
                                    //display the error message on the screen
                                    int errshift = vals[9] >> 6;
                                    int errval = (int)vals[9] - (errshift * (int)Math.Pow(2, 6));
                                    if ((vals[5] == 0 || vals[6] == 0) && errval != 2)
                                    {
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
                                preBMP = (int)vals[6];
                                if (vals[5] == 0 || vals[6] == 0)
                                {

                                    //Debug.WriteLine("Invalid readings.");
                                }
                                else
                                {
                                    int lastSpo2 = (int)vals[5];
                                    int lastBPM = (int)vals[6];

                                    uiController.SPO2_readingCompleted(lastSpo2, lastBPM, (float)((int)vals[8]) / 10);
                                }
                            }
                            /*
                            if ((int)vals[2] == 0x50 && (int)vals[4] == 2)
                            {
                                int lastSpo2 = (int)vals[5];
                                int lastBPM = (int)vals[6];
                                uiController.SPO2_readingCompleted(lastSpo2, lastBPM, (float)((int)vals[8]) / 100);

                            }*/
                        }

                        // this token is for glucose reading
                        if ((int)vals[2] == 115)
                        {
                            //Debug.WriteLine("glucose");
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

                            glucoseResult = ((int)vals[5] >> 3) & 3;
                            //Debug.WriteLine("correctResult= " + glucoseResult);
                            if (glucoseResult == 0)
                            {
                                int D0_data1 = (int)vals[5];

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
                               // Debug.WriteLine("gluUnit = " + gluUnit);
                            }
                        }



                        if ((int)vals[2] > 111 && (int)vals[2] < 115)
                        {
                            //Debug.WriteLine("Temperature related token.");

                            // from document it is written that the 5 byte is status and D4 is temperature probe is connected
                            //Byte temperatureStatus = ;
                            // bit 5
                            var D4 = (vals[5] & (1 << 4)) != 0;

                            // if D4=0 means temperature reading is completed
                            if (D4 == false)
                            {
                                // has the temperature reading
                                // combining byte 6 and byte 7 to read temperature
                                int data = ((int)vals[6] << 8) + (int)vals[7];
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
                        pretoken = (int)vals[2];
                    }
                }
            }

        }

    }
}
