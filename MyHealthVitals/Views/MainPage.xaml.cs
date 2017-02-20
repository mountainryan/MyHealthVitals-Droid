using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Diagnostics;
using System.IO;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MyHealthVitals
{
	public partial class MainPage : ContentPage, IBluetoothCallBackUpdatable
	{
		private VitalsData vitalsData = new VitalsData();

		public static bool isCOnnectedToSpotCheck = false;


		LineSeries lineSerie;
		public MainPage()
		{
			InitializeComponent();

			// Oxy plot thing
			ecgModel = new PlotModel();

			//ecgModel.TouchStarted += (object sender, OxyTouchEventArgs e) => {
			//	//ecgModel.DefaultXAxis.pa
			//});
			//ecgModel.sr
			BindingContext = this;

			lineSerie = new LineSeries
			{
				StrokeThickness = 1.5,
				Color = OxyColor.FromRgb(0, 145, 255),
				Smooth = true,
			};

			lastDataPointPrev = new DataPoint(0, 0);
			lineSerie.Points.Add(lastDataPointPrev);

			double p1 = 0;
			//float d1 = 0;

			//ecgModel.pa

			//this.ecgModel.TouchStarted += (object sender, OxyTouchEventArgs e) =>
			//{
			//	p1 = e.Position.X;

			//	//Debug.WriteLine("delta Translation: " + e.Position.X);
			//	//ecgModel.DefaultXAxis.Pan(plotView.Width - 42);

			//};

			//this.ecgModel.TouchCompleted += (object sender, OxyTouchEventArgs e) => {

			//	double dx = e.Position.X - p1;

			//	if (dx > 0)
			//	{
			//		Debug.WriteLine("delta Translation: " + dx);
			//		ecgModel.DefaultXAxis.Pan(plotView.Width - 42);

			//		//ecgModel.DefaultXAxis.Pan
			//	}
			//	else { 
			//		ecgModel.DefaultXAxis.Pan(-plotView.Width + 42);
			//		Debug.WriteLine("delta Translation: " + dx);
			//	}

			//	//ecgModel.DefaultXAxis.Pan(plotView.Width - 42);
			//};

			//ecgModel.to

			ecgModel.Series.Add(lineSerie);
			ecgModel.InvalidatePlot(true);

			if (ecgModel.DefaultXAxis != null)
			{
				ecgModel.DefaultYAxis.Minimum = 0;
				ecgModel.DefaultYAxis.Maximum = 500;

				ecgModel.DefaultXAxis.Minimum = 0;
				ecgModel.DefaultXAxis.Maximum = 4;
			}
			ecgModel.InvalidatePlot(true);

			btnFareinheit.TextColor = (Color)App.Current.Resources["colorThemeBlue"];
			btnCelcious.TextColor = Color.Gray;
			isCelcious = false;

			//if (MainPage.isCOnnectedToSpotCheck)
			//{
			//	btnBle.Image = "imgDevCon.png";
			//	btnBle.IsEnabled = false;
			//	lblStatus.Text = "Connected";
			//	//DependencyService.Get<ICBCentralManager>().
			//}

				// calling to start connecting the device this this should be implemented differently in android because it is calling the native API
				Xamarin.Forms.Device.StartTimer(TimeSpan.FromMilliseconds(250), () =>
				{
					Debug.WriteLine("searcching decice...");
					DependencyService.Get<ICBCentralManager>().ConnectToDevice((IBluetoothCallBackUpdatable)this);
					return false;
				});
			

			callAPi();
		}

		private async void callAPi() { 
			var isSuccess = await Demographics.sharedInstance.getDemographicFromApi();

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


		//void btnSaveClicked(object sender, System.EventArgs e)
		//{
		//	vitalsData.save();
		//}

		void btnBleClicked(Object sender, System.EventArgs e)
		{
			//Debug.WriteLine(sender.is);
			if (this.btnBle.IsEnabled) { 
				DependencyService.Get<ICBCentralManager>().ConnectToDevice((IBluetoothCallBackUpdatable)this);
			}
		}

		public void ShowMessageOnUI(string message, Boolean isConnected)
		{
			MainPage.isCOnnectedToSpotCheck = isConnected;
			//Debug.WriteLine(message);
			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				//layoutLoading.IsVisible = true;
				lblStatus.Text = message;

				if (!(message == "Measuring the Blood pressure..."))
				{
					lblPressure.Text = "";
				}

				if (isConnected)
				{
					btnBle.Image = "imgDevCon.png";
					btnBle.IsEnabled = false;

				}
				else { 
					btnBle.Image = "imgDevDiscon.png";
					btnBle.IsEnabled = true;
				}
				//hideMessageWthDelay();
			});
		}

		public void updateGlucoseReading(decimal gluReading, string unit) {
			//Conversion math mmol/L = mg/dL / 18
			if (unit == "Mmol/L")
			{
				vitalsData.glucose = new Reading("Glucose", gluReading, 8);
				vitalsData.glucose.MetricValue = Math.Round(gluReading / 18,1);
			}
			else {
				vitalsData.glucose = new Reading("Glucose", Math.Round((decimal)gluReading / 18, 1), 8);
				vitalsData.glucose.MetricValue = gluReading;
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

		public void noticeEndOfReadingSpo2() {
			vitalsData.sendToServer_SPO2_PI_BPM();
		}

		public void updateTemperature(decimal temperature, String type) {
			vitalsData.temperature = new Reading("Temperature(°F/°C)", temperature, 4);
			vitalsData.sendToServerTemperature();

			Device.BeginInvokeOnMainThread(() => {

				if (isCelcious)
				{
					lblTemperature.Text = ConvertFahrenheitToCelsius((double)this.vitalsData.temperature.EnglishValue).ToString();
				}
				else {
					lblTemperature.Text = temperature.ToString();
				}
			});
		}

		public PlotModel ecgModel { get; set; }
		float ecgTime = 0;
		DataPoint lastDataPointPrev;

		float xMin = 0.0f;

		public void updateECGResult(List<int> ecgPacket)
		{

			if (ecgModel.DefaultXAxis != null)
			{
				ecgModel.DefaultYAxis.Minimum = 0;
				ecgModel.DefaultYAxis.Maximum = 350;

				ecgModel.DefaultXAxis.Minimum = xMin;
				ecgModel.DefaultXAxis.Maximum = xMin + 4.0;
				ecgModel.DefaultYAxis.IsPanEnabled = false;

				// x - axis style
				ecgModel.DefaultXAxis.MinorGridlineStyle = LineStyle.Solid;
				ecgModel.DefaultXAxis.MajorGridlineStyle = LineStyle.Solid;

				ecgModel.DefaultXAxis.MajorGridlineThickness = 0.25f;
				ecgModel.DefaultXAxis.MinorGridlineThickness = 0.25f;

				ecgModel.DefaultXAxis.MinorGridlineColor = OxyColors.LightGray;
				ecgModel.DefaultXAxis.MajorGridlineColor = OxyColors.LightGray;

				// y - axis style
				ecgModel.DefaultYAxis.MinorGridlineStyle = LineStyle.Solid;
				ecgModel.DefaultYAxis.MajorGridlineStyle = LineStyle.Solid;

				ecgModel.DefaultYAxis.MajorGridlineThickness = 0.25f;
				ecgModel.DefaultYAxis.MinorGridlineThickness = 0.25f;

				ecgModel.DefaultYAxis.MajorGridlineColor = OxyColors.LightGray;
				ecgModel.DefaultYAxis.MinorGridlineColor = OxyColors.LightGray;

				//ecgModel.DefaultXAxis.TouchDelta += (object sender, OxyTouchEventArgs e) => {
				//	ecgModel.DefaultXAxis.Pan(plotView.Width - 42);
				//};

				//ecgModel.DefaultXAxis.TouchCompleted += (object sender, OxyTouchEventArgs e) => { 
				//	ecgModel.DefaultXAxis.Pan(plotView.Width - 42);
				//};

				//this.plotView.Model.TouchCompleted += this.PlotModelTouchCompletedHandler;

				//ecgModel.HandleTouchCompleted(this,

			}


			for (int i = 0; i < ecgPacket.Count; i++)
			{
				ecgTime = ecgTime + 0.006666666667f;

				//if (i == ecgPacket.Count - 1)
				//	lastDataPointPrev = new DataPoint(ecgTime, ecgPacket[i]);

				lineSerie.Points.Add(new DataPoint(ecgTime, ecgPacket[i]));
			}

			if (ecgTime > ecgModel.DefaultXAxis.Maximum)
			{
				ecgModel.PlotAreaBorderColor = OxyColors.Transparent;
				DependencyService.Get<IFileHelper>().saveToPdf(ecgModel, "ecgReport_" + (countEcgReport++) + ".pdf");
				ecgModel.PlotAreaBorderColor = OxyColors.Black;

				//lineSerie.Points.Clear();
				xMin = ecgTime;
				ecgModel.InvalidatePlot(true);

				//ecgModel.TextColor = OxyColors.Transparent;
				//ecgModel.TitleColor = OxyColors.Transparent;
				//ecgModel.LegendTextColor = OxyColors.Transparent;
			}
			else { 
				ecgModel.InvalidatePlot(true);
			}
		}

		int countEcgReport = 0;

		public void updatingPressureMeanTime(int pressure)
		{
			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				lblPressure.Text = pressure.ToString() + " mmHg";
			});
		}

		public void SPO2_readingCompleted(int sp02, int bpm, float perfusionIndex)
		{
			this.vitalsData.spo2 = new Reading("SpO2", sp02,2);
			this.vitalsData.bpm = new Reading("Pulse", bpm,2);
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
		}

		public void SYS_DIA_BPM_updated(int bpsys, int bpdia, int bpm)
		{

			this.vitalsData.bpDia = new Reading("DIA", bpdia,1);
			this.vitalsData.bpSys = new Reading("SYS", bpsys,1);
			//this.vitalsData.bpm = new Reading("Pulse", bpm,3);

			vitalsData.sendToServer_SYS_DIA();

			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				
				lblBpm.Text = bpm.ToString();
				lblDia.Text = bpdia.ToString();
				lblSys.Text = bpsys.ToString();

				//layoutLoading.IsVisible = false;
			});
		}

		//public static double ConvertCelsiusToFahrenheit(double c)
		//{
		//	return ((9.0 / 5.0) * c) + 32;
		//}

		public static double ConvertFahrenheitToCelsius(double f)
		{
			return Math.Round((5.0 / 9.0) * (f - 32),1);
		}

		void btnLogOutClicked(object sender, System.EventArgs e)
		{
			Debug.WriteLine(" log out");
			this.Navigation.PopModalAsync(true);
		}
		void btnNIBPStartClicked(object sender, System.EventArgs e)
		{
			DependencyService.Get<ICBCentralManager>().startMeasuringBP();
		}

		void btnListClicked(object sender, System.EventArgs e)
		{
			var newPage = new ParametersPage();
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

		bool isKg = true;
		void btnLbsClicked(Object sender, System.EventArgs e)
		{
			isKg = false;
			btnLbs.TextColor = (Color)App.Current.Resources["colorThemeBlue"];
			btnKgs.TextColor = Color.Gray;
		}

		void btnKgsClicked(Object sender, System.EventArgs e)
		{
			isKg = true;
			btnKgs.TextColor = (Color)App.Current.Resources["colorThemeBlue"];
			btnLbs.TextColor = Color.Gray;
		}

		// the oxy plot section
		//public void updateEcgModel(List<int> ecgPacket)
		//{
			
		//}

		//int countSec = 0;
		//public void Load()
		//{
		//	Xamarin.Forms.Device.StartTimer(TimeSpan.FromMilliseconds(250), () =>
		//		{
		//			updateModel();
		//			return countSec++ < 30;
		//		});
		//}
	}
}
