using ToastNotifications.Core;

namespace CustomNotificationsExample.CustomInput
{
    /// <summary>
    /// Interaction logic for CustomCommandDisplayPart.xaml
    /// </summary>
    public partial class CustomInputDisplayPart : NotificationDisplayPart
    {
        public CustomInputDisplayPart(CustomInputNotification notification)
        {
            InitializeComponent();
            Bind(notification);
        }
    }
}
