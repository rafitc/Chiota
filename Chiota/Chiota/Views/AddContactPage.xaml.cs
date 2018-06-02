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
  public partial class AddContactPage : ContentPage
  {
    // adds your contact automaticly to some elses contact list in form of an adress generated by yourself
    public AddContactPage(User user)
    {
      this.InitializeComponent();

      var vm = new AddContactViewModel(user) { Navigation = this.Navigation }; 

      vm.DisplayInvalidAdressPrompt += () => this.DisplayAlert("Error", "Invalid address, try again", "OK");
      vm.SuccessfulRequestPrompt += () => this.DisplayAlert("Successful Request", "Your new contact needs to accept the request before you can start chatting!", "OK");

      this.ReceiverAdress.Completed += (object sender, EventArgs e) =>
        {
          vm.SubmitCommand.Execute(null);
        };
      this.BindingContext = vm;
    }

    private void AddressTapHandle(object sender, EventArgs e)
    {
      (this.BindingContext as AddContactViewModel)?.AddAdressToClipboard();
      this.DisplayAlert("Copied", "The address has been copied to your clipboard.", "OK");
    }
  }
}