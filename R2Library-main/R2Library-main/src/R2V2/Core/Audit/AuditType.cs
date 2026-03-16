namespace R2V2.Core.Audit
{
    public enum InstitutionAuditType
    {
        EulaSigned = 1,
        PdaEulaSigned = 2,
        IpAddressInsert = 3,
        IpAddressUpdate = 4,
        IpAddressDeleted = 5,
        InstitutionUpdate = 6
    }

    public enum ResourceAuditType
    {
        Unspecificed = 1
    }
}