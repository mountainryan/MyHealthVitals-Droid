using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Xamarin.Forms;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;

namespace MyHealthVitals
{
    public class BLE_val
    {
        public static int BLE_value=0;
    }
	public partial class MainPage : ContentPage, IBluetoothCallBackUpdatable
	{
		private VitalsData vitalsData = new VitalsData();
		public static bool isCOnnectedToSpotCheck = false;

		//public BleManagerSpotCheck bleManager;

		public String deviceName = "";
		LineSeries lineSerie;
		string fileName = "";
		bool isKg = false;
		//	private EcgReport ecgReportInstance = null;
		string lashEcgFile = null;
		float userHeight = (float)0.0;
		bool isBPMeasuring = false;
		bool isupLoadedSPO2 = false;
		public bool isFromDeviceList = false;
		bool isNavigated = false;
		public MainPage(string deviceName)
		{
			//NavigationPage.SetBackButtonTitle(this, "Back");
            NavigationPage.SetHasNavigationBar(this, false);
			this.deviceName = deviceName;
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
			FakeToolbar.Children.Add(
				listbtn,
				// Using 0.5 will center it and the layout takes the size of the element into account
				// 0.5 will center, 1 will right align
				// Adds in the center, with 90% of the navbar's width and 100% of height
				new Rectangle(1, 0.5, 0.1, 1),
				AbsoluteLayoutFlags.All
			);
			resizeUI();
			btnLbs.TextColor = (Color)App.Current.Resources["colorThemeBlue"];

			//NavigationPage.SetBackButtonTitle(this, "Back");

			var tapGestureRecognizer = new TapGestureRecognizer();
			tapGestureRecognizer.Tapped += (s, e) =>
			{
				var newPage = new UserProfile();
				newPage.Title = "My Account";
				this.Navigation.PushAsync(newPage);
			};

			imgProfile.GestureRecognizers.Add(tapGestureRecognizer);

			setUpEcgDisplay();

			btnFareinheit.TextColor = (Color)App.Current.Resources["colorThemeBlue"];
			btnCelcious.TextColor = Color.Gray;
			isCelcious = false;


			BLECentralManager.sharedInstance.disConnectAll();
			if (deviceName == "PC-100")
			{
				BLECentralManager.sharedInstance.pc100ServHandler.updateController(this);
			}
			else if (deviceName == "eBody-Scale")
			{
				BLECentralManager.sharedInstance.scaleServHandle.updateController(this);
			}
			else
			{
				BLECentralManager.sharedInstance.spotServHandler.updateController(this);
			}

			callAPiToDisplayGetDemographics();
		}
		public void setSavereportbutton()
		{
			lineSerie.Points.Clear();
			if (Device.Idiom == TargetIdiom.Tablet)
			{
				ecgReportcBtnPad.IsEnabled = false;
			}
			else
			{
				ecgReportcBtn.IsEnabled = false;
			}
			//ecgReportcBtn.IsEnabled = false;
		}


