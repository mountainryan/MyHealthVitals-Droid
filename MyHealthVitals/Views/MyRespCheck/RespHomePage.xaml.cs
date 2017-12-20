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
				new Rectangle(0.1, 0.5, 0.15, 1),
				AbsoluteLayoutFlags.All
			);
			FakeToolbar.Children.Add(
				titlebtn,
				// Using 0.5 will center it and the layout takes the size of the element into account
				// 0.5 will center, 1 will right align
				// Adds in the center, with 90% of the navbar's width and 100% of height
				new Rectangle(0.5, 0.5, 0.5, 1),
				AbsoluteLayoutFlags.All
			);

			if (Device.Idiom == TargetIdiom.Tablet)
			{
				FakeToolbar.HeightRequest = 75 * Screensize.heightfactor;
                backbtn.FontSize = 30 * Screensize.heightfactor;
				titlebtn.FontSize = 30 * Screensize.heightfactor;

                layoutContainer.Spacing = (144 * Screensize.widthfactor)/6;
				imgProfile.WidthRequest = 200 * Screensize.widthfactor;
				imgProfile.HeightRequest = 240 * Screensize.heightfactor;
				lblName.FontSize = 36 * Screensize.heightfactor;
				lblEmail.FontSize = 24 * Screensize.heightfactor;
				lblClickMessage.FontSize = 20 * Screensize.heightfactor;
                lbldata.WidthRequest = (720 * Screensize.widthfactor) / 3;
                lblpef.WidthRequest = (720 * Screensize.widthfactor) / 3.6;
                lblfev.WidthRequest = (720 * Screensize.widthfactor) / 5.142857;
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
                B1.WidthRequest = (600 * Screensize.widthfactor) / 2;
				B2.WidthRequest = (600 * Screensize.widthfactor) / 2;
                B1.HeightRequest = (480 * Screensize.heightfactor) / 4;
				B2.HeightRequest = (480 * Screensize.heightfactor) / 4;
                layoutMiddle.HeightRequest = 150 * Screensize.heightfactor;
                box.HeightRequest = 150 * Screensize.heightfactor;
				//layoutbox.HeightRequest *= 2;
				layoutbox.Spacing = 12 * Screensize.widthfactor;
				lablebox.FontSize = 36 * Screensize.heightfactor;
                layoutval.Spacing = (180 * Screensize.widthfactor) / 3;
				lblDate.FontSize = 32 * Screensize.heightfactor;
				lblPef.FontSize = 32 * Screensize.heightfactor;
				lblFev1.FontSize = 32 * Screensize.heightfactor;
				layoutButton2.Margin = new Thickness(45 * Screensize.widthfactor, 0, 40 * Screensize.widthfactor, 0);
                //layoutButton2.Spacing = 100 * Screensize.widthfactor;
                B3.WidthRequest = (600 * Screensize.widthfactor) / 2;
                B4.WidthRequest = (600 * Screensize.widthfactor) / 2;
                B3.HeightRequest = (480 * Screensize.heightfactor) / 4;
                B4.HeightRequest = (480 * Screensize.heightfactor) / 4;
				B3.FontSize = 36 * Screensize.heightfactor;
				B4.FontSize = 36 * Screensize.heightfactor;

				save.FontSize = 36 * Screensize.heightfactor;
				save.WidthRequest = box.WidthRequest;
                save.HeightRequest = (360 * Screensize.heightfactor) / 4;
				save.Margin = new Thickness(45 * Screensize.widthfactor, 0, 40 * Screensize.widthfactor, 0);
                //layoutbox2.HeightRequest = 120;

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
				//icupng.Source = "icucarellc_pad.png";
                icupng.HeightRequest = 242 * Screensize.heightfactor;
                icupng.WidthRequest = 500 * Screensize.heightfactor;
                gifWebView.HeightRequest = 190;// * Screensize.heightfactor;
                gifWebView.WidthRequest = 190;// * Screensize.widthfactor;
				var html = new HtmlWebViewSource();
				html.BaseUrl = DependencyService.Get<IBaseUrl>().Get() + "gifContainer_pad.html";
				gifWebView.Source = html.BaseUrl;

                //Debug.WriteLine("ht factor: "+Screensize.heightfactor);
                //Debug.WriteLine("wd factor: " + Screensize.widthfactor);
            }
			else if (Device.Idiom == TargetIdiom.Phone)
			{
				FakeToolbar.HeightRequest = 55 * Screensize.heightfactor;
				titlebtn.FontSize = 16 * Screensize.heightfactor;
                backbtn.FontSize = 16 * Screensize.heightfactor;

                layoutContainer.Spacing = (72 * Screensize.widthfactor) / 6;
				imgProfile.WidthRequest = 100 * Screensize.widthfactor;
				imgProfile.HeightRequest = 120 * Screensize.heightfactor;
				lblName.FontSize = 16 * Screensize.heightfactor;
				lblEmail.FontSize = 15 * Screensize.heightfactor;
				lblClickMessage.FontSize = 12 * Screensize.heightfactor;
                lbldata.WidthRequest = (120 * Screensize.widthfactor);
                lblpef.WidthRequest = (90 * Screensize.widthfactor);
                lblfev.WidthRequest = (70 * Screensize.widthfactor);
                lblfev.FontSize = 15 * Screensize.heightfactor;
				lblpef.FontSize = 15 * Screensize.heightfactor;
				lbldata.FontSize = 15 * Screensize.heightfactor;
				lblPefReading.FontSize = 14 * Screensize.heightfactor;
				lblFevReading.FontSize = 14 * Screensize.heightfactor;
				lblDateReading.FontSize = 14 * Screensize.heightfactor;
				layoutButton1.Spacing = 25 * Screensize.widthfactor;
				layoutButton1.Margin = new Thickness(15* Screensize.widthfactor, 30* Screensize.heightfactor, 15* Screensize.widthfactor, 20* Screensize.heightfactor);
				B1.FontSize = 14 * Screensize.heightfactor;
				B2.FontSize = 14 * Screensize.heightfactor;
                B1.WidthRequest = (240 * Screensize.widthfactor) / 2;
				B2.WidthRequest = (240 * Screensize.widthfactor) / 2;
                B1.HeightRequest = (240 * Screensize.heightfactor) / 4;
				B2.HeightRequest = (240 * Screensize.heightfactor) / 4;
				layoutMiddle.HeightRequest = 75 * Screensize.heightfactor;
				box.HeightRequest = 75 * Screensize.heightfactor;
                //layoutbox.HeightRequest *= 2;
                layoutbox.Spacing = 6 * Screensize.heightfactor;
				lablebox.FontSize = 16 * Screensize.heightfactor;
				layoutval.Spacing = (90 * Screensize.widthfactor) / 3;
				lblDate.FontSize = 15 * Screensize.heightfactor;
				lblPef.FontSize = 15 * Screensize.heightfactor;
				lblFev1.FontSize = 15 * Screensize.heightfactor;
				layoutButton2.Margin = new Thickness(15* Screensize.widthfactor, 0, 15* Screensize.widthfactor, 20* Screensize.heightfactor);
				layoutButton2.Spacing = 25 * Screensize.widthfactor;
				B3.WidthRequest = (240 * Screensize.widthfactor) / 2;
				B4.WidthRequest = (240 * Screensize.widthfactor) / 2;
				B3.HeightRequest = (240 * Screensize.heightfactor) / 4;
				B4.HeightRequest = (240 * Screensize.heightfactor) / 4;
				B3.FontSize = 14 * Screensize.heightfactor;
				B4.FontSize = 14 * Screensize.heightfactor;
				save.FontSize = 14 * Screensize.heightfactor;
				save.WidthRequest = box.WidthRequest;
				save.HeightRequest = (240 * Screensize.heightfactor) / 4;
				//save.Margin = new Thickness(45 * Screensize.widthfactor, 0, 40*Screensize.widthfactor, 0);
				iculbl.WidthRequest = 300 * Screensize.widthfactor;
				iculbl.FontSize = 20 * Screensize.heightfactor;
				icupng.HeightRequest = 120 * Screensize.heightfactor;
				icupng.WidthRequest = 250 * Screensize.heightfactor;
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
		void btnPrevClicked(object sender, System.EventArgs e)
		{
			Navigation.PopAsync();
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

			//connect to known device
			//BLECentralManager.sharedInstance.connectToDevice("BLE-MSA", this);

			string deviceName = "BLE-MSA";

			layoutLoadingTakeReading.IsVisible = true;

			List<string> result = DependencyService.Get<IFileHelper>().getBLEinfo(deviceName);
			string BLEtype = "";
			Guid deviceID = new Guid("00000000-0000-0000-0000-000000000000");
			bool conn_success = false;

			for (int i = 0; i < result.Count; i += 3)
			{
				BLEtype = result[i + 1];
				deviceID = new Guid(result[i + 2]);
				//attempt to connect via this method
				if (BLEtype == "1")
				{
					//layoutLoading.IsVisible = true;
					conn_success = await BLECentralManager.sharedInstance.ConnectKnownDevice(deviceID, deviceName, this);
				}
				else
				{
					//BLEtype = 2
					//layoutLoading.IsVisible = true;
					//conn_success = await BLECentralManager.sharedInstance.ConnectKnownDevice2(deviceID, deviceName, this);
				}
				if (conn_success) break;
			}
			if (!conn_success)
			{
				//send the error message
				//BLECentralManager.sharedInstance.SendConnError(deviceName, 2);
				//or just try to connect the hard way!
				BLECentralManager.sharedInstance.connectToDevice(deviceName, this);
			}

		}

		// call back methods
		public void updateCaller(SpirometerReading reading)
		{
			currReading = reading;
            //Debug.WriteLine("Normal reading.");

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

		public async void FailedConn(String message, bool isConn, int camefrom)
		{
			//Debug.WriteLine("FailedConn  mainpage  : " + message);
			if (camefrom == 1)
			{
				bool ret;
				if (Device.Idiom == TargetIdiom.Tablet)
				{
					ret = await DependencyService.Get<IFileHelper>().dispAlert("BLE-MSA", message, true, "Yes", "No");
				}
				else
				{
					ret = await DependencyService.Get<IFileHelper>().dispAlert("BLE-MSA", message, false, "Yes", "No");
				}
				if (ret)
				{
					//try to connect again, this time to the second BLE manager
					try
					{
						layoutLoadingTakeReading.IsVisible = true;
						//await BLECentralManager.sharedInstance.ConnectToDevice2("BLE-MSA", this);
                        BLECentralManager.sharedInstance.connectToDevice("BLE-MSA", this);
					}
					catch (Exception ex)
					{
						//Debug.WriteLine("conn error msg : " + ex.Message);
					}

				}
			}
			else
			{
				//camefrom BLE manager 2
                //shouldn't happen now!
				bool ret;
				if (Device.Idiom == TargetIdiom.Tablet)
				{
					ret = await DependencyService.Get<IFileHelper>().dispAlert("BLE-MSA", message, true, "Yes", "No");
				}
				else
				{
					ret = await DependencyService.Get<IFileHelper>().dispAlert("BLE-MSA", message, false, "Yes", "No");
				}
				if (ret)
				{
					//try to connect again, this time to the second BLE manager
					try
					{
						layoutLoadingTakeReading.IsVisible = true;
						BLECentralManager.sharedInstance.connectToDevice("BLE-MSA", this);
					}
					catch (Exception ex)
					{
						//Debug.WriteLine("conn error msg : " + ex.Message);
					}

				}
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

		public async void btnTakeReadingClicked(object sender, System.EventArgs e)
		{
            //ignoring BLE manager 2 for now!

            string deviceName = "BLE-MSA";

            layoutLoadingTakeReading.IsVisible = true;

			List<string> result = DependencyService.Get<IFileHelper>().getBLEinfo(deviceName);
			string BLEtype = "";
			Guid deviceID = new Guid("00000000-0000-0000-0000-000000000000");
			bool conn_success = false;


			if (result.Count == 3)
			{
				//Debug.WriteLine("Found guid result in file");
				BLEtype = result[1];
				deviceID = new Guid(result[2]);
				//initializePlotModel();
				if (BLEtype == "1")
				{
					//Debug.WriteLine("Type 1 connection");
					conn_success = await BLECentralManager.sharedInstance.ConnectKnownDevice(deviceID, deviceName, this);
				}
				else
				{
					//BLEtype = 2
					//conn_success = await BLECentralManager.sharedInstance.ConnectKnownDevice2(deviceID, deviceName, this);
				}
				//Debug.WriteLine("conn_success = " + conn_success.ToString());
				if (!conn_success)
				{
					//try the hard way
					BLECentralManager.sharedInstance.connectToDevice(deviceName, this);
				}
			}
			else if (result.Count == 0)
			{
				//try first method
				try
				{
					BLECentralManager.sharedInstance.connectToDevice(deviceName, this);
				}
				catch (Exception ex)
				{
					//Debug.WriteLine("try to connect BLE failed: " + ex.Message);
				}
			}
			else
			{
				//somehow they've connected more than 1 guid of a device (2 PC-300s for example)
				//or they've managed to connect the same device to both BLE managers

				for (int i = 0; i < result.Count; i += 3)
				{
					BLEtype = result[i + 1];
					deviceID = new Guid(result[i + 2]);
					//attempt to connect via this method
					if (BLEtype == "1")
					{
						//layoutLoading.IsVisible = true;
						conn_success = await BLECentralManager.sharedInstance.ConnectKnownDevice(deviceID, deviceName, this);
					}
					else
					{
						//BLEtype = 2
						//layoutLoading.IsVisible = true;
						//conn_success = await BLECentralManager.sharedInstance.ConnectKnownDevice2(deviceID, deviceName, this);
					}
					if (conn_success) break;
				}
				if (!conn_success)
				{
					//send the error message
					//BLECentralManager.sharedInstance.SendConnError(deviceName, 2);
                    //or just try to connect the hard way!
                    BLECentralManager.sharedInstance.connectToDevice(deviceName, this);
				}

			}

            //await BLECentralManager.sharedInstance.ConnectToDevice2(deviceName, this);


            /*
			try
			{
                layoutLoadingTakeReading.IsVisible = true;
				BLECentralManager.sharedInstance.connectToDevice(deviceName, this);
			}
			catch { 
				layoutLoadingTakeReading.IsVisible = true;
                Debug.WriteLine("couldn't connect to Spirometer.");
			}
			*/

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

                    if (pefReading.EnglishValue != -1 && fevReading.EnglishValue != -1)
                    {
                        try
                        {
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
                        }
                        catch (Exception ex)
                        {
							clearReadingDisplay();
                            //Debug.WriteLine("exception on sending spirometer data to server.");
							if (Device.Idiom == TargetIdiom.Tablet)
							{
								var ret = await DependencyService.Get<IFileHelper>().dispAlert("Reading Error", "Unable to save reading.", true, "OK", null);
							}
							else
							{
								var ret = await DependencyService.Get<IFileHelper>().dispAlert("Reading Error", "Unable to save reading.", false, "OK", null);
							}
                        }


                    }
                    else
                    {
						clearReadingDisplay();

						//saved pop up
						if (Device.Idiom == TargetIdiom.Tablet)
						{
							var ret = await DependencyService.Get<IFileHelper>().dispAlert("Reading Failure", "Unable to save reading.", true, "OK", null);
						}
						else
						{
							var ret = await DependencyService.Get<IFileHelper>().dispAlert("Reading Failure", "Unable to save reading.", false, "OK", null);
						}
                    }

					//await DisplayAlert("Reading", "Reading saved.", "OK");
				}
				
			}
			catch (Exception ex)
			{
				//System.Diagnostics.Debug.WriteLine("Exception on saving curr reading: " + ex.Message);
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
				//System.Diagnostics.Debug.WriteLine("Exception " + ex.Message);
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

				//System.Diagnostics.Debug.WriteLine(latestCalibratedReading.dateString);

				//latestCalibratedReading.Date.

				//lblDate.Text = "03/10/2017 08:25 PM    ";
				lblDate.Text = latestCalibratedReading.dateString;
				//lblDate.BackgroundColor = Color.Red;

				lblFev1.Text = latestCalibratedReading.fev1String;
				lblPef.Text = latestCalibratedReading.pefString;

				//lblDate.Text = latestCalibratedReading.dateString +" AM";

				//System.Diagnostics.Debug.WriteLine("date:" + lblDate.Text);
			}
			catch
			{
				lblDate.Text = "--";
				lblFev1.Text = "--";
				lblPef.Text = "--";
				//Debug.WriteLine("Exception in getting calibrated data");
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
