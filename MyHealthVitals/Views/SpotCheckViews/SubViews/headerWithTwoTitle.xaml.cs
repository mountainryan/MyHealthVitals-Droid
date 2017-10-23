using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public partial class headerWithTwoTitle : ContentView
	{
		public headerWithTwoTitle(String firstHeaderTitle,string secondHeaderTitle)
		{
			InitializeComponent();
			if (Device.Idiom == TargetIdiom.Tablet)
			{
				layout.Spacing *= 2;
				labeldate.FontSize = 30;
                lblFirstTitle.FontSize = 30;
				lblSecondTitle.FontSize = 30;
				labeldate.WidthRequest *= 2;
				lblFirstTitle.WidthRequest *= 2;
				lblSecondTitle.WidthRequest *= 2;

			}
			lblFirstTitle.Text = firstHeaderTitle;
			lblSecondTitle.Text = secondHeaderTitle;
		}
	}
}
