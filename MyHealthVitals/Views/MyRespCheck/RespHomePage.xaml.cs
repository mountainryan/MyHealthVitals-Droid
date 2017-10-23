using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;
using System.Diagnostics;

namespace MyHealthVitals
{
	public partial class RespHomePage : ContentPage, BLEReadingUpdatableSpiroMeter
	{
		SpirometerReading currReading;
		//BleManagerSpirometer bleManager = new BleManagerSpirometer();

		public RespHomePage()
		{
			InitializeComponent();
			if (Device.Idiom == TargetIdiom.Tablet)
			{
				layoutContainer.Spacing *= 2;
				imgProfile.WidthRequest *= 2;
				imgProfile.HeightRequest *= 2;

				lblName.FontSize *= 1.5;
				lblEmail.FontSize *= 1.5;
				lblClickMessage.FontSize *= 1.5;

				lbldata.WidthRequest *= 2;
				/*	lblpef.WidthRequest *= 2;
					lblfev.WidthRequest *= 2;
					lblPefReading.WidthRequest *= 2;
					lblFevReading.WidthRequest *= 2;
					lblDateReading.WidthRequest *= 2;
				*/
				lblfev.FontSize *= 1.5;
				lblpef.FontSize *= 1.5;
				lbldata.FontSize *= 1.5;
				lblPefReading.FontSize *= 1.5;
				lblFevReading.FontSize *= 1.5;
				lblDateReading.FontSize *= 1.5;

				layoutButton1.Spacing *= 4;
				layoutButton1.Margin = new Thickness(45, 0, 40, 0);
				B1.FontSize *= 1.5;
				B2.FontSize *= 1.5;
				B1.WidthRequest *= 2;
				B2.WidthRequest *= 2;
				B1.HeightRequest *= 2;
				B2.HeightRequest *= 2;

				layoutMiddle.HeightRequest *= 2;
				box.HeightRequest *= 2;
				layoutbox.HeightRequest *= 2;
				layoutbox.Spacing *= 2;


				lablebox.FontSize *= 1.5;
				layoutval.Spacing *= 2;
				lblDate.FontSize *= 1.5;
				lblPef.FontSize *= 1.5;
				lblFev1.FontSize *= 1.5;

				layoutButton2.Margin = new Thickness(45, 0, 40, 0);
				layoutButton2.Spacing *= 4;
				B3.WidthRequest *= 2;
				B4.WidthRequest *= 2;
				B3.HeightRequest *= 2;
				B4.HeightRequest *= 2;
				B3.FontSize *= 1.5;
				B4.FontSize *= 1.5;

				save.FontSize *= 1.5;
				save.WidthRequest = box.WidthRequest;
				save.HeightRequest *= 1.5;
				save.Margin = new Thickness(45, 0, 40, 0);

				//icupng.WidthRequest *= 1.5;
				//icupng.HeightRequest *= 1.5;
				iculbl.WidthRequest *= 1.5;
				iculbl.FontSize *= 2;

				//RelativeLayout.XConstraint="{ConstraintExpression Type=RelativeToParent, Property=Width, Factor=1,Constant = -40}" HeightRequest="30" WidthRequest="30"

				btncancelread.WidthRequest *= 2;
				btncancelread.HeightRequest *= 2;
				btncancelread.Image = "deleteicon_x2.png";

				icupng.Source = "icucarellc_pad.png";

				gifWebView.HeightRequest *= 2;
				gifWebView.WidthRequest *= 2;
				var html = new HtmlWebViewSource();
				html.BaseUrl = DependencyService.Get<IBaseUrl>().Get() + "gifContainer_pad.html";
				gifWebView.Source = html.BaseUrl;
			}
			else 
			{
				save.WidthRequest = box.WidthRequest;
				var html = new HtmlWebViewSource();
				html.BaseUrl = DependencyService.Get<IBaseUrl>().Get() + "gifContainer.html";
				gifWebView.Source = html.BaseUrl;
			}


			var tapGestureRecognizer = new TapGestureRecognizer();
			tapGestureRecognizer.Tapped += (s, e) =>
			{
				var newPage = new UserProfile();
				newPage.Title = "My Account";
				this.Navigation.PushAsync(newPage);
			};

			imgProfile.GestureRecognizers.Add(tapGestureRecognizer);

			//gifWebView.Source = DependencyService.Get<IBaseUrl>().Get() + "/gifContainer.html";
			//gifWebView.Source = "http://gifimage.net/wp-content/uploads/2017/02/Loading-GIF-Image-14.gif";

			//this one looks good, but the background is gray	
			//gifWebView.Source = "https://m.popkey.co/163fce/Llgbv_s-200x150.gif";

			//I like this one, but it's off center
			//gifWebView.Source = "http://vignette2.wikia.nocookie.net/animaljam/images/4/42/Loading.gif/revision/latest?cb=20140911124847";


			//string gifpath = "";
			//Debug.WriteLine("gif file path: "+Task_vars.gifpath);
			//string html_str = "<html><head><title>Xamarin Forms</title></head><body style=\"back\">< div class=\"loader\" style=\"position: fixed;  left: 0px;  top: 0px;  width: 100%;  height: 100%;  z-index: 9999;  background: url('"+Task_vars.gifpath+"') 20% 20% no-repeat rgb(0,0,0);\"></div>  </body></html>";
			//gifWebView.Source = Task_vars.gifpath;


			callAPiToDisplayGetDemographics();


	   }
		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			BLECentralManager.sharedInstance.spiroServHandler.stopPolling();
		}
		async public void testAgainDialog() {
           await DisplayAlert("Reading", "The FEV value is too low, please take reading again.", "OK");
		//	BLECentralManager.sharedInstance.spiroServHandler.stopPolling();

			BLECentralManager.sharedInstance.connectToDevice("BLE-MSA", this);

		}

