#region

using System;

#endregion

namespace R2V2.Web.Models.Resource
{
    [Serializable]
    public class DictionaryTerms
    {
        public bool Enable { get; set; }
        public bool ShowAll { get; set; }

        public string ActionMenuText()
        {
            return (ShowAll ? "Hide " : "Show ") + "Dictionary Terms";
        }
    }
}