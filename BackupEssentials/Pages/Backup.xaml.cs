using BackupEssentials.Backup;
using BackupEssentials.Backup.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace BackupEssentials.Pages{
    public partial class Backup : Page, IPageShowData{
        private int DraggingItemIndex = -1;
        private BackupLocation DraggingItem = null;

        public Backup(){
            InitializeComponent();

            LocationsListView.Items.Clear();
            LocationsListView.ItemsSource = DataStorage.BackupLocationList;

            ExplorerIntegration.InitializeRefreshTimer();
        }

        void IPageShowData.OnShow(object data){
            LocationsListView.Items.Refresh();

            int count = DataStorage.BackupLocationList.Count;
            if (count > 0 && DataStorage.BackupLocationList[count-1].Name.Length == 0)DataStorage.BackupLocationList.RemoveAt(count-1);
        }

        private void ListStartDragging(object sender, MouseButtonEventArgs e){
            if ((Keyboard.Modifiers & ~ModifierKeys.Control & ~ModifierKeys.Shift) != Keyboard.Modifiers)return;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(1);

            timer.Tick += (sender2, args2) => {
                DraggingItemIndex = LocationsListView.SelectedIndex;
                DraggingItem = (BackupLocation)LocationsListView.SelectedItem;
                timer.Stop();
            };

            timer.Start();
        }

        private void ListStopDragging(object sender, MouseButtonEventArgs e){
            DraggingItemIndex = -1;
            DraggingItem = null;
        }

        private void ListMouseMove(object sender, MouseEventArgs e){
            if (DraggingItemIndex != -1 && DraggingItem != null){
                if (e.LeftButton == MouseButtonState.Released){
                    DraggingItemIndex = -1;
                    DraggingItem = null;
                    return;
                }

                ListViewItem container = ((ListViewItem)LocationsListView.ItemContainerGenerator.ContainerFromItem(DraggingItem));
                double mouseY = e.GetPosition(null).Y, containerY = container.TranslatePoint(new Point(),null).Y;

                if (DraggingItemIndex > 0 && mouseY < containerY){
                    DataStorage.BackupLocationList.RemoveAt(DraggingItemIndex);
                    --DraggingItemIndex;
                }
                else if (DraggingItemIndex < LocationsListView.Items.Count-1 && mouseY > containerY+(double)Resources["LocationListItemHeight"]+container.Padding.Top+container.Padding.Bottom-1){
                    DataStorage.BackupLocationList.RemoveAt(DraggingItemIndex);
                    ++DraggingItemIndex;
                }
                else return;

                DataStorage.BackupLocationList.Insert(DraggingItemIndex,DraggingItem);
                LocationsListView.SelectedIndex = DraggingItemIndex;
                DraggingItem = (BackupLocation)LocationsListView.Items[DraggingItemIndex];
                ExplorerIntegration.Refresh();
            }
        }

        private void LocationAdd(object sender, RoutedEventArgs e){
            DataStorage.BackupLocationList.Add(new BackupLocation());
            LocationsListView.SelectedIndex = DataStorage.BackupLocationList.Count-1;
            MainWindow.Instance.ShowPage(typeof(BackupEdit),DataStorage.BackupLocationList[LocationsListView.SelectedIndex]);
        }

        private void LocationEdit(object sender, RoutedEventArgs e){
            if (LocationsListView.SelectedIndex != -1)MainWindow.Instance.ShowPage(typeof(BackupEdit),DataStorage.BackupLocationList[LocationsListView.SelectedIndex]);
        }

        private void LocationRemove(object sender, RoutedEventArgs e){
            int index = LocationsListView.SelectedIndex;
            List<object> list = new List<object>();
            foreach(object obj in LocationsListView.SelectedItems)list.Add(obj); // MS doesn't need generics apparently...
            foreach(object item in list)DataStorage.BackupLocationList.Remove((BackupLocation)item);

            if (index > 0)LocationsListView.SelectedIndex = index-1;
            else if (LocationsListView.Items.Count > 0)LocationsListView.SelectedIndex = index;

            if (DataStorage.BackupLocationList.Count(loc => loc.ShouldRegister()) == 0)ExplorerIntegration.Remove();
            else ExplorerIntegration.Refresh();
        }

        private void ListViewSelectionChanged(object sender, SelectionChangedEventArgs e){
            ButtonLocationEdit.IsEnabled = LocationsListView.SelectedItems.Count == 1;
            ButtonLocationRemove.IsEnabled = LocationsListView.SelectedItems.Count > 0;
        }
    }
}
