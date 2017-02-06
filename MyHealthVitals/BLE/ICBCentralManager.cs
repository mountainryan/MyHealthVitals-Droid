using System;
namespace MyHealthVitals
{
	public interface ICBCentralManager
	{
		void ConnectToDevice(Object uiController);
		void startMeasuringBP();
	}
}
