namespace R2Utilities.DataAccess
{
    public enum ResourceTitleUpdateType
    {
        Equal = 1,
        RittenhouseEqualR2TitleAndSub = 2,
        R2EqualRittenhouseTitleAndSub = 3,
        NotExist = 4,
        DifferentSub = 5,
        RittenhouseSubNull = 6,
        R2SubNull = 7,
        Other = 8
    }
}