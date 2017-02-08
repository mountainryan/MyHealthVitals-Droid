using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

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


		public async void sendToServerTemperature() { 
			if (temperature.EnglishValue > 0)
			{
				var isServiceCallSuccess = await temperature.PostReadingToService();
				if (isServiceCallSuccess) temperature.EnglishValue = 0;
			}
		}

		public async void sendToServer_SPO2_PI_BPM() { 
			if (spo2.EnglishValue > 0)
			{
				var isServiceCallSuccess = await spo2.PostReadingToService();
				if (isServiceCallSuccess) spo2.EnglishValue = 0;
			}

			if (perfusionIndex.EnglishValue > 0)
			{
				var isServiceCallSuccess = await perfusionIndex.PostReadingToService();
				if (isServiceCallSuccess) perfusionIndex.EnglishValue = 0;
			}

			if (bpm.EnglishValue > 0)
			{
				var isServiceCallSuccess = await bpm.PostReadingToService();
				if (isServiceCallSuccess) bpm.EnglishValue = 0;
			}
		}

		public async void sendToServer_SYS_DIA_BPM() { 
			if (bpDia.EnglishValue > 0)
			{
				var isServiceCallSuccess = await bpDia.PostReadingToService();
				if (isServiceCallSuccess) bpDia.EnglishValue = 0;
			}

			if (bpSys.EnglishValue > 0)
			{
				var isServiceCallSuccess = await bpSys.PostReadingToService();
				if (isServiceCallSuccess) bpSys.EnglishValue = 0;
			}

			if (bpm.EnglishValue > 0)
			{
				var isServiceCallSuccess = await bpm.PostReadingToService();
				if (isServiceCallSuccess) bpm.EnglishValue = 0;
			}
		}
	}
}
