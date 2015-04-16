using BackupEssentials.Backup.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BackupEssentials.Backup{
    public class BackupReport{
        private readonly string _plain;
        private string _parsed;

        public string UnparsedReport { get { return _plain; } }

        public string Report {
            get {
                if (_parsed == null){
                    StringBuilder build = new StringBuilder(16+(int)Math.Floor(_plain.Length*1.5D));

                    foreach(string line in SplitByLine(_plain)){
                        if (line.Length == 0)continue;

                        if (line[0] == 'A' && line.Length > 3)build.Append(GetFullNameAction(line[1])).Append(' ').Append(GetFullNameType(line[2])).Append(": ").Append(line.Substring(3)).Append(Environment.NewLine);
                        else if (line[0] == 'I')build.Append(line.Substring(1)).Append(Environment.NewLine);
                        else if (line[0] == 'V'){
                            string[] split = line.Substring(1).Split(new char[]{ '=' },2);
                            if (split.Length == 2)build.Append(split[0]).Append(": ").Append(split[1]).Append(Environment.NewLine);
                        }
                    }

                    _parsed = build.ToString();
                }

                return _parsed;
            }
        }

        private BackupReport(string report){
            this._plain = report;
        }

        public string TryFindValue(string key, string defaultValue){
            key = key+'=';

            foreach(string line in SplitByLine(_plain)){
                if (line.Length > 0 && line[0] == 'V' && line.Substring(1).StartsWith(key))return line.Substring(key.Length+1);
            }

            return defaultValue;
        }

        public int TryFindValue(string key, int defaultValue){
            key = key+'=';

            foreach(string line in SplitByLine(_plain)){
                if (line.Length > 0 && line[0] == 'V' && line.Substring(1).StartsWith(key)){
                    int value;
                    return int.TryParse(line.Substring(key.Length+1),out value) ? value : defaultValue;
                }
            }

            return defaultValue;
        }

        public override string ToString(){
            return Report;
        }

        public class Builder{
            private StringBuilder Build = new StringBuilder(256);

            public void Add(IOAction action, IOType type, string path){
                Build.Append('A').Append(GetShortName(action)).Append(GetShortName(type)).Append(path).Append(Environment.NewLine);
            }

            public void Add(string message){
                Build.Append('I').Append(message).Append(Environment.NewLine);
            }

            public void Add(string key, string value){
                Build.Append('V').Append(key).Append('=').Append(value).Append(Environment.NewLine);
            }

            public BackupReport Finish(){
                return new BackupReport(Build.ToString());
            }
        }

        private static char GetShortName(IOAction action){
            return action == IOAction.Create ? 'C' : action == IOAction.Delete ? 'D' : action == IOAction.Replace ? 'R' : '?';
        }

        private static string GetFullNameAction(char key){
            return key == 'C' ? "Added" : key == 'D' ? "Deleted" : key == 'R' ? "Updated" : "(unknown action)";
        }

        private static char GetShortName(IOType type){
            return type == IOType.File ? 'F' : type == IOType.Directory ? 'D' : '?';
        }

        private static string GetFullNameType(char key){
            return key == 'F' ? "File" : key == 'D' ? "Folder (and files inside)" : "(unknown type)";
        }

        private static IEnumerable<string> SplitByLine(string str){
            using(StringReader reader = new StringReader(str)){
                string line;

                while((line = reader.ReadLine()) != null){
                    yield return line;
                }
            }
        }
    }
}
