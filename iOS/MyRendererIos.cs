using System;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;
using UIKit;
using MyHealthVitals.iOS;
using MyHealthVitals;

[assembly: ExportRenderer(typeof(CustomListView), typeof(MyListViewRenderer))]
[assembly: ExportRenderer(typeof(MyEntry), typeof(MyEntryRenderer))]

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

	public class MyEntryRenderer : EntryRenderer { 
		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);

			if (Control != null)
			{
				Control.SpellCheckingType = UITextSpellCheckingType.No;             // No Spellchecking
				Control.AutocorrectionType = UITextAutocorrectionType.No;           // No Autocorrection
				Control.AutocapitalizationType = UITextAutocapitalizationType.None; // No Autocapitalization
			}
		}
	}
}
