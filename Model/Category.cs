using Azure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Runtime.CompilerServices;

namespace Knowledge.Model
{
    public class Category : IDisposable
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
        public WhoWhen Created { get; set; }
        public WhoWhen? Modified { get; set; }
        public WhoWhen? Archived { get; set; }
        public IList<Question>? Questions { get; set; }
        public bool? HasMoreQuestions { get; set; }

        public Category()
        {
        }

        public Category(CategoryData categoryData)
        {
            Type = "category";
            Id = categoryData.id;
            PartitionKey = categoryData.PartitionKey!;
            Title = categoryData.title;
            //this.words =
            //    categoryData.title
            //        .ToLower()
            //        .Replace("?", "")
            //        .Split(' ', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries)
            //        .Where(w => w.Length > 1)
            //        .ToList();
            Kind = categoryData.kind;
            ParentCategory = categoryData.parentCategory;
            Level = (int)categoryData.level;
            Variations = categoryData.variations ?? [];
            NumOfQuestions = categoryData.questions == null ? 0 : categoryData.questions.Count;
            HasSubCategories = categoryData.categories != null && categoryData.categories.Count > 0;
            Created = new WhoWhen("Admin");
            Modified = null;
            Archived = null;
            Questions = null;
        }

        public Category(CategoryDto categoryDto)
        {
            Type = "category";
            Id = categoryDto.Id;
            PartitionKey = categoryDto.PartitionKey!;
            Title = categoryDto.Title;
            Kind = categoryDto.Kind;
            ParentCategory = categoryDto.ParentCategory;
            Level = (int)categoryDto.Level;
            Variations = categoryDto.Variations ?? [];
            NumOfQuestions = 0;
            HasSubCategories = false;
            Created = new WhoWhen(categoryDto.Created!.nickName); ;
            Archived = null;
            Questions = null;
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



