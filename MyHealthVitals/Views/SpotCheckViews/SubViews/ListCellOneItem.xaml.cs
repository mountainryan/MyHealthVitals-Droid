﻿using System;
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
                //layoutholder.HeightRequest = 70;
				itemdate.FontSize = 30 * Screensize.heightfactor;
				itemdate.WidthRequest = 380 * Screensize.widthfactor;
                item.FontSize = 30 * Screensize.heightfactor;
				item.WidthRequest = 360 * Screensize.widthfactor;
				//layoutholder.HeightRequest *= 1.5;
			}
            else if (Device.Idiom == TargetIdiom.Phone)
            {
				itemdate.FontSize *= Screensize.heightfactor;
				itemdate.WidthRequest *= Screensize.widthfactor;
				item.FontSize *= Screensize.heightfactor;
				item.WidthRequest *= Screensize.widthfactor;
            }
		}

	}
}
