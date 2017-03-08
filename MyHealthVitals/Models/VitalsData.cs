using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Diagnostics;

namespace MyHealthVitals
{
	public class VitalsData
	{
		public Reading bpm;
		public Reading perfusionIndex;
		public Reading spo2;
		public Reading bpSys;
		public Reading bpDia;
		public Reading temperature;
		public Reading glucose;

		public async void sendToServerTemperature()
		{
			if (temperature.EnglishValue > 0)
			{
				try
				{
					await temperature.PostReadingToService();
				}
				catch (Exception)
				{
					Debug.WriteLine("exception on sending temperature to server.");
				}
				//if (isServiceCallSuccess) temperature.EnglishValue = 0;
			}
		}

		public async void sendToServer_SPO2_PI_BPM()
		{
			if (spo2 != null && spo2.EnglishValue > 0)
			{
				await spo2.PostReadingToService();
				//if (isServiceCallSuccess) spo2.EnglishValue = 0;
			}

			//if (perfusionIndex.EnglishValue > 0)
			//{
			//	await perfusionIndex.PostReadingToService();
			//	//if (isServiceCallSuccess) perfusionIndex.EnglishValue = 0;
			//}

			if (bpm != null && bpm.EnglishValue > 0)
			{
				await bpm.PostReadingToService();
				//if (isServiceCallSuccess) bpm.EnglishValue = 0;
			}
		}

		public async void sendToServer_Glucose()
		{
			try
			{
				if (glucose.EnglishValue > 0)
					await glucose.PostReadingToService();
			}
			catch (Exception)
			{
				Debug.WriteLine("Exception occured to the sending glucose reading to the server.");
			}
		}

		public async void sendToServer_SYS_DIA()
		{
			if (bpDia != null && bpDia.EnglishValue > 0)
			{
				await bpDia.PostReadingToService();
				//if (isServiceCallSuccess) bpDia.EnglishValue = 0;
			}

			if (bpSys != null && bpSys.EnglishValue > 0)
			{
				await bpSys.PostReadingToService();
				//if (isServiceCallSuccess) bpSys.EnglishValue = 0;
			}

			if (bpm.EnglishValue > 0)
			{
				await bpm.PostReadingToService();
				//if (isServiceCallSuccess) bpm.EnglishValue = 0;
			}
		}
	}
}
