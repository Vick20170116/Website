using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using NaimeiKnowledge.Models;
using ZetaLib.AspNetCore.Forum;

namespace NaimeiKnowledge.Services
{
    public class ForumManager : ForumManager<ForumPost, ForumComment, ForumTag, ObjectId>
    {
        public ForumManager(
            IForumStore<ForumPost, ForumComment, ObjectId> forumStore,
            ILogger<ForumManager> logger) : base(forumStore, logger)
        {
        }
    }
}
