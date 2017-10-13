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
				layout.Spacing *= 2;

				labeldate.FontSize *= 1.5;
				lblFirstTitle.FontSize *= 1.5;
				labeldate.WidthRequest *= 2;
				lblFirstTitle.WidthRequest *= 2;
			}
			lblFirstTitle.Text = firstHeaderTitle;
		}
	}
}
