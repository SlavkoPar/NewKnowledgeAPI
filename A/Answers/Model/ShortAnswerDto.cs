using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using System.Net;

namespace NewKnowledgeAPI.A.Answers.Model
{
    public class ShortAnswerDto
    {
        public string PartitionKey { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string ParentGroup { get; set; }

        public ShortAnswerDto()
        {
        }

        public ShortAnswerDto(ShortAnswer shortAnswer)
        {
            PartitionKey = shortAnswer.PartitionKey;
            ParentGroup = shortAnswer.ParentGroup!;
            Id = shortAnswer.Id;
            Title = shortAnswer.Title;
        }

    }
}



