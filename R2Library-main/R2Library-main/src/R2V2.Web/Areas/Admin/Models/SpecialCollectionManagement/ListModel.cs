#region

using System.Collections.Generic;

#endregion

namespace R2V2.Web.Areas.Admin.Models.SpecialCollectionManagement
{
    public class ListModel : AdminBaseModel
    {
        public List<SpecialCollectionList> SpecialCollectionLists { get; set; }

        public string SequenceString { get; set; }
        public bool IsEditMode { get; set; }
    }

    public class SpecialCollectionList
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int Sequence { get; set; }
        public int ResourceCount { get; set; }
        public bool IsPublic { get; set; }
    }
}