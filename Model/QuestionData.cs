using System.Diagnostics.Metrics;


namespace Knowledge.Model
{
    public class QuestionData
    {
        public string? parentCategory { get; set; }
        public string? id { get; set; }
        //public string? PartitionKey { get; set; }

        public string title { get; set; }
        public IList<int>? assignedAnswers { get; set; }
        public int? source { get; set; }
        public int? status { get; set; }

        public QuestionData() { 
        }

        public QuestionData(string parentCategory, string title)
        {
            this.parentCategory = parentCategory;
            this.title = title; 
        }
    }

}
