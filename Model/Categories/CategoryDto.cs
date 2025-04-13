using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using NewKnowledgeAPI.Model.Common;
using NewKnowledgeAPI.Model.Questions;
using Newtonsoft.Json;
using System.Net;

namespace NewKnowledgeAPI.Model.Categories
{
    public class CategoryDto : RecordDto
    {
        [JsonProperty(PropertyName = "Id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "PartitionKey")]
        public string PartitionKey { get; set; }
        public string Title { get; set; }
        public int Kind { get; set; }
        public string? ParentCategory { get; set; }
        public int Level { get; set; }
        public IList<string> Variations { get; set; }
        public int? NumOfQuestions { get; set; }
        public bool? HasSubCategories { get; set; }
        
        public IList<QuestionDto>? Questions { get; set; }
        public bool? HasMoreQuestions { get; set; }

        public CategoryDto()
            : base()
        {
        }

        public CategoryDto(string parentCategory, QuestionsMore questionsMore)
            : base() // TODO
            //: base(null, null, null) // TODO prosledi 
        {
            this.Id = parentCategory;
            this.PartitionKey = "Doesn't matter";
            this.Title = "deca";
            this.Kind = 1;
            this.Level = 1;
            this.Variations = [];

            Console.WriteLine("pitanja {0}", questionsMore.questions.Count);
            //if (questionsMore.questions.Count > 0) {
            //    Question q = questionsMore.questions.First();
            //}
            Questions = Questions2Dto(questionsMore.questions);
            HasMoreQuestions = questionsMore.hasMoreQuestions;
        }

        public CategoryDto(Category category)
            : base(category.Created, category.Modified, category.Archived)
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

        public List<QuestionDto> Questions2Dto(List<Question> questions)
        {
            List<QuestionDto> list = [];
            foreach (var question in questions)
            {
                Console.WriteLine(JsonConvert.SerializeObject(question));
                list.Add(new QuestionDto(question));
            }
            return list;
        }
    }
}



