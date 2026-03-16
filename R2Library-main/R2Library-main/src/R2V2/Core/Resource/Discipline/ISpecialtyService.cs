#region

using System.Collections.Generic;

#endregion

namespace R2V2.Core.Resource.Discipline
{
    public interface ISpecialtyService
    {
        //IEnumerable<Specialty> GetAllSpecialties();
        //IEnumerable<Specialty> GetSpecialties(int institutionId, Include include, string practiceArea, bool displayAllProducts);
        //Specialty GetSpecialty(int specialtyId);
        //Specialty GetSpecialty(string specialtyId);


        IEnumerable<ISpecialty> GetAllSpecialties();
        ISpecialty GetSpecialty(int specialtyId);
        Specialty GetSpecialtyForEdit(int specialtyId);

        //Specialty GetSpecialtyForEdit(string specialtyId);
        ISpecialty GetSpecialty(string specialtyId);
        ISpecialty GetSpecialtyByCode(string code);

        void ClearCache();
    }
}