using System;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;
using UIKit;
using MyHealthVitals.iOS;
using MyHealthVitals;

[assembly: ExportRenderer(typeof(CustomListView), typeof(MyListViewRenderer))]
[assembly: ExportRenderer(typeof(MyEntry), typeof(MyEntryRenderer))]
[assembly: ExportRenderer(typeof(MyButton), typeof(MyButtonRenderer))]

[assembly: ExportRenderer(typeof(RoundedBox), typeof(RoundedBoxRenderer))]

namespace MyHealthVitals.iOS
{
	public class MyListViewRenderer : ListViewRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.ListView> e)
		{
			base.OnElementChanged(e);
			Control.AllowsSelection = false;
		}
	}

	public class RoundedBoxRenderer : BoxRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
		{
			base.OnElementChanged(e);

			if (Element != null)
			{
				Layer.MasksToBounds = true;
				UpdateCornerRadius(Element as RoundedBox);
			}
		}

		void UpdateCornerRadius(RoundedBox box)
		{
			Layer.CornerRadius = 4;
		}
	}

	public class MyButtonRenderer : ButtonRenderer { 
		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			base.OnElementChanged(e);

			if (Control != null) { 
				Control.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
				Control.TitleLabel.TextAlignment = UITextAlignment.Center;
			}
		}
	}

	public class MyEntryRenderer : EntryRenderer { 
		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);

			if (Control != null)
			{
				//Control.BackgroundColor = UIColor.FromRGB(204, 153, 255);
				Control.BorderStyle = UITextBorderStyle.None;
				Control.TextAlignment = UITextAlignment.Center;
				Control.SpellCheckingType = UITextSpellCheckingType.No;             // No Spellchecking
				Control.AutocorrectionType = UITextAutocorrectionType.No;           // No Autocorrection
				Control.AutocapitalizationType = UITextAutocapitalizationType.None; // No Autocapitalization
			}
		}
	}
}
