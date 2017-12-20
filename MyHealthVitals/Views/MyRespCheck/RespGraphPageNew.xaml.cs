using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;
using System.Diagnostics;
using System.Threading.Tasks;

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
            NavigationPage.SetHasNavigationBar(this, false);
			InitializeComponent();
			if (Device.Idiom == TargetIdiom.Tablet) {
                layoutPefContainer.WidthRequest = (600 * Screensize.widthfactor) / 2;
                layoutPefContainer.HeightRequest = 800 * Screensize.heightfactor;
				layoutFevContainer.WidthRequest = (600 * Screensize.widthfactor) / 2;
				layoutFevContainer.HeightRequest = 800 * Screensize.heightfactor;
                layoutL2.WidthRequest = (240 * Screensize.widthfactor) / 2;
				layoutL1.WidthRequest = (240 * Screensize.widthfactor) / 2;
                layoutN2.Spacing = (layoutPefContainer.HeightRequest - 74) / 3;
				layoutN1.Spacing = (layoutPefContainer.HeightRequest - 74) / 3;
                lblPef.FontSize = 30 * Screensize.heightfactor;
                lblFev1.FontSize = 30 * Screensize.heightfactor;
                lblDate.FontSize = 34 * Screensize.heightfactor;
                lblDate.WidthRequest = 475 * Screensize.widthfactor;
                peflabel.FontSize = 30 * Screensize.heightfactor;
                fevlabel.FontSize = 30 * Screensize.heightfactor;

                prevbtn.Image = "imgPrevBlue_pad.png";
                nextbtn.Image = "imgNextBlue_pad.png";
                prevbtn.WidthRequest = 103;
                prevbtn.HeightRequest = 103;
                nextbtn.WidthRequest = 103;
                nextbtn.HeightRequest = 103;
                // = "0,-25,0,0"
                peflabel.Margin = new Thickness(0, -50 * Screensize.heightfactor, 0, 0);
                fevlabel.Margin = new Thickness(0, -50 * Screensize.heightfactor, 0, 0);
				//prevcont.Margin = new Thickness(15, -100 * Screensize.heightfactor, 0, 0);


				FakeToolbar.HeightRequest = 75 * Screensize.heightfactor;
				titlebtn.FontSize = 30 * Screensize.heightfactor;
                backbtn.FontSize = 30 * Screensize.heightfactor;
                minilayoutcont.Margin = new Thickness(0, 80*Screensize.heightfactor, 0, 0);
			}
            else if (Device.Idiom == TargetIdiom.Phone)
            {
				FakeToolbar2.Children.Add(
				backarrow2,
				// Adds the Button on the top left corner, with 10% of the navbar's width and 100% height
				new Rectangle(0, 0.5, 0.1, 1),
				// The proportional flags tell the layout to scale the value using [0, 1] -> [0%, 100%]
				AbsoluteLayoutFlags.HeightProportional | AbsoluteLayoutFlags.WidthProportional
				);

				FakeToolbar2.Children.Add(
					backbtn2,
					// Using 0.5 will center it and the layout takes the size of the element into account
					// 0.5 will center, 1 will right align
					// Adds in the center, with 90% of the navbar's width and 100% of height
					new Rectangle(0.1, 0.5, 0.3, 1),
					AbsoluteLayoutFlags.All
				);
				FakeToolbar2.Children.Add(
					titlebtn2,
					// Using 0.5 will center it and the layout takes the size of the element into account
					// 0.5 will center, 1 will right align
					// Adds in the center, with 90% of the navbar's width and 100% of height
					new Rectangle(0.6, 0.5, 0.5, 1),
					AbsoluteLayoutFlags.All
				);
				FakeToolbar.IsVisible = false;
				FakeToolbar2.IsVisible = true;
				FakeToolbar2.HeightRequest = 55 * Screensize.heightfactor;
				titlebtn2.FontSize = 16 * Screensize.heightfactor;
				backbtn2.FontSize = 16 * Screensize.heightfactor;

                layoutPefContainer.WidthRequest = (240 * Screensize.widthfactor) / 2;
				layoutPefContainer.HeightRequest = 400 * Screensize.heightfactor;
				layoutFevContainer.WidthRequest = (240 * Screensize.widthfactor) / 2;
				layoutFevContainer.HeightRequest = 400 * Screensize.heightfactor;
                layoutL2.WidthRequest = (120 * Screensize.widthfactor) / 2;
				layoutL1.WidthRequest = (120 * Screensize.widthfactor) / 2;
				layoutN2.Spacing = (layoutPefContainer.HeightRequest - 74) / 3;
				layoutN1.Spacing = (layoutPefContainer.HeightRequest - 74) / 3;
				lblPef.FontSize = 15 * Screensize.heightfactor;
				lblFev1.FontSize = 15 * Screensize.heightfactor;
				lblDate.FontSize = 15 * Screensize.heightfactor;
				lblDate.WidthRequest = 156 * Screensize.widthfactor;
				peflabel.FontSize = 15 * Screensize.heightfactor;
				fevlabel.FontSize = 15 * Screensize.heightfactor;
				peflabel.Margin = new Thickness(0, -25 * Screensize.heightfactor, 0, 0);
				fevlabel.Margin = new Thickness(0, -25 * Screensize.heightfactor, 0, 0);

                minilayoutcont.Margin = new Thickness(0, 60 * Screensize.heightfactor, 0, 0);

                //prevcont.Margin = new Thickness(15, -100 * Screensize.heightfactor, 0, 0);
            }


            CallAPiGetReadings();
				//layoutContainer.IsVisible = false;
		}
		void btnBackClicked(object sender, System.EventArgs e)
		{
			Navigation.PopAsync();
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
						double height = 300 * Screensize.heightfactor;
						if (Device.Idiom == TargetIdiom.Tablet) {
							height = 800 * Screensize.heightfactor;
						}
						boxFev.HeightRequest = height * (double)currReading.Fev1 / 9;
						boxPef.HeightRequest = height * (double)currReading.Pef / 900;
					}
					else
					{
                        //Debug.WriteLine("layoutFevContainer.Height = "+layoutFevContainer.Height);
                        //Debug.WriteLine("layoutFevContainer.HeightRequest = " + layoutFevContainer.HeightRequest);
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
				await Task.Delay(1).ContinueWith(_ =>
				{
					if (ParametersPageLocal.allReadings == null)
					{
						int index = Task.WaitAny(Task_vars.tasks);
						//ParametersPageLocal.allReadings = await Reading.GetAllReadingsFromService();
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

				//currentIndex = spirometerReadingList.Count - 1;
				currentIndex = 0;
				renderCurrentSpirometer(spirometerReadingList[currentIndex]);
			}
			catch
			{
				//System.Diagnostics.Debug.WriteLine("exception occured in the api call or parsing result");
			}

			finally
			{
				layoutLoading.IsVisible = false;
				layoutContainer.IsVisible = true;
			}
		}
	}
}
