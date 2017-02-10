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
			if (connectedPeripheral != null && connectedPeripheral.State == CBPeripheralState.Connected)
			{
				byte[] bytes = new byte[] { 0xaa, 0x55, 0x40, 0x02, 0x01, 0x29 };
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
						discoverServicesOfConnectedPeripheral();
					}
					else { 
						scanPeriphealOnDifferentThread();
					}
				}
			}
		}

		public void discoverServicesOfConnectedPeripheral() {
			
			connectedPeripheral.Delegate = new BluetoothPeripheralDelegate();
			connectedPeripheral.DiscoverServices();
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
				((IBluetoothCallBackUpdatable)uiController).ShowMessageOnUI("Scanning time out. Please check if your device is turned on.",false);
			((Timer)sender).Stop();

			manager.StopScan();
			Console.WriteLine("manager scanning: " + manager.IsScanning);
		}

		public void scanPeriphealOnDifferentThread() { 
			
			CBUUID[] cbuuids = null;
			manager.ScanForPeripherals(cbuuids); //Initiates async calls of DiscoveredPeripheral	
			((IBluetoothCallBackUpdatable)uiController).ShowMessageOnUI("Searching device...", false);

			checkIfScanningTimeOut();
		}

		public void initializeBluetooth()
		{
			manager = new CBCentralManager();

			manager.UpdatedState += (sender, e) =>
			{
				scanPeriphealOnDifferentThread();
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
