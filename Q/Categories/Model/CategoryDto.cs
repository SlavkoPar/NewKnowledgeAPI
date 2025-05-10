using NewKnowledgeAPI.Common;
using NewKnowledgeAPI.Q.Questions.Model;
using Newtonsoft.Json;
using System.Net;

namespace NewKnowledgeAPI.Q.Categories.Model
{
    public class CategoryDto : RecordDto
    {
        [JsonProperty(PropertyName = "Id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "PartitionKey")]
        public string? PartitionKey { get; set; }

        public string Title { get; set; }
        public int Kind { get; set; }
        public string? ParentCategory { get; set; }
        public int Level { get; set; }
        public List<string>? Variations { get; set; }
        public int? NumOfQuestions { get; set; }
        public bool? HasSubCategories { get; set; }
        
        public List<QuestionDto>? Questions { get; set; }
        public bool? HasMoreQuestions { get; set; }

        public CategoryDto()
            : base()
        {
        }
      

        public CategoryDto(CategoryKey categoryKey, QuestionsMore questionsMore)
            : base() // TODO
            //: base(null, null, null) // TODO prosledi 
        {
            var (partitionKey, id) = categoryKey;
            Id = id;
            PartitionKey = partitionKey;
            Title = "deca";
            Kind = 1;
            Level = 1;
            Variations = [];

            //Console.WriteLine("pitanja {0}", questionsMore.questions.Count);
            //if (questionsMore.questions.Count > 0) {
            //    Question q = questionsMore.questions.First();
            //}
            Questions = Questions2Dto(questionsMore.QuestionRows.Select(row => new Question(row)).ToList());
            HasMoreQuestions = questionsMore.HasMoreQuestions;
        }

        public CategoryDto(Category category)
            : base(category.Created, category.Modified)
        {
            Id = category.Id;
            PartitionKey = category.PartitionKey!;
            Title = category.Title;
            Kind = category.Kind;
            ParentCategory = category.ParentCategory;
            Level = category.Level;
            Variations = category.Variations;
            NumOfQuestions = category.NumOfQuestions;
            HasSubCategories = category.HasSubCategories;
            if (category.Questions == null)
            {
                Questions = null;
                HasMoreQuestions = false;
            }
            else
            {
                //IList<QuestionDto> questions = new List<QuestionDto>();
                //foreach (var question in category.questions)
                //    questions.Add(new QuestionDto(question));
                Questions = Questions2Dto(category.Questions!);
                HasMoreQuestions = category.HasMoreQuestions;
            }
        }

        public List<QuestionRowDto> Questions2Dto(List<QuestionRow> questionRows)
        {
            List<QuestionRowDto> list = [];
            foreach (var questionRow in questionRows)
            {
                //Console.WriteLine(JsonConvert.SerializeObject(question));
                list.Add(new QuestionRowDto(questionRow));
            }
            return list;
        }

        public List<QuestionDto> Questions2Dto(List<Question> questions)
        {
            List<QuestionDto> list = [];
            foreach (var question in questions)
            {
                //Console.WriteLine(JsonConvert.SerializeObject(question));
                list.Add(new QuestionDto(question));
            }
            return list;
        }

        public void Deconstruct(out string partitionKey, out string id)
        {
            partitionKey = PartitionKey;
            id = Id;
        }

        public void Deconstruct(out string partitionKey, out string id, out string parentCategory, out string title, out int level, out int kind, out List<string>? variations)
        {
            partitionKey = PartitionKey;
            id = Id;
            parentCategory = ParentCategory;
            title = Title;
            kind = Kind;
            level = Level;
            variations = Variations;
        }

    }
}



