#region

using System;
using System.Text;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Core.Territory;
using R2V2.Infrastructure.Authentication;
using R2V2.Web.Areas.Admin.Models.Institution;
using R2V2.Web.Areas.Admin.Models.IpAddressRange;
using R2V2.Web.Areas.Admin.Models.User;
using Address = R2V2.Core.Authentication.Address;

#endregion

namespace R2V2.Web.Areas.Admin.Models
{
    public static class InstitutionExtensions
    {
        public static string UpdateInstitutionForEdit(this InstitutionEditViewModel viewModel, IInstitution institution,
            IAuthenticationContext authenticationContext)
        {
            var auditLogData = new StringBuilder();
            if (authenticationContext.IsRittenhouseAdmin())
            {
                //All = 0,
                //Active = 1,
                //Trial = 2,
                //Disabled = 3,
                //TrialExpired = 4
                if (institution.AccountStatusId != (int)viewModel.AccountStatus)
                {
                    if (institution.AccountStatusId ==
                        1) //If the institution was Active is changing to any other status clear out the Annual Fee
                    {
                        institution.AnnualFee = null;
                    }
                    else if (viewModel.AccountStatus ==
                             AccountStatus.Active) //If they are changing to active state set annual fee
                    {
                        if (institution.AnnualFee == null)
                        {
                            institution.AnnualFee = new AnnualFee();
                        }

                        institution.AnnualFee.FeeDate = DateTime.Now;
                    }
                }

                if (institution.AccountStatusId == 1)
                {
                    if (viewModel.AnnualFeeDate.HasValue)
                    {
                        if (institution.AnnualFee == null)
                        {
                            institution.AnnualFee = new AnnualFee();
                        }

                        AppendFieldLevelAuditDetails(auditLogData,
                            viewModel.AnnualFeeDate.GetValueOrDefault(DateTime.Now).ToString(),
                            institution.AnnualFee.ToString(), "AnnualFeeDate");
                        institution.AnnualFee.FeeDate = viewModel.AnnualFeeDate.GetValueOrDefault(DateTime.Now);
                    }
                }


                AppendFieldLevelAuditDetails(auditLogData, (int)viewModel.AccountStatus, institution.AccountStatusId,
                    "AccountStatusId");
                institution.AccountStatusId = (int)viewModel.AccountStatus;

                if (viewModel.AccountStatus == AccountStatus.Trial)
                {
                    AppendFieldLevelAuditDetails(auditLogData, viewModel.TrialEndDate, institution.Trial.EndDate,
                        "Trial.EndDate");
                    if (viewModel.TrialEndDate.HasValue)
                    {
                        institution.Trial.EndDate = viewModel.TrialEndDate;
                    }
                }

                AppendFieldLevelAuditDetails(auditLogData, viewModel.Discount, institution.Discount, "Discount");
                institution.Discount = viewModel.Discount;

                AppendFieldLevelAuditDetails(auditLogData, viewModel.HouseAccount, institution.HouseAccount,
                    "HouseAccount");
                institution.HouseAccount = viewModel.HouseAccount;
            }

            if (authenticationContext.IsRittenhouseAdmin() || authenticationContext.IsSalesAssociate())
            {
                AppendFieldLevelAuditDetails(auditLogData, viewModel.TrustedKey, institution.TrustedKey, "TrustedKey");
                institution.TrustedKey = viewModel.TrustedKey;
                if (viewModel.InstitutionTerritory != null)
                {
                    var newTerritoryId = viewModel.InstitutionTerritory.TerritoryId;
                    var oldTerritoryId = institution.Territory != null ? institution.Territory.Id : 0;
                    if (newTerritoryId != oldTerritoryId)
                    {
                        if (institution.Territory == null)
                        {
                            institution.Territory = new Territory();
                        }

                        AppendFieldLevelAuditDetails(auditLogData, viewModel.InstitutionTerritory.TerritoryId,
                            institution.Territory.Id, "TerritoryId");
                        institution.Territory = new Territory { Id = newTerritoryId };
                    }
                }

                if (viewModel.Type != null)
                {
                    var newTypeId = viewModel.Type.Id;
                    var oldTypeId = institution.Type?.Id ?? 0;
                    if (newTypeId != oldTypeId)
                    {
                        if (institution.Type == null)
                        {
                            institution.Type = new InstitutionType();
                        }

                        AppendFieldLevelAuditDetails(auditLogData, viewModel.Type.Id, institution.Type.Id,
                            "InstitutionType");
                        institution.Type = newTypeId == 0 ? null : new InstitutionType { Id = newTypeId };
                    }
                }
            }

            if (authenticationContext.IsRittenhouseAdmin() || authenticationContext.IsSalesAssociate() ||
                authenticationContext.IsInstitutionAdmin())
            {
                AppendFieldLevelAuditDetails(auditLogData, viewModel.InstitutionName, institution.Name, "Name");
                institution.Name = viewModel.InstitutionName;

                AppendFieldLevelAuditDetails(auditLogData, viewModel.Address.Address1, institution.Address.Address1,
                    "Address1");
                AppendFieldLevelAuditDetails(auditLogData, viewModel.Address.Address2, institution.Address.Address2,
                    "Address2");
                AppendFieldLevelAuditDetails(auditLogData, viewModel.Address.City, institution.Address.City, "City");
                AppendFieldLevelAuditDetails(auditLogData, viewModel.Address.State, institution.Address.State, "State");
                AppendFieldLevelAuditDetails(auditLogData, viewModel.Address.Zip, institution.Address.Zip, "Zip");
                institution.Address.Address1 = viewModel.Address.Address1;
                institution.Address.Address2 = viewModel.Address.Address2;
                institution.Address.City = viewModel.Address.City;
                institution.Address.State = viewModel.Address.State;
                institution.Address.Zip = viewModel.Address.Zip;

                AppendFieldLevelAuditDetails(auditLogData, viewModel.DisplayAllProducts, institution.DisplayAllProducts,
                    "DisplayAllProducts");
                AppendFieldLevelAuditDetails(auditLogData, (int)viewModel.AccessType, institution.AccessTypeId,
                    "AccessTypeId");
                institution.DisplayAllProducts = viewModel.DisplayAllProducts;
                institution.AccessTypeId = (int)viewModel.AccessType;

                AppendFieldLevelAuditDetails(auditLogData, viewModel.AthensAffiliation, institution.AthensAffiliation,
                    "AthensAffiliation");
                AppendFieldLevelAuditDetails(auditLogData, viewModel.LogUrl, institution.LogUrl, "LogUrl");
                AppendFieldLevelAuditDetails(auditLogData, viewModel.HomePageId, institution.HomePageId, "HomePageId");
                AppendFieldLevelAuditDetails(auditLogData, viewModel.EnableExpertReviewerUser,
                    institution.ExpertReviewerUserEnabled, "ExpertReviewerUserEnabled");
                AppendFieldLevelAuditDetails(auditLogData, viewModel.IncludeArchivedTitlesByDefault,
                    institution.IncludeArchivedTitlesByDefault, "IncludeArchivedTitlesByDefault");

                institution.AthensAffiliation = viewModel.AthensAffiliation;
                institution.LogUrl = viewModel.LogUrl;
                institution.HomePageId = viewModel.HomePageId;
                institution.ExpertReviewerUserEnabled = viewModel.EnableExpertReviewerUser;
                institution.IncludeArchivedTitlesByDefault = viewModel.IncludeArchivedTitlesByDefault;

                AppendFieldLevelAuditDetails(auditLogData, viewModel.ProxyPrefix, institution.ProxyPrefix,
                    "ProxyPrefix");
                institution.ProxyPrefix =
                    string.IsNullOrWhiteSpace(viewModel.ProxyPrefix) ? null : viewModel.ProxyPrefix;

                AppendFieldLevelAuditDetails(auditLogData, viewModel.OclcSymbol, institution.OclcSymbol, "OclcSymbol");
                institution.OclcSymbol = string.IsNullOrWhiteSpace(viewModel.OclcSymbol) ? null : viewModel.OclcSymbol;

                AppendFieldLevelAuditDetails(auditLogData, viewModel.EnableIpPlus, institution.EnableIpPlus,
                    "EnableIpPlus");
                institution.EnableIpPlus = viewModel.EnableIpPlus;

                AppendFieldLevelAuditDetails(auditLogData, viewModel.EnableHomePageCollectionLink,
                    institution.EnableHomePageCollectionLink, "EnableHomePageCollectionLink");
                institution.EnableHomePageCollectionLink = viewModel.EnableHomePageCollectionLink;
            }

            return auditLogData.ToString();
        }

