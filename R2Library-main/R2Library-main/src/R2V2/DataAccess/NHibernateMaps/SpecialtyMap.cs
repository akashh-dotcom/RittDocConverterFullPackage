#region

using R2V2.Core.Resource.Discipline;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class SpecialtyMap : BaseMap<Specialty>
    {
        public SpecialtyMap()
        {
            Table("tSpecialty");

            Id(x => x.Id).Column("iSpecialtyId").GeneratedBy.Identity();
            Map(x => x.Name).Column("vchSpecialtyName");
            Map(x => x.Code).Column("vchSpecialtyCode");
            Map(x => x.SequenceNumber).Column("iSequenceNumber");

            //HasMany(x => x.ResourceSpecialties).KeyColumn("iSpecialtyId");
        }
    }
}