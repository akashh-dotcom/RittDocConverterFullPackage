namespace R2V2.Core.Email
{
    public abstract class EmailTemplates
    {
        protected const string MainHeaderFooterTemplate = "Main_Header_Footer.html";

        // SJS - TRY TO KEEP THESE IN ALPHABETICAL ORDER //
        protected const string ArchivedBodyTemplate = "ArchivedResource_Body.html";
        protected const string ArchivedItemTemplate = "ArchivedResource_Item.html";

        protected const string ApplicationUsageBodyTemplate = "ApplicationUsage_Body.html";

        protected const string AnnualFeeBodyTemplate = "AnnualFee_Body.html";
        protected const string AnnualFeeItemTemplate = "AnnualFee_Item.html";

        protected const string DctUpdateBodyTemplate = "NewResource_Body.html";
        protected const string DctUpdateItemTemplate = "EditionResource_Item.html";

        protected const string FacultyRequestBodyTemplate = "FacultyRequest_Body.html";

        protected const string ForthcomingBodyTemplate = "PurchasedResource_Body.html";
        protected const string ForthcomingItemTemplate = "NewResource_Item.html";

        protected const string NewEditionBodyTemplate = "EditionResource_Body.html";
        protected const string NewEditionPdaBodyTemplate = "PdaEditionResource_Body.html";
        protected const string NewEditionItemTemplate = "EditionResource_Item.html";

        protected const string NewResourceBodyTemplate = "NewResource_Body.html";
        protected const string NewResourceItemTemplate = "NewResource_Item.html";

        protected const string OngoingPdaAddedBodyTemplate = "OngoingPdaAdded_Body.html";
        protected const string OngoingPdaAddedItemTemplate = "OngoingPdaAdded_Item.html";
        protected const string OngoingPdaAddedRuleTemplate = "OngoingPdaAdded_Rule.html";

        protected const string OrderSummaryBodyTemplate = "OrderSummary_Body.html";
        protected const string OrderSummaryItemTemplate = "OrderSummary_Item.html";

        protected const string PdaAddedToCartBodyTemplate = "PdaAddedToCart_Body.html";
        protected const string PdaAddedToCartItemTemplate = "PdaResource_Item.html";

        protected const string PdaHistoryBodyTemplate = "PdaHistory_Body.html";

        protected const string PdaRemovedFromCartBodyTemplate = "PdaRemovedFromCart_Body.html";
        protected const string PdaRemovedFromCartItemTemplate = "PdaResource_Item.html";

        protected const string RecommendationBodyTemplate = "NewResource_Body.html";
        protected const string RecommendationItemTemplate = "RecommendedResource_Item.html";

        protected const string ResourceUsageBodyTemplate = "ResourceUsage_Body.html";

        protected const string ResourceQaEmailBodyTemplate = "QaApproval_BodyAndItem.html";

        protected const string SearchResultsBodyTemplate = "Search_Body.html";
        protected const string SearchResultsItemTemplate = "Search_Item.html";

        protected const string ShoppingCartBodyTemplate = "CartResource_Body.html";
        protected const string ShoppingCartResourceItemTemplate = "CartResource_Item.html";
        protected const string ShoppingCartProductItemTemplate = "CartProduct_Item.html";

        protected const string TrialFirstNoticeBodyTemplate = "TrialNotice_9Day_Body.html";
        protected const string TrialSecondNoticeBodyTemplate = "TrialNotice_3Day_Body.html";
        protected const string TrialFinalNoticeBodyTemplate = "TrialNotice_Final_Body.html";
        protected const string TrialExtensionNoticeBodyTemplate = "TrialNotice_Extension_Body.html";

        protected const string TrialEndedPdaCreatedTemplate = "TrialEndedPdaCreated.html";

        protected const string TurnawayBodyTemplate = "TurnawayResource_Body.html";
        protected const string TurnawayItemTemplate = "TurnawayResource_Item.html";

        protected const string UtilityReportBodyTemplate = "UtilityReport_Body.html";
        protected const string UtilityReportItemTemplate = "UtilityReport_Item.html";
        protected const string UtilityReportStepTemplate = "UtilityReport_Item_Step.html";

        protected const string WebActivityBodyTemplate = "WebActivityReport_Body.html";
        protected const string WebActivityItemTemplate = "WebActivityReport_Item.html";
        protected const string WebActivitySubItemTemplate = "WebActivityReport_Item_Result.html";

        protected const string AutomatedCartInstitutionBodyTemplate = "AutomatedCart_Body.html";

        protected const string AccountRequestBodyTemplate = "AccountRequest_Body.html";
        protected const string AccountRequestItemTemplate = "AccountRequest_Item.html";

        protected const string AutomatedCartSummaryBodyTemplate = "AutomatedCartSummary_Body.html";
        protected const string AutomatedCartSummaryInstitutionTemplate = "AutomatedCartSummary_Institution.html";
        protected const string AutomatedCartSummaryItemTemplate = "AutomatedCartSummary_Item.html";

        protected const string RabbitMqReportBodyTemplate = "RabbitMqReport_Body.html";
        protected const string RabbitMqReportHostTemplate = "RabbitMqReport_Host.html";
        protected const string RabbitMqReportHostQueueTemplate = "RabbitMqReport_Host_Queue.html";

        protected const string LogEventsReportBodyTemplate = "LogEventsReport_Body.html";
        protected const string LogEventsReportBodyItemTemplate = "LogEventsReport_Body_Item.html";
        protected const string LogEventsReportItemTemplate = "LogEventsReport_Item.html";
        protected const string LogEventsReportStepTemplate = "LogEventsReport_Item_Step.html";

        protected const string FindEBookBodyTemplate = "FindEBook_Body.html";
        protected const string FindEBookItemTemplate = "FindEBook_Publisher.html";
        protected const string FindEBookSubItemTemplate = "FindEBook_Publisher_Item.html";
    }
}