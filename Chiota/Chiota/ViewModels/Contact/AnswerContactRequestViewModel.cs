﻿#region References

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Chiota.Exceptions;
using Chiota.Extensions;
using Chiota.Helper;
using Chiota.Resources.Localizations;
using Chiota.Services.BackgroundServices;
using Chiota.Services.BackgroundServices.Base;
using Chiota.Services.UserServices;
using Chiota.ViewModels.Base;
using Pact.Palantir.Usecase;
using Xamarin.Forms;

#endregion

namespace Chiota.ViewModels.Contact
{
    public class AnswerContactRequestViewModel : BaseViewModel
    {
        #region Attributes

        private string _username;
        private ImageSource _imageSource;

        private Pact.Palantir.Entity.Contact _contact;

        #endregion

        #region Properties

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public ImageSource ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged(nameof(ImageSource));
            }
        }

        #endregion

        #region Init

        public override void Init(object data = null)
        {
            base.Init(data);

            _contact = data as Pact.Palantir.Entity.Contact;
            if (_contact == null)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await new UnknownException(new ExcInfo()).ShowAlertAsync();
                    await PopAsync();
                });
                return;
            }

            Username = _contact.Name;

            if (_contact.ImagePath != null)
                ImageSource = ImageSource.FromUri(new Uri(ChiotaConstants.IpfsHashGateway + _contact.ImagePath));
            else
                ImageSource = ImageSource.FromFile("account.png");
        }

        #endregion

        #region Methods

        #region AcceptContactRequest

        public void AcceptContactRequest(ResponseCode responseCode)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await PopPopupAsync();

                if (responseCode == ResponseCode.Success)
                {
                    //Update the contact in the database.
                    var contact = Database.Contact.GetContactByPublicKeyAddress(_contact.PublicKeyAddress);
                    contact.Accepted = true;
                    Database.Contact.UpdateObject(contact);

                    await DisplayAlertAsync(AppResources.DlgSuccessfullAction, AppResources.DlgContactAddedDesc);
                }
                else
                    await DisplayAlertAsync(AppResources.DlgError, AppResources.DlgErrorDesc0 + $" {(int)responseCode}" + AppResources.DlgErrorDesc1);

                await PopAsync();
            });
        }

        #endregion

        #region DeclineContactRequest

        public void DeclineContactRequest(ResponseCode responseCode)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await PopPopupAsync();

                if (responseCode == ResponseCode.Success)
                {
                    //Update the contact in the database.
                    var contact = Database.Contact.GetContactByPublicKeyAddress(_contact.PublicKeyAddress);
                    Database.Contact.DeleteObject(contact.Id);

                    await DisplayAlertAsync(AppResources.DlgSuccessfullAction, AppResources.DlgContactDeclinedDesc);
                }
                else
                    await DisplayAlertAsync(AppResources.DlgError, AppResources.DlgErrorDesc0 + $" {(int)responseCode}" + AppResources.DlgErrorDesc1);

                await PopAsync();
            });
        }

        #endregion

        #endregion

        #region Commands

        #region Accept

        public ICommand AcceptCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await PushLoadingSpinnerAsync(AppResources.DlgAcceptContact);

                    MessagingCenter.Subscribe<AnswerContactRequestBackgroundJob, ResponseCode>(this, "AnswerContactRequest", (sender, args) => {

                        //First unsubscribe from the message center.
                        MessagingCenter.Unsubscribe<AnswerContactRequestBackgroundJob, ResponseCode>(this, "AnswerContactRequest");

                        AcceptContactRequest(args);
                    });

                    DependencyService.Get<IBackgroundJobWorker>().Run<AnswerContactRequestBackgroundJob>(UserService.CurrentUser, _contact, true);
                });
            }
        }

        #endregion

        #region Decline

        public ICommand DeclineCommand
        {
            get
            {
                return new Command(async () =>
                {
                    await PushLoadingSpinnerAsync(AppResources.DlgDeclineContact);

                    MessagingCenter.Subscribe<AnswerContactRequestBackgroundJob, ResponseCode>(this, "AnswerContactRequest", (sender, args) => {

                        //First unsubscribe from the message center.
                        MessagingCenter.Unsubscribe<AnswerContactRequestBackgroundJob, ResponseCode>(this, "AnswerContactRequest");

                        DeclineContactRequest(args);
                    });

                    DependencyService.Get<IBackgroundJobWorker>().Run<AnswerContactRequestBackgroundJob>(UserService.CurrentUser, _contact, false);
                });
            }
        }

        #endregion

        #endregion
    }
}