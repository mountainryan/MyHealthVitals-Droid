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

namespace MyHealthVitals
{
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
		public bool isFromDeviecList = false;
		bool isNavigated = false;
		public MainPage(string deviceName)
		{

			this.deviceName = deviceName;
			InitializeComponent();
			resizeUI();
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

			ecgReportcBtn.IsEnabled = false;
		}


		private void setUpEcgDisplay()
		{
			// Oxy plot thing
			graphModel = new PlotModel();

			//graphModel.TouchStarted += (object sender, OxyTouchEventArgs e) => {
			//	//graphModel.DefaultXAxis.pa
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
				graphModel.DefaultXAxis.IsPanEnabled = false;
				graphModel.DefaultYAxis.IsPanEnabled = false;

				graphModel.DefaultYAxis.Minimum = 0;
				graphModel.DefaultYAxis.Maximum = 255;

				graphModel.DefaultXAxis.Minimum = 0;
				graphModel.DefaultXAxis.Maximum = 50;
				graphModel.DefaultXAxis.IsZoomEnabled = false;
				graphModel.DefaultYAxis.IsZoomEnabled = false;
				styleGraphModel(graphModel);
			}
			if (lashEcgFile != null && lashEcgFile.Length > 0)
			{

				Boolean ecgExist = DependencyService.Get<IFileHelper>().checkFileExist(lashEcgFile + ".txt");
				if (!ecgExist)
				{
					setSavereportbutton();
				}
			}

			if (!isFromDeviecList || BLECentralManager.sharedInstance.checkIfDeviceScanned(deviceName))
			{
				return;
			}

			var ret = await DisplayAlert(deviceName, "Do you want to take a measurement?", "Yes", "No");
			if (ret)
			{
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
				catch
				{
					layoutLoading.IsVisible = true;
					BLECentralManager.sharedInstance.connectToDevice(deviceName, this);
				}
			}
			isFromDeviecList = false;

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



			graphModel.LegendFontSize = 5;
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
				//		this.Height = Demographics.sharedInstance.get
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
		//	vitalsData.save();
		//}

		void btnBleClicked(Object sender, System.EventArgs e)
		{
			//BLECentralManager.sharedInstance.connectToDevice("PC_300SNT", this);

			//Debug.WriteLine(sender.is);

			//this.bleManager.ScanToConnectToSpotCheck((IBluetoothCallBackUpdatable)this);

			//if (this.btnBle.IsEnabled) {
			//	this.bleManager.ScanToConnectToSpotCheck((IBluetoothCallBackUpdatable)this);
			//	//DependencyService.Get<ICBCentralManager>().ConnectToDevice((IBluetoothCallBackUpdatable)this);
			//}
		}
		int state = 100;
		public void SaveEcgState(int state)
		{
			this.state = state;
		}
		bool Measure_Interruped = false;




