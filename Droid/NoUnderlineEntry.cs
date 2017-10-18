using MyHealthVitals.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

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
		}
	}
}