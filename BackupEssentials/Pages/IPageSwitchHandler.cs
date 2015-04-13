namespace BackupEssentials.Pages{
    interface IPageSwitchHandler{
        /// <summary>
        /// Called when the display page is about to change, or when the window is closing. Return true to stop the switch (or closing) from happening.
        /// </summary>
        bool OnSwitch();
    }
}
