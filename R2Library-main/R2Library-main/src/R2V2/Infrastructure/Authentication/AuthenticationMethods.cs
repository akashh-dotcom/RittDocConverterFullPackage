namespace R2V2.Infrastructure.Authentication
{
    public enum AuthenticationMethods
    {
        Undefined,
        UsernameAndPassword,
        IP,
        Referrer,
        Trusted,
        AthensUser,
        AthensInstitution,
        PassiveReauth
    }
}