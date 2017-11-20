using System;
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
			if (secondItem.Text != null && (secondItem.Text.Equals("No Report") || secondItem.Text.Equals("Saved")))
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
			if (secondItem.Text.Equals("No Report"))
			{
				//secondItem.TextColor = Color.Blue;
				var newPage = new EcgReport(fileName, Demographics.sharedInstance.FirstName, true);
				newPage.Title = "ECG Report";
                //Navigation.PushAsync(newPage);
                Task_vars.comingfrom = "ListPage";
				await Application.Current.MainPage.Navigation.PushModalAsync(new NavigationPage(newPage));
			}
			else if (secondItem.Text.Equals("Saved"))
			{
			//	layoutLoading.IsVisible = true;
				await Task.Run(() =>
				{
					DependencyService.Get<IFileHelper>().setEmailClient();
				});
		//		Device.BeginInvokeOnMainThread(() =>
		//		{
					//	await DependencyService.Get<IFileHelper>().setEmailClient();
					Task<bool> ret = DependencyService.Get<IFileHelper>().sentToEmail(fileName + "ECG.pdf");
				//	if (ret) layoutLoading.IsVisible = false;
			//	});
			}
		}
	}
}