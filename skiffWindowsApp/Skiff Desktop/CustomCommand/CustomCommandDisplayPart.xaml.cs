using ToastNotifications.Core;

namespace CustomNotificationsExample.CustomCommand
{
    /// <summary>
    /// Interaction logic for CustomCommandDisplayPart.xaml
    /// </summary>
    public partial class CustomCommandDisplayPart : NotificationDisplayPart
    {
        public CustomCommandDisplayPart(CustomCommandNotification notification)
        {
            InitializeComponent();
            Bind(notification);
        }
    }
}
