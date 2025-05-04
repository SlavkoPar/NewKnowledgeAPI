using NewKnowledgeAPI.Common;
using NewKnowledgeAPI.Q.Questions.Model;
using Newtonsoft.Json;

namespace NewKnowledgeAPI.Q.Categories.Model
{
    public class Category : Record, IDisposable
    {
        public string Type { get; set; }
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }
        public string Title { get; set; }
        public int Kind { get; set; }
        public string? ParentCategory { get; set; }
        public int Level { get; set; }
        public List<string>? Variations { get; set; }
        public int NumOfQuestions { get; set; }
        public bool HasSubCategories { get; set; }
        public List<Question>? Questions { get; set; }
        public bool? HasMoreQuestions { get; set; }

        public Category()
            : base()
        {
        }

        public Category(Question question)
          : base()
        {
            Id = question.ParentCategory!;
            PartitionKey = question.PartitionKey;
        }


        public Category(CategoryData categoryData)
            : base(new WhoWhen("Admin"), null, null)
        {
            Type = "category";
            Id = categoryData.Id;
            PartitionKey = categoryData.PartitionKey ?? categoryData.Id;
            Title = categoryData.Title;
            Kind = categoryData.Kind;
            ParentCategory = categoryData.ParentCategory;
            Level = (int)categoryData.Level!;
            Variations = categoryData.Variations ?? null;
            NumOfQuestions = categoryData.Questions == null ? 0 : categoryData.Questions.Count;
            HasSubCategories = categoryData.Categories != null && categoryData.Categories.Count > 0;
            Questions = null;
        }

        public Category(CategoryDto categoryDto)
            :base(categoryDto.Created, categoryDto.Modified, categoryDto.Archived)
        {
            Type = "category";
            Id = categoryDto.Id;
            PartitionKey = categoryDto.PartitionKey ?? categoryDto.Id;
            Title = categoryDto.Title;
            Kind = categoryDto.Kind;
            ParentCategory = categoryDto.ParentCategory;
            Level = categoryDto.Level;
            Variations = categoryDto.Variations ?? null;
            Questions = null;
            NumOfQuestions = 0;
            HasSubCategories = false;
        }

        //public override string ToString() =>
        //    $"{PartitionKey}/{Id} : {Title}";


        public void Deconstruct(
            out string partitionKey,
            out string id, 
            out string parentCategory, 
            out string title, 
            out int level, 
            out int kind, 
            out List<string>? variations,
            out List<Question>? questions)
        {
            partitionKey = PartitionKey;
            id = Id;
            parentCategory = ParentCategory;
            title = Title;
            kind = Kind;
            level = Level;
            variations = Variations;
            questions = Questions;
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



