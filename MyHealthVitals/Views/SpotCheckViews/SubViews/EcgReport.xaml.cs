using System;
using System.Collections.Generic;
using System.Diagnostics;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Xamarin.Forms;
using System.Threading.Tasks;
using OxyPlot.Annotations;
using System.Reflection;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Polly;

namespace MyHealthVitals
{
    
    public class FileData
    {
        public long? Id { get; set; }
        public string Category { get; set; }
        public byte[] Content { get; set; }
        public string Name { get; set; }
        public DateTime? ServiceDate { get; set; }
        public long Size { get; set; }
        public DateTime? UploadDate { get; set; }
    }
	
	public partial class EcgReport : ContentPage
	{

		public PlotModel graphModel_report { get; set; }
		DataPoint lastDataPointPrev_report1;

		LineSeries lineSerie_report;
		LineSeries lineSerie_report1;
		LineSeries lineSerie_report2;
		LineSeries lineSerie_report3;
		LineSeries lineSerie_report4;
		LineSeries lineSerie_report_line0;
		float ecgTime_report = 0.0f;
		int countECGPacket_report = 0;
		List<string> ecgPacket = new List<string>();
		int baseNumber = 765;
	
		string fileName = null;
		string patientName;
		MainPage mainControl = null;

		public EcgReport(String filename, string patientName, bool rightButton = false, MainPage maincontrol = null)
		{
			NavigationPage.SetHasNavigationBar(this, false);
			Debug.WriteLine("EcgReport()");
			this.fileName = filename;
            this.mainControl = maincontrol;
			this.patientName = patientName;
			Debug.WriteLine("this.fileName==" + this.fileName);
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

			setUpEcgDisplay_Report();
			if (rightButton)
			{
				//	Debug.WriteLine("add back button");
				setGobackButton();
			}
		}

