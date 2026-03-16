#region


using R2V2.Infrastructure.DependencyInjection;
using R2Utilities.Email;
using EmailMessage = R2V2.Infrastructure.Email.EmailMessage;

#endregion

namespace R2Utilities.Tasks
{
    public abstract class EmailTaskBase : TaskBase
    {
        protected readonly EmailDeliveryService EmailDeliveryService;

        protected EmailTaskBase(string taskName, string taskSwitch, string taskSwitchSmall, TaskGroup taskGroup,
            string taskDescription, bool enabled)
            : base(taskName, taskSwitch, taskSwitchSmall, taskGroup, taskDescription, enabled)
        {
            EmailDeliveryService = ServiceLocator.Current.GetInstance<EmailDeliveryService>();
        }

        public string PopulateField(string label, string value = "")
        {
            return string.IsNullOrWhiteSpace(value)
                ? $"<strong>{label}</strong>"
                : $"<strong>{label}</strong>{value}";
        }

        public string PopulateFieldOrNull(string label, string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? ""
                : $"<strong>{label}</strong>{value}";
        }

        protected void AddTaskCcToEmailMessage(EmailMessage emailMessage)
        {
            foreach (var address in EmailSettings.TaskEmailConfig.CcAddresses)
            {
                if (!emailMessage.AddCcRecipient(address))
                {
                    Log.WarnFormat("invalid CC email address <{0}>", address);
                }
            }
        }

        protected void AddTaskBccToEmailMessage(EmailMessage emailMessage)
        {
            foreach (var address in EmailSettings.TaskEmailConfig.BccAddresses)
            {
                if (!emailMessage.AddBccRecipient(address))
                {
                    Log.WarnFormat("invalid BCC email address <{0}>", address);
                }
            }
        }
    }
}