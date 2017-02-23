using System;
namespace MyHealthVitals
{
	public interface ICBCentralManager
	{
		void ConnectToDevice(Object uiController);
		void startMeasuringBP();
		void startEcgMeasuring();
		void stopReadingECG();
	}

	public interface ICBCentralManagerSpirometer {
		void connectToSpirometer(Object callerNew);
		void StopReadingValue();
	}
}
