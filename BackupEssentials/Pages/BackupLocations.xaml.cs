using BackupEssentials.Backup;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Controls;

namespace BackupEssentials.Pages{
    public partial class BackupLocations : Page{
        public readonly ObservableCollection<BackupLocation> BackupLocationsList = new ObservableCollection<BackupLocation>();

        public BackupLocations(){
            InitializeComponent();

            // TODO uncomment once done testing
            /*
            BackupLocationsList.Add(new BackupLocation(){ Name = "Test1", Directory = @"C:\Folder\" });
            BackupLocationsList.Add(new BackupLocation(){ Name = "Test2", Directory = @"C:\Folder\" });

            LocationsListView.ItemsSource = BackupLocationsList;*/
        }
    }
}
