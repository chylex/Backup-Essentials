using BackupEssentials.Backup;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace BackupEssentials.Pages{
    public partial class BackupLocations : Page{
        public readonly ObservableCollection<BackupLocation> BackupLocationsList = new ObservableCollection<BackupLocation>();

        private int DraggingItemIndex = -1;
        private object DraggingItem = null;

        public BackupLocations(){
            InitializeComponent();

            // TODO uncomment once done testing
            /*
            BackupLocationsList.Add(new BackupLocation(){ Name = "Test1", Directory = @"C:\Folder\" });
            BackupLocationsList.Add(new BackupLocation(){ Name = "Test2", Directory = @"C:\Folder\" });

            LocationsListView.ItemsSource = BackupLocationsList;*/
        }

        private void ListStartDragging(object sender, MouseButtonEventArgs e){
            if ((Keyboard.Modifiers & ~ModifierKeys.Control & ~ModifierKeys.Shift) != Keyboard.Modifiers)return;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(1);

            timer.Tick += (sender2, args2) => {
                DraggingItemIndex = LocationsListView.SelectedIndex;
                DraggingItem = LocationsListView.SelectedItem;
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

                double mouseY = e.GetPosition(null).Y, containerY = ((ListViewItem)LocationsListView.ItemContainerGenerator.ContainerFromItem(DraggingItem)).TranslatePoint(new Point(),null).Y;

                if (DraggingItemIndex > 0 && mouseY < containerY){
                    LocationsListView.Items.RemoveAt(DraggingItemIndex);
                    --DraggingItemIndex;
                }
                else if (DraggingItemIndex < LocationsListView.Items.Count-1 && mouseY > containerY+(double)Resources["LocationListItemHeight"]+7){ // TODO figure out where the 7 came from (margin? padding?)
                    LocationsListView.Items.RemoveAt(DraggingItemIndex);
                    ++DraggingItemIndex;
                }
                else return;

                LocationsListView.Items.Insert(DraggingItemIndex,DraggingItem);
                LocationsListView.SelectedIndex = DraggingItemIndex;
                DraggingItem = LocationsListView.Items[DraggingItemIndex];
            }
        }
    }
}
