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
            //edit.SetPadding(20,0,20,0);
            edit.SetPaddingRelative(50,20,20,20);

			alert.SetView(edit);

			alert.SetTitle(popup.Title);

			alert.SetPositiveButton("Yes", (senderAlert, args) =>
			{
				popup.OnPopupClosed(new EntryPopupClosedArgs
				{
					Button = "Yes",
					Text = edit.Text
				});
			});

			alert.SetNegativeButton("No", (senderAlert, args) =>
			{
				popup.OnPopupClosed(new EntryPopupClosedArgs
				{
					Button = "No",
					Text = edit.Text
				});
			});
			alert.Show();
		}
	}
}