using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using NewKnowledgeAPI.Common;
using Newtonsoft.Json;
using System.Net;

namespace NewKnowledgeAPI.A.Answers.Model
{
    public class AnswerDto : RecordDto
    {
        public string PartitionKey { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string? Link { get; set; }
        public string? GroupTitle { get; set; }
        public string? ParentGroup { get; set; }
        public int Source { get; set; }
        public int Status { get; set; }

        public AnswerDto()
            : base()
        {
        }

        public AnswerDto(Answer answer)
            : base(answer.Created, answer.Modified, answer.Archived)
        {
            //Console.WriteLine(JsonConvert.SerializeObject(answer));
            PartitionKey = answer.PartitionKey;
            Id = answer.Id;
            Title = answer.Title;
            Link = answer.Link;
            GroupTitle = answer.GroupTitle;
            ParentGroup = answer.ParentGroup;
            Source = answer.Source;
            Status = answer.Status;
        }
    }
 }



