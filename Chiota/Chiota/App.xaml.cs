﻿using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Chiota
{
  using Chiota.Services;
  using Chiota.Views;

  using Xamarin.Forms;

  using ContactPage = Views.ContactPage;
  using LoginPage = Views.LoginPage;

  /// <summary>
  /// The app.
  /// </summary>
  public partial class App : Application
  {
    public App()
    {
      this.InitializeComponent();
      this.MainPage = new GreyPage();
    }

    public static string AppName => "Chiota";

    protected override async void OnStart()
    {
      var secureStorage = new SecureStorage();
      if (secureStorage.CheckUserStored())
      {
        var user = await secureStorage.GetUser();

        // user = null => setup probably interrupted
        this.MainPage = user != null ? new NavigationPage(new ContactPage(user)) : new NavigationPage(new LoginPage());
      }
      else
      {
        this.MainPage = new NavigationPage(new LoginPage());
      }
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