		public void ShowConcetion(String message, Boolean isConnected)
		{
			Debug.WriteLine("ShowConcetion  mainpage  :");
			layoutLoading.IsVisible = false;
			if (isConnected)
			{

				isNavigated = true;
			}
			if (!isConnected && this.deviceName == "eBody-Scale")
			{
				getLatestWeight(message);

			}
			else
			{
				DisplayAlert(deviceName, message, "OK");
			}
		}
		async public void ShowMessageOnUI(string message, Boolean isConnected, string title = null)
		{


			if (title == null)
			{
				if (this.deviceName == "eBody-Scale")
				{
					getLatestWeight("");

				}
				else
				{
					await DisplayAlert(this.deviceName, message, "OK");
				}
				//	
			}
			else if (title.Equals("Normal") || title.Equals("Abnormal"))
			{
				message += "\n Do you want to Save ECG data?";
				var ret = await DisplayAlert(title, message, "Yes", "No");
				Debug.WriteLine("ret === " + ret);
				if (ret)
				{
                    if (title.Equals("Normal"))
                    {
                        vitalsData.ecg = new Reading(null, this.state, 10, false, Task_vars.ecgmessage, null);
                    }else{
                        //Abnormal
                        vitalsData.ecg = new Reading(null, this.state, 10, true, Task_vars.ecgmessage, null);
                    }
					
					vitalsData.sendEcgToServer();

					// sending the Heart rate to server separately
					vitalsData.bpm = new Reading(null, heartRate, 3, false, null, null);
					vitalsData.bpm.Date = vitalsData.ecg.Date;
					vitalsData.sendHeartRateToServer();
					writeToTxt();
					ecgReportcBtn.IsEnabled = true;

				}
				else
				{
					reportDataList.Clear();
					ecgReportcBtn.IsEnabled = false;
					lineSerie.Points.Clear();
					graphModel.InvalidatePlot(true);

				}
			}
			else
			{
				await DisplayAlert(title, message, "OK");
				if (title.Equals("Measure Interruped"))
				{
					Measure_Interruped = true;
					countECGPacket = 0;
					ecgTime = 0;

					reportDataList.Clear();
					ecgReportcBtn.IsEnabled = false;
					lineSerie.Points.Clear();
					graphModel.InvalidatePlot(true);


					EcgcountdownCancle();

				}
				else if (title.Equals("Blood Pressure Measure Error"))
				{
					lblDia.Text = "-";
					lblSys.Text = "-";
					NIBPButton.Text = "NIBP Start";
					isBPMeasuring = false;
				}
			}

		}

