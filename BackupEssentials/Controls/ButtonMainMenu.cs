using System;
using System.Windows;
using System.Windows.Controls;

namespace BackupEssentials.Controls{
    public class ButtonMainMenu:Button{
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
