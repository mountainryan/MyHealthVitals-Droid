using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.Diagnostics;

namespace MyHealthVitals
{
	public partial class RespCalibrationPage : ContentPage,BLEReadingUpdatableSpiroMeter
	{

		ObservableCollection<SpirometerReading> calibratedReadingList = new ObservableCollection<SpirometerReading>();

		//BleManagerSpirometer bleManager = new BleManagerSpirometer();

		public RespCalibrationPage()
		{
            NavigationPage.SetHasNavigationBar(this, false);
			InitializeComponent();
			
			if (Device.Idiom == TargetIdiom.Tablet)
			{
				FakeToolbar.HeightRequest = 75 * Screensize.heightfactor;
				titlebtn.FontSize = 30 * Screensize.heightfactor;
                backbtn.FontSize = 30 * Screensize.heightfactor;

                layout.Spacing = (96 * Screensize.heightfactor) / 4;
				label.Margin = new Thickness(20 * Screensize.widthfactor, 10 * Screensize.heightfactor, 20 * Screensize.widthfactor, 10 * Screensize.heightfactor);
				label.FontSize = 32 * Screensize.heightfactor;
                layoutButton.Spacing = (60 * Screensize.heightfactor) / 3;
				button.HeightRequest = 70 * Screensize.heightfactor;
				button.FontSize = 24 * Screensize.heightfactor;
				labelpef.WidthRequest = 175 * Screensize.widthfactor;
				labelfev1.WidthRequest = 175 * Screensize.widthfactor;
				labelpef.FontSize = 30 * Screensize.heightfactor;
				labelfev1.FontSize = 30 * Screensize.heightfactor;
				listView.HeightRequest = 350 * Screensize.heightfactor;
				save.FontSize = 36 * Screensize.heightfactor;
				save.HeightRequest = 90 * Screensize.heightfactor;
                save.Margin = new Thickness(3,4,3,120*Screensize.heightfactor);
				//prevcont.Margin = new Thickness(15,-100* Screensize.heightfactor,0,0);
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

                layout.Spacing = (48 * Screensize.heightfactor) / 4;
				label.Margin = new Thickness(10 * Screensize.widthfactor, 5 * Screensize.heightfactor, 10 * Screensize.widthfactor, 5 * Screensize.heightfactor);
				label.FontSize = 16 * Screensize.heightfactor;
                layoutButton.Spacing = (30 * Screensize.heightfactor) / 3;
				//button.HeightRequest *= Screensize.heightfactor;
				button.FontSize = 14 * Screensize.heightfactor;
                button.HeightRequest = 40 * Screensize.heightfactor;
				labelpef.WidthRequest = 90 * Screensize.widthfactor;
				labelfev1.WidthRequest = 90 * Screensize.widthfactor;
				labelpef.FontSize = 14 * Screensize.heightfactor;
				labelfev1.FontSize = 14 * Screensize.heightfactor;
				listView.HeightRequest = 280 * Screensize.heightfactor;
				save.FontSize = 14 * Screensize.heightfactor;
				save.HeightRequest = 60 * Screensize.heightfactor;
                save.Margin = new Thickness(3, 10 * Screensize.heightfactor, 3, 30 * Screensize.heightfactor);
                //prevcont.Margin = new Thickness(15, -80 /** Screensize.heightfactor*/, 0, 0);
            }
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

		public async void btnCalibrateClicked(object sender, System.EventArgs e)
		{
			if (this.calibratedReadingList.Count < 3)
			{
				string readings = (3 - calibratedReadingList.Count) > 1 ? " more readings." : " more reading.";
				lblLoadingMessage.Text = "Please, take " + (3 - calibratedReadingList.Count) + readings;
				layoutLoading.IsVisible = true;

				//bleManager.ScanToConnectToSpotCheck(this);
				//BLECentralManager.sharedInstance.connectToDevice("BLE-MSA", this);
                //DependencyService.Get<ICBCentralManagerSpirometer>().connectToSpirometer((BLEReadingUpdatableSpiroMeter)this);

                string deviceName = "BLE-MSA";
				List<string> result = DependencyService.Get<IFileHelper>().getBLEinfo(deviceName);
				string BLEtype = "";
				Guid deviceID = new Guid("00000000-0000-0000-0000-000000000000");
				bool conn_success = false;


				if (result.Count == 3)
				{
					Debug.WriteLine("Found guid result in file");
					BLEtype = result[1];
					deviceID = new Guid(result[2]);
					//initializePlotModel();
					if (BLEtype == "1")
					{
						Debug.WriteLine("Type 1 connection");
						conn_success = await BLECentralManager.sharedInstance.ConnectKnownDevice(deviceID, deviceName, this);
					}
					else
					{
						//BLEtype = 2
						//conn_success = await BLECentralManager.sharedInstance.ConnectKnownDevice2(deviceID, deviceName, this);
					}
					Debug.WriteLine("conn_success = " + conn_success.ToString());
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
						Debug.WriteLine("try to connect BLE failed: " + ex.Message);
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


			}
			else {
				Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
				{
					if (Device.Idiom == TargetIdiom.Tablet)
					{
						var ret = await DependencyService.Get<IFileHelper>().dispAlert("Calibration", "Readings taken are sufficient for calibration. If you want to take more readings, Please, delete the unwanted row and take reading again.", true, "OK", null);
					}
					else
					{
						var ret = await DependencyService.Get<IFileHelper>().dispAlert("Calibration", "Readings taken are sufficient for calibration. If you want to take more readings, Please, delete the unwanted row and take reading again.", false, "OK", null);
					}
				}));
				//DisplayAlert("Calibration", "Readings taken are sufficient for calibration. If you want to take more readings, Please, delete the unwanted row and take reading again.", "OK");
			}
		}
		public async void testAgainDialog()
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
			//DisplayAlert("Reading", "The FEV value is too low, please take reading again.", "OK");
			//		DisplayAlert("Test Again", "The FEV value is too low, please test again.", "OK");"
			//BLECentralManager.sharedInstance.connectToDevice("BLE-MSA", this);

