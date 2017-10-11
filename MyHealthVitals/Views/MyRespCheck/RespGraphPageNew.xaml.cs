using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public partial class RespGraphPageNew : ContentPage
	{
		int currentIndex = 0;
		ObservableCollection<SpirometerReading> spirometerReadingList = new ObservableCollection<SpirometerReading>();
		void Handle_Clicked(object sender, System.EventArgs e)
		{
			throw new NotImplementedException();
		}

		public RespGraphPageNew()
		{
			InitializeComponent();
			if (Device.Idiom == TargetIdiom.Tablet) {
				layoutPefContainer.WidthRequest *= 2;
				layoutPefContainer.HeightRequest *= 2;
				layoutFevContainer.WidthRequest *= 2;
				layoutFevContainer.HeightRequest *= 2;
				layoutL2.WidthRequest *= 2;
				layoutL1.WidthRequest *= 2;
				layoutN2.Spacing *= 2;
				layoutN1.Spacing *= 2;
			}


            CallAPiGetReadings();
				//layoutContainer.IsVisible = false;
		}
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

		private void renderCurrentSpirometer(SpirometerReading currReading)
		{

			try
			{
				Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
				{
					lblPef.Text = currReading.pefString;
					lblFev1.Text = currReading.fev1String;
					lblDate.Text = currReading.dateString;

					if (layoutFevContainer.Height < 0 || layoutPefContainer.Height < 0)
					{
						int height = 300;
						if (Device.Idiom == TargetIdiom.Tablet) {
							height *= 2;
						}
						boxFev.HeightRequest = height * (double)currReading.Fev1 / 9;
						boxPef.HeightRequest = height * (double)currReading.Pef / 900;
					}
					else
					{
						boxFev.HeightRequest = layoutFevContainer.Height * (double)currReading.Fev1 / 9;
						boxPef.HeightRequest = layoutPefContainer.Height * (double)currReading.Pef / 900;
					}
					boxFev.BackgroundColor = Color.FromHex(currReading.color);
					boxPef.BackgroundColor = Color.FromHex(currReading.color);
				});
			}
			catch
			{

			}
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
				//	var allReadings = await Reading.GetAllReadingsFromService();

				if (ParametersPageLocal.allReadings == null)
				{
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

				//currentIndex = spirometerReadingList.Count - 1;
				currentIndex = 0;
				renderCurrentSpirometer(spirometerReadingList[currentIndex]);
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("exception occured in the api call or parsing result");
			}

			finally
			{
				layoutLoading.IsVisible = false;
				layoutContainer.IsVisible = true;
			}
		}
	}
}
