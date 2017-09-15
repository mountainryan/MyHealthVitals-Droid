using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
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
		Reading[] allReadings;

		public ParameterItemDetail(long CategoryId)
		{
			this.categoryId = CategoryId;
			InitializeComponent();


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
						this.Title = "Sp2 Data List";
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
				case 10:
					{
						this.Title = "ECG Data List";
						headerContainer.Children.Add(new headerWithTwoTitle("ECG Result","Report Status"));
						break;
					}
			}

			callApi();
		}

		public async void callApi()
		{
			layoutLoading.IsVisible = true;

			try
			{
				Debug.WriteLine("LIST");
				var allReadings = await Reading.GetAllReadingsFromService();
				Debug.WriteLine("call api  all reading = " + allReadings.Length);
				var allCategoryReading = from reading in allReadings
										 where reading.CategoryId == categoryId
										 select reading;

				//allCategoryReading = allCategoryReading.GroupBy(s => s.Date);
				Debug.WriteLine("categoryID = " + categoryId);
				switch (categoryId)
				{
					// Blood pressure
					case 1:
					//	case 10:
						{
							//		categoryId = 1;
							Debug.WriteLine("BP START");
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
							Debug.WriteLine("END data= " +data.Count());

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
											   let SpO2 = spSet.FirstOrDefault(x => x.CategoryId == 2)
											   let Pulse = spSet.FirstOrDefault(x => (x.CategoryId == 3))
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

							var weightBmiReading = (from spSet in
							   (from reading in allCategoryReading5
								group reading by reading.Date)
													orderby spSet.Key descending
													let weight = spSet.FirstOrDefault(x => x.CategoryId == 5)
													let bmi = spSet.FirstOrDefault(x => x.CategoryId == 7)
													where weight != null
													select new
													{
														Date = spSet.Key,
														weight = weight,
														bmi = bmi,
													});//.Take(1);

//where weight != null && bmi != null

							var newWeightBmiReading = (weightBmiReading.GroupBy(s => s.Date).Select(grp => grp.First())).ToArray();
							Debug.WriteLine("newWeightBmiReading = " + newWeightBmiReading);
							foreach (var reading in newWeightBmiReading)
							{
								var item = new ParameterDetailItem();
								item.date = reading.Date.ToString("MM/dd/yyyy hh:mm tt");
								item.firstItem = Math.Round((decimal)reading.weight.EnglishValue, 1) + "/" + Math.Round((decimal)reading.weight.MetricValue, 1);
								Debug.WriteLine("firstItem = " + item.firstItem);
								if (reading.bmi != null)
								{
										item.secondItem = Math.Round((decimal)reading.bmi.EnglishValue, 1).ToString();
								}else
								{
										item.secondItem = null;
								}
								item.categoryId = reading.weight.CategoryId;
								data.Add(item);
							}
							itemList.ItemsSource = data;
							break;
						}
					case 6:
							break;
					case 7:
							break;
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
						break;
					case 10:
						int count = 0;
						foreach (var reading in allCategoryReading)
						{
							var item = new ParameterDetailItem();
							item.date = reading.Date.ToString("MM/dd/yyyy hh:mm tt");
					
							if (count < 30)
							{
								var fileName = Regex.Replace(item.date, @"\s+", "");
								fileName = Regex.Replace(fileName, @"[/:]+", "");
								bool ret = DependencyService.Get<IFileHelper>().checkFileExist(fileName + ".txt");
								if (ret)
								{
									item.secondItem = "No Report";
								}
								else if (DependencyService.Get<IFileHelper>().checkFileExist(fileName + "ECG.pdf"))
								{
									//count++;
									item.secondItem = "Saved";
								}
								else
								{
									item.secondItem = "Emailed";
								}
								count++;
							}
							else {
							//	DependencyService.Get<IFileHelper>()	
							}
							item.firstItem = reading.EnglishValue == 0 ? "Normal" : "Abnormal";
							item.categoryId = reading.CategoryId;
							data.Add(item);
						}

						itemList.ItemsSource = data;
						break;
					default:
						break;
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
