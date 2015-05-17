using System.Collections.Generic;
using System.Diagnostics;

namespace BackupEssentials.Utils{
    class Profiler{
        private static readonly Dictionary<string,Stopwatch> Running = new Dictionary<string,Stopwatch>();

        public static void Start(string identifier){
            (Running[identifier] = new Stopwatch()).Start();
        }

        public static void End(string identifier){
            Stopwatch sw;

            if (Running.TryGetValue(identifier,out sw)){
                sw.Stop();
                App.LogInfo("Profiler "+identifier+" finished in "+sw.ElapsedMilliseconds+" ms.");
            }
        }
    }
}
