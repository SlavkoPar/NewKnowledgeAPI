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
            ParentCategory = question.ParentCategory;
            AssignedAnswers = question.AssignedAnswers;
            NumOfAssignedAnswers = question.NumOfAssignedAnswers;
            Source = question.Source;
            Status = question.Status;
        }
    }

    public class QuestionDtoEx
    {
        //public QuestionDtoEx(QuestionDto? questionDto, string msg)
        //{
        //    this.questionDto = questionDto;
        //    this.msg = msg;
        //}
        public QuestionDtoEx(QuestionEx questionEx)
        {
            questionDto = questionEx.question != null ? new QuestionDto(questionEx.question!) : null;
            msg = questionEx.msg!;
        }

        public QuestionDtoEx(string msg)
        {
            questionDto = null;
            this.msg = msg;
        }



        public QuestionDto? questionDto { get; set; }
        public string msg { get; set; }
    }

}



