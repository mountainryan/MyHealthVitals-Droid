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
			CallAPiGetReadings();
		}

		public async void CallAPiGetReadings()
		{
			layoutLoading.IsVisible = true;

			try
			{
				if (logcalParameteritem.localspirometerList != null && logcalParameteritem.localspirometerList.Count > 0)
				{ 
					foreach (var item in logcalParameteritem.localspirometerList) 
					{
						spirometerReadingList.Add(item);
					}
				}
				if (ParametersPageLocal.allReadings == null) { 
					ParametersPageLocal.allReadings = await Reading.GetAllReadingsFromService();
				}


				var allCategoryReading = from reading in ParametersPageLocal.allReadings 
										 where reading.CategoryId == 9
										 select reading;

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

				var newSPreadings = (spReadings.GroupBy(s => s.Date).Select(grp => grp.First())).ToArray();

				foreach (var reading in newSPreadings)
				{
					SpirometerReading rdn = new SpirometerReading(reading.PEF.Date, (Decimal)reading.PEF.EnglishValue, (Decimal)reading.FEV1.EnglishValue);
					spirometerReadingList.Add(rdn);
				}

				listView.ItemsSource = spirometerReadingList;
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("exception occured in the api call or parsing result");
			}

			finally
			{
				layoutLoading.IsVisible = false;
			}
		}
	}
}