		private void setUpEcgDisplay_Report()
		{
			// Oxy plot thing
			graphModel_report = new PlotModel();
			BindingContext = this;


			lineSerie_report_line0 = new LineSeries
			{
				StrokeThickness = 0.5,
				Color = OxyColors.DarkGray,
			};

			lineSerie_report_line0.Points.Add(new DataPoint(0, 780));
			lineSerie_report_line0.Points.Add(new DataPoint(8, 780));
			lineSerie_report_line0.Points.Add(new DataPoint(8, 520));
			lineSerie_report_line0.Points.Add(new DataPoint(0, 520));
			lineSerie_report_line0.Points.Add(new DataPoint(0, 260));
			lineSerie_report_line0.Points.Add(new DataPoint(8, 260));
			lineSerie_report_line0.Points.Add(new DataPoint(8, 0));
			lineSerie_report_line0.Points.Add(new DataPoint(7, 0));

			lineSerie_report_line0.Points.Add(new DataPoint(7, 1000));
			lineSerie_report_line0.Points.Add(new DataPoint(6, 1000));
			lineSerie_report_line0.Points.Add(new DataPoint(6, 0));
			lineSerie_report_line0.Points.Add(new DataPoint(5, 0));
			lineSerie_report_line0.Points.Add(new DataPoint(5, 1000));
			lineSerie_report_line0.Points.Add(new DataPoint(4, 1000));

			lineSerie_report_line0.Points.Add(new DataPoint(4, 0));
			lineSerie_report_line0.Points.Add(new DataPoint(3, 0));
			lineSerie_report_line0.Points.Add(new DataPoint(3, 1000));
			lineSerie_report_line0.Points.Add(new DataPoint(2, 1000));
			lineSerie_report_line0.Points.Add(new DataPoint(2, 0));
			lineSerie_report_line0.Points.Add(new DataPoint(1, 0));
			lineSerie_report_line0.Points.Add(new DataPoint(1, 1000));

			graphModel_report.Series.Add(lineSerie_report_line0);

			//===========================================================
			lineSerie_report1 = new LineSeries
			{
				StrokeThickness = 1.5,
				Color = OxyColor.FromRgb(0, 145, 255),
				Smooth = true,

			};
			lastDataPointPrev_report1 = new DataPoint(0, 0);
			lineSerie_report1.Points.Add(lastDataPointPrev_report1);

			//===========================================================
			lineSerie_report2 = new LineSeries
			{
				StrokeThickness = 1.5,
				Color = OxyColor.FromRgb(0, 145, 255),
				Smooth = true,

			};
			//  lastDataPointPrev_report2 = new DataPoint(0, 0);
			//  lineSerie_report2.Points.Add(lastDataPointPrev_report2);
			//===========================================================

			lineSerie_report3 = new LineSeries
			{
				StrokeThickness = 1.5,
				Color = OxyColor.FromRgb(0, 145, 255),
				Smooth = true,

			};
			//  lastDataPointPrev_report3 = new DataPoint(0, 0);
			//  lineSerie_report3.Points.Add(lastDataPointPrev_report3);
			//===========================================================
			lineSerie_report4 = new LineSeries
			{
				StrokeThickness = 1.5,
				Color = OxyColor.FromRgb(0, 145, 255),
				Smooth = true,

			};
			//  lastDataPointPrev_report4 = new DataPoint(0, 0);
			//  lineSerie_report4.Points.Add(lastDataPointPrev_report4);

			//===========================================================

			lineSerie_report = lineSerie_report1;
			graphModel_report.Series.Add(lineSerie_report);


			//      graphModel_report.TitlePadding = 50;



			graphModel_report.TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinView;





			//  graphModel_report.TitleToolTip = "teashgfhfhfj";

			//graphModel_report.LegendFontSize = 40;
			graphModel_report.LegendTitleFontSize = 20;
			graphModel_report.LegendTitleFont = "Arial";
			//plotModel.TitleFont = "Segoe UI";
			graphModel_report.LegendSymbolPlacement = LegendSymbolPlacement.Left;

			graphModel_report.LegendPlacement = LegendPlacement.Outside;
			graphModel_report.LegendPosition = LegendPosition.TopRight;
			graphModel_report.LegendFontWeight = 10;
			//  graphModel_report.TitleFontSize = 20;
			graphModel_report.SubtitleFontSize = 19;
			graphModel_report.TitleFontSize = 20;
			//      graphModel_report.SubtitleFontWeight = 5;
			graphModel_report.SubtitleFont = "Arial";
			graphModel_report.TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinPlotArea;
			//graphModel_report.TitleFontWeight = FontWeights.Normal;
			graphModel_report.SubtitleFontWeight = FontWeights.Bold;

			/*

        //  graphModel_report.
            OxyImage image;
            //      var embeddedImage = new Image { Aspect = Aspect.AspectFill };
            //      embeddedImage.Source = ImageSource.FromResource("MyHealthVitals.Views.download.png");

            var assembly = typeof(EcgReport).GetTypeInfo().Assembly;


            var stream = assembly.GetManifestResourceStream("MyHealthVitals.Views.download.png");

            image = new OxyImage(stream);


            // Centered in plot area, filling width
            graphModel_report.Annotations.Add(new ImageAnnotation{
                ImageSource = image,
                Opacity = 0.2,
                Interpolate = false,
                X = new PlotLength(0.5, PlotLengthUnit.RelativeToPlotArea),
                Y = new PlotLength(0.5, PlotLengthUnit.RelativeToPlotArea),
                Width = new PlotLength(1, PlotLengthUnit.RelativeToPlotArea),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Middle,
            });

            */
		}

		//close

