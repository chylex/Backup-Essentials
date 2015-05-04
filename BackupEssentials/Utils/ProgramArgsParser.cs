using System;
using System.Collections.Generic;
using System.Linq;

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
                    return Args[a+1].StartsWith("-",StringComparison.Ordinal) ? defaultValue : Args[a+1];
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Returns all arguments after a flag (-&lt;flag&gt;) until it hits another flag. The dash in front is handled automatically.
        /// </summary>
        public string[] GetMultiValue(string flag){
            flag = "-"+flag;
            List<string> list = new List<string>();

            for(int a = 0; a < Args.Length-1; a++){
                if (Args[a].Equals(flag)){
                    for(int index = a+1; index < Args.Length; index++){
                        if (Args[index].StartsWith("-",StringComparison.Ordinal))break;
                        else list.Add(Args[index]);
                    }

                    break;
                }
            }

            return list.ToArray();
        }

        public override string ToString(){
            return string.Join(" ",Args);
        }
    }
}
