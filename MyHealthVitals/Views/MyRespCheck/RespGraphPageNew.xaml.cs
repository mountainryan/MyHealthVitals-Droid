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
				layoutPefContainer.WidthRequest = 300 * Screensize.widthfactor;
                layoutPefContainer.HeightRequest = 800 * Screensize.heightfactor;
				layoutFevContainer.WidthRequest = 300 * Screensize.widthfactor;
				layoutFevContainer.HeightRequest = 800 * Screensize.heightfactor;
				layoutL2.WidthRequest = 120 * Screensize.widthfactor;
				layoutL1.WidthRequest = 120 * Screensize.widthfactor;
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
				FakeToolbar.HeightRequest = 55 * Screensize.heightfactor;
				titlebtn.FontSize = 24 * Screensize.heightfactor;
                backbtn.FontSize = 24 * Screensize.heightfactor;

				layoutPefContainer.WidthRequest *= Screensize.widthfactor;
				layoutPefContainer.HeightRequest *= Screensize.heightfactor;
				layoutFevContainer.WidthRequest *= Screensize.widthfactor;
				layoutFevContainer.HeightRequest *= Screensize.heightfactor;
				layoutL2.WidthRequest *= Screensize.widthfactor;
				layoutL1.WidthRequest *= Screensize.widthfactor;
				layoutN2.Spacing = (layoutPefContainer.HeightRequest - 74) / 3;
				layoutN1.Spacing = (layoutPefContainer.HeightRequest - 74) / 3;
				lblPef.FontSize *= Screensize.heightfactor;
				lblFev1.FontSize *= Screensize.heightfactor;
				lblDate.FontSize *= Screensize.heightfactor;
				lblDate.WidthRequest *= Screensize.widthfactor;
				peflabel.FontSize *= Screensize.heightfactor;
				fevlabel.FontSize *= Screensize.heightfactor;
				peflabel.Margin = new Thickness(0, -25 * Screensize.heightfactor, 0, 0);
				fevlabel.Margin = new Thickness(0, -25 * Screensize.heightfactor, 0, 0);

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
