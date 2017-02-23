using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public partial class RespCalibrationPage : ContentPage
	{
		async void btnSaveCLicked(object sender, System.EventArgs e)
		{
			layoutLoading.IsVisible = true;

			try
			{
				SpirometerReading highestReading = getHighestReading();

				// saving it to the local storage
				Demographics.sharedInstance.calibratedReading = highestReading;
				Demographics.sharedInstance.saveCalibratedReadig();

				Reading fevReading = new Reading("FEV1", highestReading.Fev1, 9);
				Reading pefReading = new Reading("PEF", highestReading.Pef, 9);

				await pefReading.PostReadingToService();
				await fevReading.PostReadingToService();
				await this.Navigation.PopAsync();
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Exception occured while savind the pef and fev to server");
			}
			finally
			{
				layoutLoading.IsVisible = false;
			}
		}

		ObservableCollection<SpirometerReading> calibratedReadingList = new ObservableCollection<SpirometerReading>();
		//SpirometerReading highestReading;

		void DeleteClicked(object sender, System.EventArgs e)
		{
			var btn = (Xamarin.Forms.Button)sender;

			calibratedReadingList.RemoveAt((int)btn.CommandParameter);

			//System.Diagnostics.Debug.WriteLine("the index: " + ;
		}

		//public void DeleteClicked(object sender, EventArgs e)
		//{
		//	//var item = (Xamarin.Forms.Button)sender;
		//	//Item listitem = (from itm in allItems
		//	//				 where itm.ItemName == item.CommandParameter.ToString()
		//	//				 select
		//	//itm).FirstOrDefault<Item>(); 
		//	//allItems.Remove(listitem);
		//}

		private SpirometerReading getHighestReading() {


			SpirometerReading highestReading = calibratedReadingList[0];

			foreach (var redn in calibratedReadingList) {
				if (highestReading.Pef < redn.Pef) {
					highestReading = redn;
				}
			}

			return highestReading;
		}


		public RespCalibrationPage()
		{
			InitializeComponent();



			var itm = new SpirometerReading(DateTime.Now, 456, 3.5m);
			itm.index = 0;

			var itm1 = new SpirometerReading(DateTime.Now, 789, 3.5m);
			itm1.index = 1;

			var itm2 = new SpirometerReading(DateTime.Now, 425, 3.5m);
			itm2.index = 2;

			var itm3 = new SpirometerReading(DateTime.Now, 452, 3.5m);
			itm3.index = 3;

			calibratedReadingList.Add(itm);
			calibratedReadingList.Add(itm1);
			calibratedReadingList.Add(itm2);
			calibratedReadingList.Add(itm3);

			listView.ItemsSource = calibratedReadingList;
		}
	}
}
