
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
		protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Switch> e)
		{
			base.OnElementChanged(e);
            if (Device.Idiom == TargetIdiom.Tablet)
            {
                if (Control != null)
                {
                    Control.SetTrackResource(Resource.Drawable.track);
                    Control.SetThumbResource(Resource.Drawable.thumb);
                }
            }
		}
	}
}
