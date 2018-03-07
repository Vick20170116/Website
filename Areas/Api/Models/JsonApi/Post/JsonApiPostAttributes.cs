using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ZetaLib.AspNetCore.Forum;
using ZetaLib.AspNetCore.Forum.MongoDB;

namespace NaimeiKnowledge.Areas.Api.Models.JsonApi.Post
{
    public class JsonApiPostAttributes
    {
        public IEnumerable<ForumPostBodyPart> Body { get; set; }

        public DateTime Created { get; set; }

        public JsonApiPostStatisticsAttribute Statistics { get; set; }

        public string Title { get; set; }

        [JsonConverter(typeof(StringEnumConverter), new object[] { true })]
        public ForumPostType Type { get; set; }

        public DateTime? Updated { get; set; }
    }
}
