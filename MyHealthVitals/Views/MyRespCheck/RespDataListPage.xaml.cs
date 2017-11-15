using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public partial class RespDataListPage : ContentPage
	{
		
		ObservableCollection<SpirometerReading> spirometerReadingList = new ObservableCollection<SpirometerReading>();
		public RespDataListPage()
		{
            NavigationPage.SetHasNavigationBar(this, false);
			InitializeComponent();
			FakeToolbar.Children.Add(
				backarrow,
				// Adds the Button on the top left corner, with 10% of the navbar's width and 100% height
				new Rectangle(0, 0.5, 0.1, 1),
				// The proportional flags tell the layout to scale the value using [0, 1] -> [0%, 100%]
				AbsoluteLayoutFlags.HeightProportional | AbsoluteLayoutFlags.WidthProportional
				);

			FakeToolbar.Children.Add(
				backbtn,
				// Using 0.5 will center it and the layout takes the size of the element into account
				// 0.5 will center, 1 will right align
				// Adds in the center, with 90% of the navbar's width and 100% of height
				new Rectangle(0.1, 0.5, 0.3, 1),
				AbsoluteLayoutFlags.All
			);
			FakeToolbar.Children.Add(
				titlebtn,
				// Using 0.5 will center it and the layout takes the size of the element into account
				// 0.5 will center, 1 will right align
				// Adds in the center, with 90% of the navbar's width and 100% of height
				new Rectangle(0.6, 0.5, 0.5, 1),
				AbsoluteLayoutFlags.All
			);
			FakeToolbar.HeightRequest = 55 * Screensize.heightfactor;
			titlebtn.FontSize = 16 * Screensize.heightfactor;
			backbtn.FontSize = 16 * Screensize.heightfactor;

            label.FontSize = 30 * Screensize.heightfactor;
            Date.FontSize = 16 * Screensize.heightfactor;
            PEF.FontSize = 16 * Screensize.heightfactor;
            FEV.FontSize = 16 * Screensize.heightfactor;
            //Dateval.FontSize *= Screensize.heightfactor;
            //PEFval.FontSize *= Screensize.heightfactor;
            //FEVval.FontSize *= Screensize.heightfactor;
            Date.WidthRequest = (370 * Screensize.widthfactor) / 2;
            PEF.WidthRequest  = (370 * Screensize.widthfactor) / 4.111111;
            FEV.WidthRequest = (370 * Screensize.widthfactor) / 5.285714;
			//Dateval.WidthRequest *= Screensize.widthfactor;
			//PEFval.WidthRequest *= Screensize.widthfactor;
			//FEVval.WidthRequest *= Screensize.widthfactor;

			//label.FontSize *= Screensize.heightfactor;
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
				System.Diagnostics.Debug.WriteLine("exception occured in the api call or parsing result");
			}

			finally
			{
				layoutLoading.IsVisible = false;
			}
		}
	}
}


