using System.Diagnostics.Metrics;


namespace NewKnowledgeAPI.Model.Questions
{
    public class QuestionData
    {
        public string? ParentCategory { get; set; }
        public string? Id { get; set; }
        //public string? PartitionKey { get; set; }

        public string Title { get; set; }
        public IList<int>? AssignedAnswers { get; set; }
        public int? Source { get; set; }
        public int? Status { get; set; }

        public QuestionData() { 
        }

        public QuestionData(string ParentCategory, string Title)
        {
            this.ParentCategory = ParentCategory;
            this.Title = Title; 
        }
    }

}
