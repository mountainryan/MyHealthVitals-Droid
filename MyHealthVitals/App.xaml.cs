using Xamarin.Forms;
using nexus.core.logging;
using nexus.protocols.ble;

namespace MyHealthVitals
{
	public partial class App : Application
	{
		public App(int dpwidth, int dpheight, IBluetoothLowEnergyAdapter adapter)
		{
			InitializeComponent();
			MainPage = new LoginPage(dpwidth, dpheight, adapter);
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}
