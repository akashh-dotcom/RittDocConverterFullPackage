#region

using System;
using System.Collections.Generic;

#endregion

namespace R2V2.Core.Resource
{
    [Serializable]
    public class ResourceTopics
    {
        public Dictionary<string, ResourceTopicCache> Resources { get; set; } =
            new Dictionary<string, ResourceTopicCache>();
    }

    [Serializable]
    public class ResourceTopicCache
    {
        public int ResourceId { get; set; }
        public string Isbn { get; set; }
        public List<TopicCache> Topics { get; set; } = new List<TopicCache>();
    }

    [Serializable]
    public class TopicCache
    {
        public string Topic { get; set; }
        public string Chapter { get; set; }
        public string Section { get; set; }
    }
}