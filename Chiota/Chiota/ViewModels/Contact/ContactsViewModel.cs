﻿using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Chiota.Exceptions;
using Chiota.Extensions;
using Chiota.Messenger.Usecase;
using Chiota.Messenger.Usecase.GetContacts;
using Chiota.Models;
using Chiota.Popups.PopupModels;
using Chiota.Popups.PopupPageModels;
using Chiota.Popups.PopupPages;
using Chiota.Services.DependencyInjection;
using Chiota.Services.UserServices;
using Chiota.ViewModels.Classes;
using Chiota.Views.Contact;
using Tangle.Net.Entity;
using Xamarin.Forms;

namespace Chiota.ViewModels.Contact
{
    public class ContactsViewModel : BaseViewModel
    {
        #region Attributes

        private List<ContactBinding> _contactList;

        #endregion

        #region Properties

        public List<ContactBinding> ContactList
        {
            get => _contactList;
            set
            {
                _contactList = value;
                OnPropertyChanged(nameof(ContactList));
            }
        }

        #endregion

        #region Constructors

        public ContactsViewModel()
        {
            _contactList = new List<ContactBinding>();
        }

        #endregion

        #region Init

        protected override void ViewIsAppearing()
        {
            UpdateView();

            base.ViewIsAppearing();
        }

        #endregion

        #region Methods

        #region UpdateView

        private async void UpdateView()
        {
            ContactList = await GetContactListAsync();
        }

        #endregion

        #region GetContactList

        private async Task<List<ContactBinding>> GetContactListAsync()
        {
            var tmp = new List<ContactBinding>();

            var interactor = DependencyResolver.Resolve<IUsecaseInteractor<GetContactsRequest, GetContactsResponse>>();
            var response = await interactor.ExecuteAsync(new GetContactsRequest()
            {
                ContactRequestAddress = new Address(UserService.CurrentUser.RequestAddress),
                PublicKeyAddress = new Address(UserService.CurrentUser.PublicKeyAddress)
            });

            foreach (var pending in response.PendingContactRequests)
                tmp.Add(new ContactBinding(pending, false, TapContactRequestCommand));

            foreach (var approved in response.ApprovedContacts)
                tmp.Add(new ContactBinding(approved, true, TapContactCommand));

            return tmp;
        }

        #endregion

        #endregion

        #region Commands

        #region Refresh

        public ICommand RefreshCommand
        {
            get
            {
                return new Command(() =>
                {
                    UpdateView();
                });
            }
        }

        #endregion

        #region TapContactRequest

        public ICommand TapContactRequestCommand
        {
            get
            {
                return new Command(async(param) =>
                {
                    if (param is ContactBinding contact)
                        await PushAsync<ContactRequestView>(contact.Contact);
                    else
                        await new UnknownException(new ExcInfo()).ShowAlertAsync();
                });
            }
        }

        #endregion

        #region TapContact

        public ICommand TapContactCommand
        {
            get
            {
                return new Command(() =>
                {

                });
            }
        }

        #endregion

        #endregion
    }
}