		protected override void OnAppearing()
		{
			base.OnAppearing();

			if (Device.Idiom == TargetIdiom.Tablet)
			{
				FakeToolbar.HeightRequest = 75 * Screensize.heightfactor;
				titlebtn.FontSize = 30 * Screensize.heightfactor;
                backbtn.FontSize = 30 * Screensize.heightfactor;
				reportButton.FontSize = 36 * Screensize.heightfactor;
				reportButton.HeightRequest = 80 * Screensize.heightfactor;
				plotView.HeightRequest = 900 * Screensize.heightfactor;
			}
			else if (Device.Idiom == TargetIdiom.Phone)
			{
				FakeToolbar.HeightRequest = 55 * Screensize.heightfactor;
				titlebtn.FontSize = 16 * Screensize.heightfactor;
                backbtn.FontSize = 16 * Screensize.heightfactor;
				plotView.HeightRequest = 450 * Screensize.heightfactor;
                reportButton.FontSize = 16 * Screensize.heightfactor;
			}

            Debug.WriteLine("Defaultxaxis : " + graphModel_report.DefaultXAxis);
            Task.Delay(1).ContinueWith(_ => {});
            Debug.WriteLine("Defaultxaxis : " + graphModel_report.DefaultXAxis);
			if (countECGPacket_report == 0 && graphModel_report.DefaultXAxis != null)
			{
                Debug.WriteLine("Made it into ecgreport graph if statement.");
				graphModel_report.DefaultXAxis.IsPanEnabled = false;
				graphModel_report.DefaultYAxis.IsPanEnabled = false;

				graphModel_report.DefaultYAxis.Minimum = 0;
				graphModel_report.DefaultYAxis.Maximum = 1000;

				graphModel_report.DefaultXAxis.Minimum = 0;
				graphModel_report.DefaultXAxis.Maximum = 8;
				graphModel_report.DefaultXAxis.IsZoomEnabled = false;
				graphModel_report.DefaultYAxis.IsZoomEnabled = false;
				graphModel_report.TextColor = OxyColors.Transparent; //<--This is what made the numbers along the axes disappear
																	 //but not in Android for some reason
				graphModel_report.TitleColor = OxyColors.Black;
				graphModel_report.SubtitleColor = OxyColors.Black;
				graphModel_report.LegendTitleColor = OxyColors.Blue;
				//  graphModel.LegendTextColor = OxyColors.Transparent;
				graphModel_report.InvalidatePlot(true);
				styleGraphModel(graphModel_report);
			}
			
			updateECGPacket_Report();

		}

		async void btnPrevClicked(object sender, System.EventArgs e)
		{
			//Navigation.PopAsync();
			string message = "";
			if (DependencyService.Get<IFileHelper>().checkFileExist(fileName + ".txt"))
			{
				message = "Do you want to save and email the report before leaving";
				bool ret;
				if (Device.Idiom == TargetIdiom.Tablet)
				{
					ret = await DependencyService.Get<IFileHelper>().dispAlert("Save and Email", message, true, "Yes", "No");
				}
				else
				{
					ret = await DependencyService.Get<IFileHelper>().dispAlert("Save and Email", message, false, "Yes", "No");
				}
				//var ret = await DisplayAlert("Save and Email", message, "Yes", "No");
				if (ret)
				{
					btnSaveClicked(null, null);
				}
				if (ParametersPageLocal.allReadings == null)
				{
					int index = Task.WaitAny(Task_vars.tasks);
					//ParametersPageLocal.allReadings = await Reading.GetAllReadingsFromService();
				}
				if (Device.Idiom == TargetIdiom.Tablet)
				{
					var newPage = new ParameterItemDetailNew(10, ParametersPageLocal.allReadings);
				}
				else
				{
					var newPage = new ParameterItemDetailNew(10, ParametersPageLocal.allReadings);
				}

				if (DependencyService.Get<IFileHelper>().checkFileExist(fileName + "ECG.pdf"))
                {
                    if (Task_vars.comingfrom == "MainPage")
                    {
                        await Navigation.PopModalAsync();
                    }else{
                        await Navigation.PopAsync();
                    }
                }else{
					if (Task_vars.comingfrom == "MainPage")
					{
                        await Navigation.PopAsync();
					}else{
						await Navigation.PopModalAsync();
					}
                }
			}
            else
            {
				if (Task_vars.comingfrom == "MainPage")
				{
					await Navigation.PopAsync();
				}
				else
				{
					await Navigation.PopModalAsync();
				}

            }


            //await Navigation.PopModalAsync();

			
		}

