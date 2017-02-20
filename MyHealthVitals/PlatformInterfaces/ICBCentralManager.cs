using System;
namespace MyHealthVitals
{
	public interface ICBCentralManager
	{
		void ConnectToDevice(Object uiController);
		void startMeasuringBP();
	}

	public interface IButtonManager {
		void changeBorder(MyButton btn);
	}
}
