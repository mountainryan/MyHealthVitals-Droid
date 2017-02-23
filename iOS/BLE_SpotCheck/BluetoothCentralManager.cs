using System;
using CoreBluetooth;
using Foundation;
using MyHealthVitals.iOS;
using System.Timers;
using System.Collections.Generic;

[assembly: Xamarin.Forms.Dependency(typeof(BluetoothCentralManager))]

namespace MyHealthVitals.iOS
{
	public class BluetoothCentralManager : ICBCentralManager
	{
		public static CBCentralManager manager;
		public static CBPeripheral connectedPeripheral;
		public static Object uiController;
		public List<CBPeripheral> connectedPeripherals = new List<CBPeripheral>();

		//public static 

		public void startMeasuringBP()
		{
			if (connectedPeripheral != null && connectedPeripheral.State == CBPeripheralState.Connected)
			{
				byte[] bytes = new byte[] { 0xaa, 0x55, 0x40, 0x02, 0x01, 0x29 };
				connectedPeripheral.WriteValue(NSData.FromArray(bytes), ((BluetoothPeripheralDelegate)BluetoothCentralManager.connectedPeripheral.Delegate).bmChar, CBCharacteristicWriteType.WithResponse);
			}
			else { 
				((IBluetoothCallBackUpdatable)uiController).ShowMessageOnUI("Device is not connected. Please connect and try again.", false);
			}
		}

		public void stopReadingECG() { 
			if (connectedPeripheral != null && connectedPeripheral.State == CBPeripheralState.Connected)
			{
				byte[] bytes = new byte[] { 0xaa, 0x55, 0x30, 0x02, 0x02, 0x24 };
				connectedPeripheral.WriteValue(NSData.FromArray(bytes), ((BluetoothPeripheralDelegate)BluetoothCentralManager.connectedPeripheral.Delegate).bmChar, CBCharacteristicWriteType.WithResponse);
			}
			else {
				((IBluetoothCallBackUpdatable)uiController).ShowMessageOnUI("Device is not connected. Please connect and try again.", false);
			}
		}

		public void startEcgMeasuring() { 
			if (connectedPeripheral != null && connectedPeripheral.State == CBPeripheralState.Connected)
			{
				//byte[] bytes = new byte[] { 0xaa, 0x55, 0x31, 0x02, 0x02, 0xC6 };
				byte[] bytes = new byte[] { 0xaa, 0x55, 0x31, 0x02, 0x02, 0x8F };
				//0x8F
				connectedPeripheral.WriteValue(NSData.FromArray(bytes), ((BluetoothPeripheralDelegate)BluetoothCentralManager.connectedPeripheral.Delegate).bmChar, CBCharacteristicWriteType.WithResponse);
			}
			else {
				((IBluetoothCallBackUpdatable)uiController).ShowMessageOnUI("Device is not connected. Please connect and try again.", false);
			}
		}

		public void ConnectToDevice(Object uiController)
		{
			BluetoothCentralManager.uiController = uiController;

			if (manager == null)
			{
				initializeBluetooth();
			}
			else {

				if (manager.IsScanning)
				{
					System.Console.WriteLine("is searching...");
				}
				else {
					if (connectedPeripheral!=null && connectedPeripheral.State == CBPeripheralState.Connected)
					{
						((IBluetoothCallBackUpdatable)uiController).ShowMessageOnUI("Connected", true);
						//discoverServicesOfConnectedPeripheral();
					}
					else { 
						scanPeripheals();
					}
				}
			}
		}

		public void scanPeripheals()
		{
			CBUUID[] cbuuids = null;
			manager.ScanForPeripherals(cbuuids); //Initiates async calls of DiscoveredPeripheral	
			((IBluetoothCallBackUpdatable)uiController).ShowMessageOnUI("Searching device...", false);

			checkIfScanningTimeOut();
		}

		//public void connecAndDiscoverServices(String peripheralName) {
		//	foreach (var peripheral in this.connectedPeripherals) {
		//		if (peripheralName == peripheral.Name) {
		//			manager.ConnectPeripheral(peripheral);
		//			break;
		//		}
		//	}
		//}

		public void discoverServicesOfConnectedPeripheral() {
			
			connectedPeripheral.Delegate = new BluetoothPeripheralDelegate();
			connectedPeripheral.DiscoverServices();
		}

		Timer tmr;
		public void checkIfScanningTimeOut()
		{
			tmr = new Timer();
			tmr.Interval = 10000; // 0.1 second
			tmr.Elapsed += ScanningTimeElapsed; // We'll write it in a bit
			tmr.Start(); // The countdown is launched
		}

		private void ScanningTimeElapsed(object sender, EventArgs e)
		{
			if (connectedPeripheral != null && connectedPeripheral.State == CBPeripheralState.Connected)
			{
				((IBluetoothCallBackUpdatable)uiController).ShowMessageOnUI("Connected.", true);
			}
			else { 
				((IBluetoothCallBackUpdatable)uiController).ShowMessageOnUI("Scanning time out. Please, check if device is turned on.", false);
			}
				
			((Timer)sender).Stop();

			manager.StopScan();
		}

		public void initializeBluetooth()
		{
			manager = new CBCentralManager();

			manager.UpdatedState += (sender, e) =>
			{
				scanPeripheals();
			};

			manager.DiscoveredPeripheral += (sender, e) =>
			{
				Console.WriteLine("peripheral Name: " + e.Peripheral.Name);

				//connectedPeripherals.Add(e.Peripheral);

				if (e.Peripheral.Name == "PC_300SNT")
				{
					manager.StopScan();
					tmr.Stop();
					manager.ConnectPeripheral(e.Peripheral);
				}
			};

			manager.ConnectedPeripheral += (sender, e) =>
			{
				((IBluetoothCallBackUpdatable)uiController).updateDeviceConnected(e.Peripheral.Name, true);

				((IBluetoothCallBackUpdatable)uiController).ShowMessageOnUI("Connected.",true);

				connectedPeripheral = e.Peripheral;
				connectedPeripheral.Delegate = new BluetoothPeripheralDelegate();
				connectedPeripheral.DiscoverServices();
			};

			//manager.
			manager.FailedToConnectPeripheral += (sender, e) =>
			{
				((IBluetoothCallBackUpdatable)uiController).ShowMessageOnUI("Failed to connect..",false);
			};

			manager.DisconnectedPeripheral += (sender, e) =>
			{
				((IBluetoothCallBackUpdatable)uiController).ShowMessageOnUI("Disconnected.",false);
			};
		}
	}
}
