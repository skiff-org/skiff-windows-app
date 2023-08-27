using CustomNotificationsExample.Utilities;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ToastNotifications.Core;

namespace CustomNotificationsExample.CustomCommand
{
    public class CustomCommandNotification : NotificationBase, INotifyPropertyChanged
    {
        private CustomCommandDisplayPart _displayPart;

        private Action<CustomCommandNotification> _confirmAction;
        private Action<CustomCommandNotification> _declineAction;

        public ICommand ConfirmCommand { get; set; }
        public ICommand DeclineCommand { get; set; }

        public CustomCommandNotification(string message, 
            Action<CustomCommandNotification> confirmAction, 
            Action<CustomCommandNotification> declineAction, 
            MessageOptions messageOptions) 
            : base(message, messageOptions)
        {
            Message = message;
            _confirmAction = confirmAction;
            _declineAction = declineAction;

            ConfirmCommand = new RelayCommand(x => _confirmAction(this));
            DeclineCommand = new RelayCommand(x => _declineAction(this));
        }

        public override NotificationDisplayPart DisplayPart => _displayPart ?? (_displayPart = new CustomCommandDisplayPart(this));

        #region binding properties

        private string _message;

        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
