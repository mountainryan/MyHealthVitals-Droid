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
			lblFirstTitle.Text = firstHeaderTitle;
		}
	}
}
