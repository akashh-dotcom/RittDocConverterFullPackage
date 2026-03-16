#region

using System;

#endregion

namespace R2V2.Web.Infrastructure.Storages
{
    public class ApplicationStorageItem
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime InsertDate { get; set; }
    }
}