        public static IInstitution ToCoreInstitutionFromTrialInstitution(this InstitutionEditViewModel institutionEdit)
        {
            var institution = new Core.Institution.Institution
            {
                AccountNumber = institutionEdit.AccountNumber,
                AccountStatusId = (int)AccountStatus.Trial,
                Trial = new Trial { StartDate = DateTime.Now, EndDate = institutionEdit.TrialEndDate },
                Discount = institutionEdit.Discount,
                HouseAccount = false,
                Name = institutionEdit.InstitutionName,
                Address = new Address
                {
                    Address1 = institutionEdit.Address.Address1,
                    Address2 = institutionEdit.Address.Address2,
                    City = institutionEdit.Address.City,
                    State = institutionEdit.Address.State,
                    Zip = institutionEdit.Address.Zip
                },
                AccessTypeId = (int)institutionEdit.AccessType,
                AthensAffiliation = institutionEdit.AthensAffiliation,
                LogUrl = institutionEdit.LogUrl,
                HomePageId = institutionEdit.HomePageId,
                TrustedKey = institutionEdit.TrustedKey,
                DisplayAllProducts = false
            };

            if (institutionEdit.Type != null && institutionEdit.Type.Id != 0)
            {
                institution.Type = institutionEdit.Type;
            }
            else
            {
                institution.Type = null;
            }


            if (institutionEdit.InstitutionTerritory != null)
            {
                if (institutionEdit.InstitutionTerritory.TerritoryId > 0)
                {
                    if (institution.Territory == null)
                    {
                        institution.Territory = new Territory();
                    }

                    institution.Territory = new Territory { Id = institutionEdit.InstitutionTerritory.TerritoryId };
                }
                else
                {
                    institution.Territory = null;
                }
            }

            return institution;
        }

