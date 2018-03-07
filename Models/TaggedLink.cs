using System;
using System.Collections.Generic;
using MongoDB.Bson;

namespace NaimeiKnowledge.Models
{
    public class TaggedLink
    {
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        public IList<string> Tags { get; set; }

        public string Title { get; set; }

        public Uri Url { get; set; }
    }
}
