﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Chiota.Messenger.Entity;
using Chiota.Models.Classes;
using Xamarin.Forms;

namespace Chiota.Models
{
    public class ContactBinding : BaseModel
    {
        #region Properties

        public Contact Contact { get; }

        public bool IsApproved { get; }

        public bool IsNotApproved { get; }

        public ImageSource ImageSource { get;}

        public Color BackgroundColor { get; }

        public ICommand TapCommand { get; }

        #endregion

        #region Constructors

        public ContactBinding(Contact contact, bool isApproved, ICommand command)
        {
            Contact = contact;
            IsApproved = isApproved;
            IsNotApproved = !isApproved;

            if (!IsApproved)
                BackgroundColor = Color.FromHex("#321565c0");

            if(string.IsNullOrEmpty(contact.ImageHash))
                ImageSource = ImageSource.FromFile("account.png");
            else
                ImageSource = ChiotaConstants.IpfsHashGateway + contact.ImageHash;

            TapCommand = command;
        }

        #endregion
    }
}
