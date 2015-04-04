using BackupEssentials.Backup;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace BackupEssentials.Pages{
    public partial class BackupLocations : Page{
        public readonly ObservableCollection<BackupLocation> BackupLocationsList = new ObservableCollection<BackupLocation>();

        private int DraggingItemIndex = -1;
        private BackupLocation DraggingItem = null;

        public BackupLocations(){
            InitializeComponent();

            LocationsListView.Items.Clear();
            LocationsListView.ItemsSource = null;

            BackupLocationsList.Add(new BackupLocation(){ Name = "Test1", Directory = @"C:\Folder\" });
            BackupLocationsList.Add(new BackupLocation(){ Name = "Test2", Directory = @"C:\Folder\" });

            LocationsListView.ItemsSource = BackupLocationsList;
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
                    BackupLocationsList.RemoveAt(DraggingItemIndex);
                    --DraggingItemIndex;
                }
                else if (DraggingItemIndex < LocationsListView.Items.Count-1 && mouseY > containerY+(double)Resources["LocationListItemHeight"]+container.Padding.Top+container.Padding.Bottom-1){
                    BackupLocationsList.RemoveAt(DraggingItemIndex);
                    ++DraggingItemIndex;
                }
                else return;

                BackupLocationsList.Insert(DraggingItemIndex,DraggingItem);
                LocationsListView.SelectedIndex = DraggingItemIndex;
                DraggingItem = (BackupLocation)LocationsListView.Items[DraggingItemIndex];
            }
        }

        private void LocationAdd(object sender, RoutedEventArgs e){
            BackupLocationsList.Add(new BackupLocation(){ Name = "Abc", Directory = "Abc" });
        }

        private void LocationEdit(object sender, RoutedEventArgs e){

        }

        private void LocationRemove(object sender, RoutedEventArgs e){
            List<object> list = new List<object>();
            foreach(object obj in LocationsListView.SelectedItems)list.Add(obj); // MS doesn't need generics apparently...
            foreach(object item in list)BackupLocationsList.Remove((BackupLocation)item);
        }
    }
}
