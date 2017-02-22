using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public partial class RespDataListPage : ContentPage
	{
		ObservableCollection<SpirometerReading> spirometerReadingList = new ObservableCollection<SpirometerReading>();
		public RespDataListPage()
		{
			InitializeComponent();
			//SpirometerReading rdn = new SpirometerReading();
			//rdn.Date = DateTime.Now;
			//rdn.Pef = 345;
			//rdn.Fev1 = 3.5M;
			//rdn.color = "#FFA500";

			//SpirometerReading rdn1 = new SpirometerReading();
			//rdn1.Date = DateTime.Now;
			//rdn1.Pef = 345;
			//rdn1.Fev1 = 3.5M;
			//rdn1.color = "Blue";

			//readinds.Add(rdn);
			//readinds.Add(rdn1);

			//listView.ItemsSource = readinds;

			CallAPiGetReadings();
		}

		public async void CallAPiGetReadings() {

			var allReadings = await Reading.GetAllReadingsFromService();

			var allCategoryReading = from reading in allReadings
									 where reading.CategoryId == 9
									 select reading;

			//var bpReadings = from spSet in
			//				   (from reading in allCategoryReading
			//					group reading by reading.Date)
			//				 orderby spSet.Key descending
			//				 let dia = spSet.FirstOrDefault(x => x.ValueType == "DIA" || x.ValueType == "Diastolic")
			//				 let sys = spSet.FirstOrDefault(x => x.ValueType == "SYS" || x.ValueType == "Systolic")

			//				 where sys != null && dia != null
			//				 select new
			//				 {
			//					 Date = spSet.Key,
			//					 sys = sys,
			//					 dia = dia,
			//				 };

			var spReadings = from spSet in
			   (from reading in allCategoryReading
				group reading by reading.Date)
							 orderby spSet.Key descending
							 let pef = spSet.FirstOrDefault(x => x.ValueType == "PEF")
							 let fev1 = spSet.FirstOrDefault(x => x.ValueType == "FEV1")
							 where pef != null && fev1 != null
							 select new
							 {
								 Date = spSet.Key,
								 PEF = pef,
								 FEV1 = fev1,
							 };

			//var newBpReadings = (bpReadings.GroupBy(s => s.Date).Select(grp => grp.First())).ToArray();

			//foreach (var reading in newBpReadings)
			//{
			//	var item = new ParameterDetailItem();
			//	item.date = reading.Date.ToString("MM/dd/yyyy hh:mm tt");
			//	item.firstItem = ((int)reading.sys.EnglishValue).ToString();
			//	item.secondItem = ((int)reading.dia.EnglishValue).ToString();
			//	item.categoryId = reading.sys.CategoryId;
			//	data.Add(item);
			//}
			//itemList.ItemsSource = data;

			//var allReadings = Reading.GetAllReadingsFromService();

			//var spiroReadings = from reading in allReadings
			//					where reading.CategoryId == 9
			//					select reading;

			///////////////
			//SpirometerReadingList = new List<SpirometerReading>();

			//var spReadings = from spSet in
			//   (from reading in spiroReadings
			//	group reading by reading.Date)
			//				 orderby spSet.Key descending
			//				 let pef = spSet.FirstOrDefault(x => x.ValueType == "PEF")
			//				 let fev1 = spSet.FirstOrDefault(x => x.ValueType == "FEV1")
			//				 where pef != null && fev1 != null
			//				 select new
			//				 {
			//					 Date = spSet.Key,
			//					 PEF = pef,
			//					 FEV1 = fev1,
			//				 };

			var newSPreadings = (spReadings.GroupBy(s => s.Date).Select(grp => grp.First())).ToArray();

			foreach (var reading in newSPreadings)
			{
				//spirometerReadingList.Add(new SpirometerReading
				//{
				//	Date = reading.Date,
				//	Pef = Convert.ToDouble(reading.PEF.EnglishValue),
				//	Fev1 = Convert.ToDouble(reading.FEV1.EnglishValue)
				//});

				SpirometerReading rdn = new SpirometerReading(reading.PEF.Date,(Decimal)reading.PEF.EnglishValue,(Decimal)reading.FEV1.EnglishValue);
				spirometerReadingList.Add(rdn);

			}

			listView.ItemsSource = spirometerReadingList;
			///////////////

			//ReadingTableView.Source = new DataListSource(SpirometerReadingList);
			//ReadingTableView.ReloadData();
		}
	}
}
