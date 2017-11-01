using MyHealthVitals.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Views;

[assembly: ExportRenderer(typeof(Entry), typeof(NoUnderlineEntry))]
namespace MyHealthVitals.Droid
{
    public class NoUnderlineEntry : EntryRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);
			Control?.SetBackgroundColor(Android.Graphics.Color.Transparent);
            Control?.SetTextColor(Android.Graphics.Color.Black);
			Control.SetWidth(600);
            Control?.SetWidth(600);
			Control.Gravity = GravityFlags.CenterVertical;
			Control.Gravity = GravityFlags.CenterHorizontal;

		}
	}
}