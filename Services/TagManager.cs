using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using NaimeiKnowledge.Models;
using ZetaLib.AspNetCore.Forum;

namespace NaimeiKnowledge.Services
{
    public class TagManager : TagManager<ForumTag, ObjectId>
    {
        public TagManager(
            ITagStore<ForumTag, ObjectId> tagStore,
            ILogger<TagManager> logger) : base(tagStore, logger)
        {
        }
    }
}