		private void setGobackButton()
		{

			//public ToolbarItem(string name, string icon, Action activated, ToolbarItemOrder order = ToolbarItemOrder.Default, int priority = 0);
			var goBack = new ToolbarItem
			{
				//Icon = "settings32.png", 
				Text = "Back", 
                //  Command = new Command((c,e) => this.OnBackButtonPressed(c,e)),
			};
			goBack.Clicked += async (object sender, EventArgs e) =>
			{
				string message = "";
				if (DependencyService.Get<IFileHelper>().checkFileExist(fileName + ".txt"))
				{
					message = "Do you want to save and email the report before leaving";
                    bool ret;
					if (Device.Idiom == TargetIdiom.Tablet)
					{
						ret = await DependencyService.Get<IFileHelper>().dispAlert("Save and Email", message, true, "Yes", "No");
					}
					else
					{
						ret = await DependencyService.Get<IFileHelper>().dispAlert("Save and Email", message, false, "Yes", "No");
					}
					//var ret = await DisplayAlert("Save and Email", message, "Yes", "No");
					if (ret)
					{
						btnSaveClicked(null, null);
					}
					if (ParametersPageLocal.allReadings == null)
					{
						int index = Task.WaitAny(Task_vars.tasks);
						//ParametersPageLocal.allReadings = await Reading.GetAllReadingsFromService();
					}
                    if (Device.Idiom == TargetIdiom.Tablet)
                    {
                        var newPage = new ParameterItemDetailNew(10, ParametersPageLocal.allReadings);
                    }
                    else
                    {
                        var newPage = new ParameterItemDetailNew(10, ParametersPageLocal.allReadings);
                    }
					
					//  await this.Navigation.PushModalAsync(newPage);
					//  this.Navigation.RemovePage(this);
				}
				await Navigation.PopModalAsync();
			};
			this.ToolbarItems.Add(goBack);

		}

