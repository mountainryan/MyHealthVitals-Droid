using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Diagnostics;
using Android.Support.V7.AppCompat;

namespace MyHealthVitals.Droid
{
	[Activity(Label = "MyHealth\nVitals", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(bundle);

			var metrics = Resources.DisplayMetrics;
			var widthInDp = ConvertPixelsToDp(metrics.WidthPixels);
			var heightInDp = ConvertPixelsToDp(metrics.HeightPixels);



            System.Diagnostics.Debug.WriteLine("Pixel Width = "+metrics.WidthPixels);
            System.Diagnostics.Debug.WriteLine("Pixel Height = "+metrics.HeightPixels);
            System.Diagnostics.Debug.WriteLine("DP Width = "+widthInDp);
            System.Diagnostics.Debug.WriteLine("DP Height = "+heightInDp);

			global::Xamarin.Forms.Forms.Init(this, bundle);

			OxyPlot.Xamarin.Forms.Platform.Android.PlotViewRenderer.Init();

			LoadApplication(new App(widthInDp, heightInDp));
		}
		private int ConvertPixelsToDp(float pixelValue)
		{
			var dp = (int)((pixelValue) / Resources.DisplayMetrics.Density);
            return dp;
		}

	}
	public class Screensize
	{
        int dpwidth;
        int dpheight;
	}
}
