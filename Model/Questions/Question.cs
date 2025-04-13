using System.Drawing.Printing;
using System.Net;
using Microsoft.Azure.Cosmos;
using NewKnowledgeAPI.Model.Categories;
using NewKnowledgeAPI.Model.Common;
using Newtonsoft.Json;

namespace NewKnowledgeAPI.Model.Questions
{
    public class Question : Record, IDisposable
    {
        public string Type { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }

        public string Title { get; set; }
        public string? ParentCategory { get; set; }
        public List<long> AssignedAnswers { get; set; }
        public int NumOfAssignedAnswers  {get; set;}
        public int Source { get; set; }
        public int Status { get; set; }

        public Question()
            :  base()
        {
        }

  
        public Question(QuestionData questionData)
            : base(new WhoWhen("Admin"), null, null)
        {
            string s = DateTime.Now.Ticks.ToString();
            Id = s.Substring(s.Length-10);// Guid.NewGuid().ToString();
            Type = "question";
            PartitionKey = questionData.ParentCategory!;
            ParentCategory = questionData.ParentCategory;
            Title = questionData.Title;
            AssignedAnswers = [];
            NumOfAssignedAnswers = 0;
            Source = 0;
            Status = 0;
        }

        public Question(QuestionDto questionDto)
        : base(questionDto.Created, questionDto.Modified, questionDto.Archived)
        {
            string s = DateTime.Now.Ticks.ToString();
            Id = s.Substring(s.Length - 10);// Guid.NewGuid().ToString();
            Type = "question";
            PartitionKey = questionDto.PartitionKey!;
            ParentCategory = questionDto.ParentCategory;
            Title = questionDto.Title;
            AssignedAnswers = questionDto.AssignedAnswers!;
            NumOfAssignedAnswers = questionDto.NumOfAssignedAnswers;
            Source = questionDto.Source;
            Status = questionDto.Status;    
        }

        //public override string ToString() => 
        //    $"{PartitionKey}/{Id}, {Title} {ParentCategory} ";

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }

    public class QuestionEx
    {
        public QuestionEx(Question? question, string msg)
        {
            this.question = question;
            this.msg = msg;
        }

        public Question? question { get; set; }
        public string msg { get; set; }    
    }
}
