using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using NewKnowledgeAPI.Common;
using Newtonsoft.Json;
using System.Net;

namespace NewKnowledgeAPI.Questions.Model
{
    public class QuestionDto : RecordDto
    {
        public string PartitionKey { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string? CategoryTitle { get; set; }
        public string? ParentCategory { get; set; }
        public List<long>? AssignedAnswers { get; set; }
        public int NumOfAssignedAnswers { get; set; }
        public int Source { get; set; }
        public int Status { get; set; }

        public QuestionDto()
            : base()
        {
        }

        public QuestionDto(Question question)
            : base(question.Created, question.Modified, question.Archived)
        {
            //Console.WriteLine(JsonConvert.SerializeObject(question));
            PartitionKey = question.PartitionKey;
            Id = question.Id;
            Title = question.Title;
            CategoryTitle = question.CategoryTitle;
            ParentCategory = question.ParentCategory;
            AssignedAnswers = question.AssignedAnswers;
            NumOfAssignedAnswers = question.NumOfAssignedAnswers;
            Source = question.Source;
            Status = question.Status;
        }
    }
 }



