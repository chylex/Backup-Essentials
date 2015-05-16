using System.Text;

namespace BackupEssentials.Utils.IO{
    static class WindowsFileUtils{
        private static readonly string InvalidChars = @"<>:/\|?*"+'"';

        public static string ReplaceInvalidFileCharacters(string filename, char replacement){
            StringBuilder build = new StringBuilder(filename);

            for(int a = 0; a < build.Length; a++){
                if (build[a] < 32 || InvalidChars.IndexOf(build[a]) != -1)build[a] = replacement;
            }

            return build.ToString();
        }
    }
}
