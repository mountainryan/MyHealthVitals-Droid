using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;

namespace MyHealthVitals
{

	//public class SpO2Reading
	//{
	//	DateTime date;
	//	decimal spo2;
	//	decimal pulse;

	//	public SpO2Reading(DateTime newDate, decimal newSpo2, decimal newPulse)
	//	{
	//		this.date = newDate;
	//		this.spo2 = newSpo2;
	//		this.pulse = newPulse;
	//	}
	//}

	public partial class ParameterItemDetail : ContentPage
	{

		public long categoryId;

		public ObservableCollection<ParameterDetailItem> data = new ObservableCollection<ParameterDetailItem>();
		public ParameterItemDetail(long CategoryId)
		{
			this.categoryId = CategoryId;
			InitializeComponent();

			switch (categoryId) {
				case 1: {
						this.Title = "Blood Pressure Data List";
						headerWithTwoTitle header = new headerWithTwoTitle("SYS", "DIA");
						headerContainer.Children.Add(header);
						break;
						}
				case 2: {
						this.Title = "SpO2 Data List";
						headerWithTwoTitle header = new headerWithTwoTitle("SpO2", "Pulse");
						headerContainer.Children.Add(header);
						break;
					}
				case 4: {
						this.Title = "Temperature Data List";
						headerWithOneTitle header = new headerWithOneTitle("Temperature(°F/°C)");
						headerContainer.Children.Add(header);
						break;
					}
				case 8: {
						this.Title = "Glucose Data List";
						headerWithOneTitle header = new headerWithOneTitle("Glucose (Mmol/L)");
						headerContainer.Children.Add(header);
						break;
					}
			}

			callApi();
		}

		public async void callApi()
		{

			layoutLoading.IsVisible = true;


			//Reading[] allReadings = new Reading[]();
			try
			{
				var allReadings = await Reading.GetAllReadingsFromService();

				var allCategoryReading = from reading in allReadings
								 where reading.CategoryId == categoryId
								 select reading;

				//allCategoryReading = allCategoryReading.GroupBy(s => s.Date);

				switch (categoryId)
				{
					// Blood pressure
					case 1:
						{
							var bpReadings = from spSet in
							   (from reading in allCategoryReading
								group reading by reading.Date)
											 orderby spSet.Key descending
							                              let dia = spSet.FirstOrDefault(x => x.ValueType == "DIA" || x.ValueType=="Diastolic")
											 			let sys = spSet.FirstOrDefault(x => x.ValueType == "SYS" || x.ValueType == "Systolic")

											 where sys != null && dia != null
											 select new
											 {
												 Date = spSet.Key,
												 sys = sys,
												 dia = dia,
											 };

							var newBpReadings = (bpReadings.GroupBy(s => s.Date).Select(grp => grp.First())).ToArray();

							foreach (var reading in newBpReadings)
							{
								var item = new ParameterDetailItem();
								item.date = reading.Date.ToString("MM/dd/yyyy hh:mm tt");
								item.firstItem = ((int)reading.sys.EnglishValue).ToString();
								item.secondItem = ((int)reading.dia.EnglishValue).ToString();
								item.categoryId = reading.sys.CategoryId;
								data.Add(item);
							}
							itemList.ItemsSource = data;

							break;
						}
					case 2:
						{
							var Spo2Readings = from spSet in
							   (from reading in allCategoryReading
								group reading by reading.Date)
											 orderby spSet.Key descending
											 let SpO2 = spSet.FirstOrDefault(x => x.ValueType == "SpO2")
											 let Pulse = spSet.FirstOrDefault(x => x.ValueType == "Pulse")
								                              where SpO2 != null && Pulse != null
											 select new
											 {
												Date = spSet.Key,
												SpO2 = SpO2,
												Pulse = Pulse,
											 };

							//Spo2Readings.GroupBy

							var newSpo2Readings = (Spo2Readings.GroupBy(s => s.Date).Select(grp => grp.First())).ToArray();

							foreach (var reading in newSpo2Readings)
							{
								var item = new ParameterDetailItem();
								item.date = reading.Date.ToString("MM/dd/yyyy hh:mm tt");
								item.firstItem = ((int)reading.SpO2.EnglishValue).ToString();
								item.secondItem = ((int)reading.Pulse.EnglishValue).ToString();
								item.categoryId = reading.Pulse.CategoryId;
								data.Add(item);
							}
							itemList.ItemsSource = data;
							break;
						}
					case 3:
						{
							break;
						}
					// temperature
					case 4:
						{
							var sortedTemps = allCategoryReading.Reverse();
							
							foreach (var reading in sortedTemps)
							{
								var item = new ParameterDetailItem();
								item.date = reading.Date.ToString("MM/dd/yyyy hh:mm tt");
								item.firstItem = Math.Round((decimal)reading.EnglishValue,1) +"/"+ Math.Round((decimal)reading.MetricValue, 1);
								item.categoryId = reading.CategoryId;
								data.Add(item);
							}

							itemList.ItemsSource = data;
							break;
						}
					case 5:
						{
							break;
						}
					case 6:
						{
							break;
						}
					case 7:
						{
							break;
						}
					case 8:
						{
							var sortedTemps = allCategoryReading.Reverse();

							foreach (var reading in sortedTemps)
							{
								var item = new ParameterDetailItem();
								item.date = reading.Date.ToString("MM/dd/yyyy hh:mm tt");
								item.firstItem = Math.Round((decimal)reading.EnglishValue, 1).ToString();
								item.categoryId = reading.CategoryId;
								data.Add(item);
							}

							itemList.ItemsSource = data;
							break;
						}
					case 9:
						{
							break;
						}
				}

				itemList.ItemsSource = data;
			}
			catch (Exception)
			{
				Debug.WriteLine("error in calling server or parsing");
			}
			finally { 
				layoutLoading.IsVisible = false;
			}

			this.itemList.HeightRequest = this.Content.Bounds.Size.Height - 110;
		}
	}
}