		private void styleGraphModel(PlotModel graphModel)
		{

			// x - axis style
			graphModel.DefaultXAxis.MinorGridlineStyle = LineStyle.Solid;
			graphModel.DefaultXAxis.MajorGridlineStyle = LineStyle.Solid;

			graphModel.DefaultXAxis.MajorGridlineThickness = 0.25f;
			graphModel.DefaultXAxis.MinorGridlineThickness = 0.25f;

			Debug.WriteLine("graphModel.DefaultXAxis.MinorGridlineThickness = " + graphModel.DefaultXAxis.MinorGridlineThickness);

			graphModel.DefaultXAxis.MinorGridlineColor = OxyColors.LightGray;
			graphModel.DefaultXAxis.MajorGridlineColor = OxyColors.LightGray;

			// y - axis style
			graphModel.DefaultYAxis.MinorGridlineStyle = LineStyle.Solid;
			graphModel.DefaultYAxis.MajorGridlineStyle = LineStyle.Solid;

			graphModel.DefaultYAxis.MajorGridlineThickness = 0.25f;
			graphModel.DefaultYAxis.MinorGridlineThickness = 0.25f;
			graphModel.DefaultYAxis.MajorGridlineColor = OxyColors.LightGray;
			graphModel.DefaultYAxis.MinorGridlineColor = OxyColors.LightGray;
			this.graphModel_report.InvalidatePlot(true);
		}
		int count = 1;
		public void updateECGPacket_Report()
		{
			try
			{
				ecgPacket = DependencyService.Get<IFileHelper>().readFromTxt(fileName);
				Debug.WriteLine("count ============" + ecgPacket.Count);
				//Debug.WriteLine("graphModel_report.DefaultXAxis.Maximum = "+graphModel_report.DefaultXAxis.Maximum);
				//Debug.WriteLine("ecgTime_report = "+ecgTime_report);
				for (int i = 0; i < ecgPacket.Count; i++)
				{

					ecgTime_report = ecgTime_report + 0.006666666667f;
					lineSerie_report.Points.Add(new DataPoint(ecgTime_report, Convert.ToInt32(ecgPacket[i]) + baseNumber));

					// find the end and save the screen into pdf 12

					//if (ecgTime_report > graphModel_report.DefaultXAxis.Maximum)
					if (ecgTime_report > 8)
					{
						//Debug.WriteLine("Made it into if statement.");
						count++;
						ecgTime_report = 0;
						baseNumber -= 250;
						//Debug.WriteLine("Count = "+count);

						if (count == 2)
						{
							lineSerie_report = lineSerie_report2;
						}
						else if (count == 3)
						{
							lineSerie_report = lineSerie_report3;
						}
						else if (count == 4)
						{
							lineSerie_report = lineSerie_report4;
						}
						graphModel_report.Series.Add(lineSerie_report);

						graphModel_report.InvalidatePlot(true);
					}
					else
					{
						//Debug.WriteLine("Didn't make it into if statement.");
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("report   Exception " + ex);
			}
            Device.BeginInvokeOnMainThread(new Action(async () =>
            {
                if (DependencyService.Get<IFileHelper>().checkFileExist(fileName + ".pdf"))
                {
                    reportButton.Text = "Send email";
                }
                graphModel_report.InvalidatePlot(false);
                //  await DependencyService.Get<IFileHelper>().saveToPdf(graphModel_report, "EcgReport.pdf");
                //  return true;
                //plotView.RefreshPlot(true);
                await Task.Delay(1).ContinueWith(_ => { });
                initializePlotModel();
            }));


		}

		public void initializePlotModel()
		{
            //Debug.WriteLine("countECGPacket : "+countECGPacket_report);

			if (countECGPacket_report == 0 && graphModel_report.DefaultXAxis != null)
			{
				Debug.WriteLine("Made it into ecgreport graph if statement.");
				graphModel_report.DefaultXAxis.IsPanEnabled = false;
				graphModel_report.DefaultYAxis.IsPanEnabled = false;

				graphModel_report.DefaultYAxis.Minimum = 0;
				graphModel_report.DefaultYAxis.Maximum = 1000;

				graphModel_report.DefaultXAxis.Minimum = 0;
				graphModel_report.DefaultXAxis.Maximum = 8;
				graphModel_report.DefaultXAxis.IsZoomEnabled = false;
				graphModel_report.DefaultYAxis.IsZoomEnabled = false;
				graphModel_report.TextColor = OxyColors.Transparent; //<--This is what made the numbers along the axes disappear
																	 //but not in Android for some reason
				graphModel_report.TitleColor = OxyColors.Black;
				graphModel_report.SubtitleColor = OxyColors.Black;
				graphModel_report.LegendTitleColor = OxyColors.Blue;
				//  graphModel.LegendTextColor = OxyColors.Transparent;
				graphModel_report.InvalidatePlot(true);
				styleGraphModel(graphModel_report);
			}
			else
			{
				//Debug.WriteLine("WHY!!!!!");
			}
		}

		async void btnSaveClicked(object sender, System.EventArgs e)
		{
			Debug.WriteLine("btn Save Clicked");

			//  if (reportButton.Text.Equals("Save ECG Report"))
			//  {
			ReportSaving.IsVisible = true;
			//reportButton.IsEnabled = false;


			//reportButton.Text = "exporting report to Pdf";
			//  graphModel_report.InvalidatePlot(false);

			graphModel_report.Title = "   ";// ecgPacket[0];
			graphModel_report.Subtitle = "  ";//ecgPacket[1];
			graphModel_report.LegendTitle = "  ";
			graphModel_report.TitlePadding = 30;
			await Task.Run(() =>
			{
				DependencyService.Get<IFileHelper>().setEmailClient();
			});
			Device.BeginInvokeOnMainThread(async () =>
			{
				byte[] retdata = DependencyService.Get<IFileHelper>().saveToPdf(graphModel_report, fileName, patientName);
				Task_vars.ecgcontent = retdata;
				FileUpload();
				if (retdata != null && retdata.Length > 0)
				{
					Debug.WriteLine("save to pdf ret = true");
					reportButton.IsEnabled = false;
					//  reportButton.Text = "Send email";
					/*
                    */
				}
				else
				{
					if (Device.Idiom == TargetIdiom.Tablet)
					{
						var ret = await DependencyService.Get<IFileHelper>().dispAlert("Failure", "Can not export the ecg report.", true, "OK", null);
					}
					else
					{
						var ret = await DependencyService.Get<IFileHelper>().dispAlert("Failure", "Can not export the ecg report.", false, "OK", null);
					}
					//await DisplayAlert("Failure", "Can not export the ecg report.", "OK");

				}
				if (mainControl != null) mainControl.setSavereportbutton();
				ReportSaving.IsVisible = false;
			});
			//          }
			/*  else if(reportButton.Text.Equals("Send email")){ 
					DependencyService.Get<IFileHelper>().sentToEmail(fileName+".pdf");
					reportButton.IsEnabled = false;
				}
	*/
		}
		public void FileUpload()
		{
			//var serviceUri = Credential.BASE_URL + $"Patient/{Credential.sharedInstance.Mrn}/HomeHealth/Reading";
			FileData ecgfile = new FileData();

			//ecgfile.Id = 2643;

			//PDF File Path: /var/mobile/Containers/Data/Application/EC012C73-B1C2-495E-A4A3-C126B54E00A0/Documents/100320170917AM.pdf
			//Test File Path: /var/mobile/Containers/Data/Application/EC012C73-B1C2-495E-A4A3-C126B54E00A0/Documents/test.txt

			//build the file name
			String filedate = Task_vars.ecgdate.ToString("MMddyyyy_HHmm") + ".pdf";
            Debug.WriteLine("filedate = "+filedate);
			String file_name = Task_vars.patient_name + "_ECGReport_" + filedate;
            Debug.WriteLine("file_name = " + file_name);

			ecgfile.Category = "Cardiology (ECG, EKGs, Stress Test, etc.)";
			ecgfile.Content = Task_vars.ecgcontent;
			ecgfile.Name = file_name;
			ecgfile.ServiceDate = Task_vars.ecgdate;
			//FileInfo Finfo = new FileInfo(filepath)
			ecgfile.Size = Task_vars.ecgfilelength;
			ecgfile.UploadDate = DateTime.Now;

			//var msg = PostFileToService(Credential.BASE_URL + $"Patient/{Credential.sharedInstance.Mrn}/File", ecgfile);
			//var msg = PostFileToService(Credential.BASE_URL + $"Patient/574/File", ecgfile);

			FPostAsync(Credential.BASE_URL + $"Patient/{Credential.sharedInstance.Mrn}/File", ecgfile, 1);


		}

		public async Task PostFileToService(string url, object arg)
		{
			//this.Narrative = Task_vars.ecgmessage;
			//Debug.WriteLine("ecgmessage: " + Task_vars.ecgmessage);
			//Debug.WriteLine("ecgmessage sent: " + this.Narrative);
			//var item = await Client.PostAsync(credential, $"api/v1/Patient/{credential.Mrn}/HomeHealth/Reading", this);
			//Id = item.Id;
			//Abnormal = item.Abnormal;
			//EnglishValue = item.EnglishValue;
			Debug.WriteLine("PostFileToService");
			HttpClient client = new HttpClient();
			//client.Timeout = new TimeSpan(0, timeout.Value, 0);
			client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Credential.sharedInstance.Token}");

			// converting the this reading into string to send it to the service as application/json
			var content = new StringContent(JsonConvert.SerializeObject(arg), Encoding.UTF8, "application/json");
			Debug.WriteLine("json stuff: " + content.ToString());

			var serviceUri = url;
			Debug.WriteLine("sent to :" + serviceUri.ToString());

			var response = await client.PostAsync(serviceUri, content);

			if (response.IsSuccessStatusCode)
			{
				Debug.WriteLine("Successful file upload! Woohoo!");
				return;
			}
			else
			{
				//what did it return
				Debug.WriteLine("Response: " + response.StatusCode);

			}
			var message = await DoWithRetryAsync(() => response.Content.ReadAsStringAsync());
			//var message = await response.Content.ReadAsStringAsync();
			Debug.WriteLine("file error message: " + message);
			throw new HttpStatusException(response.StatusCode, message);
		}



		public static async Task FPostAsync(string url, object arg = null, int? timeout = null)
		{
			Debug.WriteLine("url: " + url);
			Debug.WriteLine("MRN: " + Credential.sharedInstance.Mrn.ToString());
			if (arg == null)
			{
				Debug.WriteLine("File arg was null!");
			}
			else
			{
				Debug.WriteLine("File arg was not null!");
			}
			var client = new HttpClient(); // { BaseAddress = new Uri(url) };
			Debug.WriteLine("client: " + client.ToString());
			//if (timeout.HasValue) client.Timeout = new TimeSpan(0, timeout.Value, 0);

			client.Timeout = new TimeSpan(0, 5, 0); //5 minute timeout


			if (!string.IsNullOrEmpty(Credential.sharedInstance.Token)) client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Credential.sharedInstance.Token}");

			//var test_content = new StringContent(JsonConvert.SerializeObject(arg), Encoding.UTF8, "application/json");
			//string output = JsonConvert.SerializeObject(arg);
			//Debug.WriteLine("Json stuff: "+ output);
			//Debug.WriteLine("Json decode: "+ JsonConvert.DeserializeObject(test_content.ToString()).ToString());

			Debug.WriteLine("I made it!");

			var content = arg != null ? new StringContent(JsonConvert.SerializeObject(arg), Encoding.UTF8, "application/json") : null;
			//var response = await DoWithRetryAsync(() => client.PostAsync(url, content));

			var response = await DoWithRetryAsync(() => client.PostAsync(url, content));
			if (response.IsSuccessStatusCode)
			{
				Debug.WriteLine("Successful file upload! Woohoo!");
				return;
			}
			else
			{
				//what did it return
				Debug.WriteLine("Response: " + response.StatusCode);

			}
			var message = await DoWithRetryAsync(() => response.Content.ReadAsStringAsync());
			//var message = await response.Content.ReadAsStringAsync();
			Debug.WriteLine("file error message: " + message);
			throw new HttpStatusException(response.StatusCode, message);
		}

