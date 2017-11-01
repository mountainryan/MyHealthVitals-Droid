/*
using MyHealthVitals;
using MyHealthVitals.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using System;
using Android.Content.Res;


[assembly: ExportRenderer(typeof(Switch), typeof(CustomSwitch))]
namespace MyHealthVitals.Droid
{
	public class CustomSwitch : SwitchRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
		{
			base.OnElementChanged(e);
            Control?.SetHeight(120);
            Control.SetHeight(120);
            Control.SetMinimumHeight(120);
            Control?.SetMinimumHeight(120);
            Control.SetMinHeight(120);
            Control?.SetMinHeight(120);

			if (Control != null)
			{
				//Control.TextOn = "AAN";
				//Control.TextOff = "UIT";
                //Control.SetTextColor(Android.Graphics.Color.HotPink);//Color.White);
			}

			if (Control.Checked == true)
			{
                //Control.SetBackgroundColor(Android.Graphics.Color.Gold);//Color.Green);
			}
		}
	}
}
*/