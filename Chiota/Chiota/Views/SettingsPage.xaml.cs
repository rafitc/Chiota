﻿namespace Chiota.Views
{
  using System;

  using Chiota.Models;
  using Chiota.ViewModels;

  using Xamarin.Forms;
  using Xamarin.Forms.Xaml;

  /// <summary>
  /// The add contact.
  /// </summary>
  [XamlCompilation(XamlCompilationOptions.Compile)]
  public partial class SettingsPage : ContentPage
  {
    // adds your contact automaticly to some elses contact list in form of an adress generated by yourself
    public SettingsPage(User user)
    {
      this.InitializeComponent();
    }
  }
}