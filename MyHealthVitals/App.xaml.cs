using Xamarin.Forms;

namespace MyHealthVitals
{
	public partial class App : Application
	{
		public App(int dpwidth, int dpheight)
		{
			InitializeComponent();
			MainPage = new LoginPage(dpwidth, dpheight);
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
