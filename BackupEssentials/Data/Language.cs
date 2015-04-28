using BackupEssentials.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace BackupEssentials.Data{
    public class Language{
        private readonly Dictionary<string,string> Data;
        
        public string File { get; private set; }
        public string LangName { get; private set; }

        public string this[string key] {
            get {
                if (Data.Count == 0)LoadLanguage();

                string res;
                return Data.TryGetValue(key,out res) ? res : key;
            }
        }

        public string this[string key, params object[] data] {
            get {
                string text = this[key];
                for(int a = data.Length-1; a >= 0; a--)text = text.Replace("$"+a,data[a].ToString());
                return text;
            }
        }

        public Language(string file, string langName){
            this.File = file;
            this.LangName = langName;
            this.Data = new Dictionary<string,string>();
        }

        private void LoadLanguage(){
            FileUtils.ReadFile(Path.Combine("Resources/Lang",File),FileMode.Open,(line) => {
                if (line.Length == 0 || line[0] == '#')return;

                string[] data = line.Split(new string[]{ " = " },2,StringSplitOptions.None);
                if (data.Length == 2)Data.Add(data[0],data[1]);
            });
        }
    }
}