		// call back methods
		public void updateCaller(SpirometerReading reading)
		{
			currReading = reading;
            Debug.WriteLine("Normal reading.");

			if (reading !=null)
			{
				this.layoutLoading.IsVisible = false;
				//this.currReading = new SpirometerReading(DateTime.Now, pef, fev1);

				Device.BeginInvokeOnMainThread(() =>
				{
					lblPefReading.Text = currReading.pefString;
					lblFevReading.Text = currReading.fev1String;
					lblDateReading.Text = currReading.dateString;

					btnDelete.IsVisible = true;
					btnIndicator.IsVisible = true;

					btnIndicator.BackgroundColor = Color.FromHex(currReading.color);

					layoutLoadingTakeReading.IsVisible = false;
				});
			}
		}

		public void updateDeviceStateOnUI(String message, bool isConnected) 
        {

			Device.BeginInvokeOnMainThread(() =>
			{
				layoutLoadingTakeReading.IsVisible = false;
                DisplayAlert("Spirometer", message, "OK");
			});

			
		}

		void btnTakeReadingClicked(object sender, System.EventArgs e)
		{
			//updateCaller(new SpirometerReading(DateTime.Now, 680, 3.5m));

			try
			{
				layoutLoadingTakeReading.IsVisible = true;
				BLECentralManager.sharedInstance.connectToDevice("BLE-MSA", this);
			}
			catch { 
				layoutLoadingTakeReading.IsVisible = true;
			}

			//bleManager.ScanToConnectToSpotCheck((BLEReadingUpdatableSpiroMeter)this);
			//DependencyService.Get<ICBCentralManagerSpirometer>().connectToSpirometer((BLEReadingUpdatableSpiroMeter)this);
		}

		void btnCancelTakeReadingClicked(object sender, System.EventArgs e)
		{
			layoutLoadingTakeReading.IsVisible = false;
			BLECentralManager.sharedInstance.spiroServHandler.stopPolling();
			//((SpirometerServiceHandler)BLECentralManager.sharedInstance.devServiceHandler).stopPolling();
			//DependencyService.Get<ICBCentralManagerSpirometer>().StopReadingValue();
		}

