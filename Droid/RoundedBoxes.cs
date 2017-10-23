
using MyHealthVitals.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(BoxView), typeof(RoundedBoxRenderer))]
namespace MyHealthVitals.Droid
{
	

	public class RoundedBoxRenderer : BoxRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
		{
			base.OnElementChanged(e);
			if (e.OldElement == null)
			{
				SetBackgroundResource(MyHealthVitals.Droid.Resource.Drawable.CustomButtonBackground);
			}
		}

	}



}
