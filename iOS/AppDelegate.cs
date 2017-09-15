using System;
using System.Collections.Generic;
using System.Linq;
using SegmentedControl;
using Foundation;
using UIKit;

namespace MyHealthVitals.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init();

			SegmentedControl.FormsPlugin.iOS.SegmentedControlRenderer.Init();

			OxyPlot.Xamarin.Forms.Platform.iOS.PlotViewRenderer.Init();

			LoadApplication(new App());

			return base.FinishedLaunching(app, options);
		}
	}
}
