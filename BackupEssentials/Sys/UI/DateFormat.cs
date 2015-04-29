using System;
using System.Globalization;

namespace BackupEssentials.Sys.UI{
    public class DateFormat{
        private static readonly DateTime ExampleDate = new DateTime(2015,8,15,22,1,0);
        
        private readonly DateTimeFormatInfo FormatProvider;
        private readonly string _format;
        private readonly string _example;

        public string Format { get { return _format; } }
        public string Display { get { return _example; } }

        public DateFormat(CultureInfo culture){
            FormatProvider = culture.DateTimeFormat;
            this._format = FormatProvider.ShortDatePattern+" "+FormatProvider.ShortTimePattern;
            this._example = ParseDate(ExampleDate);
        }

        public DateFormat(string format){
            string[] formatSplit = format.Split(new char[]{ ' ' },2);
            if (formatSplit.Length != 2)throw new ArgumentException("Invalid date/time format: "+format);

            FormatProvider = new DateTimeFormatInfo();
            FormatProvider.ShortDatePattern = formatSplit[0];
            FormatProvider.ShortTimePattern = formatSplit[1];
            
            this._format = format;
            this._example = ParseDate(ExampleDate);
        }

        public string ParseDate(DateTime dt){
            return dt.ToString("d",FormatProvider)+" "+dt.ToString("t",FormatProvider);
        }

        public override bool Equals(object obj){
            DateFormat format = obj as DateFormat;
            return format != null && format.Format.Equals(Format);
        }

        public override int GetHashCode(){
            return Format.GetHashCode();
        }

        public class Detect : DateFormat{
            public new string Display { get { return Settings.Default.Language["Settings.Option.DateFormat.Detect",_example]; } }

            public Detect() : base(CultureInfo.CurrentCulture){}
        }
    }
}