		public async void updateGlucoseReading(decimal gluReading, string unit)
		{
			//Conversion math mmol/L = mg/dL / 18

			if (unit == "Mmol/L")
			{
				vitalsData.glucose = new Reading(null, Math.Round((decimal)gluReading * 18, 1), 8, false, null, null);
				vitalsData.glucose.MetricValue = Math.Round(gluReading, 1);
			}
			else
			{
				vitalsData.glucose = new Reading(null, gluReading, 8, false, null, null);
				vitalsData.glucose.MetricValue = gluReading;
			}

			var ret = await DisplayAlert("Measuring Result", "Do you want to save the result?\n "
										 + "GLU:  " + vitalsData.glucose.MetricValue + unit, "Yes", "No");
			if (!ret)
			{
				return;
			}

			vitalsData.sendToServer_Glucose();

			Device.BeginInvokeOnMainThread(() =>
			{
				lblGlucose.Text = (unit == "Mmol/L") ? vitalsData.glucose.EnglishValue.ToString() : vitalsData.glucose.MetricValue.ToString();
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
				string message = (vitalsData.spo2 == null ? "" : "SpO2: " + vitalsData.spo2.EnglishValue) + (vitalsData.bpm == null ? "" : "  Bpm: " + vitalsData.bpm.EnglishValue);
				var ret = await DisplayAlert("Measuring Result", "Do you want to save the result?\n " + message, "Yes", "No");
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
				
			}



			//lineSerie.Points.Clear();
			//graphModel.InvalidatePlot(true);
			//	graphModel.DefaultXAxis.IsPanEnabled = false;
		}

		public async void updateTemperature(decimal temperature)
		{

			vitalsData.temperature = new Reading(null, temperature, 4, false, null, null);
			if (vitalsData != null && vitalsData.temperature != null)
			{
				string message = "Temperature: " + temperature.ToString() + "°C / " + ConvertFahrenheitToCelsius((double)this.vitalsData.temperature.EnglishValue) + "°F";
				var ret = await DisplayAlert("Measuring Result", "Do you want to save the result?\n " + message, "Yes", "No");
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
			}

		}
		public void updated_Weight(decimal weight)
		{
			this.vitalsData.weight = new Reading(null, weight, 5, false, null, null);


			Device.BeginInvokeOnMainThread(() =>
			{
				if (!isKg)
				{
					lblWeight.Text = weight.ToString();

				}
				else
				{
					lblWeight.Text = ConvertLBToKG((double)weight).ToString();
				}
			});
		}


		// ECG waveform portion
		public PlotModel graphModel { get; set; }
		DataPoint lastDataPointPrev;

		float xMin = 0.0f;
		float ecgTime = 0.0f;
		int countECGPacket = 0;
		//	int countEcgReport = 0;


		List<int> reportDataList = new List<int>();
		int heartRate = 0;
		int ECG = 0;
		public void updateECGEnded(int bpm, int ecg)
		{
			countECGPacket = 0;
			//	countEcgReport = 0;
			ecgTime = 0.0f;
			heartRate = bpm;
			if (state == 17) return;
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
			//	updateECGEnded_Report();
		}
		int N= 0;
		private  void countDown()
		{
			progressBar.IsVisible = true;

			if (progressBar.AnimationIsRunning("SetProgress"))
			{
				progressBar.AbortAnimation("SetProgress");
			}
			else
			{
				progressBar.Animate("SetProgress", (arg) => { progressBar.Progress = arg;}, 0, 30000 , Easing.Linear);
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

			Device.BeginInvokeOnMainThread(() =>	countDown());

			for (; N >= 0; N--)
			 {
				 countDownLabel.Text = "Please keep measuring " + N + " seconds";
				 await Task.Delay(1000);
			 }
					
		}
		private void EcgcountdownCancle() {
			N = -1;
			Debug.WriteLine("EcgcountdownCancle timer="+timer);
			if (timer!= null && timer.running)
			{
				timer.Stop();
			//	timer = null;
				Debug.WriteLine("timer = "+timer);
			}
			if (progressBar!= null && progressBar.AnimationIsRunning("SetProgress"))
	        {
	            progressBar.AbortAnimation("SetProgress");
			}
			//countDown();
			progressBar.IsVisible = false;
			countDownLabel.IsVisible = false;
		}
		CommonMethod.MySystemDeviceTimer timer;

		private async Task initEcgCountdown() {
			await Task.Run(() =>
			{
				if (timer == null)
				{
					Debug.WriteLine("timer   =============" + timer);

					timer = new CommonMethod.MySystemDeviceTimer(TimeSpan.FromSeconds(12), async () =>
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
				if (Measure_Interruped) {
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
						graphModel.LegendTitle = "ECG";
						countDownLabel.Text = "Stabilizing reading, please continue.";
						countDownLabel.IsVisible = true;
						progressBar.IsVisible = false;


					await initEcgCountdown();

				
						ecgReportcBtn.IsEnabled = false;
						// reseting the graphmodel for ecg waveform
						graphModel.DefaultXAxis.IsPanEnabled = false;
						xMin = 0;
						graphModel.DefaultXAxis.Minimum = 0;
						graphModel.DefaultXAxis.Maximum = 6.0;

						graphModel.DefaultYAxis.Minimum = 0;
						graphModel.DefaultYAxis.Maximum = 255;

						lineSerie.Points.Clear();
						//	graphModel.DefaultXAxis.IsPanEnabled = false;
						graphModel.InvalidatePlot(true);

						Debug.WriteLine("countECGPacket   end =============" + countECGPacket);


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

	//		updateECGPacket_Report(ecgPacket);
		}

		// Spo2 waveform portion
		float pulseTime = 0.0f;
		bool initBpm = true;

		private void initBpmWaveForm() { 
			Debug.WriteLine("initBpmWaveForm");
			if (graphModel == null) return;

			if (graphModel.DefaultXAxis != null)
			{
				Debug.WriteLine("initBpmWaveForm ing");
				//initBpm = false;
				pulseTime = 0.0f;
				graphModel.DefaultXAxis.IsPanEnabled = false;
				graphModel.LegendTitle = "Pulse";

				graphModel.DefaultYAxis.Minimum = -10;
				graphModel.DefaultYAxis.Maximum = 265;

				xMin = 0;
				graphModel.DefaultXAxis.Minimum = xMin;
				graphModel.DefaultXAxis.Maximum = xMin + 3.0;
				if(lineSerie!= null) lineSerie.Points.Clear();
				graphModel.InvalidatePlot(true);
				graphModel.DefaultXAxis.IsZoomEnabled = false;
				graphModel.DefaultYAxis.IsZoomEnabled = false;
			}
		}
		public void updateBpmWaveform(int bpm)
		{
			if (graphModel == null) return;

			try
			{
				if (initBpm && graphModel.DefaultXAxis != null)
				{
					Debug.WriteLine("updateBpmWaveform initBpmWaveForm ing");

					initBpm = false;
					pulseTime = 0.0f;
					graphModel.LegendTitle = "Pulse";

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

			//	Debug.WriteLine("pulseTime = " + pulseTime);
			//	Debug.WriteLine(" graphModel.DefaultXAxis.Maximum = " +  graphModel.DefaultXAxis.Maximum);
				if (pulseTime > graphModel.DefaultXAxis.Maximum)
				{
					lineSerie.Points.Clear();
					xMin = pulseTime;
					graphModel.DefaultXAxis.Minimum = xMin;
					graphModel.DefaultXAxis.Maximum = xMin + 3.0;
				}

				pulseTime = pulseTime + 0.02f;
			//	Debug.WriteLine("lineSerie.point.add  bpm = " +bpm);
				if(bpm != 0)
					lineSerie.Points.Add(new DataPoint(pulseTime, bpm));

				graphModel.InvalidatePlot(true);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Exception on updateting ui:" + ex.Message);
			}
		}

		public void updatingPressureMeanTime(int pressure)
		{
			//Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			//{
			//	lblPressure.Text = pressure.ToString() + " mmHg";
			//});
		}
		public void SPO2_readingUpload() { 
		
		}
		public async void SPO2_readingCompleted(int sp02, int bpm, float perfusionIndex)
		{
			Debug.WriteLine("SPO2_readingCompleted");
			if (isupLoadedSPO2) return;
			this.vitalsData.spo2 = new Reading(null, sp02,2, false, null, null);
			this.vitalsData.bpm = new Reading(null, bpm,3, false, null, null);
			//this.vitalsData.bpSys = new Reading("Perfusion Index", perfusionIndex,2);

			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				if (bpm == 0)
				{
					lblBpm.Text = "...";
				}
				else { 
					lblBpm.Text = bpm.ToString();	
				}

				if (sp02 == 0)
				{
					lblSpo2.Text = "...";
				}
				else {
					lblSpo2.Text = sp02.ToString();
				}

				if (perfusionIndex > 0)
				{
					lblPerfusionIndex.Text = perfusionIndex.ToString();
				}
				else {
					lblPerfusionIndex.Text = "...";
				}
			});
			if (isBPMeasuring && !isupLoadedSPO2) {

				if(this.deviceName.Equals("PC_300SNT"))
				{
					BLECentralManager.sharedInstance.spotServHandler.stopMeasuringSpo2();
				}
				else if(this.deviceName.Equals("PC-100")){
					BLECentralManager.sharedInstance.pc100ServHandler.stopMeasuringSpo2();
				}
				isupLoadedSPO2 = true;
				if (vitalsData != null && (vitalsData.spo2 != null || vitalsData.spo2 != null)) {
					string message = (vitalsData.spo2 == null ? "" : "SpO2: " + vitalsData.spo2.EnglishValue) + (vitalsData.bpm == null ? "" : "  Bpm: " + vitalsData.bpm.EnglishValue);
					var ret = await DisplayAlert("Measuring Result", "Do you want to save the result?\n " + message, "Yes", "No");
					if (ret)
					{
						vitalsData.sendToServer_SPO2_PI_BPM();
					}
					else
					{
					//	isupLoadedSPO2 = false;

						Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
						{
							lblBpm.Text = "-";
							lblSpo2.Text = "-";
							lblPerfusionIndex.Text = "-";
						});				
					}

				}
			}
		}

		public async void SYS_DIA_BPM_updated(int bpsys, int bpdia, int bpm)
		{
			isBPMeasuring = true;
			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				if(bpm != 0){
					lblBpm.Text = bpm.ToString();	
				}
				Debug.WriteLine("bpdia" + bpdia);
				lblDia.Text = (bpdia != 0 && bpdia != 170) ? bpdia.ToString() : "-";
				lblSys.Text = bpsys != 0 ? bpsys.ToString() : "-";
			});
			Debug.WriteLine("bpsys = "+ bpsys +", dia  = "+bpdia +"  bpm ="+lblBpm );
			if (bpsys == 0 || bpdia == 0 || bpdia == 170|| bpm == 0) {
				return;
			}

			if (bpsys < bpdia)
			{
				await DisplayAlert("Measuring Error", "Abnormal measurement results. bpsys= " + bpsys +"bpdia="+bpdia, "OK");
				lblBpm.Text = "-";
				lblDia.Text = "-";
				lblSys.Text = "-";
			}
			else
			{
				string message = "SYS: " + bpsys + " DIA: " + bpdia + " Bpm: " + bpm;
				var ret = await DisplayAlert("Measuring Result", "Do you want to save the result?\n " + message, "Yes", "No");
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
			}
			isBPMeasuring = false;
			NIBPButton.Text = "NIBP Start";
		}

		public static double ConvertKGToLB(double f)
		{
			//1 kg = 2.20462262185 lb
			return Math.Round(f * 2.20462262185, 2);
		}
		public static double ConvertLBToKG(double f)
		{
			//1 lb = 0.45359237 kg
			return Math.Round(f * 0.45359237, 2);
		}

		public static double ConvertFahrenheitToCelsius(double f)
		{
			return Math.Round((5.0 / 9.0) * (f - 32),1);
		}

		void btnLogOutClicked(object sender, System.EventArgs e)
		{
			Debug.WriteLine(" log out");
			BLECentralManager.sharedInstance.disConnectAll();
			Demographics.sharedInstance.calibratedReadingList.Clear();
			this.Navigation.PopModalAsync(true);
		}
		void btnNIBPStartClicked(object sender, System.EventArgs e)
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
			else {

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

			//bleManager.startMeasuringBP();
			//DependencyService.Get<ICBCentralManager>().startMeasuringBP();
		}
		void btnEcgReportClicked(object sender, System.EventArgs e) { 
			Debug.WriteLine("vitalsData = " + vitalsData);
			//	bool ret = await DependencyService.Get<IFileHelper>().sentToEmail("" + ".pdf");
			//==================
			startECGReportPage();
		}



		void btnEcgStartClicked(object sender, System.EventArgs e)
		{
			BLECentralManager.sharedInstance.spotServHandler.startEcgMeasuring();
			//DependencyService.Get<ICBCentralManager>().startEcgMeasuring();
		}

		void btnListClicked(object sender, System.EventArgs e)
		{
			//		ecgReportInstance = EcgReport.Instance;
			//		ecgReportInstance.setMainPage(this);

			if (vitalsData.ecg != null) {
				string date = vitalsData.ecg.Date.ToString("MM/dd/yyyy hh:mm:tt");
				fileName = Regex.Replace(date, @"\s+", "");//dateTime.Trim(' ');
				fileName = Regex.Replace(fileName, @"[/:]+", "");
				lashEcgFile = fileName;
			}

		    var newPage = new ParametersPageLocal();
			//var newPage = new ParametersPage();
			newPage.Title = "Parameter List Screen";
			this.Navigation.PushAsync(newPage);
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
			catch (Exception){
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
			catch(Exception) {
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
			catch (Exception){
				Debug.WriteLine("exception nullpointer");
			}
		}

		void btnKgsClicked (Object sender, System.EventArgs e)
		{
			isKg = true;
			btnKgs.TextColor = (Color)App.Current.Resources["colorThemeBlue"];
			btnLbs.TextColor = Color.Gray;

			try
			{
				lblWeight.Text = ConvertLBToKG((double)this.vitalsData.weight.EnglishValue).ToString();
			}
			catch (Exception){
				Debug.WriteLine("exception nullpointer");
			}
		}
		async void getLatestWeight(string m)
		{
			string message = "Weight: " + (double)this.vitalsData.weight.EnglishValue + "Lbs / "
			                  + ConvertLBToKG((double)this.vitalsData.weight.EnglishValue) + "Kg";
			var ret = await DisplayAlert("Measuring Result", "Do you want to save the result?\n " + message, "Yes", "No");
			if (!ret)
			{				return;
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
					ParametersPageLocal.allReadings = await Reading.GetAllReadingsFromService();
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

		//	var allReadings = await Reading.GetAllReadingsFromService();



			double diff = Math.Abs((double)Math.Round((double)this.vitalsData.weight.EnglishValue - weightReading, 1));
			if (diff < 1) { 
				var myEmoji = "\U0001F60A";
				await DisplayAlert("No Change", myEmoji + "Looking good.", "OK");
			}
			else if (weightReading > (double)this.vitalsData.weight.EnglishValue)
			{
				var myEmoji = "\U0001F600";
				await DisplayAlert("Lost Weight", myEmoji + " Good job! you lost " + diff + " pounds!", "OK");
			}
			else {
				
				var myEmoji = "\U0001F61F";
				await DisplayAlert("Gained Weight", myEmoji + " OOPS! You gained " + diff + " pounds!", "OK");

			}
            this.vitalsData.sendToServerWeight(userHeight);
			if(m != "") await DisplayAlert(deviceName, m, "OK");
		}

		public async void startECGReportPage()
		{
			progressBar.IsVisible = false;
			countDownLabel.IsVisible = false;
		//	creatReportTitle();
			var newPage = new EcgReport(fileName, Demographics.sharedInstance.FirstName, false, this );
			newPage.Title = "ECG Report";
			await this.Navigation.PushAsync(newPage);
		//	lineSerie.Points.Clear();

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
				imgProfile.WidthRequest *= 2;
				imgProfile.HeightRequest *= 2;
				lblName.FontSize *= 1.5;
				lblEmail.FontSize *= 1.5;
				lblClickMessage.FontSize *= 1.5;

				layoutContainer.Spacing *= 2;
				lblSYS.FontSize *= 1.5;
				lblmmHg.FontSize *= 1.5;
				lblDIA.FontSize *= 1.5;
				lblmm.FontSize *= 1.5;
				lblSys.FontSize *= 1.5;
				lblDia.FontSize *= 1.5;
				lblSpo2.FontSize *= 1.5;
				lblSPO2.FontSize *= 1.5;
				lblpct.FontSize *= 1.5;
				lblPR.FontSize *= 1.5;
				lblBPM.FontSize *= 1.5;
				lblBpm.FontSize *= 1.5;

				lblPI.FontSize *= 1.5;
				lblPIPCT.FontSize *= 1.5;
				lblPerfusionIndex.FontSize *= 1.5;
				lblTEMP.FontSize *= 1.5;
				lblGLU.FontSize *= 1.5;
				lblUnitGlucose.FontSize *= 1.5;
				lblGlucose.FontSize *= 1.5;
				lblWeight.FontSize *= 1.5;
				lblWEIT.FontSize *= 1.5;

				countDownLabel.FontSize *= 1.5;
				layout1.WidthRequest *= 2;
				layout2.WidthRequest *= 2;
				layout3.WidthRequest *= 2;
				layout4.WidthRequest *= 2;
				layout5.WidthRequest *= 2;
				layout6.WidthRequest *= 2;
				layout7.WidthRequest *= 2;
				layout8.WidthRequest *= 2;
				layout9.WidthRequest *= 2;
				layout10.WidthRequest *= 2;
				layout11.WidthRequest *= 2;
				layout12.WidthRequest *= 2;
				layout13.WidthRequest *= 2;
				layout14.WidthRequest *= 2;
				layout15.WidthRequest *= 2;
				layout16.WidthRequest *= 2;

				plotView.HeightRequest *= 2;
				NIBPButtonPad.FontSize *= 1.5;
				ecgReportcBtnPad.FontSize *= 1.5;
				layoutButton.IsVisible = false;
				layoutButtonPad.IsVisible = true;
			}
		}
	}
}
