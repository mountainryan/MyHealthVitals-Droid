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
		public ParameterItemDetail()
		{
			InitializeComponent();

			//headerWithTwoTitle header = new headerWithTwoTitle("20/20/20");
			//header.l

			headerWithTwoTitle header = new headerWithTwoTitle("DIA", "SYS");

			headerContainer.Children.Add(header);
		}

		public ObservableCollection<ParameterDetailItem> readingBP = new ObservableCollection<ParameterDetailItem>();

		protected override void OnAppearing()
		{
			base.OnAppearing();

			//var allReadings = await Reading.GetAllReadingsFromService();

			//var allBpReadings = from reading in allReadings
			//					where reading.CategoryId == 1
			//					select reading;

			//var bpReadings = from spSet in
			//   (from reading in allBpReadings
			//	group reading by reading.Date)
			//				 orderby spSet.Key descending
			//				 let dia = spSet.FirstOrDefault(x => x.ValueType == "Diastolic")
			//				 let sys = spSet.FirstOrDefault(x => x.ValueType == "Systolic") where sys != null && dia != null
			//				 select new
			//				 {
			//					 Date = spSet.Key,
			//					 sys = sys,
			//					 dia = dia,
			//				 };

			//var newBpReadings = (bpReadings.GroupBy(s => s.Date).Select(grp => grp.First())).ToArray();

			//foreach (var reading in newBpReadings)
			//{
			//	readingBP.Add(new BloodPressure(reading.Date,(int)reading.sys.EnglishValue, (int)reading.dia.EnglishValue));
			//}

			var newItem = new ParameterDetailItem();
			newItem.date = "10/14/1990 7:30 PM";
			newItem.firstItem = "190";

			var newItem1 = new ParameterDetailItem();
			newItem1.date = "10/14/1990 7:30 PM";
			newItem1.firstItem = "190";

			readingBP.Add(newItem);
			readingBP.Add(newItem1);

			itemList.ItemsSource = readingBP;
		}
	}
}
