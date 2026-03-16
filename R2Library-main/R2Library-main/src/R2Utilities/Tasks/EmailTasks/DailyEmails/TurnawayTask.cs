#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess;
using R2Utilities.Infrastructure.Settings;
using R2V2.Core.Authentication;
using R2V2.Core.Email;
using R2V2.Core.Reports;
using R2V2.Infrastructure.Email;

#endregion

namespace R2Utilities.Tasks.EmailTasks.DailyEmails
{
    public class TurnawayTask : EmailTaskBase
    {
        private readonly EmailTaskService _emailTaskService;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly TurnawayEmailBuildService _turnawayEmailBuildService;

        public TurnawayTask(
            EmailTaskService emailTaskService
            , TurnawayEmailBuildService turnawayEmailBuildService
            , IR2UtilitiesSettings r2UtilitiesSettings
        )
            : base("TurnawayTask", "-SendTurnawayEmails", "44", TaskGroup.CustomerEmails,
                "Sends turnaway emails to customers (IAs)", true)
        {
            _emailTaskService = emailTaskService;
            _turnawayEmailBuildService = turnawayEmailBuildService;
            _r2UtilitiesSettings = r2UtilitiesSettings;
        }

        public override void Run()
        {
            // todo: create an interface to so this can be set in the EmailTaskBase.cs
            // init
            EmailDeliveryService.DebugMaxEmailsSent = _r2UtilitiesSettings.EmailTestNumberOfEmails;

            TaskResult.Information = "Turnaway Resource Emails Task";
            var step = new TaskResultStep { Name = "TurnawayTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();
            try
            {
                var successEmails = 0;
                var failureEmails = 0;
                var failureToBuild = 0;

                var failureEmailAddress = new StringBuilder();


                var turnaways2 = _emailTaskService.GetInstitutionTurnaways(_r2UtilitiesSettings.R2ReportsDatabaseName,
                    _r2UtilitiesSettings.R2DatabaseName);
                var turnawayUsers = _emailTaskService.GetUsersForTurnawayEmail();


                var turnawayGroupings = (from t in turnaways2
                    group t by t.InstitutionId
                    into g
                    select new { InstitutionId = g.Key, Turnaways = g.ToList() }).ToList();

                var turnawayGroupingUsers = turnawayUsers.Where(x =>
                        turnawayGroupings.Select(y => y.InstitutionId).Contains(x.InstitutionId.GetValueOrDefault()))
                    .ToList();

                Log.Info($"Processing {turnawayGroupings.Count} Institutions with Turnaways");
                Log.Info($"Processing {turnawayGroupingUsers.Count} users for those Turnaways.");
                var userCount = 0;

                foreach (var grouping in turnawayGroupings)
                {
                    var institutionUsers = turnawayGroupingUsers.Where(x => x.InstitutionId == grouping.InstitutionId)
                        .ToList();
                    var turnaways = grouping.Turnaways.OrderBy(x =>
                    {
                        var firstOrDefault = x.Resource.Specialties.OrderBy(y => y.Name).FirstOrDefault();
                        return firstOrDefault != null
                            ? x.Resource.Specialties != null ? firstOrDefault.Name : "zzz"
                            : null;
                    }).ToList();

                    if (institutionUsers.Any())
                    {
                        Log.Info($"Processing Institution {grouping.InstitutionId}");
                        foreach (var item in turnaways)
                        {
                            Log.Info(
                                $"[ResourceId: {item.ResourceId} | AccessTurnaway: {item.TurnawayDates.Count(x => x.IsAccessTurnaway)} | ConcurrencyTurnaway: {item.TurnawayDates.Count(x => !x.IsAccessTurnaway)}]");
                        }

                        foreach (var user in institutionUsers)
                        {
                            userCount++;
                            var emailMessage = GetTurnawayEmail(user, turnaways);
                            var userInfo =
                                $"[InstitutionId:{user.Institution.Id} | UserId:{user.Id} | UserEmail: {user.Email}]";
                            if (emailMessage != null)
                            {
                                var success = EmailDeliveryService.SendCustomerTaskEmail(emailMessage,
                                    _r2UtilitiesSettings.DefaultFromAddress,
                                    _r2UtilitiesSettings.DefaultFromAddressName);
                                if (success)
                                {
                                    successEmails++;
                                }
                                else
                                {
                                    failureEmails++;
                                    failureEmailAddress.Append($"{userInfo}<br/>");
                                }
                            }
                            else
                            {
                                Log.Info(
                                    $"{userCount} of {turnawayGroupingUsers.Count} ignored, turnaway email is null {userInfo}");
                                failureToBuild++;
                                failureEmailAddress.Append($"{userInfo}<br/>");
                            }
                        }
                    }
                    else
                    {
                        Log.Info(
                            $"No Users found for institution : {grouping.InstitutionId} || {grouping.Turnaways.Count} turnaways found;");
                    }
                }

                step.CompletedSuccessfully = failureEmails == 0 && failureToBuild == 0;
                step.Results = $@"
{successEmails} Turnaway Resource emails sent,
{failureToBuild} emails failed to build,
{failureEmails} emails failed to send.
Failed Emails information: {failureEmailAddress}";
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                step.CompletedSuccessfully = false;
                step.Results = ex.Message;
                throw;
            }
            finally
            {
                step.EndTime = DateTime.Now;
                UpdateTaskResult();
            }
        }

        public EmailMessage GetTurnawayEmail(User user, List<TurnawayResource> turnawayResources)
        {
            try
            {
                var itemBuilder = new StringBuilder();
                string lastSpecialtyName = null;

                foreach (var item in turnawayResources.Where(x => x.Resource != null))
                {
                    var specialty = item.Resource.Specialties != null
                        ? item.Resource.Specialties.OrderBy(x => x.Name).FirstOrDefault()
                        : null;

                    if (specialty != null)
                    {
                        if (lastSpecialtyName != specialty.Name)
                        {
                            itemBuilder.Append(
                                _turnawayEmailBuildService.BuildSpecialtyHeader(item.Resource, specialty));

                            lastSpecialtyName = specialty.Name;
                        }
                    }

                    itemBuilder.Append(_turnawayEmailBuildService.BuildItemHtml(item.Resource, GetTurnawayField(item),
                        user.InstitutionId.GetValueOrDefault(), user.Institution.AccountNumber));
                }

                return _turnawayEmailBuildService.BuildTurnawayEmail(user, itemBuilder.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return null;
            }
        }

        public string GetTurnawayField(TurnawayResource turnawayResource)
        {
            var sb = new StringBuilder();
            if (turnawayResource.TurnawayDates.Any())
            {
                var concurrentTurnaways = turnawayResource.TurnawayDates.Where(x => !x.IsAccessTurnaway).ToList();
                var accessTurnaways = turnawayResource.TurnawayDates.Where(x => x.IsAccessTurnaway).ToList();

                BuildTurnaway("Concurrent Licenses Exceeded Turnaways: ", concurrentTurnaways, sb);
                BuildTurnaway("         Non-Purchased Title Turnaways: ", accessTurnaways, sb);
            }

            return sb.ToString();
        }

        private void BuildTurnaway(string label, List<TurnawayDate> turnaways, StringBuilder sb)
        {
            var maxCount = 5;
            if (turnaways.Any())
            {
                sb.Append(turnaways.Count > maxCount
                    ? PopulateFieldOrNull(label, $"{turnaways.Count}")
                    : PopulateField(label));
                sb.Append("<br/>");
                sb.Append("<br/>");
                var test = turnaways.OrderByDescending(x => x.TurnawayTimeStamp).Take(maxCount);

                foreach (var concurrentTurnaway in test)
                {
                    sb.Append("<div style=\"text - align: left\">");
                    sb.Append(
                        $"{PopulateFieldOrNull("Occurrence: ", concurrentTurnaway.TurnawayTimeStamp.ToString("MM/dd/yyyy hh:mm:ss tt"))}<br/>");
                    sb.Append($"{PopulateFieldOrNull("RequestId: ", concurrentTurnaway.RequestId)}<br/>");
                    sb.Append($"{PopulateFieldOrNull("SessionId: ", concurrentTurnaway.SessionId)}<br/>");
                    sb.Append($"{PopulateFieldOrNull("IpAddress: ", concurrentTurnaway.IpAddress)}<br/>");
                    sb.Append("</div>");
                    sb.Append("<br/>");
                }
            }
        }
    }
}