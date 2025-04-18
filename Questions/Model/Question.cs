﻿using NewKnowledgeAPI.Common;
using Newtonsoft.Json;

namespace NewKnowledgeAPI.Questions.Model
{
    public class Question : Record, IDisposable
    {
        public string Type { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }

        public string? CategoryTitle { get; set; }
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
            CategoryTitle = null;
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
            CategoryTitle = null;
            Title = questionDto.Title;
            AssignedAnswers = questionDto.AssignedAnswers!;
            NumOfAssignedAnswers = questionDto.NumOfAssignedAnswers;
            Source = questionDto.Source;
            Status = questionDto.Status;    
        }

        //public override string ToString() => 
        //    $"{PartitionKey}/{Id}, {Title} {ParentCategory} ";

        public void Deconstruct(out string partitionKey, out string id, out string title, out string? parentCategory,
                                out string type, out int source, out int status, out List<long> assignedAnswers)
        {
            partitionKey = PartitionKey;
            id = Id;
            title = Title;
            parentCategory = ParentCategory;
            type = Type;
            source = Source;
            status = Status;
            assignedAnswers = AssignedAnswers;
        }

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
}
