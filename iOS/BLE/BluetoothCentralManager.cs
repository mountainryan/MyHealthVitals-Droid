using System;
using CoreBluetooth;
using Foundation;
using MyHealthVitals.iOS;
using System.Timers;

[assembly: Xamarin.Forms.Dependency(typeof(BluetoothCentralManager))]

namespace MyHealthVitals.iOS
{
	public class BluetoothCentralManager : ICBCentralManager
	{
		public static CBCentralManager manager;
		public static CBPeripheral connectedPeripheral;
		public static Object uiController;

		public void startMeasuringBP()
		{
			byte[] bytes = new byte[] { 0xaa, 0x55, 0x40, 0x02, 0x01, 0x29 };
			connectedPeripheral.WriteValue(NSData.FromArray(bytes), ((BluetoothPeripheralDelegate)BluetoothCentralManager.connectedPeripheral.Delegate).bmChar, CBCharacteristicWriteType.WithResponse);
		}

		public void ConnectToDevice(Object uiController)
		{
			BluetoothCentralManager.uiController = uiController;

			if (manager == null)
			{
				initializeBluetooth();
			}
			else {
				if (connectedPeripheral != null && connectedPeripheral.State == CBPeripheralState.Connected)
				{
					connectedPeripheral.Delegate = new BluetoothPeripheralDelegate();
					connectedPeripheral.DiscoverServices();
				}
				else {
					scanPeriphealOnDifferentThread("Searching device...");
				}
			}
		}

		public void checkIfScanningTimeOut()
		{

			Timer tmr = new Timer();
			tmr.Interval = 10000; // 0.1 second
			tmr.Elapsed += ScanningTimeElapsed; // We'll write it in a bit
			tmr.Start(); // The countdown is launched
		}

		private void ScanningTimeElapsed(object sender, EventArgs e)
		{
			if(connectedPeripheral == null)
				((BluetoothCallBackUpdatable)uiController).ShowMessageOnUI("Scanning time out. Please check if your device is turned on.");
			((Timer)sender).Stop();
		}

		public void scanPeriphealOnDifferentThread(String messageUi) { 
			new System.Threading.Thread(new System.Threading.ThreadStart(() =>
				{
					CBUUID[] cbuuids = null;
					manager.ScanForPeripherals(cbuuids); //Initiates async calls of DiscoveredPeripheral	
				((BluetoothCallBackUpdatable)uiController).ShowMessageOnUI(messageUi);
				})).Start();

			checkIfScanningTimeOut();
		}

		public void initializeBluetooth()
		{
			manager = new CBCentralManager();

			manager.UpdatedState += (sender, e) =>
			{
				scanPeriphealOnDifferentThread("Searching device...");
			};

			manager.DiscoveredPeripheral += (sender, e) =>
			{
				Console.WriteLine("peripheral Name: " + e.Peripheral.Name);
				if (e.Peripheral.Name == "PC_300SNT")
				{
					manager.StopScan();
					manager.ConnectPeripheral(e.Peripheral);
				}
			};

			manager.ConnectedPeripheral += (sender, e) =>
			{
				((BluetoothCallBackUpdatable)uiController).ShowMessageOnUI("Spot check monitor connected.");
				connectedPeripheral = e.Peripheral;
				connectedPeripheral.Delegate = new BluetoothPeripheralDelegate();
				connectedPeripheral.DiscoverServices();
			};

			//manager.
			manager.FailedToConnectPeripheral += (sender, e) =>
			{
				((BluetoothCallBackUpdatable)uiController).ShowMessageOnUI("Failed to connect Spot check monitor.");
			};

			manager.DisconnectedPeripheral += (sender, e) =>
			{
				scanPeriphealOnDifferentThread("Lost connection. Searching device...");
			};
		}
	}
}
