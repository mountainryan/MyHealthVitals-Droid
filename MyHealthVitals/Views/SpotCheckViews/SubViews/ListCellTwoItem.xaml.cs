using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public partial class ListCellTwoItem : ViewCell
	{
	//	private const int V = 0x53A7DE;;
		public static String fileName;
		public static String itemDate;
		public ListCellTwoItem()
		{
			InitializeComponent();
			if (Device.Idiom == TargetIdiom.Tablet)
			{
				itemdate.FontSize = 30 * Screensize.heightfactor;
				itemdate.WidthRequest = 400 * Screensize.widthfactor;
				firstItem.FontSize = 30 * Screensize.heightfactor;
				firstItem.WidthRequest = 180 * Screensize.widthfactor;
				secondItem.FontSize = 30 * Screensize.heightfactor;
				secondItem.WidthRequest = 180 * Screensize.widthfactor;
				underline.WidthRequest = 100 * Screensize.widthfactor;

                //layoutholder.HeightRequest *= 1.5;
			}
            else if (Device.Idiom == TargetIdiom.Phone)
            {
				itemdate.FontSize = 16 * Screensize.heightfactor;
				itemdate.WidthRequest = 200 * Screensize.widthfactor;
				firstItem.FontSize = 16 * Screensize.heightfactor;
				firstItem.WidthRequest = 90 * Screensize.widthfactor;
				secondItem.FontSize = 16 * Screensize.heightfactor;
				secondItem.WidthRequest = 90 * Screensize.widthfactor;
				underline.WidthRequest = 50 * Screensize.widthfactor;
            }
			secondItem.GestureRecognizers.Add(new TapGestureRecognizer((view) => OnLabelClicked()));
		}
		protected override void OnAppearing()
		{
			base.OnAppearing();

			//Debug.WriteLine("ListCellTwoItem OnAppearing");
			if (secondItem.Text != null && secondItem.Text.Equals("Saved"))
            {
				underline.IsVisible = true;
			}
			else
			{
				secondItem.TextColor = Color.White;
			}
		}
		async void OnLabelClicked()
		{
            //	String fileName;
            itemDate = itemdate.Text;
            DateTime iDate = Convert.ToDateTime(itemDate);
            String date_nosec = iDate.ToString("MM/dd/yyyy hh:mm tt");

			fileName = Regex.Replace(date_nosec, @"\s+", "");//dateTime.Trim(' ')
			fileName = Regex.Replace(fileName, @"[/:]+", "");
			Debug.WriteLine("OnLabelClicked.fileName==" + fileName);
			
            Task_vars.ecgdate = Convert.ToDateTime(itemdate.Text);
			bool ret = DependencyService.Get<IFileHelper>().checkFileExist(fileName + ".txt");
			bool ret2 = DependencyService.Get<IFileHelper>().checkFileExist(fileName + "ECG.pdf");

			if (secondItem.Text.Equals("Saved") && ret)
			{
                try
                {
					Task_vars.lastecgreading = await Reading.GetSingleReadingFromService(Convert.ToInt64(id.Text));
					var newPage = new EcgReport(fileName, Demographics.sharedInstance.FirstName, true);
					newPage.Title = "ECG Report";
					Task_vars.comingfrom = "ListPage";
					await Application.Current.MainPage.Navigation.PushModalAsync(new NavigationPage(newPage));
                }
                catch (Exception ex)
                {
					Debug.WriteLine("error message: " + ex.Message);
					if (Device.Idiom == TargetIdiom.Tablet)
					{
						await DependencyService.Get<IFileHelper>().dispAlert("Error", "There was an error retrieving the data", true, "OK", null);
					}
					else
					{
						await DependencyService.Get<IFileHelper>().dispAlert("Error", "There was an error retrieving the data", false, "OK", null);
					}
                }

		    }
			else if (secondItem.Text.Equals("Saved") && ret2)
			{
				//actually saved, but pdf still exists so email they can email it
				//don't actually need to change anything in the DB
				LayoutLoading();
                try
                {
					Task_vars.lastecgreading = await Reading.GetSingleReadingFromService(Convert.ToInt64(id.Text));
					var ecgread = Task_vars.lastecgreading;
					if (ecgread.FileId == 0)
					{
						//somehow made it to the device but not to the server
						//get the pdf in byte[]
						FileData ecgfile = new FileData();
						ecgfile.ServiceDate = ecgread.Date;
						ecgfile.Content = await DependencyService.Get<IFileHelper>().BytesFromFile(fileName + "ECG.pdf");
						Demographics demo = Demographics.sharedInstance;
						string name = demo.FirstName + "_" + demo.MiddleName + "_" + demo.LastName;
						string fdate = ecgread.Date.ToString("MMddyyyy_HHmm");
						ecgfile.Name = name + "_ECGReport_" + fdate + ".pdf";
						ecgfile.Category = "Cardiology (ECG, EKGs, Stress Test, etc.)";
						ecgfile.Size = Task_vars.ecgfilelength;
						ecgfile.UploadDate = DateTime.Now;

						var fileId = await EcgReport.FPostAsync(Credential.BASE_URL + $"Patient/{Credential.sharedInstance.Mrn}/File", ecgfile);
						Debug.WriteLine("fileID = " + fileId);
						//now update the reading with the new FileId
						ecgread.FileId = fileId;
						var val = await ecgread.UpdateReadingToService();
						Debug.WriteLine("val for update = " + val);
					}
					LayoutLoadingDone();
					await Task.Run(() =>
					{
						DependencyService.Get<IFileHelper>().setEmailClient();
					});

					var vals = await DependencyService.Get<IFileHelper>().sentToEmail(fileName + "ECG.pdf");
                }
                catch (Exception ex)
                {
                    LayoutLoadingDone();
					Debug.WriteLine("error message: " + ex.Message);
					if (Device.Idiom == TargetIdiom.Tablet)
					{
						await DependencyService.Get<IFileHelper>().dispAlert("Error", "There was an error retrieving the data", true, "OK", null);
					}
					else
					{
						await DependencyService.Get<IFileHelper>().dispAlert("Error", "There was an error retrieving the data", false, "OK", null);
					}
                }
				

			}
			else if (secondItem.Text.Equals("Saved"))
			{
				//it's been saved to the DB, but hasn't yet been saved locally or has already been emailed

				//call for a layout loading screen
				layoutholder.HeightRequest *= 2;
				LayoutLoading();

                try
                {
					Task_vars.lastecgreading = await Reading.GetSingleReadingFromService(Convert.ToInt64(id.Text));
					var ecgfile = await Reading.GetFileFromService(Task_vars.lastecgreading.FileId);

					Debug.WriteLine("filename = " + fileName + "ECG.pdf");

					Task_vars.ecgfiles.Add(fileName);

					//save byte[] to pdf on device and email it
					var val = await DependencyService.Get<IFileHelper>().SaveFromBytes(ecgfile.Content, fileName + "ECG.pdf");

					LayoutLoadingDone();
					layoutholder.HeightRequest /= 2;
					Debug.WriteLine("lastecgreading.Id = " + Task_vars.lastecgreading.Id);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("error message: "+ ex.Message);
                    if (Device.Idiom == TargetIdiom.Tablet)
                    {
                        await DependencyService.Get<IFileHelper>().dispAlert("Error", "There was an error retrieving the data", true, "OK", null);
                    }else{
                        await DependencyService.Get<IFileHelper>().dispAlert("Error", "There was an error retrieving the data", false, "OK", null);
                    }
                    //var val = await DependencyService.Get<IFileHelper>().SaveFromBytes(ecgfile.Content, fileName + "ECG.pdf");
                }

				

			}
			else
			{
				//Unavailable so we can't do anything with it
			}
		}
		public async void LayoutLoading()
		{
			layoutLoading.IsVisible = true;
		}
		public async void LayoutLoadingDone()
		{
			layoutLoading.IsVisible = false;
		}
	}
}