using System;
namespace MyHealthVitals
{
	public interface IBluetoothCallBackUpdatable
	{
		void ShowMessageOnUI(String message, Boolean isConnected);
		void SPO2_readingCompleted(int sp02, int bpm, float perfusionIndex);
		void SYS_DIA_BPM_updated(int bpsys, int bpdia, int bpm);
		void updatingPressureMeanTime(int pressure);
		void updateTemperature(int temperature, String type);

		void noticeEndOfReadingSpo2();
	}
}
