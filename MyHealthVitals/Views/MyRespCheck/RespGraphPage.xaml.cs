using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public partial class RespGraphPage : ContentPage
	{
		void btnNextClicked(object sender, System.EventArgs e)
		{
			// next
			if (currentIndex > 0)
			{
				renderCurrentSpirometer(spirometerReadingList[--currentIndex]);
			}
		}

		void btnPrevClicked(object sender, System.EventArgs e)
		{
			if (currentIndex < spirometerReadingList.Count - 1)
			{
				renderCurrentSpirometer(spirometerReadingList[++currentIndex]);
			}
		}

		private void renderCurrentSpirometer(SpirometerReading currReading) {

			try
			{
				Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
				{
					lblPef.Text = currReading.pefString;
					lblFev1.Text = currReading.fev1String;
					lblDate.Text = currReading.dateString;
					boxFev.HeightRequest = layoutFevContainer.Height * (double)currReading.Fev1 / 9;
					boxPef.HeightRequest = layoutPefContainer.Height * (double)currReading.Pef / 900;

					boxFev.BackgroundColor = Color.FromHex(currReading.color);
					boxPef.BackgroundColor = Color.FromHex(currReading.color);
				});
			}
			catch{
				
			}
		}

		int currentIndex = 0;

		ObservableCollection<SpirometerReading> spirometerReadingList = new ObservableCollection<SpirometerReading>();
		public RespGraphPage()
		{
			InitializeComponent();
			CallAPiGetReadings();
			layoutContainer.IsVisible = false;
		}

		public async void CallAPiGetReadings()
		{
			layoutLoading.IsVisible = true;

			try
			{
				var allReadings = await Reading.GetAllReadingsFromService();

				var allCategoryReading = from reading in allReadings
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

				//currentIndex = spirometerReadingList.Count - 1;
				currentIndex = 0;
				renderCurrentSpirometer(spirometerReadingList[currentIndex]);
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("exception occured in the api call or parsing result");
			}

			finally {
				layoutLoading.IsVisible = false;
				layoutContainer.IsVisible = true;
			}
		}
	}
}
