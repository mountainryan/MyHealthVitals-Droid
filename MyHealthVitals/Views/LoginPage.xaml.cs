using System;
using System.Collections.Generic;
using System.Diagnostics;

using Xamarin.Forms;

namespace MyHealthVitals
{
	public partial class LoginPage : ContentPage
	{
		public LoginPage()
		{
			InitializeComponent();

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
			else {
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

			scrollView.HeightRequest = this.Content.Bounds.Size.Height - layoutImgContainer.Height - layoutLoginContainer.Height-40;
		}

		public async void doLogin(string username,string password) { 
			layoutLoading.IsVisible = true;

			try
			{
				//Credential cred = await Credential.sharedInstance.CallApiForLogin(txtUsername.Text.Trim(), txtPassword.Text.Trim());
				Credential cred = await Credential.sharedInstance.CallApiForLogin(username, password);

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
			}

			catch (HttpStatusException ex)
			{
				if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				{
					this.ShowAlertForLogin("Username Password Combination is incorrect.");
				}
				else
				{
					this.ShowAlertForLogin("An Error has occurred.");
				}
			}

			catch (Exception)
			{
				this.ShowAlertForLogin("An Error has occurred.");
			}
			finally
			{
				this.layoutLoading.IsVisible = false;
			}
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
