using BackupEssentials.Backup;
using System.Windows.Controls;

namespace BackupEssentials.Pages{
    public partial class BackupDrop : Page, IPageShowData{
        public BackupDrop(){
            InitializeComponent();

            LocationsListView.Items.Clear();
            LocationsListView.ItemsSource = DataStorage.BackupLocationList;
        }

        void IPageShowData.OnShow(object data){
            string[] files = (string[])data;
        }

        private void ListViewSelectionChanged(object sender, SelectionChangedEventArgs e){
            ButtonBackup.IsEnabled = LocationsListView.SelectedItems.Count == 1;
        }
    }
}
