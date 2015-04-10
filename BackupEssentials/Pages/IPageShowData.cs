namespace BackupEssentials.Pages{
    interface IPageShowData{
        /// <summary>
        /// Use this to not call OnShow(data) when changing the page. Only use when showing 'overlay' windows that do not modify any data.
        /// </summary>
        public static readonly object IgnoreShowData = new object();

        void OnShow(object data);
    }
}
