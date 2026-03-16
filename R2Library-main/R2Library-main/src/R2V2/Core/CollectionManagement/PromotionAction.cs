namespace R2V2.Core.CollectionManagement
{
    public enum PromotionAction
    {
        PromotionApplied = 1,
        PromotionDeleted = 2,
        PromotionError = 0,
        PromotionNotFound = -1,
        PromotionExpired = -2,
        PromotionNotActive = -3,
        PromotionPreviouslyApplied = -4,
        PromotionLowerThenCurrent = -5
    }
}