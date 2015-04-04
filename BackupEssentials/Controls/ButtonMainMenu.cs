using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BackupEssentials.Controls{
    public class ButtonMainMenu:Button{
        public static DependencyProperty ClickPageProperty = DependencyProperty.Register("ClickPage",typeof(string),typeof(ButtonMainMenu));

        public string ClickPage {
            get { return (string)base.GetValue(ClickPageProperty); }
            set { base.SetValue(ClickPageProperty,(string)value); }
        }

        private bool IsCheckedVar;

        public bool IsChecked {
            get { return IsCheckedVar; }
            set { IsCheckedVar = value; VisualStateManager.GoToState(this,IsCheckedVar ? "Pressed" : "Normal",true); }
        }

        public ButtonMainMenu(){
            SizeChanged += (sender, args) => {
                if (IsChecked)VisualStateManager.GoToState(this,"Pressed",false);
            };
        }

        public override void OnApplyTemplate(){
            base.OnApplyTemplate();

            foreach(VisualStateGroup group in VisualStateManager.GetVisualStateGroups((FrameworkElement)Template.FindName("MainMenuButtonGrid",this))){
                group.CurrentStateChanging += (sender, args) => {
                    if (IsChecked && !args.NewState.Name.Equals("Pressed")){
                        DispatcherTimer timer = new DispatcherTimer();
                        timer.Interval = new TimeSpan(1);

                        timer.Tick += (sender2, args2) => {
                            if (IsChecked)VisualStateManager.GoToState(this,"Pressed",false);
                        };

                        timer.Start();
                    }
                };
            }
        }
    }
}
