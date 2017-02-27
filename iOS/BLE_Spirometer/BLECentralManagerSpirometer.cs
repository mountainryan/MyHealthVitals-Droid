using System;
using CoreBluetooth;
using MyHealthVitals;
using MyHealthVitals.iOS;

[assembly: Xamarin.Forms.Dependency(typeof(BLECentralManagerSpirometer))]

namespace MyHealthVitals.iOS
{
	public class BLECentralManagerSpirometer:ICBCentralManagerSpirometer
	{
		public static CBCentralManager manager ;
		public static CBPeripheral connectedPeripheral;
		public static BLEReadingUpdatableSpiroMeter caller;
		//public static SpirometerMonitorDelegate peripheralDel;

		public void connectToSpirometer(BLEReadingUpdatableSpiroMeter callerNew) { 
			caller = callerNew;

			if (manager == null)
			{
				initializeBluetooth();
			}
			else { 
				if (connectedPeripheral!= null &&  connectedPeripheral.State == CBPeripheralState.Connected)
				{
					connectedPeripheral.Delegate = new BLEPeripheralDelSpirometer(caller);
					connectedPeripheral.DiscoverServices();
				}
				else {
					CBUUID[] cbuuids = new CBUUID[] { CBUUID.FromString("FFF0") };
					manager.ScanForPeripherals(cbuuids); //Initiates async calls of DiscoveredPeripheral
				}
			}
		}

		public void StopReadingValue() {
			try
			{
				((BLEPeripheralDelSpirometer)connectedPeripheral.Delegate).pollingTimer.Enabled = false;
			}
			catch {
				Console.WriteLine("polling timer is nil");	
			}
		}

		public static void initializeBluetooth() { 

			manager = new CBCentralManager();

			manager.UpdatedState += (sender, e) =>
			{
				Console.WriteLine("bluetooth is on on device");
				//if (connectedPeripheral.State == CBPeripheralState.Connected) { 
				CBUUID[] cbuuids = new CBUUID[] { CBUUID.FromString("FFF0") };
				manager.ScanForPeripherals(cbuuids); //Initiates async calls of DiscoveredPeripheral	
				//}
			};

			manager.DiscoveredPeripheral += (sender, e) =>
			{
				Console.WriteLine("peripheral Name: " + e.Peripheral.Name);

				if (e.Peripheral.Name != "BLE-MSA")
				{
					Console.WriteLine("thi is not the spirometer.");
					//CBUUID[] cbuuids = new CBUUID[] { CBUUID.FromString("FFF0") };
					//manager.ScanForPeripherals(cbuuids); //Initiates async calls of DiscoveredPeripheral	
				}
				else { 
					manager.ConnectPeripheral(e.Peripheral);
					manager.StopScan();
				}
			};

			manager.ConnectedPeripheral += (sender, e) =>
			{
				Console.WriteLine("ConnectedPeripheral");
				connectedPeripheral = e.Peripheral;
				connectedPeripheral.Delegate = new BLEPeripheralDelSpirometer(caller);
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
