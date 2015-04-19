using System;

namespace BackupEssentials.Utils{
    static class NumberSerialization{
        public static string WriteInt(int value){
            return Convert.ToBase64String(BitConverter.GetBytes(value));
        }

        public static string WriteLong(long value){
            return Convert.ToBase64String(BitConverter.GetBytes(value));
        }

        public static int ReadInt(string text){
            return BitConverter.ToInt32(Convert.FromBase64String(text),0);
        }

        public static long ReadLong(string text){
            return BitConverter.ToInt64(Convert.FromBase64String(text),0);
        }
    }
}
