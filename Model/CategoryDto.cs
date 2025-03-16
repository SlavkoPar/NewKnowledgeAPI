using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using System.Net;

namespace Knowledge.Model
{
    public class CategoryDto
    {

        [JsonProperty(PropertyName = "Id")]
        public string Id { get; set; }
        public string PartitionKey { get; set; }
        public string Title { get; set; }
        public int Kind { get; set; }
        public string? ParentCategory { get; set; }
        public int Level { get; set; }
        public IList<string>? Variations { get; set; }
        public int NumOfQuestions { get; set; }
        public bool HasSubCategories { get; set; }
        public WhoWhen Created { get; set; }
        public WhoWhen? Modified { get; set; }
        public WhoWhen? Archived { get; set; }
        public IList<QuestionDto>? Questions { get; set; }
        public bool? HasMoreQuestions { get; set; }

        public  CategoryDto(QuestionsMore questionsMore)
        {
            this.Questions = this.Questions2Dto(questionsMore.questions);
            this.HasMoreQuestions = questionsMore.hasMoreQuestions;
        }

        public CategoryDto(Category category)
        {
            this.Id = category.Id;
            this.PartitionKey = category.PartitionKey!;
            this.Title = category.Title;
            this.Kind = category.Kind;
            this.ParentCategory = category.ParentCategory;
            this.Level = 1;
            this.Variations = category.Variations;
            this.NumOfQuestions = category.NumOfQuestions;
            this.HasSubCategories = category.HasSubCategories;
            this.Created = category.Created;
            this.Modified = category.Modified;
            this.Archived = category.Archived;
            if (category.Questions == null)
            {
                this.Questions = null;
                this.HasMoreQuestions = false;
            }
            else
            {
                //IList<QuestionDto> questions = new List<QuestionDto>();
                //foreach (var question in category.questions)
                //    questions.Add(new QuestionDto(question));
                this.Questions = this.Questions2Dto(category.Questions);
                this.HasMoreQuestions = category.HasMoreQuestions;
            }

        }

        public IList<QuestionDto> Questions2Dto(IList<Question> questions)
        {
            IList<QuestionDto> list = [];
            foreach (var question in questions)
            {
                list.Add(new QuestionDto(question));
            }
            return list;
        }
    }
}



