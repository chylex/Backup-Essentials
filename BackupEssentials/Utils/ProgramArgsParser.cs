using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackupEssentials.Utils{
    class ProgramArgsParser{
        private string[] Args;

        public ProgramArgsParser(string[] args){
            this.Args = args;
        }

        /// <summary>
        /// Returns true if the arguments contain a flag (-&lt;flag&gt;). The dash in front is handled automatically.
        /// </summary>
        public bool HasFlag(string flag){
            return Args.Contains("-"+flag);
        }

        /// <summary>
        /// Returns the next argument after a flag (-&lt;flag&gt;), as long as it is not another flag. The dash in front is handled automatically.
        /// </summary>
        public string GetValue(string flag, string defaultValue){
            flag = "-"+flag;

            for(int a = 0; a < Args.Length-1; a++){
                if (Args[a].Equals(flag)){
                    return Args[a+1].StartsWith("-") ? defaultValue : Args[a+1];
                }
            }

            return defaultValue;
        }
    }
}
