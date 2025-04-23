using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using System.Net;

namespace NewKnowledgeAPI.A.Answers.Model
{
    public class AnsDto
    {
        public string PartitionKey { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string ParentGroup { get; set; }

        public AnsDto()
        {
        }

        public AnsDto(Answer answer)
        {
            PartitionKey = answer.PartitionKey;
            ParentGroup = answer.ParentGroup!;
            Id = answer.Id;
            Title = answer.Title;
        }

    }
}



