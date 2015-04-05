using System;
using System.Windows.Threading;

namespace BackupEssentials.Utils{
    class ScheduledUpdate{
        public static ScheduledUpdate Forever(int seconds, Action update){
            return new ScheduledUpdate(seconds,update);
        }

        public bool NeedsUpdate = false;
        private readonly DispatcherTimer timer;

        private ScheduledUpdate(int seconds, Action update){
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0,0,seconds);

            timer.Tick += (sender, args) => {
                if (NeedsUpdate){
                    NeedsUpdate = false;
                    update();
                }
            };
        }

        public void Start(){
            timer.Start();
        }
    }
}
