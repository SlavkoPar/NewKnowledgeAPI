using System.Drawing.Printing;
using System.Net;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;


namespace Knowledge.Model
{
    public class Question : IDisposable
    {
        public string Type { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }
        public string Title { get; set; }
        public string? ParentCategory { get; set; }
        public List<long> AssignedAnswers { get; set; }
        public int Source { get; set; }
        public int Status { get; set; }

        public WhoWhen Created { get; set; }
        public WhoWhen? Modified { get; set; }
        public WhoWhen? Archived { get; set; }

        public Question()
        {
        }

  
        public Question(QuestionData questionData)
        {
            string s = DateTime.Now.Ticks.ToString();
            Type = "question";
            Id = s.Substring(s.Length-10);// Guid.NewGuid().ToString();
            PartitionKey = questionData.parentCategory!;
            Title = questionData.title;
            //words =
            //    categoryData.title
            //        .ToLower()
            //        .Replace("?", "")
            //        .Split(' ', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries)
            //        .Where(w => w.Length > 1)
            //        .ToList();
            ParentCategory = questionData.parentCategory;
            AssignedAnswers = [];
            Created = new WhoWhen("Admin");
            Modified = null;
            Archived = null;
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