			string deviceName = "BLE-MSA";
			List<string> result = DependencyService.Get<IFileHelper>().getBLEinfo(deviceName);
			string BLEtype = "";
			Guid deviceID = new Guid("00000000-0000-0000-0000-000000000000");
			bool conn_success = false;


			if (result.Count == 3)
			{
				Debug.WriteLine("Found guid result in file");
				BLEtype = result[1];
				deviceID = new Guid(result[2]);
				//initializePlotModel();
				if (BLEtype == "1")
				{
					Debug.WriteLine("Type 1 connection");
					conn_success = await BLECentralManager.sharedInstance.ConnectKnownDevice(deviceID, deviceName, this);
				}
				else
				{
					//BLEtype = 2
					//conn_success = await BLECentralManager.sharedInstance.ConnectKnownDevice2(deviceID, deviceName, this);
				}
				Debug.WriteLine("conn_success = " + conn_success.ToString());
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
					Debug.WriteLine("try to connect BLE failed: " + ex.Message);
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
		}
		// call back methods
		public void updateCaller(SpirometerReading currReading)
		{
            Debug.WriteLine("Calibration reading.");
			//var currReading = new SpirometerReading(DateTime.Now, pef, fev1);

			

			if (Device.Idiom == TargetIdiom.Tablet)
			{
				currReading.fontsize = 30 * Screensize.heightfactor;
                currReading.spacing = 20 * Screensize.heightfactor;
                currReading.stackheight = 120 * Screensize.heightfactor;
                currReading.imagepng = "deleteicon_tab.png";
            }else{
				currReading.fontsize = 15 * Screensize.heightfactor;
				currReading.spacing = 10 * Screensize.heightfactor;
				currReading.stackheight = 80 * Screensize.heightfactor;
				currReading.imagepng = "deleteicon.png";
            }

			currReading.index = calibratedReadingList.Count;
			calibratedReadingList.Add(currReading);

			Debug.WriteLine("loaded spirometer reading:" + currReading.pefString);

			Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
			{

				if (this.calibratedReadingList.Count < 3)
				{
					lblLoadingMessage.Text = "Please, take " + (3 - calibratedReadingList.Count) + " more reading.";
					//bleManager.ScanToConnectToSpotCheck(this);
					//DependencyService.Get<ICBCentralManagerSpirometer>().connectToSpirometer((BLEReadingUpdatableSpiroMeter)this);
					//BLECentralManager.sharedInstance.connectToDevice("BLE-MSA", this);
					string deviceName = "BLE-MSA";
					List<string> result = DependencyService.Get<IFileHelper>().getBLEinfo(deviceName);
					string BLEtype = "";
					Guid deviceID = new Guid("00000000-0000-0000-0000-000000000000");
					bool conn_success = false;


					if (result.Count == 3)
					{
						Debug.WriteLine("Found guid result in file");
						BLEtype = result[1];
						deviceID = new Guid(result[2]);
						//initializePlotModel();
						if (BLEtype == "1")
						{
							Debug.WriteLine("Type 1 connection");
							conn_success = await BLECentralManager.sharedInstance.ConnectKnownDevice(deviceID, deviceName, this);
						}
						else
						{
							//BLEtype = 2
							//conn_success = await BLECentralManager.sharedInstance.ConnectKnownDevice2(deviceID, deviceName, this);
						}
						Debug.WriteLine("conn_success = " + conn_success.ToString());
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
							Debug.WriteLine("try to connect BLE failed: " + ex.Message);
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
				}
				else {
					layoutLoading.IsVisible = false;
				}

				listView.ItemsSource = calibratedReadingList;
			}));
		}

