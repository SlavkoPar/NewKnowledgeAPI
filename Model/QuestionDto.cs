using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using System.Net;

namespace Knowledge.Model
{
    public class QuestionDto
    {
        // public string PartitionKey { get; set; }
        [JsonProperty(PropertyName = "Id")]
        public string Id { get; set; }
        public string Title { get; set; }
        // public string? parentCategory { get; set; }
        public List<long>? AssignedAnswers { get; set; }
        public int Source { get; set; }
        public int Status { get; set; }

        public WhoWhen Created { get; set; }
        public WhoWhen? Modified { get; set; }
        public WhoWhen? Archived { get; set; }

        public QuestionDto(Question question)
        {
            this.Id = question.Id;
            this.Title = question.Title;
            //this.parentCategory = question.parentCategory;
            this.Source = question.Source;
            this.Status = question.Status;
            this.Created = question.Created;
            this.Modified = question.Modified;
            this.Archived = question.Archived;
            //this.PartitionKey = question.PartitionKey;  
        }

    }
}



