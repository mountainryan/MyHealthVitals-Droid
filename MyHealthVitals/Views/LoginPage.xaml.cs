using System;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MyHealthVitals
{
	public class Task_vars
	{
		public static Task[] tasks = new Task[1];
        public static string ecgmessage = "";
        public static DateTime ecgdate;
        public static Byte[] ecgcontent;
        public static string ecgfilename;
        public static string ecgfilepath;
        public static long ecgfilelength;
        public static string patient_name;
		public static string gifpath;
	}
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
			if (Device.Idiom == TargetIdiom.Tablet)
			{
				//layout.WidthRequest = 350;
				//layout.Margin = new Thickness(160,10,10,10);
				//layout.HorizontalOptions = LayoutOptions.Center;
				txtUsername.WidthRequest = 400;
				txtUsername.HeightRequest = 75;
				txtPassword.WidthRequest = 400;
				txtPassword.HeightRequest = 75;
				txtUsername.FontSize = 30;
				txtPassword.FontSize = 30;
				txtUsername.Margin = new Thickness(0, 2, 0, 1);
				txtPassword.Margin = new Thickness(0, 1, 0, 2);
				btnLogin.WidthRequest = 150;
				btnLogin.FontSize = 30;
				//txtUsername.HorizontalOptions = LayoutOptions.Center;
				//txtPassword.HorizontalOptions = LayoutOptions.Center;
				//btnLogin.HeightRequest = 30;//txtPassword.HeightRequest + txtUsername.HeightRequest   ;
				Debug.WriteLine("btnLogin.HeightRequest===" + btnLogin.HeightRequest);
			}
			else
			{
				icucareimg.Source = "icucarellc_phone.png";
				layoutImgContainer.Margin = new Thickness(45);
				//txtUsername.WidthRequest *= (2 / 3);
				//txtPassword.WidthRequest *= (2 / 3);
				//txtUsername.HeightRequest *= (2 / 3);
				//txtPassword.HeightRequest *= (2 / 3);
				//txtUsername.FontSize *= (2 / 3);
				//txtPassword.FontSize *= (2 / 3);
				//btnLogin.WidthRequest *= (2 / 3);
				//btnLogin.FontSize *= (2 / 3);
				scrollView.MinimumWidthRequest *= .5;
				scrollView.Margin = new Thickness(30);
				lblWelcome.Margin = new Thickness(30);
				lblWelcome.FontSize = 17;
			}

            if (Demographics.sharedInstance.isAutoLogin)
            {
                txtUsername.Text = Demographics.sharedInstance.username;
                txtPassword.Text = Demographics.sharedInstance.password;
                doLogin(Demographics.sharedInstance.username, Demographics.sharedInstance.password);
            }
            else if (Demographics.sharedInstance.isRememberUsername)
            {
                txtUsername.Text = Demographics.sharedInstance.username;
            }
            else
            {
                txtUsername.Text = "";
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

			Debug.WriteLine("txtUsername width: "+txtUsername.WidthRequest);
			Debug.WriteLine("txtPassword width: " + txtPassword.WidthRequest);
			Debug.WriteLine("txtUsername Height: " + txtUsername.HeightRequest);
			Debug.WriteLine("txtPassword Height: " + txtPassword.HeightRequest);

		//	scrollView.HeightRequest = this.Content.Bounds.Size.Height - layoutImgContainer.Height - layoutLoginContainer.Height - 40;
		}

        public async void doLogin(string username, string password)
        {
            layoutLoading.IsVisible = true;
            //var status = Reachability.   .InternetConnectionStatus();
            try
            {
				//first copy the gif asset
				//DependencyService.Get<IFileHelper>().copyAsset();
                Debug.WriteLine("name= " + username + " pw = " + password);
                Debug.WriteLine("Credential.sharedInstance = " + Credential.sharedInstance);
                //Credential cred = await Credential.sharedInstance.CallApiForLogin(txtUsername.Text.Trim(), txtPassword.Text.Trim());
                Credential cred = await Credential.sharedInstance.CallApiForLogin(username, password);

                Debug.WriteLine("cred= " + cred);



                Application.Current.Properties["_username"] = txtUsername.Text.Trim();
                await Application.Current.SavePropertiesAsync();

                if (cred.Token.Length > 0)
                {
                    Demographics.sharedInstance.username = txtUsername.Text.Trim();
                    Demographics.sharedInstance.password = txtPassword.Text.Trim();
                }

                this.txtPassword.Text = "";

                Demographics.sharedInstance.updateDemographicsFromLocal();

                Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                {
                    Debug.WriteLine("Login succesfull.");
                    var newScreen = new DeviceListPage();
                    //var newScreen = new RespHomePage();
                    newScreen.Title = " ";

                    var nav = new NavigationPage(newScreen);
                    this.Navigation.PushModalAsync(nav);
                });


                Task_vars.tasks[0] = Task.Run(async () =>
                {
					//var th = new Thread(GetReadings());
					//th.Start();
                    //GetReadings();
					Debug.WriteLine("task Id = " + Task.CurrentId);
					ParametersPageLocal.allReadings = await Reading.GetAllReadingsFromService(); Debug.WriteLine("sync data from website");
                });

            }

            catch (HttpStatusException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    this.ShowAlertForLogin("Username Password Combination is incorrect.");
                }
                else
                {
                    this.ShowAlertForLogin("An Error has occurred. ex.StatusCode=" + ex.StatusCode);
                }
            }

            catch (Exception e)
            {
                this.ShowAlertForLogin("An Error has occurred. Exception=" + e);
            }
            finally
            {
                this.layoutLoading.IsVisible = false;
            }
        }

        public void GetReadings()
        {
			//Debug.WriteLine("task Id = " + Task.CurrentId);
			//ParametersPageLocal.allReadings = await Reading.GetAllReadingsFromService(); Debug.WriteLine("sync data from website");
        }

        public void btnLoginClicked(object sender, System.EventArgs e)
        {
            doLogin(txtUsername.Text.Trim(), txtPassword.Text.Trim());
        }

        public void ShowAlertForLogin(String message)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                {
                    DisplayAlert("Login Error", message, "OK");
                });
        }
    }


}
