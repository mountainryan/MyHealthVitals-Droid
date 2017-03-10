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

			//var newItem = new ParameterDetailItem();
			//newItem.date = "date";
			//newItem.firstItem = "first item";
			//newItem.secondItem = "second item";

			//var newItem1 = new ParameterDetailItem();
			//newItem1.date = "date";
			//newItem1.firstItem = "first item";
			//newItem1.secondItem = "second item";

			//data.Add(newItem);
			//data.Add(newItem1);

			//itemList.ItemsSource = data;

			switch (categoryId)
			{
				case 1:
					{
						this.Title = "Blood Pressure Data List";
						headerContainer.Children.Add(new headerWithTwoTitle("SYS", "DIA"));
						break;
					}
				case 2:
					{
						this.Title = "SpO2 Data List";
						headerContainer.Children.Add(new headerWithTwoTitle("SpO2", "Pulse"));
						break;
					}
				case 3:
					{
						this.Title = "Heart Rate Data List";
						headerContainer.Children.Add(new headerWithOneTitle("Heart Rate (Pulse)"));
						break;
					}
				
				case 4:
					{
						this.Title = "Temperature Data List";
						headerContainer.Children.Add(new headerWithOneTitle("Temperature °F/°C"));
						break;
					}
				case 5:
					{
						this.Title = "Weight/BMI Data List";
						headerContainer.Children.Add(new headerWithTwoTitle("Weight lbs/kg", "BMI"));
						break;
					}
				case 8:
					{
						this.Title = "Glucose Data List";
						headerContainer.Children.Add(new headerWithOneTitle("Glucose (Mmol/L)"));
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
							                              let dia = spSet.FirstOrDefault(x => (x.ValueType == "DIA" || x.ValueType == "Diastolic"))
							                              let sys = spSet.FirstOrDefault(x => (x.ValueType == "SYS" || x.ValueType == "Systolic"))

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
							var allCategoryReading2 = from reading in allReadings
													  where reading.CategoryId == 2 || reading.CategoryId == 3
													  select reading;

							var Spo2Readings = from spSet in
							   (from reading in allCategoryReading2
								group reading by reading.Date)
											   orderby spSet.Key descending
											   let SpO2 = spSet.FirstOrDefault(x => x.ValueType == "SpO2")
											   let Pulse = spSet.FirstOrDefault(x => (x.ValueType == "Heart Rate" || x.ValueType == "Pulse"))
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
								item.categoryId = reading.SpO2.CategoryId;
								data.Add(item);
							}
							itemList.ItemsSource = data;
							break;
						}
					case 3:
						{
							//var sortedHeartRates = allCategoryReading.Reverse();

							foreach (var reading in allCategoryReading)
							{
								var item = new ParameterDetailItem();
								item.date = reading.Date.ToString("MM/dd/yyyy hh:mm tt");
								item.firstItem = ((int)reading.EnglishValue).ToString();
								item.categoryId = reading.CategoryId;
								data.Add(item);
							}

							itemList.ItemsSource = data;
							break;
						}
					// temperature
					case 4:
						{
							//var sortedTemps = allCategoryReading.Reverse();

							foreach (var reading in allCategoryReading)
							{
								var item = new ParameterDetailItem();
								item.date = reading.Date.ToString("MM/dd/yyyy hh:mm tt");
								item.firstItem = Math.Round((decimal)reading.EnglishValue, 1) + "/" + Math.Round((decimal)reading.MetricValue, 1);
								item.categoryId = reading.CategoryId;
								data.Add(item);
							}

							itemList.ItemsSource = data;
							break;
						}
					case 5:
						{
							var allCategoryReading5 = from reading in allReadings
													  where reading.CategoryId == 5 || reading.CategoryId == 7
													  select reading;

							var weightBmiReading = from spSet in
							   (from reading in allCategoryReading5
								group reading by reading.Date)
											   orderby spSet.Key descending
							                                let weight = spSet.FirstOrDefault(x => x.CategoryId == 5)
							                                let bmi = spSet.FirstOrDefault(x => x.CategoryId == 7)
											   where weight != null && bmi != null
											   select new
											   {
												   Date = spSet.Key,
												   weight = weight,
												   bmi = bmi,
											   };

							//Spo2Readings.GroupBy

							var newWeightBmiReading = (weightBmiReading.GroupBy(s => s.Date).Select(grp => grp.First())).ToArray();

							foreach (var reading in newWeightBmiReading)
							{
								var item = new ParameterDetailItem();
								item.date = reading.Date.ToString("MM/dd/yyyy hh:mm tt");
								item.firstItem = Math.Round((decimal)reading.weight.EnglishValue, 1) + "/" + Math.Round((decimal)reading.weight.MetricValue, 1);
								item.secondItem = Math.Round((decimal)reading.bmi.EnglishValue, 1).ToString();
								item.categoryId = reading.weight.CategoryId;
								data.Add(item);
							}
							itemList.ItemsSource = data;
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
							// Glucose data list
							//var sortedTemps = allCategoryReading.Reverse();

							foreach (var reading in allCategoryReading)
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
			finally
			{
				layoutLoading.IsVisible = false;
			}

			this.itemList.HeightRequest = this.Content.Bounds.Size.Height - 110;
		}
	}
}
