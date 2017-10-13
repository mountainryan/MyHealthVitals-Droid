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
					itemdate.FontSize *= 1.5;
					itemdate.WidthRequest *= 2;
					item.FontSize *= 1.5;
					item.WidthRequest *= 2;
				}
		}

	}
}
