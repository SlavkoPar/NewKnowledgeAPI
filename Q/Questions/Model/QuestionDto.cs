using Knowledge.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using NewKnowledgeAPI.A.Answers;
using NewKnowledgeAPI.Common;
using Newtonsoft.Json;
using System.Net;
using NewKnowledgeAPI.Q.Questions.Model;

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
        public List<RelatedFilterDto>? RelatedFilterDtos { get; set; }
        public int NumOfRelatedFilters { get; set; }
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
            var questionKey = new QuestionKey(question);
            PartitionKey = question.PartitionKey;
            Id = question.Id;
            Title = question.Title;
            CategoryTitle = question.CategoryTitle;
            ParentCategory = question.ParentCategory;
            //
            // We don't modify question AssignedAnswers through QuestionDto
            //
            //var assignedAnswers = question.AssignedAnswers ?? [];
            //assignedAnswers.Sort(AssignedAnswer.Comparer); // put the most rated AssignedAnswers to the top
            //AssignedAnswerDtos = assignedAnswers
            //    .Select(assignedAnswer => new AssignedAnswerDto(questionKey, assignedAnswer))
            //    .ToList();
            //NumOfAssignedAnswers = question.NumOfAssignedAnswers;
            //
            //var relatedFilters = question.AssignedAnswers ?? [];
            //assignedAnswers.Sort(AssignedAnswer.Comparer); // put the most rated AssignedAnswers to the top
            //RelatedFilterDtos = relatedFilters
            //    //.Select(relatedFilters => new RelatedFilterDto(questionKey, relatedFilters))
            //    .Select(relatedFilters => new RelatedFilterDto(questionKey, relatedFilters))
            //    .ToList();
            //NumOfRelatedFilters = question.NumOfRelatedFilters;
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



