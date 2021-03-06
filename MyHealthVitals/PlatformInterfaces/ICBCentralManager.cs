﻿using System;
namespace MyHealthVitals
{
	public interface ICBCentralManager
	{
		void ConnectToDevice(Object uiController);
		void startMeasuringBP();
		void startEcgMeasuring();
		void stopReadingECG();
		void stopMeasuringSpo2();
	}

	public interface ICBCentralManagerSpirometer {
		void connectToSpirometer(BLEReadingUpdatableSpiroMeter callerNew);
		void StopReadingValue();
	}
}
