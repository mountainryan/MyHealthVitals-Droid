using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MyHealthVitals
{
	public partial class ListCellOneItem : ViewCell
	{
		public ListCellOneItem()
		{
			InitializeComponent();
			if (Device.Idiom == TargetIdiom.Tablet)
			{
                layoutholder.HeightRequest = 70;
				itemdate.FontSize = 30;
				itemdate.WidthRequest *= 2;
                item.FontSize = 30;
				item.WidthRequest *= 2;
				//layoutholder.HeightRequest *= 1.5;
			}	
		}

	}
}