		private void setUpEcgDisplay()
		{
			// Oxy plot thing
			graphModel = new PlotModel();

			//graphModel.TouchStarted += (object sender, OxyTouchEventArgs e) => {
			//  //graphModel.DefaultXAxis.pa
			//});
			//graphModel.sr
			BindingContext = this;

			lineSerie = new LineSeries
			{
				StrokeThickness = 1.5,
				Color = OxyColor.FromRgb(0, 145, 255),
				Smooth = true,
			};

			lastDataPointPrev = new DataPoint(0, 0);
			lineSerie.Points.Add(lastDataPointPrev);

			graphModel.Series.Add(lineSerie);
			graphModel.InvalidatePlot(true);
			Debug.WriteLine("graphModel      143   :" + graphModel);
		}
		protected async override void OnAppearing()
		{
			base.OnAppearing();
			Debug.WriteLine("OnAppearing  :");

			if (graphModel == null) return;
			Debug.WriteLine("graphModel.DefaultXAxis  :" + graphModel.DefaultXAxis);

            if (countECGPacket == 0 && graphModel.DefaultXAxis != null)
			{
                Debug.WriteLine("Made it in to graph mod if statement");
				graphModel.DefaultXAxis.IsPanEnabled = false;
				graphModel.DefaultYAxis.IsPanEnabled = false;

                if (Device.Idiom == TargetIdiom.Tablet)
                {
                    graphModel.DefaultFontSize = 20;
                    graphModel.DefaultXAxis.FontSize = 20;
                    graphModel.DefaultYAxis.FontSize = 20;
                }else{
					graphModel.DefaultFontSize = 10;
					graphModel.DefaultXAxis.FontSize = 10;
					graphModel.DefaultYAxis.FontSize = 10;
                }

				graphModel.DefaultYAxis.Minimum = 0;
				graphModel.DefaultYAxis.Maximum = 255;

				graphModel.DefaultXAxis.Minimum = 0;
				graphModel.DefaultXAxis.Maximum = 50;
				graphModel.DefaultXAxis.IsZoomEnabled = false;
				graphModel.DefaultYAxis.IsZoomEnabled = false;
				styleGraphModel(graphModel);
			}
			
            var linearAxis1 = new LinearAxis();
            linearAxis1.IsZoomEnabled = false;
			linearAxis1.IsPanEnabled = false;
			
            if (lashEcgFile != null && lashEcgFile.Length > 0)
			{

				Boolean ecgExist = DependencyService.Get<IFileHelper>().checkFileExist(lashEcgFile + ".txt");
				if (!ecgExist)
				{
					setSavereportbutton();
				}
			}

			if (!isFromDeviceList || BLECentralManager.sharedInstance.checkIfDeviceScanned(deviceName))
			{
				return;
			}
            bool ret;
            if (Device.Idiom == TargetIdiom.Tablet)
            {
                Debug.WriteLine("recognized tablet");
                ret = await DependencyService.Get<IFileHelper>().dispAlert(deviceName, "Do you want to take a measurement?", true, "Yes", "No");
            }else{
                Debug.WriteLine("recognized phone");
                ret = await DependencyService.Get<IFileHelper>().dispAlert(deviceName, "Do you want to take a measurement?", false, "Yes", "No");
            }
			//var ret = await DisplayAlert(deviceName, "Do you want to take a measurement?", "Yes", "No");
			if (ret)
			{
                //check the internet connection
                if (!CrossConnectivity.Current.IsConnected)
                {
					if (Device.Idiom == TargetIdiom.Tablet)
					{
						Debug.WriteLine("recognized tablet");
						ret = await DependencyService.Get<IFileHelper>().dispAlert("Internet Connection", "No Service Available", true, "OK", null);
					}
					else
					{
						Debug.WriteLine("recognized phone");
						ret = await DependencyService.Get<IFileHelper>().dispAlert("Internet Connection", "No Service Available", false, "OK", null);
					}
                }else{
                    //check the connection to the server
                    //display a message if it is down
                    lblLoadingMessage.Text = "Checking connection to server...";
                    layoutLoading.IsVisible = true;

                    //var connval = await CrossConnectivity.Current.IsRemoteReachable("104.44.133.25"); //prod ip
                    var connval = await CrossConnectivity.Current.IsRemoteReachable("13.84.153.187"); //test ip


					Debug.WriteLine("connection to server = "+connval.ToString());
                    if (!connval)
                    {
						if (Device.Idiom == TargetIdiom.Tablet)
						{
							Debug.WriteLine("recognized tablet");
							ret = await DependencyService.Get<IFileHelper>().dispAlert("Internet Connection", "Unable to connect to server", true, "OK", null);
						}
						else
						{
							Debug.WriteLine("recognized phone");
							ret = await DependencyService.Get<IFileHelper>().dispAlert("Internet Connection", "Unable to connect to server", false, "OK", null);
						}
                    }
                    lblLoadingMessage.Text = "Connecting with device...";

                }

                //DependencyService.Get<IFileHelper>().delBLEinfo();
                Debug.WriteLine("   device Name ======  " + deviceName);
                //check if the current android machine has connected to this device before.
                List<string> result = DependencyService.Get<IFileHelper>().getBLEinfo(deviceName);
                string BLEtype = "";
                Guid deviceID = new Guid("00000000-0000-0000-0000-000000000000");
                bool conn_success = false;
				if (result.Count == 3)
				{
                    Debug.WriteLine("Found guid result in file");
                    BLEtype = result[1];
                    deviceID = new Guid(result[2]);
                    initializePlotModel();
                    if (BLEtype == "1")
                    {
                        Debug.WriteLine("Type 1 connection");
                        layoutLoading.IsVisible = true;
                        conn_success = await BLECentralManager.sharedInstance.ConnectKnownDevice(deviceID, deviceName, this);
                    }else{
                        //BLEtype = 2
                        layoutLoading.IsVisible = true;
                        conn_success = await BLECentralManager.sharedInstance.ConnectKnownDevice2(deviceID, deviceName, this);
                    }
                    Debug.WriteLine("conn_success = "+conn_success.ToString());
                    if (!conn_success)
                    {
                        //try the hard way
                        lblLoadingMessage.Text = "Retrying...";
                        layoutLoading.IsVisible = true;
                        BLECentralManager.sharedInstance.connectToDevice(deviceName, this);
                    }
                }
                else if (result.Count == 0)
                {
                    //try first method
					initializePlotModel();
                    try
                    {
                        if (BLECentralManager.sharedInstance.scaleServHandle.connectedDevice.State
                            != Plugin.BLE.Abstractions.DeviceState.Connected)
                        {
                            {
                                BLECentralManager.sharedInstance.connectToDevice(deviceName, this);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("try to connect BLE failed: " + ex.Message);
                        layoutLoading.IsVisible = true;
                        BLECentralManager.sharedInstance.connectToDevice(deviceName, this);
                        //BLECentralManager.sharedInstance.connectToDevice(deviceName, this);
                        //BLECentralManager.sharedInstance.checkIfDeviceScanned(deviceName);
                    }					
                }else{
					//somehow they've connected more than 1 guid of a device (2 PC-300s for example)
					//or they've managed to connect the same device to both BLE managers

                    initializePlotModel();
					for (int i = 0; i < result.Count; i += 3)
					{
                        BLEtype = result[i + 1];
                        deviceID = new Guid(result[i + 2]);
						//attempt to connect via this method
						if (BLEtype == "1")
						{
							layoutLoading.IsVisible = true;
							conn_success = await BLECentralManager.sharedInstance.ConnectKnownDevice(deviceID, deviceName, this);
						}
						else
						{
							//BLEtype = 2
							layoutLoading.IsVisible = true;
							conn_success = await BLECentralManager.sharedInstance.ConnectKnownDevice2(deviceID, deviceName, this);
						}
                        if (conn_success) break;
					}
                    if (!conn_success)
                    {
						//send the error message
						//BLECentralManager.sharedInstance.SendConnError(deviceName, 2);
						//or just try to connect the hard way!
						lblLoadingMessage.Text = "Retrying...";
						layoutLoading.IsVisible = true;
						BLECentralManager.sharedInstance.connectToDevice(deviceName, this);
                    }

                }

            }else{
                initializePlotModel();
                //layoutLoading.IsVisible = true;
                //await BLECentralManager.sharedInstance.ConnectToDevice2(deviceName, this);
            }
			isFromDeviceList = false;

		}

        public void initializePlotModel()
        {
			if (countECGPacket == 0 && graphModel.DefaultXAxis != null)
			{
				Debug.WriteLine("Made it in to graph mod on initialize if statement");
				graphModel.DefaultXAxis.IsPanEnabled = false;
				graphModel.DefaultYAxis.IsPanEnabled = false;

                if (Device.Idiom == TargetIdiom.Tablet)
                {
					graphModel.DefaultFontSize = 20;
					graphModel.DefaultXAxis.FontSize = 20;
					graphModel.DefaultYAxis.FontSize = 20;
				}
			    else
			    {
    				graphModel.DefaultFontSize = 10;
    				graphModel.DefaultXAxis.FontSize = 10;
    				graphModel.DefaultYAxis.FontSize = 10;
			    }


				graphModel.DefaultYAxis.Minimum = 0;
				graphModel.DefaultYAxis.Maximum = 255;

				graphModel.DefaultXAxis.Minimum = 0;
				graphModel.DefaultXAxis.Maximum = 50;
				graphModel.DefaultXAxis.IsZoomEnabled = false;
				graphModel.DefaultYAxis.IsZoomEnabled = false;
				styleGraphModel(graphModel);
            }else{
                //Debug.WriteLine("WHY!!!!!");
            }
        }

		private void styleGraphModel(PlotModel graphModel)
		{
			// x - axis style
			if (graphModel == null) return;

			graphModel.DefaultXAxis.MinorGridlineStyle = LineStyle.Solid;
			graphModel.DefaultXAxis.MajorGridlineStyle = LineStyle.Solid;

			graphModel.DefaultXAxis.MajorGridlineThickness = 0.25f;
			graphModel.DefaultXAxis.MinorGridlineThickness = 0.25f;

			graphModel.DefaultXAxis.MinorGridlineColor = OxyColors.LightGray;
			graphModel.DefaultXAxis.MajorGridlineColor = OxyColors.LightGray;

			// y - axis style
			graphModel.DefaultYAxis.MinorGridlineStyle = LineStyle.Solid;
			graphModel.DefaultYAxis.MajorGridlineStyle = LineStyle.Solid;

			graphModel.DefaultYAxis.MajorGridlineThickness = 0.25f;
			graphModel.DefaultYAxis.MinorGridlineThickness = 0.25f;

			graphModel.DefaultYAxis.MajorGridlineColor = OxyColors.LightGray;
			graphModel.DefaultYAxis.MinorGridlineColor = OxyColors.LightGray;

			if (Device.Idiom == TargetIdiom.Tablet)
			{
				graphModel.DefaultFontSize = 20;
				graphModel.DefaultXAxis.FontSize = 20;
				graphModel.DefaultYAxis.FontSize = 20;
				graphModel.LegendTitleFontSize = 20;
				graphModel.LegendFontSize = 20;
			}
			else
			{
				graphModel.DefaultFontSize = 10;
				graphModel.DefaultXAxis.FontSize = 10;
				graphModel.DefaultYAxis.FontSize = 10;
				graphModel.LegendTitleFontSize = 10;
				graphModel.LegendFontSize = 10;
			}



			graphModel.LegendSymbolPlacement = LegendSymbolPlacement.Left;
			graphModel.LegendPosition = LegendPosition.TopLeft;
			graphModel.LegendTitle = "Pulse";
			//graphModel.LegendFontWeight = FontWeights.Normal;
			graphModel.LegendFontWeight = 1;

			this.graphModel.InvalidatePlot(true);
		}

		private async void callAPiToDisplayGetDemographics()
		{
			var isSuccess = await Demographics.sharedInstance.getDemographicFromApi();
			Debug.WriteLine("Demographics.sharedInstance.Height  " + Demographics.sharedInstance.Height);
			if (Demographics.sharedInstance.Height != null)
				userHeight = float.Parse(Demographics.sharedInstance.Height.Split(' ')[0]);
			Debug.WriteLine("userHeight  " + userHeight);
			if (isSuccess)
			{
				this.lblName.Text = Demographics.sharedInstance.getFullName();
				this.lblEmail.Text = Demographics.sharedInstance.Email;
				//      this.Height = Demographics.sharedInstance.get
				// calling async to download the image and setting in to the image
				String imageBase64 = await Demographics.sharedInstance.downloadProfilePic();

				if (imageBase64 != null)
				{
					this.imgProfile.Source = Xamarin.Forms.ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(imageBase64)));
				}
			}
		}


		//void btnSaveClicked(object sender, System.EventArgs e)
		//{
		//  vitalsData.save();
		//}

		void btnBleClicked(Object sender, System.EventArgs e)
		{
			//BLECentralManager.sharedInstance.connectToDevice("PC_300SNT", this);

			//Debug.WriteLine(sender.is);

			//this.bleManager.ScanToConnectToSpotCheck((IBluetoothCallBackUpdatable)this);

			//if (this.btnBle.IsEnabled) {
			//  this.bleManager.ScanToConnectToSpotCheck((IBluetoothCallBackUpdatable)this);
			//  //DependencyService.Get<ICBCentralManager>().ConnectToDevice((IBluetoothCallBackUpdatable)this);
			//}
		}
		int state = 100;
		public void SaveEcgState(int state)
		{
			this.state = state;
		}
		bool Measure_Interruped = false;

        public async void FailedConn(String message, bool isConn, int camefrom)
        {
            layoutLoading.IsVisible = false;
            Debug.WriteLine("FailedConn  mainpage  : " + message);
            if (camefrom == 1)
            {
                bool ret;
				if (Device.Idiom == TargetIdiom.Tablet)
				{
					ret = await DependencyService.Get<IFileHelper>().dispAlert(this.deviceName, message, true, "Yes", "No");
				}
				else
				{
					ret = await DependencyService.Get<IFileHelper>().dispAlert(this.deviceName, message, false, "Yes", "No");
				}
                if (ret)
                {
                    //try to connect again, this time to the second BLE manager
                    try
                    {
                        layoutLoading.IsVisible = true;
                        await BLECentralManager.sharedInstance.ConnectToDevice2(this.deviceName, this);    
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("conn error msg : "+ex.Message);
                    }

                }
            }else{
				//camefrom BLE manager 2
				bool ret;
				if (Device.Idiom == TargetIdiom.Tablet)
				{
					ret = await DependencyService.Get<IFileHelper>().dispAlert(this.deviceName, message, true, "Yes", "No");
				}
				else
				{
					ret = await DependencyService.Get<IFileHelper>().dispAlert(this.deviceName, message, false, "Yes", "No");
				}
				if (ret)
				{
					//try to connect again, this time to the second BLE manager
					try
					{
						layoutLoading.IsVisible = true;
						BLECentralManager.sharedInstance.connectToDevice(this.deviceName, this);
					}
					catch (Exception ex)
					{
						Debug.WriteLine("conn error msg : " + ex.Message);
					}

				}
            }
        }


