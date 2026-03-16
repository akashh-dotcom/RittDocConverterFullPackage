#region

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.Promotion
{
    public enum OngoingPdaEventType
    {
        Promotion = 1,
        NewEdition = 2,
        DoodyUpdate = 3
    }

    public class OngoingPdaEventMessage : IDebugInfo
    {
        private readonly List<string> _isbns = new List<string>();

        public OngoingPdaEventMessage()
        {
        }

        public OngoingPdaEventMessage(OngoingPdaEventType eventType)
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.Now;
            EventType = eventType;
        }

        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public OngoingPdaEventType EventType { get; set; }
        public int ProcessCount { get; set; }

        public IEnumerable<string> Isbns => _isbns;

        public string ToDebugString()
        {
            return $"OngoingPdaEventMessage = {ToJsonString()}";
        }

        public void AddIsbn(string isbn)
        {
            _isbns.Add(isbn);
        }

        public void AddIsbns(IEnumerable<string> isbns)
        {
            _isbns.AddRange(isbns);
        }

        public void ClearIsbns()
        {
            _isbns.Clear();
        }

        public void AddResourceIds(List<int> resourceIds)
        {
            // Convert resource IDs to ISBNs - this would need actual resource lookup
            // For now, just add them as string representations
            foreach (var resourceId in resourceIds)
            {
                _isbns.Add(resourceId.ToString());
            }
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public OngoingPdaEvent ToOngoingPdaEvent(IList<IResource> resources)
        {
            var ongoingPdaEvent = new OngoingPdaEvent
            {
                TransactionId = Id,
                EventTypeId = (int)EventType
            };

            foreach (var isbn in Isbns)
            {
                var resource = resources.FirstOrDefault(x => x.Isbn == isbn);
                if (resource != null)
                {
                    ongoingPdaEvent.AddResource(resource.Id, isbn, ongoingPdaEvent);
                }
            }

            return ongoingPdaEvent;
        }
    }
}