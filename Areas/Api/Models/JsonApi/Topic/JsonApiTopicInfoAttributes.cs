using System;
using ZetaLib.AspNetCore.Forum;

namespace NaimeiKnowledge.Areas.Api.Models.JsonApi.Topic
{
    public class JsonApiTopicInfoAttributes
    {
        public DateTime Created { get; set; }

        public JsonApiTopicInfoStatisticsAttribute Statistics { get; set; } = new JsonApiTopicInfoStatisticsAttribute();

        public string Title { get; set; }

        public ForumPostType Type { get; set; }

        public DateTime? Updated { get; set; }
    }
}
