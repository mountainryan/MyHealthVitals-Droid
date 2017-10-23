//doesn't work!!
using MyHealthVitals;
using MyHealthVitals.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ListView), typeof(ListViewNoSel))]
namespace MyHealthVitals.Droid
{
	public class ListViewNoSel : ListViewRenderer
	{
        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
		{
			base.OnElementChanged(e);
			//Control.AllowsSelection = false;
            Control.SetSelector(Android.Resource.Color.Transparent);
			Control.CacheColorHint = Xamarin.Forms.Color.Transparent.ToAndroid();
            Control?.SetSelector(Android.Resource.Color.Transparent);

		}
	}
}