		async void btnSaveClicked(object sender, System.EventArgs e)
		{
			//layoutLoading.IsVisible = true;

			try
			{
				Reading fevReading = new Reading("FEV1", currReading.Fev1, 9, false, null, null);
				Reading pefReading = new Reading("PEF", currReading.Pef, 9, false, null, null);
                pefReading.Date = fevReading.Date;
				logcalParameteritem.localspirometerList.Insert(0, new SpirometerReading(fevReading.Date, currReading.Pef, currReading.Fev1));

				//await pefReading.PostReadingToService();
				//await fevReading.PostReadingToService();

				pefReading.PostReadingToService();
				fevReading.PostReadingToService();

				clearReadingDisplay();

                //saved pop up
                await DisplayAlert("Reading", "Reading saved.", "OK");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Exception on saving curr reading: " + ex.Message);
			}
			finally
			{
				layoutLoading.IsVisible = false;
			}
		}

		void btnDeleteCurrentReadingClicked(object sender, System.EventArgs e)
		{
			clearReadingDisplay();
		}

		private void clearReadingDisplay()
		{
			try
			{
				this.currReading.Pef = -1;
				this.currReading.Fev1 = -1;

				lblPefReading.Text = " ";
				lblFevReading.Text = " ";
				lblDateReading.Text = " ";

				btnDelete.IsVisible = false;
				btnIndicator.IsVisible = false;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Exception " + ex.Message);
			}
		}

		void btnViewProfileClicked(object sender, System.EventArgs e)
		{

		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			try
			{
				var latestCalibratedReading = Demographics.sharedInstance.getLatestCalibratedReading();

				System.Diagnostics.Debug.WriteLine(latestCalibratedReading.dateString);

				//latestCalibratedReading.Date.

				//lblDate.Text = "03/10/2017 08:25 PM    ";
				lblDate.Text = latestCalibratedReading.dateString;
				//lblDate.BackgroundColor = Color.Red;

				lblFev1.Text = latestCalibratedReading.fev1String;
				lblPef.Text = latestCalibratedReading.pefString;

				//lblDate.Text = latestCalibratedReading.dateString +" AM";

				System.Diagnostics.Debug.WriteLine("date:" + lblDate.Text);
			}
			catch
			{
				lblDate.Text = "--";
				lblFev1.Text = "--";
				lblPef.Text = "--";
				Debug.WriteLine("Exception in getting calibrated data");
			}
		}

		private async void callAPiToDisplayGetDemographics()
		{
			var isSuccess = await Demographics.sharedInstance.getDemographicFromApi();

			lblClickMessage.IsVisible = true;

			if (isSuccess)
			{
				this.lblName.Text = Demographics.sharedInstance.getFullName();
				this.lblEmail.Text = Demographics.sharedInstance.Email;

				// calling async to download the image and setting in to the image
				String imageBase64 = await Demographics.sharedInstance.downloadProfilePic();

				if (imageBase64 != null)
				{
					this.imgProfile.Source = Xamarin.Forms.ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(imageBase64)));
				}
			}
		}

		void btnCalibrateClicked(object sender, System.EventArgs e)
		{
			var newPage = new RespCalibrationPage();
			newPage.Title = "Calibration Screen";
			this.Navigation.PushAsync(newPage);
		}

		void btnViewGraphPageClicked(object sender, System.EventArgs e)
		{
			//	var newPage = new RespGraphPage();
			var newPage = new RespGraphPageNew();
			newPage.Title = "Data Graph Screen";
			this.Navigation.PushAsync(newPage);
		}

		void btnViewDataListClicked(object sender, System.EventArgs e)
		{
			if (Device.Idiom == TargetIdiom.Tablet)
			{
				var newPage = new RespDataListPagePad();
				newPage.Title = "Data List Screen";
            	this.Navigation.PushAsync(newPage);
			}
			else
			{
				var newPage = new RespDataListPage();
				newPage.Title = "Data List Screen";
				this.Navigation.PushAsync(newPage);
			}
			
		}
	}
}
