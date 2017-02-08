﻿using System;
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
			// calculating the height of
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			this.layoutLoading.IsVisible = false;

		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			scrollView.HeightRequest = this.Content.Bounds.Size.Height - layoutImgContainer.Height - layoutLoginContainer.Height-40;
		}

		public async void btnLoginClicked(object sender, System.EventArgs e)
		{
			//var newScreen = new TestPage();
			//newScreen.Title = "Main Screeen";
			//var nav = new NavigationPage(newScreen);
			//this.Navigation.PushModalAsync(nav);

			layoutLoading.IsVisible = true;

			try
			{
				//credential = await Credential.Create("https://test.myemhr.com", 0, txtUsername.Text.Trim(), txtPassword.Text.Trim(), "Mobile");

				Credential cred = await Credential.sharedInstance.CallApiForLogin(txtUsername.Text.Trim(), txtPassword.Text.Trim());

				this.txtPassword.Text = "";

				Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
				{
					Debug.WriteLine("Login succesfull.");
					var newScreen = new MainPage();
					//var newScreen = new RespHomePage();
					newScreen.Title = "Main Screeen";
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
			finally{
				this.layoutLoading.IsVisible = false;
			}

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
