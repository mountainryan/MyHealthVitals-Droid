using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace MyHealthVitals
{
    
	public partial class headerWithOneTitle : ContentView
	{
		public headerWithOneTitle(string firstHeaderTitle)
		{
			InitializeComponent();
			if (Device.Idiom == TargetIdiom.Tablet)
			{
                
				layout.Spacing = 12 * Screensize.heightfactor;
                labeldate.FontSize = 30 * Screensize.heightfactor;
				lblFirstTitle.FontSize = 30 * Screensize.heightfactor;
				labeldate.WidthRequest = 360 * Screensize.widthfactor;
				lblFirstTitle.WidthRequest = 360 * Screensize.widthfactor;
			}
            else if (Device.Idiom == TargetIdiom.Phone)
            {
                layout.Spacing = 6 * Screensize.heightfactor;
				labeldate.FontSize = 16 * Screensize.heightfactor;
				lblFirstTitle.FontSize = 16 * Screensize.heightfactor;
				labeldate.WidthRequest = 180 * Screensize.widthfactor;
                lblFirstTitle.WidthRequest = 180 * Screensize.widthfactor;
            }
			lblFirstTitle.Text = firstHeaderTitle;
		}
	}
}
