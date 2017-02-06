using System;
using CoreBluetooth;

namespace SLxaml
{
	public class BluetoothCentralManager
	{
		public static CBCentralManager manager ;
		public static CBPeripheral connectedPeripheral;
		public static Object uiController;
		//public static SpirometerMonitorDelegate peripheralDel;

		public static void startReadingValue(Object callerNew) { 
			BluetoothCentralManager.uiController = callerNew;

			if (manager == null)
			{
				initializeBluetooth();
			}
			else { 
				if (connectedPeripheral!= null &&  connectedPeripheral.State == CBPeripheralState.Connected)
				{
					connectedPeripheral.Delegate = new BluetoothPeripheralDelegate();
					connectedPeripheral.DiscoverServices();
				}
				else {
					CBUUID[] cbuuids = null;
					manager.ScanForPeripherals(cbuuids); //Initiates async calls of DiscoveredPeripheral	
				}
			}

		}

		public static void initializeBluetooth() { 

			manager = new CBCentralManager();

			manager.UpdatedState += (sender, e) =>
			{
				Console.WriteLine("bluetooth is on on device");
				//if (connectedPeripheral.State == CBPeripheralState.Connected) { 
				//CBUUID[] cbuuids = new CBUUID[] { CBUUID.FromString("PC_300SNT") };
				CBUUID[] cbuuids = null;
				manager.ScanForPeripherals(cbuuids); //Initiates async calls of DiscoveredPeripheral	
				//}
			};

			manager.DiscoveredPeripheral += (sender, e) =>
			{
				Console.WriteLine("peripheral Name: " + e.Peripheral.Name);
				if (e.Peripheral.Name == "PC_300SNT") { 
					manager.StopScan();
					manager.ConnectPeripheral(e.Peripheral);	
				}
			};

			manager.ConnectedPeripheral += (sender, e) =>
			{
				Console.WriteLine("ConnectedPeripheral");
				connectedPeripheral = e.Peripheral;
				connectedPeripheral.Delegate = new BluetoothPeripheralDelegate();
				connectedPeripheral.DiscoverServices();
			};


			manager.FailedToConnectPeripheral += (sender, e) =>
			{
				Console.WriteLine("FailedToConnectPeripheral");
			};

			manager.DisconnectedPeripheral += (sender, e) =>
			{
				Console.WriteLine("DisconnectedPeripheral");
			};	
		}
	}
}