		/*
        public static async Task<T> PostAsync<T>(string url, T arg, int? timeout = null)
        {
            Debug.WriteLine("url: " + url);
            var client = new HttpClient() { BaseAddress = new Uri(url) };
            if (timeout.HasValue) client.Timeout = new TimeSpan(0, timeout.Value, 0);
            if (!string.IsNullOrEmpty(Credential.sharedInstance.Token)) client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Credential.sharedInstance.Token}");

            var content = new StringContent(JsonConvert.SerializeObject(arg), Encoding.UTF8, "application/json");
            var response = await DoWithRetryAsync(() => client.PostAsync(url, content));
            if (!response.IsSuccessStatusCode)
            {
                var message = await DoWithRetryAsync(() => response.Content.ReadAsStringAsync());
                throw new HttpStatusException(response.StatusCode, message);
            }

            var json = await DoWithRetryAsync(() => response.Content.ReadAsStringAsync());
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static async Task<TR> PostAsync<T, TR>(string url, T arg, int? timeout = null)
        {
            Debug.WriteLine("url: " + url);
            var client = new HttpClient() { BaseAddress = new Uri(url) };
            if (timeout.HasValue) client.Timeout = new TimeSpan(0, timeout.Value, 0);
            if (!string.IsNullOrEmpty(Credential.sharedInstance.Token)) client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Credential.sharedInstance.Token}");

            var content = new StringContent(JsonConvert.SerializeObject(arg), Encoding.UTF8, "application/json");
            var response = await DoWithRetryAsync(() => client.PostAsync(url, content));
            if (!response.IsSuccessStatusCode)
            {
                var message = await DoWithRetryAsync(() => response.Content.ReadAsStringAsync());
                throw new HttpStatusException(response.StatusCode, message);
            }

            var json = await DoWithRetryAsync(() => response.Content.ReadAsStringAsync());
            return JsonConvert.DeserializeObject<TR>(json);
        }

        public static async Task<T> PutAsync<T>(string url, T arg, int? timeout = null)
        {
            Debug.WriteLine("url: " + url);
            var client = new HttpClient() { BaseAddress = new Uri(url) };
            if (timeout.HasValue) client.Timeout = new TimeSpan(0, timeout.Value, 0);
            if (!string.IsNullOrEmpty(Credential.sharedInstance.Token)) client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Credential.sharedInstance.Token}");

            var content = new StringContent(JsonConvert.SerializeObject(arg), Encoding.UTF8, "application/json");
            var response = await DoWithRetryAsync(() => client.PutAsync(url, content));
            if (!response.IsSuccessStatusCode)
            {
                var message = await DoWithRetryAsync(() => response.Content.ReadAsStringAsync());
                throw new HttpStatusException(response.StatusCode, message);
            }

            var json = await DoWithRetryAsync(() => response.Content.ReadAsStringAsync());
            return JsonConvert.DeserializeObject<T>(json);
        }
        */
		private static Task<T> DoWithRetryAsync<T>(Func<Task<T>> procedure)
		{
			return Policy.Handle<WebException>()
				.WaitAndRetryAsync(5, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)))
				.ExecuteAsync(procedure);
		}
	}
}