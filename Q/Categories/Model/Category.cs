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
        public string? Link { get; set; }
        public string Header { get; set; }

        public int Kind { get; set; }
        public string? ParentCategory { get; set; }
        public int Level { get; set; }
        public List<string>? Variations { get; set; }
        public int NumOfQuestions { get; set; }
        public bool HasSubCategories { get; set; }

        [JsonProperty(PropertyName = "Questions", NullValueHandling = NullValueHandling.Ignore)]
        public List<Question>? Questions { get; set; }

        [JsonProperty(PropertyName = "HasMoreQuestions", NullValueHandling = NullValueHandling.Ignore)]
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
            var (partitionKey, id, title, link, header, parentCategory, kind, level, variations, categories, questions) = categoryData;

            Type = "category";
            Id = id;
            PartitionKey = partitionKey ?? categoryData.Id;
            Title = title;
            Link = link;
            Header = header ?? ""; 
            Kind = kind;
            ParentCategory = parentCategory;
            Level = (int)level!;
            Variations = variations ?? null;
            NumOfQuestions = questions == null ? 0 : questions.Count;
            HasSubCategories = categories != null && categories.Count > 0;
            Questions = null;
        }

        public Category(CategoryDto categoryDto)
            :base(categoryDto.Created, categoryDto.Modified, null)
        {
            Type = "category";
            Id = categoryDto.Id;
            PartitionKey = categoryDto.PartitionKey ?? categoryDto.Id;
            Title = categoryDto.Title;
            Link = categoryDto.Link;
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
            out string? link,
            out string header,
            out int level, 
            out int kind,
            out bool hasSubCategories,
            out bool? hasMoreQuestions,
            out List<string>? variations,
            out List<Question>? questions)
        {
            partitionKey = PartitionKey;
            id = Id;
            parentCategory = ParentCategory;
            title = Title;
            link = Link;
            header = Header;
            kind = Kind;
            level = Level;
            hasSubCategories = HasSubCategories;
            hasMoreQuestions = HasMoreQuestions;
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



