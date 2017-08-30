using System;
using System.Collections.Generic;

namespace MyHealthVitals
{
	public interface IBluetoothCallBackUpdatable
	{
		void SaveEcgState(int state);
		void ShowMessageOnUI(String message, Boolean isConnected, String title = null);
		void ShowConcetion(String message, Boolean isConnected);
		void SPO2_readingCompleted(int sp02, int bpm, float perfusionIndex);
		void SYS_DIA_BPM_updated(int bpsys, int bpdia, int bpm);
		void updatingPressureMeanTime(int pressure);
		void updated_Weight(decimal weight);
		void updateTemperature(decimal temperature);

		void noticeEndOfReadingSpo2();
		void updateDeviceConnected(String deviceName, bool isConnected);

		void updateGlucoseReading(decimal gluReading,string unit);

		void updateECGPacket(List<int> ecgPacket);
		void updateECGEnded(int bpm, int ecg);

		void updateBpmWaveform(int bpm);

	}

	public interface BLEReadingUpdatableSpiroMeter
	{
		void updateCaller(SpirometerReading currReading);
		void updateDeviceStateOnUI(String message, bool isConnected);
		void testAgainDialog();
	}
}
