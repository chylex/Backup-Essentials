using BackupEssentials.Backup;
using System.Diagnostics;
using System.Windows.Controls;

namespace BackupEssentials.Pages{
    public partial class BackupLocationsEdit : Page, IPageShowData{
        public BackupLocation EditLocation;

        public BackupLocationsEdit(){
            InitializeComponent();
        }

        public void OnShow(object data){
            EditLocation = (BackupLocation)data;
            TextBoxName.DataContext = EditLocation;
            TextBoxDirectory.DataContext = EditLocation;
        }
    }
}