		public async Task ShowConnection(String message, Boolean isConnected)
		{

			Debug.WriteLine("ShowConnection  mainpage  : "+message);
			
			Device.BeginInvokeOnMainThread(new Action(async () =>
			{
				layoutLoading.IsVisible = false;
                lblLoadingMessage.Text = "Connecting with device...";
			
				if (isConnected)
				{
					setWeight = false;
					isNavigated = true;
				}


				if (!isConnected && this.deviceName == "eBody-Scale")
				{
					getLatestWeight(message);

					/*
					if (Device.Idiom == TargetIdiom.Tablet)
					{
						var ret = await DependencyService.Get<IFileHelper>().dispAlert(deviceName, message, true, "OK", null);
					}
					else
					{
						var ret = await DependencyService.Get<IFileHelper>().dispAlert(deviceName, message, false, "OK", null);
					}
					*/
					//await DisplayAlert(deviceName, message, "OK");
				}
				else
				{
                    Debug.WriteLine("made it to the display!");
                    //await DisplayAlert(deviceName, message, "OK");
					if (Device.Idiom == TargetIdiom.Tablet)
					{
						var ret = await DependencyService.Get<IFileHelper>().dispAlert(deviceName, message, true, "OK", null);
                        //var ret = DependencyService.Get<IFileHelper>().dispAlert(deviceName, message, true, "OK", null);
					    Debug.WriteLine("ret val = " + ret.ToString());
                    }
					else
					{
						var ret = await DependencyService.Get<IFileHelper>().dispAlert(deviceName, message, false, "OK", null);
                        //var ret = DependencyService.Get<IFileHelper>().dispAlert(deviceName, message, false, "OK", null);
					    Debug.WriteLine("ret val = " + ret.ToString());
                    }

				}

                //if (this.deviceName != "eBody-Scale")
                //{
                //await DisplayAlert(deviceName, message, "OK");
                //}
                if (isConnected) {await checkBattery(); }

			}));


			//});
		}

		public async Task checkBattery()
		{
			//then call getBatteryInfo()
			if (deviceName == "PC_300SNT")
			{
				BLECentralManager.sharedInstance.spotServHandler.getBatteryInfo();
			}
			if (deviceName == "PC-100")
			{
				BLECentralManager.sharedInstance.pc100ServHandler.getBatteryInfo();
			}
		}

		async public void ShowMessageOnUI(string message, Boolean isConnected, string title = null)
		{

            Debug.WriteLine("ShowMessageOnUI : "+ message);
			if (title == null)
			{
				if (this.deviceName == "eBody-Scale")
				{
					//getLatestWeight("");

				}
				else
				{
					Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
					{
						if (Device.Idiom == TargetIdiom.Tablet)
						{
							var ret = await DependencyService.Get<IFileHelper>().dispAlert(deviceName, message, true, "OK", null);
						}
						else
						{
							var ret = await DependencyService.Get<IFileHelper>().dispAlert(deviceName, message, false, "OK", null);
						}
						//await DisplayAlert(this.deviceName, message, "OK");
					}));
				}
				//  
			}
			else if (title.Equals("Normal") || title.Equals("Abnormal"))
			{
				Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
				{
                    var message_pad = message;
                    message_pad += "<br/> Do you want to save ECG data and create report?";
					message += "\n Do you want to save ECG data and create report?";
                    bool ret;
					if (Device.Idiom == TargetIdiom.Tablet)
					{
						ret = await DependencyService.Get<IFileHelper>().dispAlert(title, message_pad, true, "Yes", "No");
					}
					else
					{
						ret = await DependencyService.Get<IFileHelper>().dispAlert(title, message, false, "Yes", "No");
					}
                    //var ret = await DisplayAlert(title, message, "Yes", "No");
					Debug.WriteLine("ret === " + ret);
					if (ret)
					{
						if (title.Equals("Normal"))
						{
							vitalsData.ecg = new Reading(null, this.state, 10, false, Task_vars.ecgmessage, null, null, null);
						}
						else
						{
							//Abnormal
							vitalsData.ecg = new Reading(null, this.state, 10, true, Task_vars.ecgmessage, null, null, null);
						}
                        lblLoadingMessage.Text = "Loading...";
                        layoutLoading.IsVisible = true;

                        await Task.Delay(100).ContinueWith(_ =>
                        {
                        });

						vitalsData.sendEcgToServer();

						// sending the Heart rate to server separately
						vitalsData.bpm = new Reading(null, heartRate, 3, false, null, null);
						vitalsData.bpm.Date = vitalsData.ecg.Date;
						vitalsData.sendHeartRateToServer();
						writeToTxt();
						if (Device.Idiom == TargetIdiom.Tablet)
						{
							ecgReportcBtnPad.IsEnabled = true;
						}
						else
						{
							ecgReportcBtn.IsEnabled = true;
						}
						//ecgReportcBtn.IsEnabled = true;
                        Task_vars.comingfrom = "MainPage";

                        startECGReportPage();
					}
					else
					{
						reportDataList.Clear();
						if (Device.Idiom == TargetIdiom.Tablet)
						{
							ecgReportcBtnPad.IsEnabled = false;
						}
						else
						{
							ecgReportcBtn.IsEnabled = false;
						}
						//ecgReportcBtn.IsEnabled = false;
						lineSerie.Points.Clear();
						graphModel.InvalidatePlot(true);

					}
				}));
			}
			else
			{
				Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
				{
					if (Device.Idiom == TargetIdiom.Tablet)
					{
						var ret = await DependencyService.Get<IFileHelper>().dispAlert(title, message, true, "OK", null);
					}
					else
					{
						var ret = await DependencyService.Get<IFileHelper>().dispAlert(title, message, false, "OK", null);
					}
					//await DisplayAlert(title, message, "OK");
					if (title.Equals("Measure Interrupted"))
					{
                        Debug.WriteLine("Measure Interrupted!");
						Measure_Interruped = true;
						countECGPacket = 0;
						ecgTime = 0;

						reportDataList.Clear();
						if (Device.Idiom == TargetIdiom.Tablet)
						{
							ecgReportcBtnPad.IsEnabled = false;
						}
						else
						{
							ecgReportcBtn.IsEnabled = false;
						}
						//ecgReportcBtn.IsEnabled = false;
						lineSerie.Points.Clear();
						graphModel.InvalidatePlot(true);


						EcgcountdownCancle();

					}
					else if (title.Equals("Blood Pressure Measure Error") || message=="Device is not connected. Please connect and try again.")
					{
						lblDia.Text = "-";
						lblSys.Text = "-";
                        if (Device.Idiom == TargetIdiom.Tablet)
                        {
                            NIBPButtonPad.Text = "NIBP Start";
                        }else{
                            NIBPButton.Text = "NIBP Start";
                        }
						
						isBPMeasuring = false;
					}
					else if (title.Equals("Poor Signal"))
					{
						countECGPacket = 0;
						ecgTime = 0;
						reportDataList.Clear();
						if (Device.Idiom == TargetIdiom.Tablet)
						{
							ecgReportcBtnPad.IsEnabled = false;
						}
						else
						{
							ecgReportcBtn.IsEnabled = false;
						}
						lineSerie.Points.Clear();
						graphModel.InvalidatePlot(true);
					}
				}));
			}

		}

		public async void updateGlucoseReading(decimal gluReading, string unit)
		{
			//Conversion math mmol/L = mg/dL / 18
			Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
			{
				if (unit == "mmol/L")
				{
					vitalsData.glucose = new Reading(null, Math.Round((decimal)gluReading * 18, 1), 8, false, null, null);
					vitalsData.glucose.MetricValue = Math.Round(gluReading, 1);
				}
				else
				{
					vitalsData.glucose = new Reading(null, gluReading, 8, false, null, null);
					vitalsData.glucose.MetricValue = gluReading;
				}

                bool ret;
				if (Device.Idiom == TargetIdiom.Tablet)
				{
					ret = await DependencyService.Get<IFileHelper>().dispAlert("Measuring Result", "Do you want to save the result?<br/> "
											 + "GLU:  " + vitalsData.glucose.MetricValue + unit, true, "Yes", "No");
				}
				else
				{
					ret = await DependencyService.Get<IFileHelper>().dispAlert("Measuring Result", "Do you want to save the result?\n "
											 + "GLU:  " + vitalsData.glucose.MetricValue + unit, false, "Yes", "No");
				}
				//var ret = await DisplayAlert("Measuring Result", "Do you want to save the result?\n "
											// + "GLU:  " + vitalsData.glucose.MetricValue + unit, "Yes", "No");
				if (!ret)
				{
					return;
				}

				vitalsData.sendToServer_Glucose();
			}));
			Device.BeginInvokeOnMainThread(() =>
			{
				lblGlucose.Text = (unit == "mmol/L") ? vitalsData.glucose.EnglishValue.ToString() : vitalsData.glucose.MetricValue.ToString();
				lblUnitGlucose.Text = unit;
			});
		}

		public void updateDeviceConnected(String deviceName, bool isConnected)
		{
			System.Diagnostics.Debug.WriteLine("deviceName: " + deviceName);
		}

		//new method to reset spo2 values when it starts


		async public void noticeEndOfReadingSpo2()
		{

			Debug.WriteLine("noticeEndOfReadingSpo2");
			pulseTime = 0.0f;

			initBpm = true;
			initBpmWaveForm();
			if (isupLoadedSPO2)
			{
				isupLoadedSPO2 = false;
				return;
			}

			if (vitalsData != null && (vitalsData.spo2 != null || vitalsData.spo2 != null))
			{
				Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
				{
					string message = (vitalsData.spo2 == null ? "" : "SpO2: " + vitalsData.spo2.EnglishValue) + (vitalsData.bpm == null ? "" : "  Bpm: " + vitalsData.bpm.EnglishValue);
					bool ret;
					//var ret = await DisplayAlert("Measuring Result", "Do you want to save the result?\n " + message, "Yes", "No");
					if (Device.Idiom == TargetIdiom.Tablet)
					{
						ret = await DependencyService.Get<IFileHelper>().dispAlert("Measuring Result", "Do you want to save the result?<br/> "+message, true, "Yes", "No");
					}
					else
					{
						ret = await DependencyService.Get<IFileHelper>().dispAlert("Measuring Result", "Do you want to save the result?\n " + message, false, "Yes", "No");
					}


                    if (ret)
					{
						vitalsData.sendToServer_SPO2_PI_BPM();
					}
					else
					{
						Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
						{
							lblBpm.Text = "-";
							lblSpo2.Text = "-";
							lblPerfusionIndex.Text = "-";
						});
					}
				}));
			}



			//lineSerie.Points.Clear();
			//graphModel.InvalidatePlot(true);
			//  graphModel.DefaultXAxis.IsPanEnabled = false;
		}

		public async void updateTemperature(decimal temperature)
		{

			vitalsData.temperature = new Reading(null, temperature, 4, false, null, null);
			if (vitalsData != null && vitalsData.temperature != null)
			{
				Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
				{
					string message = "Temperature: " + temperature.ToString() + "°F / " + ConvertFahrenheitToCelsius((double)this.vitalsData.temperature.EnglishValue) + "°C";
                    bool ret;
					//var ret = await DisplayAlert("Measuring Result", "Do you want to save the result?\n " + message, "Yes", "No");
					if (Device.Idiom == TargetIdiom.Tablet)
					{
						ret = await DependencyService.Get<IFileHelper>().dispAlert("Measuring Result", "Do you want to save the result?<br/> " + message, true, "Yes", "No");
					}
					else
					{
						ret = await DependencyService.Get<IFileHelper>().dispAlert("Measuring Result", "Do you want to save the result?\n " + message, false, "Yes", "No");
					}
                    if (ret)
					{
						vitalsData.sendToServerTemperature();
					}


					Device.BeginInvokeOnMainThread(() =>
					{
						Debug.WriteLine("BeginInvokeOnMainThread  isCelcious = " + isCelcious);

						if (isCelcious)
						{
							lblTemperature.Text = ConvertFahrenheitToCelsius((double)this.vitalsData.temperature.EnglishValue).ToString();
						}
						else
						{
							lblTemperature.Text = temperature.ToString();
						}
					});
				}));
			}

		}
        bool setWeight = false;
		public async void updated_Weight(decimal weight)
		{
			this.vitalsData.weight = new Reading(null, weight, 5, false, null, null);

			Device.BeginInvokeOnMainThread(new Action(async () =>
			{
				if (!isKg)
				{
					lblWeight.Text = weight.ToString();

				}
				else
				{
					lblWeight.Text = ConvertLBToKG((double)weight).ToString();
				}
            }));
			if (!setWeight)
			{
				setWeight = true;

				//wait 5 seconds
				//await Task.Delay(5000);
				//getLatestWeight("");
			}
            


		}


		// ECG waveform portion
		public PlotModel graphModel { get; set; }
		DataPoint lastDataPointPrev;

		float xMin = 0.0f;
		float ecgTime = 0.0f;
		int countECGPacket = 0;
		//  int countEcgReport = 0;


		List<int> reportDataList = new List<int>();
		int heartRate = 0;
		int ECG = 0;
		public void updateECGEnded(int bpm, int ecg)
		{
			Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
			{
				countECGPacket = 0;
				//  countEcgReport = 0;
				ecgTime = 0.0f;
				heartRate = bpm;
				if (state == 17) return;
				if (Device.Idiom == TargetIdiom.Tablet)
				{
					graphModel.DefaultFontSize = 20;
					graphModel.DefaultXAxis.FontSize = 20;
					graphModel.DefaultYAxis.FontSize = 20;
				}
				else
				{
					graphModel.DefaultFontSize = 10;
					graphModel.DefaultXAxis.FontSize = 10;
					graphModel.DefaultYAxis.FontSize = 10;
				}
				graphModel.DefaultXAxis.IsPanEnabled = false;
				graphModel.DefaultXAxis.Minimum = 0;
				graphModel.DefaultXAxis.Maximum = 50;

				graphModel.DefaultYAxis.Minimum = 0;
				graphModel.DefaultYAxis.Maximum = 255;
				graphModel.InvalidatePlot(true);
				EcgcountdownCancle();
				if (Measure_Interruped)
				{
					return;
				}

				ECG = ecg;
			}));
			/*
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        lblBpm.Text = bpm.ToString();
                    });

                // sending the Heart rate to server separately
                    vitalsData.bpm = new Reading(null, bpm, 3);
                    vitalsData.sendHeartRateToServer();
                    ecgReportcBtn.IsEnabled = true;

                    vitalsData.ecg = new Reading(null, ecg, 10);
                    vitalsData.sendEcgToServer();

                    writeToTxt();
        */
			//  updateECGEnded_Report();
		}
		int N = 0;
		private void countDown()
		{
			progressBar.IsVisible = true;

			if (progressBar.AnimationIsRunning("SetProgress"))
			{
				progressBar.AbortAnimation("SetProgress");
			}
			else
			{
				progressBar.Animate("SetProgress", (arg) => { progressBar.Progress = arg; }, 0, 34000, Easing.Linear);
			}

		}
		private async Task ECGcountdown()
		{
#if false
            N = 30;
                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    if (N == -1) return false;
                    countDownLabel.Text = "Please keep measuring " + N + " seconds";
                    N--;
                    return true; // True = Repeat again, False = Stop the timer
                        });
