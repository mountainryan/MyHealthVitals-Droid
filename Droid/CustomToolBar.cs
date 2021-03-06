﻿using MyHealthVitals.Droid;
using Xamarin.Forms;
using Android.Util;

using Xamarin.Forms.Platform.Android;
using System;
using System.Threading;
using System.Diagnostics;
using Android.Graphics;

//using AToolbar = Android.Support.V7.Widget.Toolbar;



[assembly: ExportRenderer(typeof(NavigationPage), typeof(CustomToolBar))]
namespace MyHealthVitals.Droid
{
    public class CustomToolBar : Xamarin.Forms.Platform.Android.AppCompat.NavigationPageRenderer
    {

		private Android.Support.V7.Widget.Toolbar toolbar;

		public override void OnViewAdded(Android.Views.View child)
		{
			base.OnViewAdded(child);
			//System.Diagnostics.Debug.WriteLine("child type = "+child.GetType().ToString());
			//System.Diagnostics.Debug.WriteLine("child type fullname = " + child.GetType().FullName);
			//System.Diagnostics.Debug.WriteLine("child type name = " + child.GetType().Name);
			if (child.GetType() == typeof(Android.Support.V7.Widget.Toolbar))
			{
				toolbar = (Android.Support.V7.Widget.Toolbar)child;
				toolbar.ChildViewAdded += Toolbar_ChildViewAdded;
			}
		}

		private void Toolbar_ChildViewAdded(object sender, ChildViewAddedEventArgs e)
		{
			var view = e.Child.GetType();
			if (e.Child.GetType() == typeof(Android.Widget.TextView))
			{
				
				var textView = (Android.Widget.TextView)e.Child;

				//var spaceFont = Typeface.CreateFromAsset(Xamarin.Forms.Forms.Context.ApplicationContext.Assets, "space_age.ttf");
				textView.SetTextSize(ComplexUnitType.Px, 80);
				//textView.Typeface = spaceFont;
				//toolbar.ChildViewAdded -= Toolbar_ChildViewAdded;

			}
		}


	}

}

/*
 		
	protected override void OnElementChanged(ElementChangedEventArgs<NavigationPage> e)
	{
		base.OnElementChanged(e);

	}
		protected override void OnElementChanged(ElementChangedEventArgs<NavigationPage> e)
		{
			base.OnElementChanged(e);
			//if (e.NewElement != null)
			//{


                var toolbar2 = GetToolbar();

            if (toolbar2 != null)
            {
				toolbar2.SetMinimumHeight(100);
				System.Diagnostics.Debug.WriteLine("Toolbar Height = " + toolbar2.Height);
            }
                
				//var navController = (INavigationPageController)e.NewElement;
				//navController.PushRequested += NavController_PushRequested;
				//navController.PopRequested += NavController_PopRequested;
			//}
		}

		private void NavController_PopRequested(object sender, Xamarin.Forms.Internals.NavigationRequestedEventArgs e)
		{
			Device.StartTimer(TimeSpan.FromMilliseconds(220), () =>
			{
				ChangeIconColor();
				return false;
			});
		}

		private void NavController_PushRequested(object sender, Xamarin.Forms.Internals.NavigationRequestedEventArgs e)
		{
			ChangeIconColor();
		}

		private void ChangeIconColor()
		{
			int count = this.ViewGroup.ChildCount;

			var toolbar = GetToolbar();

			if (toolbar.NavigationIcon != null)
			{
				//var drawable = (toolbar.NavigationIcon as DrawerArrowDrawable);
				//drawable.Color = Resource.Color.material_grey_850;//set the navigation icon color here
			}
		}

		private AToolbar GetToolbar()
		{
			for (int i = 0; i < this.ViewGroup.ChildCount; i++)
			{
				var child = this.ViewGroup.GetChildAt(i);
				if (child is AToolbar)
				{
					return (AToolbar)child;
				}
			}

			return null;
		}
	}
}
*/