        public static Core.Authentication.IpAddressRange ToIpAddressRange(this InstitutionIpRanges institutionIpRange)
        {
            var ipAddressRange = new Core.Authentication.IpAddressRange();
            if (ipAddressRange.Institution == null)
            {
                ipAddressRange.Institution = new Core.Institution.Institution();
            }

            ipAddressRange.Institution.Id = institutionIpRange.InstitutionId;
            ipAddressRange.InstitutionId = institutionIpRange.InstitutionId;

            ipAddressRange.Id = institutionIpRange.EditIpAddressRange.Id;
            ipAddressRange.OctetA = institutionIpRange.EditIpAddressRange.OctetA;
            ipAddressRange.OctetB = institutionIpRange.EditIpAddressRange.OctetB;
            ipAddressRange.OctetCStart = institutionIpRange.EditIpAddressRange.OctetCStart;
            ipAddressRange.OctetCEnd = institutionIpRange.EditIpAddressRange.OctetCEnd;
            ipAddressRange.OctetDStart = institutionIpRange.EditIpAddressRange.OctetDStart;
            ipAddressRange.OctetDEnd = institutionIpRange.EditIpAddressRange.OctetDEnd;

            ipAddressRange.Description = institutionIpRange.EditIpAddressRange.Description;

            ipAddressRange.RecordStatus = true;
            return ipAddressRange;
        }

        public static Core.Authentication.User ToCoreUser(this UserEdit userEdit, Core.Authentication.User user)
        {
            user.ExpirationDate = userEdit.ExpirationDate;
            user.FirstName = userEdit.FirstName;
            user.LastName = userEdit.LastName;
            user.Email = userEdit.Email;

            user.Department = userEdit.Department;

            user.Role = new Role { Code = userEdit.Role.Code, Id = (int)userEdit.Role.Code };

            if (!string.IsNullOrWhiteSpace(userEdit.UserName))
            {
                user.UserName = userEdit.UserName;
            }

            if (!string.IsNullOrWhiteSpace(userEdit.Password))
            {
                user.Password = userEdit.Password;
            }

            user.AthensTargetedId = userEdit.AthensTargetedId;

            return user;
        }

