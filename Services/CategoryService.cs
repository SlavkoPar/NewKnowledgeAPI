using Azure;
using Knowledge.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Net;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Knowledge.Services
{
    public class CategoryService : IDisposable
    {
        public DbService? Db { get; set; } = null;

        private readonly string containerId = "Questions";
        private Container? _container = null;

        public async Task<Container> container()
        {
            _container ??= await Db!.GetContainer(containerId);
            return _container;
        }

        public string? _partitionKey { get; set; } = null;

        public CategoryService()
        {
        }

        //public Category(IConfiguration configuration)
        //{
        //    Category.Db = new Db(configuration);
        //}

        public CategoryService(DbService db)
        {
            Db = db;
        }

        internal async Task<List<Category>> GetAllCategories()
        {
            var myContainer = await container();
            var sqlQuery = "SELECT * FROM c WHERE c.Type = 'category' AND IS_NULL(c.Archived) ORDER BY c.Title ASC";
            QueryDefinition queryDefinition = new(sqlQuery);
            FeedIterator<Category> queryResultSetIterator = myContainer.GetItemQueryIterator<Category>(queryDefinition);
            //List<CategoryDto> subCategories = new List<CategoryDto>();
            List<Category> subCategories = [];
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Category> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Category category in currentResultSet)
                {
                    //subCategories.Add(new CategoryDto(category));
                    subCategories.Add(category);
                }
            }
            return subCategories;
        }


        internal async Task<List<Category>> GetSubCategories(string partitionKey, string parentCategory)
        {
            var myContainer = await container();
            var sqlQuery = $"SELECT * FROM c WHERE c.Type = 'category' AND IS_NULL(c.Archived) AND " 
            + (
                partitionKey == "null"
                    ? $""
                    : $" c.partitionKey = '{partitionKey}' AND "
            )
            + (
                parentCategory == "null"
                    ? $" IS_NULL(c.ParentCategory)"
                    : $" c.ParentCategory = '{parentCategory}'"
            );
            QueryDefinition queryDefinition = new(sqlQuery);
            FeedIterator<Category> queryResultSetIterator = myContainer!.GetItemQueryIterator<Category>(queryDefinition);
            //List<CategoryDto> subCategories = new List<CategoryDto>();
            List<Category> subCategories = [];
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Category> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Category category in currentResultSet)
                {
                    //subCategories.Add(new CategoryDto(category));
                    subCategories.Add(category);
                }
            }
            return subCategories;
        }



        public async Task<Category> GetCategory(string partitionKey, string Id, bool hidrate, int pageSize, string? includeQuestionId)
        {
            var myContainer = await container();
            try
            {
                // Read the item to see if it exists.  
                //ItemResponse<Category> aResponse =
                Category category = await myContainer!.ReadItemAsync<Category>(Id, new PartitionKey(partitionKey));
                if (hidrate && category != null)
                {
                    // hidrate collections except questions, like  category.x = hidrate;  
                    if (pageSize > 0 && category.NumOfQuestions > 0)
                    {
                        var questionService = new QuestionService(Db);
                        QuestionsMore questionsMore = await questionService.GetQuestions(Id, 0, pageSize, includeQuestionId);
                        category.Questions = questionsMore.questions;
                        category.HasMoreQuestions = questionsMore.hasMoreQuestions;
                    }
                }
                return category;
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public async Task AddCategory(CategoryData categoryData)
        {
            var myContainer = await container();

            if (categoryData.parentCategory == null)
            {
                _partitionKey = categoryData.id;
            }
            categoryData.PartitionKey = _partitionKey;
            // Create a category object 
            if (categoryData.id == "DOMAIN")
            {
                for (var i = 1; i <= 500; i++)
                    categoryData.questions!.Add(new QuestionData(categoryData.id, $"Demo data for DOMAIN " + i.ToString("D3")));
            }

            Category category = new(categoryData);
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<Category> aResponse =
                    await myContainer!.ReadItemAsync<Category>(
                        category.Id,
                        new PartitionKey(_partitionKey)
                    );
                Console.WriteLine("Item in database with id: {0} already exists\n", aResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in container.Note we provide the value of the partition key for this item
                ItemResponse<Category> aResponse =
                    await myContainer!.CreateItemAsync(
                        category,
                        new PartitionKey(_partitionKey)
                    );

                if (categoryData.categories != null)
                {
                    foreach (var subCategoryData in categoryData.categories)
                    {
                        //subCategoryData.PartitionKey = partitionKey;
                        subCategoryData.parentCategory = category.Id;
                        subCategoryData.level = category.Level + 1;
                        await AddCategory(subCategoryData);
                    }
                }
                // questions
                if (categoryData.questions != null)
                {
                    QuestionService questionService = new(Db!);
                    foreach (var questionData in categoryData.questions)
                    {
                        questionData.parentCategory = category.Id;
                        await questionService.AddQuestion(questionData);
                    }
                }
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", aResponse.Resource.Id, aResponse.RequestCharge);
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<Category> CreateCategory(CategoryDto categoryDto)
        {
            var myContainer = await container();
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<Category> aResponse =
                    await myContainer!.ReadItemAsync<Category>(
                        categoryDto.Id,
                        new PartitionKey(categoryDto.PartitionKey)
                    );
                Console.WriteLine("Item in database with id: {0} already exists\n", aResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Category category = new(categoryDto);
                // Create an item in container.Note we provide the value of the partition key for this item
                ItemResponse<Category> aResponse =
                    await myContainer!.CreateItemAsync(category, new PartitionKey(categoryDto.PartitionKey));
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", aResponse.Resource.Id, aResponse.RequestCharge);
                return category;
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public async Task<Category> UpdateCategory(CategoryDto categoryDto)
        {
            var myContainer = await container();
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<Category> aResponse =
                    await myContainer!.ReadItemAsync<Category>(
                        categoryDto.Id,
                        new PartitionKey(categoryDto.PartitionKey)
                    );
                Category category = aResponse.Resource;
                // Update the item fields
                category.Title = categoryDto.Title;
                category.Kind = categoryDto.Kind;
                category.Variations = categoryDto.Variations;
                category.Modified = new WhoWhen(categoryDto.Modified!.nickName);

                aResponse = await myContainer.ReplaceItemAsync<Category>(category, category.Id, new PartitionKey(category.PartitionKey));
                Console.WriteLine("Updated Category [{0},{1}].\n \tBody is now: {2}\n", category.Title, category.Id, category);
                return category;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Category item {0} NotFound in database.\n", categoryDto.Id); //, aResponse.RequestCharge);
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public async Task<string> DeleteCategory(CategoryKey categoryKey)
        {
            var myContainer = await container();
            try
            {
                // Read the item to see if it exists.
                
                ItemResponse<Category> aResponse =
                    await myContainer!.ReadItemAsync<Category>(
                        categoryKey.Id,
                        new PartitionKey(categoryKey.PartitionKey)
                    );
                Category category = aResponse.Resource;
                if (category.HasSubCategories)
                    return "HasSubCategories";
                if (category.NumOfQuestions > 0)
                    return "NumOfQuestions";
                aResponse = await myContainer.DeleteItemAsync<Category>(category.Id, new PartitionKey(category.PartitionKey));
                Console.WriteLine("Deleted Category [{0},{1}].\n \tBody is now: {2}\n", category.Title, category.Id, category);
                return "OK";
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Category item {0} NotFound in database.\n", categoryKey.Id); //, aResponse.RequestCharge);
                return  "NotFound";
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
                return ex.Message;
            }
        }

        public void Dispose()
        {
            _container = null;
            Db = null;
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



