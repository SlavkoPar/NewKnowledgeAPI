using Knowledge.Services;
using Microsoft.Azure.Cosmos;
using NewKnowledgeAPI.Categories.Model;
using NewKnowledgeAPI.Common;
using NewKnowledgeAPI.Questions;
using NewKnowledgeAPI.Questions.Model;
using System.Net;

namespace NewKnowledgeAPI.Categories
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
                    subCategories.Add(category);
                }
            }
            return subCategories;
        }


        internal async Task<List<Category>> GetSubCategories(string PartitionKey, string parentCategory)
        {
            var myContainer = await container();
            var sqlQuery = $"SELECT * FROM c WHERE c.Type = 'category' AND IS_NULL(c.Archived) AND " 
            + (
                PartitionKey == "null"
                    ? $""
                    : $" c.partitionKey = '{PartitionKey}' AND "
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



        public async Task<Category> GetCategory(string PartitionKey, string Id, bool hidrate, int pageSize, string? includeQuestionId)
        {
            var myContainer = await container();
            try
            {
                // Read the item to see if it exists.  
                //ItemResponse<Category> aResponse =
                Category category = await myContainer!.ReadItemAsync<Category>(Id, new PartitionKey(PartitionKey));
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

        public async Task<HttpStatusCode> CheckDuplicate(string title) //QuestionData questionData)
        {
            var sqlQuery = $"SELECT * FROM c WHERE c.Type = 'category' AND c.Title = '{title}'";
            QueryDefinition queryDefinition = new(sqlQuery);
            FeedIterator<Question> queryResultSetIterator =
                _container!.GetItemQueryIterator<Question>(queryDefinition);
            if (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Question> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                if (currentResultSet.Count == 0)
                {
                    throw new CosmosException("Category Title already exists", HttpStatusCode.NotFound, 0, "0", 0);
                }
            }
            return HttpStatusCode.OK;
        }

        public async Task AddCategory(CategoryData categoryData)
        {
            var myContainer = await container();

            if (categoryData.ParentCategory == null)
            {
                _partitionKey = categoryData.Id;
            }
            categoryData.PartitionKey = _partitionKey;
            // Create a category object 
            if (categoryData.Id == "DOMAIN")
            {
                for (var i = 1; i <= 500; i++)
                    categoryData.Questions!.Add(new QuestionData(categoryData.Id, $"Demo data for DOMAIN " + i.ToString("D3")));
            }

            Category category = new(categoryData);

            try
            {
                // Check if the id already exists
                ItemResponse<Category> aResponse =
                    await myContainer!.ReadItemAsync<Category>(
                        category.Id,
                        new PartitionKey(_partitionKey)
                    );
                Console.WriteLine("Category in database with id: {0} already exists\n", aResponse.Resource.Id);
                // Check if the title already exists
                HttpStatusCode statusCode = await CheckDuplicate(categoryData.Title);
                Console.WriteLine("Category in database with Title: {0} already exists\n", categoryData.Title);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in container.Note we provide the value of the partition key for this item
                ItemResponse<Category> aResponse =
                    await myContainer!.CreateItemAsync(
                        category,
                        new PartitionKey(_partitionKey)
                    );

                if (categoryData.Categories != null)
                {
                    foreach (var subCategoryData in categoryData.Categories)
                    {
                        //subCategoryData.PartitionKey = partitionKey;
                        subCategoryData.ParentCategory = category.Id;
                        subCategoryData.Level = category.Level + 1;
                        await AddCategory(subCategoryData);
                    }
                }
                // questions
                if (categoryData.Questions != null)
                {
                    QuestionService questionService = new(Db!);
                    foreach (var questionData in categoryData.Questions)
                    {
                        questionData.ParentCategory = category.Id;
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
                //category.Created = new WhoWhen(categoryDto.Created!.NickName);
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
                //category.Modified = new WhoWhen(categoryDto.Modified!.NickName);

                aResponse = await myContainer.ReplaceItemAsync(category, category.Id, new PartitionKey(category.PartitionKey));
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

        public async Task<Category> UpdateNumOfQuestions(QuestionDto questionDto, int incr)
        {
            var myContainer = await container();
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<Category> aResponse =
                    await myContainer!.ReadItemAsync<Category>(
                        questionDto.ParentCategory,
                        new PartitionKey(questionDto.PartitionKey)
                    );
                Category category = aResponse.Resource;
                
                // Update the item fields
                if (incr == 1)
                    category.NumOfQuestions++;
                else
                    category.NumOfQuestions--;
                category.Modified = new WhoWhen(questionDto.Modified!);

                aResponse = await myContainer.ReplaceItemAsync(category, category.Id, new PartitionKey(category.PartitionKey));
                Console.WriteLine("Updated Category [{0},{1}].\n \tBody is now: {2}\n", category.Title, category.Id, category);
                return category;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Category item {0}/{1} NotFound in database.\n", questionDto.PartitionKey, questionDto.Id); //, aResponse.RequestCharge);
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
            return null;
        }


        public async Task<string?> getCategoryTitle(Question question)
        {
            var myContainer = await container();
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<Category> aResponse =
                    await myContainer.ReadItemAsync<Category>(
                        question.ParentCategory,
                        new PartitionKey(question.PartitionKey)
                    );
                Category category = aResponse.Resource;
                return category.Title;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Category item {0}/{1} NotFound in database.\n", question.PartitionKey, question.Id); //, aResponse.RequestCharge);
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
            return "Not Found category";
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



