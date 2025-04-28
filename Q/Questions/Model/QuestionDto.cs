using Knowledge.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using NewKnowledgeAPI.A.Answers;
using NewKnowledgeAPI.Common;
using Newtonsoft.Json;
using System.Net;

namespace NewKnowledgeAPI.Q.Questions.Model
{
    public class QuestionDto : RecordDto
    {
        public string PartitionKey { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string? CategoryTitle { get; set; }
        public string? ParentCategory { get; set; }
        public List<AssignedAnswerDto>? AssignedAnswerDtos { get; set; }
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
            var assignedAnswers = question.AssignedAnswers ?? [];
            var questionKey = new QuestionKey(question);
            PartitionKey = question.PartitionKey;
            Id = question.Id;
            Title = question.Title;
            CategoryTitle = question.CategoryTitle;
            ParentCategory = question.ParentCategory;
            AssignedAnswerDtos = assignedAnswers
                .Select(assignedAnswer => new AssignedAnswerDto(assignedAnswer))
                .ToList();
            NumOfAssignedAnswers = question.NumOfAssignedAnswers;
            Source = question.Source;
            Status = question.Status;

            //QuestionKey questionKey = new(question);
            //AssignedAnswerDtos = new List<AssignedAnswerDto>();
            //foreach (AssignedAnswer assignedAnswer in question.AssignedAnswers) {
            //    AssignedAnswerDtos.Add(new AssignedAnswerDto(questionKey, assignedAnswer));
            //}
        }
    }
 }



