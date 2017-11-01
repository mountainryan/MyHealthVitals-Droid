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
				layoutContainer.Spacing = 24 * Screensize.widthfactor;
				imgProfile.WidthRequest = 200 * Screensize.widthfactor;
				imgProfile.HeightRequest = 240 * Screensize.heightfactor;
				lblName.FontSize = 36 * Screensize.heightfactor;
				lblEmail.FontSize = 24 * Screensize.heightfactor;
				lblClickMessage.FontSize = 20 * Screensize.heightfactor;
				lbldata.WidthRequest = 240 * Screensize.widthfactor;
                lblpef.WidthRequest = 200 * Screensize.widthfactor;
                lblfev.WidthRequest = 140 * Screensize.widthfactor;
				lblfev.FontSize = 34 * Screensize.heightfactor;
				lblpef.FontSize = 34 * Screensize.heightfactor;
				lbldata.FontSize = 34 * Screensize.heightfactor;
				lblPefReading.FontSize = 30 * Screensize.heightfactor;
				lblFevReading.FontSize = 30 * Screensize.heightfactor;
				lblDateReading.FontSize = 30 * Screensize.heightfactor;
				//layoutButton1.Spacing = 100 * Screensize.widthfactor;
				layoutButton1.Margin = new Thickness(45 * Screensize.widthfactor, 0, 40 * Screensize.widthfactor, 0);
                B1.FontSize = 36 * Screensize.heightfactor;
                B2.FontSize = 36 * Screensize.heightfactor;
				B1.WidthRequest = 300 * Screensize.widthfactor;
				B2.WidthRequest = 300 * Screensize.widthfactor;
				B1.HeightRequest = 120 * Screensize.heightfactor;
				B2.HeightRequest = 120 * Screensize.heightfactor;
                layoutMiddle.HeightRequest = 150 * Screensize.heightfactor;
                box.HeightRequest = 150 * Screensize.heightfactor;
				//layoutbox.HeightRequest *= 2;
				layoutbox.Spacing = 12 * Screensize.widthfactor;
				lablebox.FontSize = 36 * Screensize.heightfactor;
				layoutval.Spacing = 60 * Screensize.widthfactor;
				lblDate.FontSize = 32 * Screensize.heightfactor;
				lblPef.FontSize = 32 * Screensize.heightfactor;
				lblFev1.FontSize = 32 * Screensize.heightfactor;
				layoutButton2.Margin = new Thickness(45 * Screensize.widthfactor, 0, 40 * Screensize.widthfactor, 0);
                //layoutButton2.Spacing = 100 * Screensize.widthfactor;
                B3.WidthRequest = 300 * Screensize.widthfactor;
                B4.WidthRequest = 300 * Screensize.widthfactor;
                B3.HeightRequest = 120 * Screensize.heightfactor;
                B4.HeightRequest = 120 * Screensize.heightfactor;
				B3.FontSize = 36 * Screensize.heightfactor;
				B4.FontSize = 36 * Screensize.heightfactor;
				save.FontSize = 36 * Screensize.heightfactor;
				save.WidthRequest = box.WidthRequest;
				save.HeightRequest = 90 * Screensize.heightfactor;
				save.Margin = new Thickness(45 * Screensize.widthfactor, 0, 40 * Screensize.widthfactor, 0);
                iculbl.WidthRequest = 450 * Screensize.widthfactor;
				iculbl.FontSize = 40 * Screensize.heightfactor;

                btnIndicator.HeightRequest = 40 * Screensize.heightfactor;
                btnIndicator.WidthRequest = 40 * Screensize.heightfactor;
                btnDelete.Image = "deleteicon_tab.png";
                btnDelete.HeightRequest = 45 * Screensize.heightfactor;
                btnDelete.WidthRequest = 45 * Screensize.heightfactor;

                btncancelread.WidthRequest = 60;
                btncancelread.HeightRequest = 60;
                btncancelread.Image = "deleteicon_x2.png";
				icupng.Source = "icucarellc_pad.png";
				gifWebView.HeightRequest = 190 * Screensize.heightfactor;
				gifWebView.WidthRequest = 190 * Screensize.widthfactor;
				var html = new HtmlWebViewSource();
				html.BaseUrl = DependencyService.Get<IBaseUrl>().Get() + "gifContainer_pad.html";
				gifWebView.Source = html.BaseUrl;
            }
			else if (Device.Idiom == TargetIdiom.Phone)
			{
				layoutContainer.Spacing *= Screensize.widthfactor;
				imgProfile.WidthRequest *= Screensize.widthfactor;
				imgProfile.HeightRequest *= Screensize.heightfactor;
				lblName.FontSize *= Screensize.heightfactor;
				lblEmail.FontSize *= Screensize.heightfactor;
				lblClickMessage.FontSize *= Screensize.heightfactor;
				lbldata.WidthRequest *= Screensize.widthfactor;
                lblfev.WidthRequest *= Screensize.widthfactor;
                lblpef.WidthRequest *= Screensize.widthfactor;
				lblfev.FontSize *= Screensize.heightfactor;
				lblpef.FontSize *= Screensize.heightfactor;
				lbldata.FontSize *= Screensize.heightfactor;
				lblPefReading.FontSize *= Screensize.heightfactor;
				lblFevReading.FontSize *= Screensize.heightfactor;
				lblDateReading.FontSize *= Screensize.heightfactor;
				layoutButton1.Spacing *= Screensize.widthfactor;
				layoutButton1.Margin = new Thickness(15* Screensize.widthfactor, 30* Screensize.heightfactor, 15* Screensize.widthfactor, 20* Screensize.heightfactor);
				B1.FontSize *= Screensize.heightfactor;
				B2.FontSize *= Screensize.heightfactor;
				B1.WidthRequest *= Screensize.widthfactor;
				B2.WidthRequest *= Screensize.widthfactor;
				B1.HeightRequest *= Screensize.heightfactor;
				B2.HeightRequest *= Screensize.heightfactor;
				layoutMiddle.HeightRequest *= Screensize.heightfactor;
				box.HeightRequest *= Screensize.heightfactor;
				//layoutbox.HeightRequest *= 2;
				layoutbox.Spacing *= Screensize.widthfactor;
				lablebox.FontSize *= Screensize.heightfactor;
				layoutval.Spacing *= Screensize.widthfactor;
				lblDate.FontSize *= Screensize.heightfactor;
				lblPef.FontSize *= Screensize.heightfactor;
				lblFev1.FontSize *= Screensize.heightfactor;
				layoutButton2.Margin = new Thickness(15* Screensize.widthfactor, 0, 15* Screensize.widthfactor, 20* Screensize.heightfactor);
				layoutButton2.Spacing *= Screensize.widthfactor;
				B3.WidthRequest *= Screensize.widthfactor;
				B4.WidthRequest *= Screensize.widthfactor;
				B3.HeightRequest *= Screensize.heightfactor;
				B4.HeightRequest *= Screensize.heightfactor;
				B3.FontSize *= Screensize.heightfactor;
				B4.FontSize *= Screensize.heightfactor;
				save.FontSize *= Screensize.heightfactor;
				save.WidthRequest = box.WidthRequest;
				save.HeightRequest *= Screensize.heightfactor;
				//save.Margin = new Thickness(45 * Screensize.widthfactor, 0, 40*Screensize.widthfactor, 0);
				iculbl.WidthRequest *= Screensize.widthfactor;
				iculbl.FontSize *= Screensize.heightfactor;
				var html = new HtmlWebViewSource();
				html.BaseUrl = DependencyService.Get<IBaseUrl>().Get() + "gifContainer.html";
				gifWebView.Source = html.BaseUrl;

            }
			//Debug.WriteLine("lblfev.WidthRequest = " + lblfev.WidthRequest);
			//Debug.WriteLine("lblFevReading.WidthRequest = " + lblFevReading.WidthRequest);

			//Debug.WriteLine("lblpef.WidthRequest = "+ lblpef.WidthRequest);
            //Debug.WriteLine("lblPefReading.WidthRequest = " + lblPefReading.WidthRequest);

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
		async public void testAgainDialog() 
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
            {
                if (Device.Idiom == TargetIdiom.Tablet)
                {
                    var ret = await DependencyService.Get<IFileHelper>().dispAlert("Reading", "The FEV value is too low, please take reading again.", true, "OK", null);
                }
                else
                {
                    var ret = await DependencyService.Get<IFileHelper>().dispAlert("Reading", "The FEV value is too low, please take reading again.", false, "OK", null);
                }
            }));

           //await DisplayAlert("Reading", "The FEV value is too low, please take reading again.", "OK");
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

			Device.BeginInvokeOnMainThread(async () =>
			{
				layoutLoadingTakeReading.IsVisible = false;
				if (Device.Idiom == TargetIdiom.Tablet)
				{
					var ret = await DependencyService.Get<IFileHelper>().dispAlert("Spirometer", message, true, "OK", null);
				}
				else
				{
					var ret = await DependencyService.Get<IFileHelper>().dispAlert("Spirometer", message, false, "OK", null);
				}
                //await DisplayAlert("Spirometer", message, "OK");
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
                if (currReading.Fev1 == -1 || currReading.Pef==-1)
                {
					//display error message
					if (Device.Idiom == TargetIdiom.Tablet)
					{
						var ret = await DependencyService.Get<IFileHelper>().dispAlert("Reading Error", "Invalid reading. Did not save.", true, "OK", null);
					}
					else
					{
						var ret = await DependencyService.Get<IFileHelper>().dispAlert("Reading Error", "Invalid reading. Did not save.", false, "OK", null);
					}
                }else{
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
					if (Device.Idiom == TargetIdiom.Tablet)
					{
						var ret = await DependencyService.Get<IFileHelper>().dispAlert("Reading", "Reading saved.", true, "OK", null);
					}
					else
					{
						var ret = await DependencyService.Get<IFileHelper>().dispAlert("Reading", "Reading saved.", false, "OK", null);
					}

					//await DisplayAlert("Reading", "Reading saved.", "OK");
				}
				
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
