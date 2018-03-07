namespace NaimeiKnowledge.Areas.Api.Models.JsonApi.Topic
{
    public class JsonApiTopicInfoStatisticsAttribute
    {
        public int CommentCount { get; set; }

        public int DownvoteCount { get; set; }

        public int PostCount { get; set; }

        public int UpvoteCount { get; set; }
    }
}
