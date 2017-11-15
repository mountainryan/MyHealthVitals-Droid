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
				layout.Spacing = 12 * Screensize.heightfactor;
				labeldate.FontSize = 30 * Screensize.heightfactor;
                lblFirstTitle.FontSize = 30 * Screensize.heightfactor;
				lblSecondTitle.FontSize = 30 * Screensize.heightfactor;
				labeldate.WidthRequest = 380 * Screensize.widthfactor;
				lblFirstTitle.WidthRequest = 180 * Screensize.widthfactor;
				lblSecondTitle.WidthRequest = 180 * Screensize.widthfactor;
            }
            else if (Device.Idiom == TargetIdiom.Phone)
            {
				layout.Spacing = 6 * Screensize.heightfactor;
				labeldate.FontSize = 16 * Screensize.heightfactor;
				lblFirstTitle.FontSize = 16 * Screensize.heightfactor;
				lblSecondTitle.FontSize = 16 * Screensize.heightfactor;
				labeldate.WidthRequest = 190 * Screensize.widthfactor;
				lblFirstTitle.WidthRequest = 90 * Screensize.widthfactor;
				lblSecondTitle.WidthRequest = 90 * Screensize.widthfactor;
            }
			lblFirstTitle.Text = firstHeaderTitle;
			lblSecondTitle.Text = secondHeaderTitle;
		}
	}
}
