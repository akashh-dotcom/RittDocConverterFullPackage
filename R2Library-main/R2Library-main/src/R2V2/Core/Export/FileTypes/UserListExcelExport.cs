#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Authentication;

#endregion

namespace R2V2.Core.Export.FileTypes
{
    public class UserListExcelExport : ExcelBase
    {
        private readonly User _currentUser;

        public UserListExcelExport(IList<User> users, bool isRa, int currentUserInstitutonId)
        {
            var displayTerritory = users.Any(x => x.InstitutionId != currentUserInstitutonId);

            SpecifyColumn("Active", "String");
            if (isRa)
            {
                SpecifyColumn("Institution", "String");
                SpecifyColumn("Account Number", "String");
                SpecifyColumn("Institution Account Status", "String");
            }

            SpecifyColumn("Last Name, First Name", "String");
            SpecifyColumn("User Name", "String");
            SpecifyColumn("Is Athens User", "boolean");
            SpecifyColumn("Email", "String");
            SpecifyColumn("Role", "String");
            SpecifyColumn("Department", "String");
            if (displayTerritory)
            {
                SpecifyColumn("Institution Territory", "String");
            }

            SpecifyColumn("Created By", "String");
            SpecifyColumn("Date Created", "String");
            SpecifyColumn("Expiration Date", "String");
            SpecifyColumn("Last Session", "String");

            SpecifyColumn("New Resource Information", "String");
            SpecifyColumn("New Edition Information", "String");
            SpecifyColumn("Cart Reminder", "String");
            SpecifyColumn("Pre-Order Purchases", "String");
            SpecifyColumn("DCT Medical Update", "String");
            SpecifyColumn("DCT Nursing Update", "String");
            SpecifyColumn("DCT Allied Health Update", "String");
            SpecifyColumn("Pda Trigger Events", "String");
            SpecifyColumn("Pda History Report", "String");
            SpecifyColumn("Archived Resource Alert", "String");
            SpecifyColumn("Expert Reviewer Recommendations", "String");
            SpecifyColumn("Expert Reviewer User Requests", "String");
            SpecifyColumn("Monthly Institution Summary", "String");
            SpecifyColumn("Resource Access Denied", "String");

            foreach (var user in users)
            {
                _currentUser = user;

                PopulateFirstColumn(user.RecordStatus ? user.IsLocked ? "Locked" : "Yes" : "No");

                if (isRa)
                {
                    PopulateNextColumn(user.Institution.Name);
                    PopulateNextColumn($"{user.Institution.AccountNumber}");
                    PopulateNextColumn(user.Institution.AccountStatus.Description);
                }

                PopulateNextColumn(
                    $"{(string.IsNullOrWhiteSpace(user.LastName) ? "N/A" : $"{user.LastName}, {user.FirstName}")}");
                PopulateNextColumn(user.UserName);
                PopulateNextColumn(!string.IsNullOrWhiteSpace(user.AthensTargetedId));
                PopulateNextColumn(user.Email);
                PopulateNextColumn(user.Role.Description);
                PopulateNextColumn(user.Department != null ? user.Department.Name : "N/A");

                if (displayTerritory)
                {
                    PopulateNextColumn(_currentUser.Institution?.Territory?.Code ?? "Not Set");
                }

                PopulateNextColumn(user.CreatedBy);
                PopulateNextColumn($"{user.CreationDate:M/d/yyyy}");
                PopulateNextColumn($"{user.ExpirationDate:M/d/yyyy}");
                PopulateNextColumn($"{user.LastSession:M/d/yyyy}");


                PopulateLastColumn(user.UserHasEmailOption(UserOptionCode.NewResource) ? "Yes" : "No");
                PopulateNextColumn(user.UserHasEmailOption(UserOptionCode.NewEdition) ? "Yes" : "No");
                PopulateNextColumn(user.UserHasEmailOption(UserOptionCode.CartRemind) ? "Yes" : "No");
                PopulateNextColumn(user.UserHasEmailOption(UserOptionCode.ForthcomingPurchase) ? "Yes" : "No");
                PopulateNextColumn(user.UserHasEmailOption(UserOptionCode.DctMedical) ? "Yes" : "No");
                PopulateNextColumn(user.UserHasEmailOption(UserOptionCode.DctNursing) ? "Yes" : "No");
                PopulateNextColumn(user.UserHasEmailOption(UserOptionCode.DctAlliedHealth) ? "Yes" : "No");
                PopulateNextColumn(user.UserHasEmailOption(UserOptionCode.PdaAddToCart) ? "Yes" : "No");
                PopulateNextColumn(user.UserHasEmailOption(UserOptionCode.PdaReport) ? "Yes" : "No");
                PopulateNextColumn(user.UserHasEmailOption(UserOptionCode.ArchivedAlert) ? "Yes" : "No");
                PopulateNextColumn(user.UserHasEmailOption(UserOptionCode.ExpertReviewRecommend) ? "Yes" : "No");
                PopulateNextColumn(user.UserHasEmailOption(UserOptionCode.ExpertReviewUserRequest) ? "Yes" : "No");
                PopulateNextColumn(user.UserHasEmailOption(UserOptionCode.Dashboard) ? "Yes" : "No");
                PopulateNextColumn(user.UserHasEmailOption(UserOptionCode.AccessDenied) ? "Yes" : "No");
            }
        }
    }
}