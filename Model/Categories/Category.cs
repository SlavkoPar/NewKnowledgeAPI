using Azure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using NewKnowledgeAPI.Model.Common;
using NewKnowledgeAPI.Model.Questions;
using Newtonsoft.Json;
using System.Net;
using System.Runtime.CompilerServices;

namespace NewKnowledgeAPI.Model.Categories
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
        public IList<string>? Variations { get; set; }
        public int NumOfQuestions { get; set; }
        public bool HasSubCategories { get; set; }
        public List<Question>? Questions { get; set; }
        public bool? HasMoreQuestions { get; set; }

        public Category()
            : base()
        {
        }

        public Category(CategoryData categoryData)
            : base(new WhoWhen("Admin"), null, null)
        {
            Type = "category";
            Id = categoryData.Id;
            PartitionKey = categoryData.PartitionKey!;
            Title = categoryData.Title;
            Kind = categoryData.Kind;
            ParentCategory = categoryData.ParentCategory;
            Level = (int)categoryData.Level;
            Variations = categoryData.Variations ?? [];
            NumOfQuestions = categoryData.Questions == null ? 0 : categoryData.Questions.Count;
            HasSubCategories = categoryData.Categories != null && categoryData.Categories.Count > 0;
            Questions = null;
        }

        public Category(CategoryDto categoryDto)
            :base(categoryDto.Created, categoryDto.Modified, categoryDto.Archived)
        {
            Type = "category";
            Id = categoryDto.Id;
            PartitionKey = categoryDto.PartitionKey!;
            Title = categoryDto.Title;
            Kind = categoryDto.Kind;
            ParentCategory = categoryDto.ParentCategory;
            Level = categoryDto.Level;
            Variations = categoryDto.Variations ?? [];
            Questions = null;
            NumOfQuestions = 0;
            HasSubCategories = false;
        }

        //public override string ToString() =>
        //    $"{PartitionKey}/{Id} : {Title}";


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



