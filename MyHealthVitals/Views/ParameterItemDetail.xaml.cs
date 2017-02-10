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

			//itemList.ItemTapped += (object sender, ItemTappedEventArgs e) =>
			//{
			//	// don't do anything if we just de-selected the row
			//	if (e.Item == null) return;
			//	// do something with e.SelectedItem
			//	((ListView)sender).SelectedItem = null; // de-select the row
			//};

			//headerWithTwoTitle header = new headerWithTwoTitle("DIA", "SYS");

			switch (categoryId) {
				case 1:
				case 2: {
						headerWithTwoTitle header = new headerWithTwoTitle("DIA", "SYS");
						headerContainer.Children.Add(header);
						break;
					}
				case 4: {
						headerWithOneTitle header = new headerWithOneTitle("Temperature");
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

				var allReading = from reading in allReadings
								 where reading.CategoryId == categoryId
								 select reading;

				switch (categoryId)
				{
					// Blood pressure
					case 1:
						{
							var bpReadings = from spSet in
							   (from reading in allReading
								group reading by reading.Date)
											 orderby spSet.Key descending
											 let dia = spSet.FirstOrDefault(x => x.ValueType == "Diastolic")
											 let sys = spSet.FirstOrDefault(x => x.ValueType == "Systolic")
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
								item.firstItem = ((int)reading.dia.EnglishValue).ToString();
								item.secondItem = ((int)reading.sys.EnglishValue).ToString();

								data.Add(item);
							}
							itemList.ItemsSource = data;
							break;
						}
					case 2:
						{
							var bpReadings = from spSet in
							   (from reading in allReading
								group reading by reading.Date)
											 orderby spSet.Key descending
											 let dia = spSet.FirstOrDefault(x => x.ValueType == "Diastolic")
											 let sys = spSet.FirstOrDefault(x => x.ValueType == "Systolic")
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
								item.firstItem = ((int)reading.dia.EnglishValue).ToString();
								item.secondItem = ((int)reading.sys.EnglishValue).ToString();

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
							foreach (var reading in allReading)
							{
								var item = new ParameterDetailItem();
								item.date = reading.Date.ToString("MM/dd/yyyy hh:mm tt");
								item.firstItem = ((int)reading.EnglishValue).ToString();

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
		}
	}
}
