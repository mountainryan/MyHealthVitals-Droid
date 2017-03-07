using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public partial class DeviceListPage : ContentPage
	{
		void btnSpirometerClicked(object sender, System.EventArgs e)
		{
			var newScreen = new RespHomePage();
			newScreen.Title = "My Resp Check";
			this.Navigation.PushAsync(newScreen);
		}

		void btnPC300clicked(object sender, System.EventArgs e)
		{
			var newScreen = new MainPage();
			newScreen.Title = "Main Screeen";
			this.Navigation.PushAsync(newScreen);

			//btnMyButton.ab
		}

		void btnLogOutClicked(object sender, System.EventArgs e)
		{
			//Debug.WriteLine(" log out");
			//Demographics.sharedInstance.clearLocalStorageOnLogout();

			//Demographics.sharedInstance.password = "";

			this.Navigation.PopModalAsync(true);
		}

		public DeviceListPage()
		{
			InitializeComponent();

			//DependencyService.Get<ICBCentralManager>().ConnectToDevice((IBluetoothCallBackUpdatable)this);
		}
	}
}
