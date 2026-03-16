#region

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace R2V2.Core.Email
{
    public class InstitutionResourceEmail : IDebugInfo
    {
        private readonly IList<InstitutionResourceEmailRecipient> _recipients =
            new List<InstitutionResourceEmailRecipient>();

        public virtual int Id { get; set; }
        public virtual int InstitutionId { get; set; }
        public virtual int ResourceId { get; set; }
        public virtual int? UserId { get; set; }
        public virtual string ChapterSectionId { get; set; }
        public virtual string UserEmailAddress { get; set; }
        public virtual string Subject { get; set; }
        public virtual string Comment { get; set; }
        public virtual string SessionId { get; set; }
        public virtual string RequestId { get; set; }
        public virtual bool Queued { get; set; }
        public virtual DateTime CreationDate { get; set; }

        public virtual IEnumerable<InstitutionResourceEmailRecipient> Recipients => _recipients;

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("InstitutionResourceEmail = [");
            sb.Append($"Id: {Id}");
            sb.Append($", InstitutionId: {InstitutionId}");
            sb.Append($", ResourceId: {ResourceId}");
            sb.Append($", UserId: {UserId}");
            sb.Append($", ChapterSectionId: {ChapterSectionId}");
            sb.Append($", UserEmailAddress: {UserEmailAddress}");
            sb.Append($", Subject: {Subject}");
            sb.Append($", Comment: {Comment}");
            sb.Append($", SessionId: {SessionId}");
            sb.Append($", RequestId: {RequestId}");
            sb.Append($", Queued: {Queued}");
            sb.Append($", CreationDate: {CreationDate}");

            sb.AppendLine().AppendLine("\tRecipient: [");
            foreach (var recipient in Recipients)
            {
                sb.AppendLine().Append($"\t\t{recipient.ToDebugString()}");
            }

            sb.AppendLine("\t]");
            sb.Append("]");
            return sb.ToString();
        }

        public virtual void AddRecipient(InstitutionResourceEmailRecipient recipient)
        {
            recipient.InstitutionResourceEmail = this;
            _recipients.Add(recipient);
        }
    }
}