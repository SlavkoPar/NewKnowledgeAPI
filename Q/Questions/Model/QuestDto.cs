using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using System.Net;

namespace NewKnowledgeAPI.Q.Questions.Model
{
    public class QuestDto
    {
        public string PartitionKey { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string ParentCategory { get; set; }

        public QuestDto()
        {
        }

        public QuestDto(Question question)
        {
            PartitionKey = question.PartitionKey;
            ParentCategory = question.ParentCategory!;
            Id = question.Id;
            Title = question.Title;
        }

    }
}



