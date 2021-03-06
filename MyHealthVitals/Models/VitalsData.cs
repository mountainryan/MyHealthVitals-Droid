﻿using System;
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
		public Reading ecg;
		public Reading weight;
		public Reading bmi;

		private void addDataTolocal(ParameterDetailItem pdi)
		{
			Dictionary<int, List<ParameterDetailItem>> dic = logcalParameteritem.localhashmap;
			if (dic.ContainsKey((int)pdi.categoryId))
			{
				List<ParameterDetailItem> valuelist = dic[(int)pdi.categoryId];
				valuelist.Insert(0, pdi);
			}
			else {
				List<ParameterDetailItem> valuelist = new List<ParameterDetailItem>();
				valuelist.Insert(0, pdi);
				dic.Add((int)pdi.categoryId, valuelist);
			}

		}

		public async void sendToServerTemperature()
		{
			if (temperature.EnglishValue > 0)
			{
				try
				{
					ParameterDetailItem pdi = new ParameterDetailItem();
					pdi.categoryId = temperature.CategoryId;
					pdi.date = temperature.Date.ToString("MM/dd/yyyy hh:mm tt");
					pdi.firstItem = temperature.EnglishValue.ToString() + "/" +
									MainPage.ConvertFahrenheitToCelsius((double)this.temperature.EnglishValue).ToString();;
					addDataTolocal(pdi);
					var retval = await temperature.PostReadingToService();
				}
				catch (Exception ex)
				{
                    //Debug.WriteLine("error message = "+ex.Message);
					//Debug.WriteLine("exception on sending temperature to server.");
				}
				//if (isServiceCallSuccess) temperature.EnglishValue = 0;
			}
		}
		public async void sendToServerWeight(float height)
		{
			//Debug.WriteLine("send to server weight");
			if (weight!= null && weight.EnglishValue > 0)
			{
				try
				{
					ParameterDetailItem pdi = new ParameterDetailItem();
					pdi.categoryId = weight.CategoryId;
					pdi.date = weight.Date.ToString("MM/dd/yyyy hh:mm tt");
                    pdi.firstItem = weight.EnglishValue.ToString() + "/"+ Math.Round(MainPage.ConvertLBToKG((double)weight.EnglishValue),1).ToString();
					if (height > 1)
					{
						float bmivalue = calculateBMI((float)height, (float)weight.EnglishValue);
						pdi.secondItem = Convert.ToString(bmivalue);
						//BMI = weight (lb) ÷ height2 (in2) × 703
						bmi = new Reading(null, (decimal)bmivalue, 7, false, null, null);
						bmi.Date = weight.Date;
					}
                    addDataTolocal(pdi);
					await weight.PostReadingToService();
					if (height >= 1) await bmi.PostReadingToService();
				}
				catch (Exception)
				{
					//Debug.WriteLine("exception on sending weight to server.");
				}
				//if (isServiceCallSuccess) temperature.EnglishValue = 0;
			}
		}
		private float calculateBMI(float h, float w) {
			
			return (float)Math.Round((double)(w / h / h * 703), 1);
		}
		public async void sendToServer_SPO2_PI_BPM()
		{
            ParameterDetailItem pdi = new ParameterDetailItem();
            try
            {
                if (spo2 != null && spo2.EnglishValue > 0)
                {
                    pdi.categoryId = spo2.CategoryId;
                    pdi.date = spo2.Date.ToString("MM/dd/yyyy hh:mm tt");
                    pdi.firstItem = spo2.EnglishValue.ToString();

                    await spo2.PostReadingToService();
                    //if (isServiceCallSuccess) spo2.EnglishValue = 0;
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine("exception on sending spo2 to server.");
            }
            try
            {
                if (bpm != null && bpm.EnglishValue > 0)
                {
                    pdi.secondItem = bpm.EnglishValue.ToString();
                    addDataTolocal(pdi);
                    await bpm.PostReadingToService();
                    //if (isServiceCallSuccess) bpm.EnglishValue = 0;
                }
            }
            catch (Exception ex)
            {
               // Debug.WriteLine("exception on sending bpm to server.");
            }
			
            spo2 = null;
            bpm = null;
		}

		public async void sendToServer_Glucose()
		{
			try
			{
				if (glucose.EnglishValue > 0)
				{
					ParameterDetailItem pdi = new ParameterDetailItem();
					pdi.categoryId = glucose.CategoryId;
					pdi.date = glucose.Date.ToString("MM/dd/yyyy hh:mm tt");
					pdi.firstItem = glucose.EnglishValue.ToString();
					addDataTolocal(pdi);

					await glucose.PostReadingToService();
				}
			}
			catch (Exception)
			{
				//Debug.WriteLine("Exception occured to the sending glucose reading to the server.");
			}
		}

		public async void sendHeartRateToServer() { 
            try
            {
				if (bpm.EnglishValue > 0)
				{
					ParameterDetailItem pdi = new ParameterDetailItem();
					pdi.categoryId = bpm.CategoryId;
					pdi.date = bpm.Date.ToString("MM/dd/yyyy hh:mm tt");
					pdi.firstItem = bpm.EnglishValue.ToString();
					addDataTolocal(pdi);
					await bpm.PostReadingToService();
				}
            }
            catch (Exception ex)
            {
                //Debug.WriteLine("Exception occured to the sending Heart rate reading to the server.");
            }
			
		}
		public async void sendEcgToServer()
		{
			if (ecg.EnglishValue >= 0)
			{

				ParameterDetailItem pdi = new ParameterDetailItem();
				pdi.categoryId = ecg.CategoryId;
				pdi.date =ecg.Date.ToString("MM/dd/yyyy hh:mm tt");
                Task_vars.ecgdate = ecg.Date;
                pdi.firstItem = ecg.Abnormal == false ? "Normal" : "Abnormal";

                try
                {
					var read = await ecg.PostReadingToService();
					if (read != null)
					{
						Task_vars.lastecgreading = read;
						Task_vars.ecgreadingid = read.Id;
						pdi.getID = read.Id;
					}
                }
                catch (Exception ex)
                {
                   // Debug.WriteLine("Exception occured to the sending ECG reading to the server.");
                }
				
				addDataTolocal(pdi);

			}

		}
		public async void sendToServer_SYS_DIA()
		{
            try
            {
                //Debug.WriteLine("sendToServer_SYS_DIA  and bpm");
                if (bpDia != null && bpDia.EnglishValue > 0 && bpSys != null && bpSys.EnglishValue > 0)
                {
                    if (bpSys.EnglishValue < bpDia.EnglishValue)
                    {
                        return;
                    }
                    ParameterDetailItem pdi = new ParameterDetailItem();
                    pdi.categoryId = bpDia.CategoryId;
                    pdi.date = bpDia.Date.ToString("MM/dd/yyyy hh:mm tt");
                    pdi.firstItem = bpSys.EnglishValue.ToString();
                    pdi.secondItem = bpDia.EnglishValue.ToString();
                    addDataTolocal(pdi);

                    await bpDia.PostReadingToService();
                    await bpSys.PostReadingToService();
                    if (bpm != null)
                        bpm.Date = bpDia.Date;

                    //if (isServiceCallSuccess) bpDia.EnglishValue = 0;
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine("Exception occured to the sending BP reading to the server.");
            }
				
            try
            {
				if (bpm != null && bpm.EnglishValue > 0)
				{
					ParameterDetailItem pdi = new ParameterDetailItem();
					pdi.categoryId = bpm.CategoryId;
					pdi.date = bpm.Date.ToString("MM/dd/yyyy hh:mm tt");
					pdi.firstItem = bpm.EnglishValue.ToString();
					addDataTolocal(pdi);

					await bpm.PostReadingToService();
					//if (isServiceCallSuccess) bpm.EnglishValue = 0;
				}
            }
            catch (Exception ex)
            {
                //Debug.WriteLine("Exception occured to the sending bpm reading to the server.");
            }
			
		}
	}
}
