using Android.App;
using Android.Widget;
//using MyHealthVitals.Common.Controls;
using MyNameSpace.Droid.Implementation;
using MyHealthVitals.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(EntryPopupLoader))]
namespace MyNameSpace.Droid.Implementation
{
	public class EntryPopupLoader : IEntryPopupLoader
	{
		public void ShowPopup(EntryPopup popup)
		{
			var alert = new AlertDialog.Builder(Forms.Context);

			var edit = new EditText(Forms.Context) { Text = popup.Text };
			alert.SetView(edit);

			alert.SetTitle(popup.Title);

			alert.SetPositiveButton("OK", (senderAlert, args) =>
			{
				popup.OnPopupClosed(new EntryPopupClosedArgs
				{
					Button = "OK",
					Text = edit.Text
				});
			});

			alert.SetNegativeButton("Cancel", (senderAlert, args) =>
			{
				popup.OnPopupClosed(new EntryPopupClosedArgs
				{
					Button = "Cancel",
					Text = edit.Text
				});
			});
			alert.Show();
		}
	}
}