		public async void FailedConn(String message, bool isConn, int camefrom)
		{
			Debug.WriteLine("FailedConn  mainpage  : " + message);
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
						layoutLoading.IsVisible = true;
						//await BLECentralManager.sharedInstance.ConnectToDevice2("BLE-MSA", this);
                        BLECentralManager.sharedInstance.connectToDevice("BLE-MSA", this);
					}
					catch (Exception ex)
					{
						Debug.WriteLine("conn error msg : " + ex.Message);
					}

				}
			}
			else
			{
				//camefrom BLE manager 2
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
						layoutLoading.IsVisible = true;
						BLECentralManager.sharedInstance.connectToDevice("BLE-MSA", this);
					}
					catch (Exception ex)
					{
						Debug.WriteLine("conn error msg : " + ex.Message);
					}

				}
			}
		}

		public void updateDeviceStateOnUI(String message, bool isConnected)
		{
			try
			{
				Device.BeginInvokeOnMainThread(() =>
					{
						layoutLoading.IsVisible = false;
					});

			}
            catch { 
			
			}
			Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
			{
				if (Device.Idiom == TargetIdiom.Tablet)
				{
					var ret = await DependencyService.Get<IFileHelper>().dispAlert("Spirometer", message, true, "OK", null);
				}
				else
				{
					var ret = await DependencyService.Get<IFileHelper>().dispAlert("Spirometer", message, false, "OK", null);
				}
			}));
			//DisplayAlert("Spirometer", message, "OK");
		}

		async void btnSaveCLicked(object sender, System.EventArgs e)
		{
			layoutLoading.IsVisible = true;
			lblLoadingMessage.Text = "Saving Calibrated Readings.";

			try
			{
				SpirometerReading highestReading = getHighestReading();

				// saving it to the local storage
				//Demographics.sharedInstance.calibratedReading = highestReading;
				//Demographics.sharedInstance.saveCalibratedReadig();

				Demographics.sharedInstance.saveCalibratedReadig(highestReading);

				Reading fevReading = new Reading("FEV1", highestReading.Fev1, 9, false, null, null);
				Reading pefReading = new Reading("PEF", highestReading.Pef, 9, false, null, null);

                if (fevReading.EnglishValue != -1 && pefReading.EnglishValue != -1)
                {
                    try
                    {
						logcalParameteritem.localspirometerList.Insert(0, new SpirometerReading(fevReading.Date, highestReading.Pef, highestReading.Fev1));
						await pefReading.PostReadingToService();
						await fevReading.PostReadingToService();
						layoutLoading.IsVisible = false;
						//saved pop up
						if (Device.Idiom == TargetIdiom.Tablet)
						{
							var ret = await DependencyService.Get<IFileHelper>().dispAlert("Calibration Reading", "Calibrated readings saved.", true, "OK", null);
						}
						else
						{
							var ret = await DependencyService.Get<IFileHelper>().dispAlert("Calibration Reading", "Calibrated readings saved.", false, "OK", null);
						}
                    }
                    catch (Exception ex)
                    {
						layoutLoading.IsVisible = false;
                        Debug.WriteLine("exception on sending spirometer data to server.");
						if (Device.Idiom == TargetIdiom.Tablet)
						{
							var ret = await DependencyService.Get<IFileHelper>().dispAlert("Calibration Reading Error", "Unable to save calibrated readings.", true, "OK", null);
						}
						else
						{
							var ret = await DependencyService.Get<IFileHelper>().dispAlert("Calibration Reading Error", "Unable to save calibrated readings.", false, "OK", null);
						}
                    }

                }
                else
                {
					layoutLoading.IsVisible = false;
					if (Device.Idiom == TargetIdiom.Tablet)
					{
						var ret = await DependencyService.Get<IFileHelper>().dispAlert("Calibration Reading", "Unable to save calibrated readings.", true, "OK", null);
					}
					else
					{
						var ret = await DependencyService.Get<IFileHelper>().dispAlert("Calibration Reading", "Unable to save calibrated readings.", false, "OK", null);
					}
					
                }

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

		public async void DeleteClicked(object sender, System.EventArgs e)
		{
			var btn = (Xamarin.Forms.Button)sender;

			System.Diagnostics.Debug.WriteLine("Remove calibrated item "+(int)btn.CommandParameter);

			calibratedReadingList.RemoveAt((int)btn.CommandParameter);

			// update the index value for reading display on left of each row
			int count = 0;
			foreach (var rdn in calibratedReadingList) {
				rdn.index = count++; 
			}

			if (this.calibratedReadingList.Count < 3)
			{
				lblLoadingMessage.Text = "Please, take " + (3 - calibratedReadingList.Count) + " more reading.";
                layoutLoading.IsVisible = true;
				//bleManager.ScanToConnectToSpotCheck(this);
				//DependencyService.Get<ICBCentralManagerSpirometer>().connectToSpirometer(this);
				//BLECentralManager.sharedInstance.connectToDevice("BLE-MSA", this);
				

				string deviceName = "BLE-MSA";
				List<string> result = DependencyService.Get<IFileHelper>().getBLEinfo(deviceName);
				string BLEtype = "";
				Guid deviceID = new Guid("00000000-0000-0000-0000-000000000000");
				bool conn_success = false;


				if (result.Count == 3)
				{
					Debug.WriteLine("Found guid result in file");
					BLEtype = result[1];
					deviceID = new Guid(result[2]);
					//initializePlotModel();
					if (BLEtype == "1")
					{
						Debug.WriteLine("Type 1 connection");
						conn_success = await BLECentralManager.sharedInstance.ConnectKnownDevice(deviceID, deviceName, this);
					}
					else
					{
						//BLEtype = 2
						//conn_success = await BLECentralManager.sharedInstance.ConnectKnownDevice2(deviceID, deviceName, this);
					}
					Debug.WriteLine("conn_success = " + conn_success.ToString());
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
						Debug.WriteLine("try to connect BLE failed: " + ex.Message);
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
            
            }
		}

		private SpirometerReading getHighestReading() {


			SpirometerReading highestReading = calibratedReadingList[0];

			foreach (var redn in calibratedReadingList) {
				if (highestReading.Pef < redn.Pef) {
					highestReading = redn;
				}
			}

			return highestReading;
		}
	}
}
