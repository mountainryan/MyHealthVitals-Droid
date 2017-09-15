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

			lblFirstTitle.Text = firstHeaderTitle;
			lblSecondTitle.Text = secondHeaderTitle;
		}
	}
}
