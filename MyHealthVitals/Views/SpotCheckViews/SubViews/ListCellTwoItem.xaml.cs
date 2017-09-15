﻿using System;
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
		String fileName;
		public ListCellTwoItem()
		{
			InitializeComponent();
			secondItem.GestureRecognizers.Add(new TapGestureRecognizer((view) => OnLabelClicked()));
	
		}
		protected override void OnAppearing()
		{
			base.OnAppearing();
			Debug.WriteLine("ListCellTwoItem OnAppearing");
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
			fileName = Regex.Replace(itemdate.Text, @"\s+", "");//dateTime.Trim(' ')
			fileName = Regex.Replace(fileName, @"[/:]+", "");
			Debug.WriteLine("OnLabelClicked.fileName==" + fileName);

			if (secondItem.Text.Equals("No Report"))
			{
				//secondItem.TextColor = Color.Blue;
				var newPage = new EcgReport(fileName, Demographics.sharedInstance.FirstName, true);
				newPage.Title = "ECG Report";
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