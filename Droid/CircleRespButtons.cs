using MyHealthVitals;
using MyHealthVitals.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CircleResp), typeof(CircleRespButtons))]
namespace MyHealthVitals.Droid
{
	public class CircleRespButtons : Xamarin.Forms.Platform.Android.AppCompat.ButtonRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			base.OnElementChanged(e);
			if (e.OldElement == null)
			{
				Control.SetBackgroundResource(MyHealthVitals.Droid.Resource.Drawable.CustomButtonBackground2);
			}
		}
	}
}