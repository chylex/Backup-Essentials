using System;
using System.Windows;
using System.Windows.Controls;

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
            MouseEnter += ResetCheckedState;
            MouseLeave += ResetCheckedState;
            SizeChanged += ResetCheckedState;
        }

        private void ResetCheckedState(object sender, EventArgs args){
            if (IsChecked)VisualStateManager.GoToState(this,"Pressed",false);
        }
    }
}
