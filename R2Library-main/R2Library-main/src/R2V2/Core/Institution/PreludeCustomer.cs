#region

using System;
using R2V2.Core.Authentication;

#endregion

namespace R2V2.Core.Institution
{
    [Serializable]
    public class PreludeCustomer
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string AccountNumber { get; set; }
        public virtual Address Address { get; set; }

        public virtual string AdministratorEmail { get; set; }

        public virtual string Territory { get; set; }
        public virtual string TypeName { get; set; }
    }
}