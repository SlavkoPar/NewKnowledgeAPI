using NewKnowledgeAPI.Model.Questions;
using System.Diagnostics.Metrics;


namespace NewKnowledgeAPI.Model.Categories
{
    public class CategoryData
    {
        public string? ParentCategory { get; set; }
        public string Id { get; set; }
        public string? PartitionKey { get; set; }
        public string Title { get; set; }
        public int Kind { get; set; }
        public int? Level { get; set; }
        public IList<string>? Variations { get; set; }
        public IList<CategoryData>? Categories { get; set; }
        public IList<QuestionData>? Questions { get; set; }
    }

    public class CategoriesData
    {
        public List<CategoryData> Categories { get; set; }
    }
}
