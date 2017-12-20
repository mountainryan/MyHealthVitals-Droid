using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using Xamarin.Forms;
using System.Diagnostics;

namespace MyHealthVitals
{
	public partial class RespDataListPagePad : ContentPage
	{
		ObservableCollection<SpirometerReading> spirometerReadingList = new ObservableCollection<SpirometerReading>();
		public RespDataListPagePad()
		{
            NavigationPage.SetHasNavigationBar(this, false);
			InitializeComponent();

			FakeToolbar.HeightRequest = 75 * Screensize.heightfactor;
			titlebtn.FontSize = 30 * Screensize.heightfactor;
            backbtn.FontSize = 30 * Screensize.heightfactor;
            label.FontSize = 45 * Screensize.heightfactor;

			CallAPiGetReadings();
		}

		void btnPrevClicked(object sender, System.EventArgs e)
		{
			Navigation.PopAsync();
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

				await Task.Delay(1).ContinueWith(_ =>
				{
					//PushData(e);
					if (ParametersPageLocal.allReadings == null)
					{
						int index = Task.WaitAny(Task_vars.tasks);
					}

				});

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
				//System.Diagnostics.Debug.WriteLine("exception occured in the api call or parsing result");
			}

			finally
			{
				layoutLoading.IsVisible = false;
			}
            //Debug.WriteLine("font size of Most Recent Readings = " + label.FontSize);
		}
	}
}


