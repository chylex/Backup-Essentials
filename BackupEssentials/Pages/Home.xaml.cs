using BackupEssentials.Utils;
using System.Windows.Controls;

namespace BackupEssentials.Pages{
    public partial class Home : Page{
        public Home(){
            InitializeComponent();

            if (!WindowsVersion.IsFullySupported())TextBlockContextMenuWarning.Opacity = 1;
        }
    }
}
