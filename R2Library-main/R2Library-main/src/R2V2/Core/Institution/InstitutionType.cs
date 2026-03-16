#region

using System;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Institution
{
    [Serializable]
    public class InstitutionType : AuditableEntity, ISoftDeletable, IInstitutionType
    {
        public virtual string Code { get; set; }
        public virtual string Name { get; set; }
        public virtual bool RecordStatus { get; set; }

        public virtual string ToDebugString()
        {
            return new StringBuilder()
                .Append("InstitutionType = [")
                .Append($"Id: {Id}")
                .Append($", Code: {Code}")
                .Append($", Name: {Name}")
                .Append($", CreationDate: {CreationDate}")
                .Append($", CreatedBy: {CreatedBy}")
                .Append($", RecordStatus: {RecordStatus}")
                .ToString();
        }
    }

    public interface IInstitutionType
    {
        bool RecordStatus { get; set; }
        string Code { get; set; }
        string Name { get; set; }
        int Id { get; set; }
    }

    [Serializable]
    public class CachedInstitutionType : IInstitutionType
    {
        public CachedInstitutionType(IInstitutionType institutionType)
        {
            Id = institutionType.Id;
            Name = institutionType.Name;
            Code = institutionType.Code;
            RecordStatus = institutionType.RecordStatus;
        }

        public bool RecordStatus { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
    }
}