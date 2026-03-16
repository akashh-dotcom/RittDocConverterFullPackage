#region

using R2V2.Core.Resource.Discipline;

#endregion

namespace R2V2.Web.Models
{
    public static class SpecialtyExtensions
    {
        public static SpecialtyDetail ToSpecialtyDetail(this Specialty specialty)
        {
            var specialtyDetail = new SpecialtyDetail();

            if (specialty != null)
            {
                specialtyDetail.Id = specialty.Id;
                specialtyDetail.Name = specialty.Name;
            }

            return specialtyDetail;
        }

        public static SpecialtyDetail ToSpecialtyDetail(this ISpecialty specialty)
        {
            var specialtyDetail = new SpecialtyDetail();

            if (specialty != null)
            {
                specialtyDetail.Id = specialty.Id;
                specialtyDetail.Name = specialty.Name;
            }

            return specialtyDetail;
        }
    }
}