#endif

			Device.BeginInvokeOnMainThread(() => countDown());

			for (; N >= 0; N--)
			{
				countDownLabel.Text = "Please keep measuring " + N + " seconds";
				await Task.Delay(1000);
			}

		}
		private void EcgcountdownCancle()
		{
			N = -1;
			Debug.WriteLine("EcgcountdownCancle timer=" + timer);
			if (timer != null && timer.running)
			{
				timer.Stop();
				//  timer = null;
				Debug.WriteLine("timer = " + timer);
			}
			if (progressBar != null && progressBar.AnimationIsRunning("SetProgress"))
			{
				progressBar.AbortAnimation("SetProgress");
			}
			//countDown();
			progressBar.IsVisible = false;
			countDownLabel.IsVisible = false;
		}
		CommonMethod.MySystemDeviceTimer timer;

		private async Task initEcgCountdown()
		{
			await Task.Run(() =>
			{
				if (timer == null)
				{
					Debug.WriteLine("timer   =============" + timer);

					timer = new CommonMethod.MySystemDeviceTimer(TimeSpan.FromSeconds(8), async () =>
					{
						Debug.WriteLine("StartTsure_Interruped==" + Measure_Interruped);
						if (!Measure_Interruped)
						{
                            await ECGcountdown();
						}
						return;
					});
					timer.FireOnce();
				}
				else
				{
					timer.FireOnce();
				}
			});
		}
		public async void updateECGPacket(List<int> ecgPacket)
		{
			try
			{
                //Debug.WriteLine("countECGPacket = "+countECGPacket);
				if (Measure_Interruped)
				{
                    Debug.WriteLine("Measure Interrupted.");
					Measure_Interruped = false;
					countECGPacket = 0;
					ecgTime = 0;
					N = -1;
				}
				if (countECGPacket > 90)
				{
					reportDataList.AddRange(ecgPacket);
				}

				if (countECGPacket == 0)
				{
					N = 30;
					Debug.WriteLine("countECGPacket   =============" + countECGPacket);
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
                    {
						if (Device.Idiom == TargetIdiom.Tablet)
						{
							graphModel.DefaultFontSize = 20;
							graphModel.DefaultXAxis.FontSize = 20;
							graphModel.DefaultYAxis.FontSize = 20;
							graphModel.LegendTitleFontSize = 20;
							graphModel.LegendFontSize = 20;
						}
						else
						{
							graphModel.DefaultFontSize = 10;
							graphModel.DefaultXAxis.FontSize = 10;
							graphModel.DefaultYAxis.FontSize = 10;
							graphModel.LegendTitleFontSize = 10;
							graphModel.LegendFontSize = 10;
						}
                       
                        graphModel.LegendTitle = "ECG";
                        countDownLabel.Text = "Stabilizing reading, please continue.";
                        countDownLabel.IsVisible = true;
                        progressBar.IsVisible = false;


                        //await initEcgCountdown();
                        await initEcgCountdown();

                        if (Device.Idiom == TargetIdiom.Tablet)
                        {
                            ecgReportcBtnPad.IsEnabled = false;
                        }
                        else
                        {
                            ecgReportcBtn.IsEnabled = false;
                        }

						// reseting the graphmodel for ecg waveform

                        graphModel.DefaultXAxis.IsPanEnabled = false;
                        xMin = 0;
                        graphModel.DefaultXAxis.Minimum = 0;
                        graphModel.DefaultXAxis.Maximum = 6.0;

                        graphModel.DefaultYAxis.Minimum = 0;
                        graphModel.DefaultYAxis.Maximum = 255;

                        lineSerie.Points.Clear();
                        //  graphModel.DefaultXAxis.IsPanEnabled = false;
                        graphModel.InvalidatePlot(true);

                        Debug.WriteLine("countECGPacket   end =============" + countECGPacket);
                    }));


				}
				countECGPacket++;

				Device.BeginInvokeOnMainThread(() =>
				{
					for (int i = 0; i < ecgPacket.Count; i++)
					{
						ecgTime = ecgTime + 0.006666666667f;
						lineSerie.Points.Add(new DataPoint(ecgTime, ecgPacket[i]));
					}

					// find the end and save the screen into pdf
					if (ecgTime > graphModel.DefaultXAxis.Maximum)
					{
						//graphModel.PlotAreaBorderColor = OxyColors.Transparent;
						//DependencyService.Get<IFileHelper>().saveToPdf(graphModel, "ecgReport_" + (countEcgReport++) + ".pdf");
						//graphModel.PlotAreaBorderColor = OxyColors.Black;

						//lineSerie.Points.Clear();

						xMin = ecgTime;
						graphModel.DefaultXAxis.Minimum = xMin;
						graphModel.DefaultXAxis.Maximum = xMin + 6.0;

						graphModel.InvalidatePlot(true);

						//graphModel.TextColor = OxyColors.Transparent;
						//graphModel.TitleColor = OxyColors.Transparent;
						//graphModel.LegendTextColor = OxyColors.Transparent;
					}
					else
					{
						if (graphModel != null)
						{
							graphModel.InvalidatePlot(true);
						}
					}
				});

			}
			catch (Exception ex)
			{
				Debug.WriteLine("Exception :" + ex);
			}

			//      updateECGPacket_Report(ecgPacket);
		}

		// Spo2 waveform portion
		float pulseTime = 0.0f;
		bool initBpm = true;

		private void initBpmWaveForm()
		{
            Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
            {
                Debug.WriteLine("initBpmWaveForm");
                if (graphModel == null) return;

                if (graphModel.DefaultXAxis != null)
                {
                    Debug.WriteLine("initBpmWaveForm ing");
                    //initBpm = false;
                    pulseTime = 0.0f;
                    graphModel.DefaultXAxis.IsPanEnabled = false;

                    graphModel.LegendTitle = "Pulse";

					if (Device.Idiom == TargetIdiom.Tablet)
					{
						graphModel.DefaultFontSize = 20;
						graphModel.DefaultXAxis.FontSize = 20;
						graphModel.DefaultYAxis.FontSize = 20;
						graphModel.LegendTitleFontSize = 20;
						graphModel.LegendFontSize = 20;
					}
					else
					{
						graphModel.DefaultFontSize = 10;
						graphModel.DefaultXAxis.FontSize = 10;
						graphModel.DefaultYAxis.FontSize = 10;
						graphModel.LegendTitleFontSize = 10;
						graphModel.LegendFontSize = 10;
					}
                    graphModel.DefaultYAxis.Minimum = -10;
                    graphModel.DefaultYAxis.Maximum = 265;

                    xMin = 0;
                    graphModel.DefaultXAxis.Minimum = xMin;
                    graphModel.DefaultXAxis.Maximum = xMin + 3.0;
                    if (lineSerie != null) lineSerie.Points.Clear();
                    graphModel.InvalidatePlot(true);
                    graphModel.DefaultXAxis.IsZoomEnabled = false;
                    graphModel.DefaultYAxis.IsZoomEnabled = false;
                }
            }));
		}
		public void updateBpmWaveform(int bpm)
		{
			if (graphModel == null) return;

            Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
            {
                try
                {
                    if (initBpm && graphModel.DefaultXAxis != null)
                    {
                        Debug.WriteLine("updateBpmWaveform initBpmWaveForm ing");

                        initBpm = false;
                        pulseTime = 0.0f;
                        graphModel.LegendFontSize = 10;
                        graphModel.LegendTitleFontSize = 20;
                        graphModel.LegendTitle = "Pulse";

						if (Device.Idiom == TargetIdiom.Tablet)
						{
							graphModel.DefaultFontSize = 20;
							graphModel.DefaultXAxis.FontSize = 20;
							graphModel.DefaultYAxis.FontSize = 20;
						}
						else
						{
							graphModel.DefaultFontSize = 10;
							graphModel.DefaultXAxis.FontSize = 10;
							graphModel.DefaultYAxis.FontSize = 10;
						}
                        graphModel.DefaultYAxis.Minimum = -10;
                        graphModel.DefaultYAxis.Maximum = 265;

                        xMin = 0;
                        graphModel.DefaultXAxis.Minimum = xMin;
                        graphModel.DefaultXAxis.Maximum = xMin + 3.0;

                        lineSerie.Points.Clear();
                        graphModel.DefaultXAxis.IsPanEnabled = false;

                        graphModel.InvalidatePlot(true);
                        //graphModel.DefaultXAxis.IsZoomEnabled = false;
                        //graphModel.DefaultYAxis.IsZoomEnabled = false;
                    }

                    //  Debug.WriteLine("pulseTime = " + pulseTime);
                    //  Debug.WriteLine(" graphModel.DefaultXAxis.Maximum = " +  graphModel.DefaultXAxis.Maximum);
                    if (pulseTime > graphModel.DefaultXAxis.Maximum)
                    {
                        lineSerie.Points.Clear();
                        xMin = pulseTime;
						if (Device.Idiom == TargetIdiom.Tablet)
						{
							graphModel.DefaultFontSize = 20;
							graphModel.DefaultXAxis.FontSize = 20;
							graphModel.DefaultYAxis.FontSize = 20;
						}
						else
						{
							graphModel.DefaultFontSize = 10;
							graphModel.DefaultXAxis.FontSize = 10;
							graphModel.DefaultYAxis.FontSize = 10;
						}
                        graphModel.DefaultXAxis.Minimum = xMin;
                        graphModel.DefaultXAxis.Maximum = xMin + 3.0;
                    }

                    pulseTime = pulseTime + 0.02f;
                    //  Debug.WriteLine("lineSerie.point.add  bpm = " +bpm);
                    if (bpm != 0)
                        lineSerie.Points.Add(new DataPoint(pulseTime, bpm));

                    graphModel.InvalidatePlot(true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception on updateting ui:" + ex.Message);
                }
            }));
		}

		public void updatingPressureMeanTime(int pressure)
		{
			//Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			//{
			//  lblPressure.Text = pressure.ToString() + " mmHg";
			//});
		}
		public void SPO2_readingUpload()
		{

		}
		public async void SPO2_readingCompleted(int sp02, int bpm, float perfusionIndex)
		{
			Debug.WriteLine("SPO2_readingCompleted");
			if (isupLoadedSPO2) return;
			this.vitalsData.spo2 = new Reading(null, sp02, 2, false, null, null);
			this.vitalsData.bpm = new Reading(null, bpm, 3, false, null, null);
			//this.vitalsData.bpSys = new Reading("Perfusion Index", perfusionIndex,2);

			Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
			{
				if (bpm == 0)
				{
					lblBpm.Text = "...";
				}
				else
				{
					lblBpm.Text = bpm.ToString();
				}

				if (sp02 == 0)
				{
					lblSpo2.Text = "...";
				}
				else
				{
					lblSpo2.Text = sp02.ToString();
				}

				if (perfusionIndex > 0)
				{
					lblPerfusionIndex.Text = perfusionIndex.ToString();
				}
				else
				{
					lblPerfusionIndex.Text = "...";
				}
            }));
			if (isBPMeasuring && !isupLoadedSPO2)
			{

				if (this.deviceName.Equals("PC_300SNT"))
				{
					BLECentralManager.sharedInstance.spotServHandler.stopMeasuringSpo2();
				}
				else if (this.deviceName.Equals("PC-100"))
				{
					BLECentralManager.sharedInstance.pc100ServHandler.stopMeasuringSpo2();
				}
				isupLoadedSPO2 = true;
				if (vitalsData != null && (vitalsData.spo2 != null || vitalsData.spo2 != null))
				{
					Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
					{
						string message = (vitalsData.spo2 == null ? "" : "SpO2: " + vitalsData.spo2.EnglishValue) + (vitalsData.bpm == null ? "" : "  Bpm: " + vitalsData.bpm.EnglishValue);
                        bool ret;
						//var ret = await DisplayAlert("Measuring Result", "Do you want to save the result?\n " + message, "Yes", "No");
						if (Device.Idiom == TargetIdiom.Tablet)
						{
							ret = await DependencyService.Get<IFileHelper>().dispAlert("Measuring Result", "Do you want to save the result?<br/> " + message, true, "Yes", "No");
						}
						else
						{
							ret = await DependencyService.Get<IFileHelper>().dispAlert("Measuring Result", "Do you want to save the result?\n " + message, false, "Yes", "No");
						}
                        if (ret)
						{
							vitalsData.sendToServer_SPO2_PI_BPM();
						}
						else
						{
							//  isupLoadedSPO2 = false;

							Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
							{
								lblBpm.Text = "-";
								lblSpo2.Text = "-";
								lblPerfusionIndex.Text = "-";
							});
						}
					}));
				}
			}
		}

		public async void SYS_DIA_BPM_updated(int bpsys, int bpdia, int bpm)
		{
			isBPMeasuring = true;

            if (bpm == 9999)
            {
                BLECentralManager.sharedInstance.pc100ServHandler.getBPreading();
            }

			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				if (bpm != 0)
				{
					lblBpm.Text = bpm.ToString();
				}
				//Debug.WriteLine("bpdia" + bpdia);
				lblDia.Text = (bpdia != 0 && bpdia != 170) ? bpdia.ToString() : "-";
				lblSys.Text = bpsys != 0 ? bpsys.ToString() : "-";
			});
			//Debug.WriteLine("bpsys = " + bpsys + ", dia  = " + bpdia + "  bpm =" + bpm);
			if (bpsys == 0 || bpdia == 0 || bpdia == 170 || bpm == 0)
			{
				return;
			}

			if (bpsys < bpdia)
			{
				Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
				{
					if (Device.Idiom == TargetIdiom.Tablet)
					{
						var ret = await DependencyService.Get<IFileHelper>().dispAlert("Measuring Error", "Abnormal measurement results. bpsys= " + bpsys + "bpdia= " + bpdia, true, "OK", null);
					}
					else
					{
						var ret = await DependencyService.Get<IFileHelper>().dispAlert("Measuring Error", "Abnormal measurement results. bpsys= " + bpsys + "bpdia= " + bpdia, false, "OK", null);
					}
					//await DisplayAlert("Measuring Error", "Abnormal measurement results. bpsys= " + bpsys + "bpdia=" + bpdia, "OK");
					lblBpm.Text = "-";
					lblDia.Text = "-";
					lblSys.Text = "-";
				}));
			}
			else
			{
				string message = "SYS: " + bpsys + " DIA: " + bpdia + " Bpm: " + bpm;
				Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
				{
                    bool ret;
					if (Device.Idiom == TargetIdiom.Tablet)
					{
						ret = await DependencyService.Get<IFileHelper>().dispAlert("Measuring Result", "Do you want to save the result?<br/> " + message, true, "Yes", "No");
					}
					else
					{
						ret = await DependencyService.Get<IFileHelper>().dispAlert("Measuring Result", "Do you want to save the result?\n " + message, false, "Yes", "No");
					}
					//var ret = await DisplayAlert("Measuring Result", "Do you want to save the result?\n " + message, "Yes", "No");
					if (ret)
					{
						this.vitalsData.bpDia = new Reading("Diastolic", bpdia, 1, false, null, null);
						this.vitalsData.bpSys = new Reading("Systolic", bpsys, 1, false, null, null);
						this.vitalsData.bpm = new Reading(null, bpm, 3, false, null, null);
						vitalsData.sendToServer_SYS_DIA();
					}
					else
					{
						lblBpm.Text = "-";
						lblDia.Text = "-";
						lblSys.Text = "-";
					}
				}));
			}
			isBPMeasuring = false;
			Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
			{
				NIBPButton.Text = "NIBP Start";
			}));
		}

		public static double ConvertKGToLB(double f)
		{
			//1 kg = 2.20462262185 lb
			return Math.Round(f * 2.20462262185, 1);
		}
		public static double ConvertLBToKG(double f)
		{
			//1 lb = 0.45359237 kg
			return Math.Round(f * 0.45359237, 1);
		}

		public static double ConvertFahrenheitToCelsius(double f)
		{
			return Math.Round((5.0 / 9.0) * (f - 32), 1);
		}

		void btnLogOutClicked(object sender, System.EventArgs e)
		{
			Debug.WriteLine(" log out");
			BLECentralManager.sharedInstance.disConnectAll();
			Demographics.sharedInstance.calibratedReadingList.Clear();
			this.Navigation.PopModalAsync(true);
		}

		void btnPrevClicked(object sender, System.EventArgs e)
		{
			Navigation.PopAsync();
		}

		void btnNIBPStartClicked(object sender, System.EventArgs e)
		{
			Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
			{
    			if (BLECentralManager.sharedInstance.checkIfDeviceScanned(this.deviceName))
    			{
    				
    					var btn = (Button)sender;

    					if (btn.Text == "NIBP Start")
    					{

    						btn.Text = "NIBP Stop";

    						switch (this.deviceName)
    						{

    							case "PC_300SNT":
    								{
    									BLECentralManager.sharedInstance.spotServHandler.startMeasuringBP();
    									break;
    								}

    							case "PC-100":
    								{
    									BLECentralManager.sharedInstance.pc100ServHandler.startMeasuringBP();
    									break;
    								}
    						}
    					}
    					else
    					{

    						btn.Text = "NIBP Start";

    						switch (this.deviceName)
    						{

    							case "PC_300SNT":
    								{
    									BLECentralManager.sharedInstance.spotServHandler.stoptMeasuringBP();
    									break;
    								}

    							case "PC-100":
    								{
    									BLECentralManager.sharedInstance.pc100ServHandler.stoptMeasuringBP();
    									break;
    								}
    						}
    					}
    				
    			}
    			else
    			{
    				//device not connected error message
    				await ShowConnection("Device is not connected.", false);
    			}
             }));                                                           

			//bleManager.startMeasuringBP();
			//DependencyService.Get<ICBCentralManager>().startMeasuringBP();
		}
		void btnEcgReportClicked(object sender, System.EventArgs e)
		{
			Debug.WriteLine("vitalsData = " + vitalsData);
            //  bool ret = await DependencyService.Get<IFileHelper>().sentToEmail("" + ".pdf");
            //==================
            Task_vars.comingfrom = "MainPage";
			startECGReportPage();
		}



		void btnEcgStartClicked(object sender, System.EventArgs e)
		{
			BLECentralManager.sharedInstance.spotServHandler.startEcgMeasuring();
			//DependencyService.Get<ICBCentralManager>().startEcgMeasuring();
		}

		void btnListClicked(object sender, System.EventArgs e)
		{
			//      ecgReportInstance = EcgReport.Instance;
			//      ecgReportInstance.setMainPage(this);

			if (vitalsData.ecg != null)
			{
				string date = vitalsData.ecg.Date.ToString("MM/dd/yyyy hh:mm:tt");
				fileName = Regex.Replace(date, @"\s+", "");//dateTime.Trim(' ');
				fileName = Regex.Replace(fileName, @"[/:]+", "");
				lashEcgFile = fileName;
			}
           
            if (Device.Idiom == TargetIdiom.Tablet)
            {
                Debug.WriteLine("Went to Pad page!");
                var newPage = new ParametersPageLocalPad();
				//var newPage = new ParametersPage();
				newPage.Title = "Parameter List Screen";
				this.Navigation.PushAsync(newPage);
            }else{
                Debug.WriteLine("Went to phone page!");
                var newPage = new ParametersPageLocal();
				//var newPage = new ParametersPage();
				newPage.Title = "Parameter List Screen";
				this.Navigation.PushAsync(newPage);
            }
			
			
		}

		void btnViewProfileClicked(object sender, System.EventArgs e)
		{
			var newPage = new UserProfile();
			newPage.Title = "My Account";
			this.Navigation.PushAsync(newPage);
		}

		void btnFareinheitClicked(Object sender, System.EventArgs e)
		{
			btnFareinheit.TextColor = (Color)App.Current.Resources["colorThemeBlue"];
			btnCelcious.TextColor = Color.Gray;
			isCelcious = false;

			try
			{
				lblTemperature.Text = this.vitalsData.temperature.EnglishValue.ToString();
			}
			catch (Exception)
			{
				Debug.WriteLine("exception nullpointer");
			}
		}
		bool isCelcious = false;
		void btnCelciusClicked(Object sender, System.EventArgs e)
		{
			isCelcious = true;
			btnFareinheit.TextColor = Color.Gray;
			btnCelcious.TextColor = (Color)App.Current.Resources["colorThemeBlue"];

			try
			{
				lblTemperature.Text = ConvertFahrenheitToCelsius((double)this.vitalsData.temperature.EnglishValue).ToString();
			}
			catch (Exception)
			{
				Debug.WriteLine("conversion exception");
			}
		}


		void btnLbsClicked(Object sender, System.EventArgs e)
		{
			isKg = false;
			btnLbs.TextColor = (Color)App.Current.Resources["colorThemeBlue"];
			btnKgs.TextColor = Color.Gray;
			try
			{
				lblWeight.Text = ((double)this.vitalsData.weight.EnglishValue).ToString();
			}
			catch (Exception)
			{
				Debug.WriteLine("exception nullpointer");
			}
		}

		void btnKgsClicked(Object sender, System.EventArgs e)
		{
            //var val = await DependencyService.Get<IFileHelper>().dispAlert("PC_300SNT","Do you want to take a measurement?", true, "Yes", "No");

            //await DependencyService.Get<IFileHelper>().setEmailClient();
            //Debug.WriteLine("alert value = "+val.ToString());

			isKg = true;
			btnKgs.TextColor = (Color)App.Current.Resources["colorThemeBlue"];
			btnLbs.TextColor = Color.Gray;

			try
			{
				lblWeight.Text = ConvertLBToKG((double)this.vitalsData.weight.EnglishValue).ToString();
			}
			catch (Exception)
			{
				Debug.WriteLine("exception nullpointer");
			}
		}
		async void getLatestWeight(string m)
		{
            Debug.WriteLine("latest weight msg:"+m);
            if (this.vitalsData.weight != null)
            {
				string message = "Weight: " + (double)this.vitalsData.weight.EnglishValue + "Lbs / "
							  + ConvertLBToKG((double)this.vitalsData.weight.EnglishValue) + "Kg";
				Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
				{
					bool ret;

					if (Device.Idiom == TargetIdiom.Tablet)
					{
						ret = await DependencyService.Get<IFileHelper>().dispAlert("Measuring Result", "Do you want to save the result?<br/> " + message, true, "Yes", "No");
					}
					else
					{
						ret = await DependencyService.Get<IFileHelper>().dispAlert("Measuring Result", "Do you want to save the result?\n " + message, false, "Yes", "No");
					}

					//ret = await DisplayAlert("Measuring Result", "Do you want to save the result?\n " + message, "Yes", "No");
					if (!ret)
					{
						if (m != "")
						{
							if (Device.Idiom == TargetIdiom.Tablet)
							{
								var val = await DependencyService.Get<IFileHelper>().dispAlert(deviceName, m, true, "OK", null);
							}
							else
							{
								var val = await DependencyService.Get<IFileHelper>().dispAlert(deviceName, m, false, "OK", null);
							}
							//await DisplayAlert(deviceName, m, "OK");
						}
						return;
					}

					double weightReading = 0;

					if (logcalParameteritem.localhashmap.Count() > 0 && logcalParameteritem.localhashmap.ContainsKey(5))
					{
						var list = logcalParameteritem.localhashmap[5];
						foreach (var val in logcalParameteritem.localhashmap[5])
						{
							string[] weights = val.firstItem.Split('/');
							weightReading = Convert.ToDouble(weights[0]);
							break;
						};
					}
					else
					{
						if (ParametersPageLocal.allReadings == null)
						{
							int index = Task.WaitAny(Task_vars.tasks);
							//ParametersPageLocal.allReadings = await Reading.GetAllReadingsFromService();
						}
						var allCategoryReading5 = (from reading in ParametersPageLocal.allReadings
												   where reading.CategoryId == 5
												   select reading).Take(1);


						Debug.WriteLine("weightReading = " + allCategoryReading5);
						foreach (var reading in allCategoryReading5)
						{
							weightReading = (double)Math.Round((decimal)reading.EnglishValue, 1);
						}
					}

					//  var allReadings = await Reading.GetAllReadingsFromService();



					double diff = Math.Abs((double)Math.Round((double)this.vitalsData.weight.EnglishValue - weightReading, 1));
					//Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
					//{

					if (diff < 1)
					{
						var myEmoji = "\U0001F60A";
						if (Device.Idiom == TargetIdiom.Tablet)
						{
							var val = await DependencyService.Get<IFileHelper>().dispAlert("No Change", myEmoji + "Looking good.", true, "OK", null);
						}
						else
						{
							var val = await DependencyService.Get<IFileHelper>().dispAlert("No Change", myEmoji + "Looking good.", false, "OK", null);
						}
						//await DisplayAlert("No Change", myEmoji + "Looking good.", "OK");
					}
					else if (weightReading > (double)this.vitalsData.weight.EnglishValue)
					{
						var myEmoji = "\U0001F600";
						if (Device.Idiom == TargetIdiom.Tablet)
						{
							var val = await DependencyService.Get<IFileHelper>().dispAlert("Lost Weight", myEmoji + " Good job! you lost " + diff + " pounds!", true, "OK", null);
						}
						else
						{
							var val = await DependencyService.Get<IFileHelper>().dispAlert("Lost Weight", myEmoji + " Good job! you lost " + diff + " pounds!", false, "OK", null);
						}
						//await DisplayAlert("Lost Weight", myEmoji + " Good job! you lost " + diff + " pounds!", "OK");
					}
					else
					{

						var myEmoji = "\U0001F61F";
						if (Device.Idiom == TargetIdiom.Tablet)
						{
							var val = await DependencyService.Get<IFileHelper>().dispAlert("Gained Weight", myEmoji + " OOPS! You gained " + diff + " pounds!", true, "OK", null);
						}
						else
						{
							var val = await DependencyService.Get<IFileHelper>().dispAlert("Gained Weight", myEmoji + " OOPS! You gained " + diff + " pounds!", false, "OK", null);
						}
						//await DisplayAlert("Gained Weight", myEmoji + " OOPS! You gained " + diff + " pounds!", "OK");

					}
					this.vitalsData.sendToServerWeight(userHeight);
					if (m != "")
					{
						if (Device.Idiom == TargetIdiom.Tablet)
						{
							var val = await DependencyService.Get<IFileHelper>().dispAlert(deviceName, m, true, "OK", null);
						}
						else
						{
							var val = await DependencyService.Get<IFileHelper>().dispAlert(deviceName, m, false, "OK", null);
						}
						//await DisplayAlert(deviceName, m, "OK");
					}
				}));
			}else{
                //if it is null, then we disconnected before we ever got any data
                Xamarin.Forms.Device.BeginInvokeOnMainThread(new Action(async () =>
                {
                    bool ret;

                    if (Device.Idiom == TargetIdiom.Tablet)
                    {
                        ret = await DependencyService.Get<IFileHelper>().dispAlert("Scale Connection Error", "Unable to get reading from scale. Please try to connect again.", true, "OK", null);
                    }
                    else
                    {
                        ret = await DependencyService.Get<IFileHelper>().dispAlert("Scale Connection Error", "Unable to get reading from scale. Please try to connect again.", false, "OK", null);
                    }
                }));
            }			  
		}

		public async void startECGReportPage()
		{
			progressBar.IsVisible = false;
			countDownLabel.IsVisible = false;
			//  creatReportTitle();
			var newPage = new EcgReport(fileName, Demographics.sharedInstance.FirstName, false, this);
			newPage.Title = "ECG Report";
            layoutLoading.IsVisible = false;
            lblLoadingMessage.Text = "Connecting with device...";
			await this.Navigation.PushAsync(newPage);
			//  lineSerie.Points.Clear();

		}

		private void writeToTxt()
		{
			Debug.WriteLine("Writ to txt");
			Demographics demo = Demographics.sharedInstance;
			if (vitalsData.ecg == null)
			{
				return;
			}
			string name = demo.FirstName + " " + demo.MiddleName + " " + demo.LastName;
			string birthday = String.Format("{0:MM/dd/yyyy}", demo.DateOfBirth);
			int age = DateTime.Now.Year - DateTime.Parse(birthday).Year;

			string report_State = state == 0 ? "Normal" : "Abnormal";
			string date = vitalsData.ecg.Date.ToString("MM/dd/yyyy hh:mm:tt");
			string explanation = CommonMethod.sharedInstance.getExplanation(state);

			string Recorded = vitalsData.ecg.Date.DayOfWeek.ToString() + " ," + date;


			fileName = Regex.Replace(date, @"\s+", "");//dateTime.Trim(' ');
			fileName = Regex.Replace(fileName, @"[/:]+", "");
			lashEcgFile = fileName;

			DependencyService.Get<IFileHelper>().setEcgInof(name, birthday + " (" + age + "yrs)", report_State, Recorded,
															explanation, heartRate + "bpm", "30s");

			DependencyService.Get<IFileHelper>().saveTotxt(reportDataList, null, null, fileName);
			reportDataList.Clear();
		}

		private void resizeUI()
		{ 
			if (Device.Idiom == TargetIdiom.Tablet)
			{
				FakeToolbar.HeightRequest = 75 * Screensize.heightfactor;
				titlebtn.FontSize = 30 * Screensize.heightfactor;
                backbtn.FontSize = 30 * Screensize.heightfactor;

                imgProfile.WidthRequest = 160 * Screensize.widthfactor;
				imgProfile.HeightRequest = 192 * Screensize.heightfactor;
				lblName.FontSize = 24 * Screensize.heightfactor;
				lblEmail.FontSize = 21 * Screensize.heightfactor;
                lblClickMessage.FontSize = 15 * Screensize.heightfactor;
                layoutContainer.Spacing = (56 * Screensize.heightfactor) / 4;
				lblSYS.FontSize = 21 * Screensize.heightfactor;
				lblmmHg.FontSize = 21 * Screensize.heightfactor;
				lblDIA.FontSize = 21 * Screensize.heightfactor;
				lblmm.FontSize = 21 * Screensize.heightfactor;
				lblSPO2.FontSize = 21 * Screensize.heightfactor;
				lblpct.FontSize = 21 * Screensize.heightfactor;
				lblPR.FontSize = 21 * Screensize.heightfactor;
				lblBPM.FontSize = 21 * Screensize.heightfactor;
				lblPI.FontSize = 21 * Screensize.heightfactor;
				lblPIPCT.FontSize = 21 * Screensize.heightfactor;
				lblTEMP.FontSize = 21 * Screensize.heightfactor;
                lblGLU.FontSize = 21 * Screensize.heightfactor;
				lblUnitGlucose.FontSize = 21 * Screensize.heightfactor;
			    lblWEIT.FontSize = 21 * Screensize.heightfactor;
                countDownLabel.FontSize = 15 * Screensize.heightfactor;
				layout1.WidthRequest = (360 * Screensize.widthfactor) / 2;
				layout2.WidthRequest = (360 * Screensize.widthfactor) / 2;
				layout3.WidthRequest = (360 * Screensize.widthfactor) / 2;
				layout4.WidthRequest = (360 * Screensize.widthfactor) / 2;
				layout5.WidthRequest = (360 * Screensize.widthfactor) / 2;
				layout6.WidthRequest = (360 * Screensize.widthfactor) / 2;
				layout7.WidthRequest = (360 * Screensize.widthfactor) / 2;
				layout8.WidthRequest = (360 * Screensize.widthfactor) / 2;
				layout9.WidthRequest = (360 * Screensize.widthfactor) / 2;
				layout10.WidthRequest = (360 * Screensize.widthfactor) / 2;
				layout11.WidthRequest = (360 * Screensize.widthfactor) / 2;
				layout12.WidthRequest = (360 * Screensize.widthfactor) / 2;
				layout13.WidthRequest = (360 * Screensize.widthfactor) / 2;
				layout14.WidthRequest = (360 * Screensize.widthfactor) / 2;
				layout15.WidthRequest = (360 * Screensize.widthfactor) / 2;
				layout16.WidthRequest = (360 * Screensize.widthfactor) / 2;
                plotView.HeightRequest = 300 * Screensize.heightfactor;
				NIBPButtonPad.FontSize = 21 * Screensize.heightfactor;
                NIBPButtonPad.HeightRequest = 100 * Screensize.heightfactor;
				ecgReportcBtnPad.FontSize = 21 * Screensize.heightfactor;
                ecgReportcBtnPad.HeightRequest = 100 * Screensize.heightfactor;
				layoutButton.IsVisible = false;
				layoutButtonPad.IsVisible = true;
                //layoutButtonPad.Margin = new Thickness(15, -200 * Screensize.heightfactor, 15, 0);
				btnFareinheit.FontSize = 21 * Screensize.heightfactor;
				btnCelcious.FontSize = 21 * Screensize.heightfactor;
                btnFareinheit.WidthRequest = 75;
				btnCelcious.WidthRequest = 75;
                btnLbs.FontSize = 21 * Screensize.heightfactor;
                btnKgs.FontSize = 21 * Screensize.heightfactor;
				btnLbs.WidthRequest = 75;
				btnKgs.WidthRequest = 75;

                lblSys.FontSize = 30 * Screensize.heightfactor;
				lblDia.FontSize = 30 * Screensize.heightfactor;
				lblSpo2.FontSize = 30 * Screensize.heightfactor;
                lblBpm.FontSize = 30 * Screensize.heightfactor;
                lblPerfusionIndex.FontSize = 30 * Screensize.heightfactor;
                lblTemperature.FontSize = 30 * Screensize.heightfactor;
				lblGlucose.FontSize = 30 * Screensize.heightfactor;
				lblWeight.FontSize = 30 * Screensize.heightfactor;

                //layoutButton.BackgroundColor = layoutButtonPad.BackgroundColor;
                //layoutButton.Spacing = layoutButtonPad.Spacing;
                //layoutButton.Margin = layoutButtonPad.Margin;
                //layoutButton.RelativeLayout.YConstraint = layoutButtonPad.RelativeLayout.YConstraint;
                //layoutButton.Spacing = layoutButtonPad.Spacing;
				//<StackLayout x:Name = "layoutButtonPad" IsVisible = "false" BackgroundColor="{StaticResource colorBlackBg}"  Orientation="Horizontal" Spacing ="20" Margin="15,-80,15,0" RelativeLayout.YConstraint="{ConstraintExpression Type=RelativeToParent, Property=Height, Factor=1}" RelativeLayout.WidthConstraint="{ConstraintExpression Type=RelativeToParent, Property=Width, Factor=1}" >
			}
            else if (Device.Idiom == TargetIdiom.Phone)
            {
				FakeToolbar.HeightRequest = 55 * Screensize.heightfactor;
				titlebtn.FontSize = 16 * Screensize.heightfactor;
                backbtn.FontSize = 16 * Screensize.heightfactor;

				imgProfile.WidthRequest = 80 * Screensize.widthfactor;
                imgProfile.HeightRequest = 96 * Screensize.heightfactor;
				lblName.FontSize = 15 * Screensize.heightfactor;
				lblEmail.FontSize = 14 * Screensize.heightfactor;
				lblClickMessage.FontSize = 10 * Screensize.heightfactor;
                layoutContainer.Spacing = (28 * Screensize.heightfactor)/4;
				lblSYS.FontSize = 14 * Screensize.heightfactor;
				lblmmHg.FontSize = 14 * Screensize.heightfactor;
				lblDIA.FontSize = 14 * Screensize.heightfactor;
				lblmm.FontSize = 14 * Screensize.heightfactor;
				lblSys.FontSize = 14 * Screensize.heightfactor;
				lblDia.FontSize = 14 * Screensize.heightfactor;
				lblSpo2.FontSize = 14 * Screensize.heightfactor;
				lblSPO2.FontSize = 14 * Screensize.heightfactor;
				lblpct.FontSize = 14 * Screensize.heightfactor;
				lblPR.FontSize = 14 * Screensize.heightfactor;
				lblBPM.FontSize = 14 * Screensize.heightfactor;
				lblBpm.FontSize = 14 * Screensize.heightfactor;
				lblPI.FontSize = 14 * Screensize.heightfactor;
				lblPIPCT.FontSize = 14 * Screensize.heightfactor;
				lblPerfusionIndex.FontSize = 14 * Screensize.heightfactor;
				lblTEMP.FontSize = 14 * Screensize.heightfactor;
				lblTemperature.FontSize = 14 * Screensize.heightfactor;
				lblGLU.FontSize = 14 * Screensize.heightfactor;
				lblUnitGlucose.FontSize = 14 * Screensize.heightfactor;
				lblGlucose.FontSize = 14 * Screensize.heightfactor;
				lblWeight.FontSize = 14 * Screensize.heightfactor;
				lblWEIT.FontSize = 14 * Screensize.heightfactor;
				countDownLabel.FontSize = 10 * Screensize.heightfactor;
                layout1.WidthRequest = (180 * Screensize.widthfactor) / 2;
				layout2.WidthRequest = (180 * Screensize.widthfactor) / 2;
				layout3.WidthRequest = (180 * Screensize.widthfactor) / 2;
				layout4.WidthRequest = (180 * Screensize.widthfactor) / 2;
				layout5.WidthRequest = (180 * Screensize.widthfactor) / 2;
				layout6.WidthRequest = (180 * Screensize.widthfactor) / 2;
				layout7.WidthRequest = (180 * Screensize.widthfactor) / 2;
				layout8.WidthRequest = (180 * Screensize.widthfactor) / 2;
				layout9.WidthRequest = (180 * Screensize.widthfactor) / 2;
				layout10.WidthRequest = (180 * Screensize.widthfactor) / 2;
				layout11.WidthRequest = (180 * Screensize.widthfactor) / 2;
				layout12.WidthRequest = (180 * Screensize.widthfactor) / 2;
				layout13.WidthRequest = (180 * Screensize.widthfactor) / 2;
				layout14.WidthRequest = (180 * Screensize.widthfactor) / 2;
				layout15.WidthRequest = (180 * Screensize.widthfactor) / 2;
				layout16.WidthRequest = (180 * Screensize.widthfactor) / 2;
				plotView.HeightRequest = 150 * Screensize.heightfactor;
				NIBPButton.FontSize = 14 * Screensize.heightfactor;
				ecgReportcBtn.FontSize = 14 * Screensize.heightfactor;
				//layoutButton.IsVisible = false;
				//layoutButtonPad.IsVisible = true;
				btnFareinheit.FontSize = 14 * Screensize.heightfactor;
				btnCelcious.FontSize = 14 * Screensize.heightfactor;
				//btnFareinheit.WidthRequest *= Screensize.widthfactor;
				//btnCelcious.WidthRequest *= Screensize.widthfactor;
				btnLbs.FontSize = 14 * Screensize.heightfactor;
				btnKgs.FontSize = 14 * Screensize.heightfactor;
				//btnLbs.WidthRequest = 75;
				//btnKgs.WidthRequest = 75;
            }
		}
	}
}