        public static Core.Authentication.User ToCoreUserFromTrialInstitution(this UserEdit userEdit,
            IInstitution institution)
        {
            var passwordSalt = PasswordService.GenerateNewSalt();
            var passwordHash = PasswordService.GenerateSlowPasswordHash(userEdit.Password, passwordSalt);

            return new Core.Authentication.User
            {
                FirstName = userEdit.FirstName,
                LastName = userEdit.LastName,
                Email = userEdit.Email,
                Department = userEdit.Department,
                Role = new Role { Code = RoleCode.INSTADMIN, Id = (int)RoleCode.INSTADMIN },
                UserName = institution.AccountNumber,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Password = userEdit.Password,
                //ReceiveLockoutInfo = userEdit.ReceiveLockoutInfoValue,
                //ReceiveNewResourceInfo = userEdit.ReceiveNewResourceInfoValue,
                //ReceiveForthcomingPurchase = userEdit.ReceiveForthComingPurchaseValue,
                //ReceiveNewSearchResource = userEdit.ReceiveNewSearchResourceValue,
                //ReceiveNewEditionInfo = userEdit.ReceiveNewEditionInfoValue,
                //ReceiveCartRemind = userEdit.ReceiveCartRemindValue,
                //ReceiveDctMedicalUpdate = userEdit.ReceiveDctMedicalUpdateValue,
                //ReceiveDctNursingUpdate = userEdit.ReceiveDctNursingUpdateValue,
                //ReceiveDctAlliedHealthUpdate = userEdit.ReceiveDctAlliedHealthUpdateValue,
                //ReceivePdaAddToCart = userEdit.ReceivePdaAddToCartValue,
                //ReceivePdaReport = userEdit.ReceivePdaReportValue,
                //ReceiveArchivedAlert = userEdit.ReceiveArchivedAlertValue,
                //ReceiveLibrarianAlert = userEdit.ReceiveLibrarianAlertValue,
                //ReceiveExpertReviewerRequests = userEdit.ReceiveExpertReviewerRequestsValue,
                //ReceiveExpertReviewerRecommendations = userEdit.ReceiveExpertReviewerRecommendationsValue,
                //ReceiveDashboardEmail = userEdit.ReceiveDashboardEmailValue
                Institution = (Core.Institution.Institution)institution,
                InstitutionId = institution.Id
            };
        }

        public static Core.Institution.InstitutionBranding ToCoreInstitutionBranding(
            this InstitutionBranding.InstitutionBranding editInstitutionBranding,
            Core.Institution.InstitutionBranding institutionBranding, bool clearImage)
        {
            if (institutionBranding == null)
            {
                institutionBranding = new Core.Institution.InstitutionBranding
                {
                    Institution = new Core.Institution.Institution { Id = editInstitutionBranding.InstitutionId }
                };
            }

            institutionBranding.InstitutionDisplayName = editInstitutionBranding.InstitutionDisplayName;
            institutionBranding.Message = editInstitutionBranding.Message;

            if (!string.IsNullOrWhiteSpace(editInstitutionBranding.LogoFileName) || clearImage)
            {
                institutionBranding.LogoFileName = editInstitutionBranding.LogoFileName;
            }

            return institutionBranding;
        }

        public static Core.Institution.InstitutionReferrer ToInstitutionReferrer(
            this InstitutionReferrer.InstitutionReferrer editInstitutionReferrer,
            Core.Institution.InstitutionReferrer institutionReferrer)
        {
            institutionReferrer.Id = editInstitutionReferrer.ValidReferrerId;
            institutionReferrer.InstitutionId = editInstitutionReferrer.InstitutionId;
            institutionReferrer.ValidReferer = editInstitutionReferrer.ValidReferer;

            return institutionReferrer;
        }

        public static Note ToCoreNote(this Notes.Note editNote, Note coreNote)
        {
            coreNote.Comment = editNote.Comment;

            return coreNote;
        }

        private static void AppendFieldLevelAuditDetails(StringBuilder sb, string newValue, string originalValue,
            string fieldName)
        {
            if (originalValue != newValue &&
                !(string.IsNullOrWhiteSpace(newValue) && string.IsNullOrWhiteSpace(originalValue)))
            {
                sb.AppendFormat(" [{0} changed from '{1}' to '{2}'] ", fieldName, originalValue, newValue);
            }
        }

        private static void AppendFieldLevelAuditDetails(StringBuilder sb, int newValue, int originalValue,
            string fieldName)
        {
            if (originalValue != newValue)
            {
                sb.AppendFormat(" [{0} changed from '{1}' to '{2}'] ", fieldName, originalValue, newValue);
            }
        }

        private static void AppendFieldLevelAuditDetails(StringBuilder sb, bool newValue, bool originalValue,
            string fieldName)
        {
            if (originalValue != newValue)
            {
                sb.AppendFormat(" [{0} changed from '{1}' to '{2}'] ", fieldName, originalValue, newValue);
            }
        }

        private static void AppendFieldLevelAuditDetails(StringBuilder sb, DateTime? newValue, DateTime? originalValue,
            string fieldName)
        {
            if (originalValue != newValue)
            {
                sb.AppendFormat(" [{0} changed from '{1}' to '{2}'] ", fieldName, originalValue, newValue);
            }
        }

        private static void AppendFieldLevelAuditDetails(StringBuilder sb, decimal newValue, decimal originalValue,
            string fieldName)
        {
            if (originalValue != newValue)
            {
                sb.AppendFormat(" [{0} changed from '{1}' to '{2}'] ", fieldName, originalValue, newValue);
            }
        }
    }
}