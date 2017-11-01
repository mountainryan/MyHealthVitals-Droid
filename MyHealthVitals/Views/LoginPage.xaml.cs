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
    public class Screensize
    {
        public static int targetwidth_pad = 800;
        public static int targetheight_pad = 1280;
        public static int targetwidth_phone = 411;
        public static int targetheight_phone = 797;
        public static int dpwidth;
        public static int dpheight;
        public static double widthfactor;
        public static double heightfactor;
    }
    public partial class LoginPage : ContentPage
    {
        public LoginPage(int dpwidth, int dpheight)
        {
            Screensize.dpwidth = dpwidth;
            Screensize.dpheight = dpheight;



            InitializeComponent();
			if (Device.Idiom == TargetIdiom.Tablet)
			{
                Screensize.widthfactor = Convert.ToDouble(dpwidth) / Convert.ToDouble(Screensize.targetwidth_pad);
                Screensize.heightfactor = Convert.ToDouble(dpheight) / Convert.ToDouble(Screensize.targetheight_pad);
                layoutImgContainer.Margin = new Thickness(90 * Screensize.widthfactor);
                icucareimg.Source = "icucarellc.png";
				scrollView.Margin = new Thickness(60 * Screensize.widthfactor);
				lblWelcome.Margin = new Thickness(60 * Screensize.widthfactor);
                //layout.WidthRequest = 350;
                //layout.Margin = new Thickness(160,10,10,10);
                //layout.HorizontalOptions = LayoutOptions.Center;
                scrollView.MinimumWidthRequest = 300 * Screensize.widthfactor;
				txtUsername.WidthRequest = 400 * Screensize.widthfactor;
				txtUsername.HeightRequest = 75 * Screensize.heightfactor;
				txtPassword.WidthRequest = 400 * Screensize.widthfactor;
				txtPassword.HeightRequest = 75 * Screensize.heightfactor;
				txtUsername.FontSize = 30 * Screensize.heightfactor;
				txtPassword.FontSize = 30 * Screensize.heightfactor;
				txtUsername.Margin = new Thickness(0, 2, 0, 1);
				txtPassword.Margin = new Thickness(0, 1, 0, 2);
				btnLogin.WidthRequest = 150 * Screensize.widthfactor;
				btnLogin.FontSize = 30 * Screensize.heightfactor;
                lblWelcome.FontSize = 24 * Screensize.heightfactor;
				//txtUsername.HorizontalOptions = LayoutOptions.Center;
				//txtPassword.HorizontalOptions = LayoutOptions.Center;
				//btnLogin.HeightRequest = 30;//txtPassword.HeightRequest + txtUsername.HeightRequest   ;
				Debug.WriteLine("btnLogin.HeightRequest===" + btnLogin.HeightRequest);
			}
			else if (Device.Idiom == TargetIdiom.Phone)
			{
				Screensize.widthfactor = Convert.ToDouble(dpwidth) / Convert.ToDouble(Screensize.targetwidth_phone);
				Screensize.heightfactor = Convert.ToDouble(dpheight) / Convert.ToDouble(Screensize.targetheight_phone);
				layoutImgContainer.Margin = new Thickness(45 * Screensize.widthfactor);
				//icucareimg.Source = "icucarellc.png";
                scrollView.Margin = new Thickness(30 * Screensize.widthfactor);
				lblWelcome.Margin = new Thickness(30 * Screensize.widthfactor);
				//layout.WidthRequest = 350;
				//layout.Margin = new Thickness(160,10,10,10);
				//layout.HorizontalOptions = LayoutOptions.Center;
				scrollView.MinimumWidthRequest *= Screensize.widthfactor;
                txtUsername.WidthRequest *= Screensize.widthfactor;
                txtUsername.HeightRequest *= Screensize.heightfactor;
				txtPassword.WidthRequest *= Screensize.widthfactor;
				txtPassword.HeightRequest *= Screensize.heightfactor;
				txtUsername.FontSize *= Screensize.heightfactor;
				txtPassword.FontSize *= Screensize.heightfactor;
				//txtUsername.Margin = new Thickness(0, 2, 0, 1);
				//txtPassword.Margin = new Thickness(0, 1, 0, 2);
				btnLogin.WidthRequest *= Screensize.widthfactor;
				btnLogin.FontSize *= Screensize.heightfactor;
				lblWelcome.FontSize *= Screensize.heightfactor;
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


            var result = 142^1;
            Debug.WriteLine("result ==== "+result);

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
            Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
            {
				if (Device.Idiom == TargetIdiom.Tablet)
				{
					var ret = await DependencyService.Get<IFileHelper>().dispAlert("Login Error", message, true, "OK", null);
				}
				else
				{
					var ret = await DependencyService.Get<IFileHelper>().dispAlert("Login Error", message, false, "OK", null);
				}
                //await DisplayAlert("Login Error", message, "OK");
            });
        }
    }


}
