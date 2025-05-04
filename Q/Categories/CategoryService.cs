using Azure;
using Knowledge.Services;
using Microsoft.Azure.Cosmos;
using NewKnowledgeAPI.Common;
using NewKnowledgeAPI.Q.Categories.Model;
using NewKnowledgeAPI.Q.Questions;
using NewKnowledgeAPI.Q.Questions.Model;
using Newtonsoft.Json;
using System.Net;

namespace NewKnowledgeAPI.Q.Categories
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


        internal async Task<List<Category>> GetSubCategories(string PartitionKey, string id)
        {
            var myContainer = await container();
            var sqlQuery = $"SELECT * FROM c WHERE c.Type = 'category' AND IS_NULL(c.Archived) AND "
            // for categories partitionKey is same as Id
            //+ (
            //    PartitionKey == "null"
            //        ? $""
            //        : $" c.partitionKey = '{PartitionKey}' AND "  
            //)
            + (
                id == "null"
                    ? $" IS_NULL(c.ParentCategory)"
                    : $" c.ParentCategory = '{id}'"
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

        public async Task<CategoryEx> GetCategory(CategoryKey categoryKey, bool hidrate, int pageSize, string? includeQuestionId)
        {
            var (PartitionKey, Id) = categoryKey;
            var myContainer = await container();
            try
            {
                // Read the item to see if it exists.  
                //ItemResponse<Category> aResponse =
                Category category = await myContainer!.ReadItemAsync<Category>(Id, new PartitionKey(PartitionKey));
                Console.WriteLine(JsonConvert.SerializeObject(category));

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
                return new CategoryEx(category, "");
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
                return new CategoryEx(null, ex.Message);
            }
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
            var (PartitionKey, Id, Title, ParentCategory, Kind, Level, Variations, Categories, Questions) = categoryData;
            //Console.WriteLine(JsonConvert.SerializeObject(categoryData));
            var myContainer = await container();

            if (Questions != null && Id == "DOMAIN")
            {
                for (var i = 1; i <= 500; i++)
                    Questions!.Add(new QuestionData(Id, $"Demo data for DOMAIN " + i.ToString("D3")));
            }

            try
            {
                Category c = new(categoryData);
                CategoryEx categoryEx = await AddNewCategory(c);
                if (categoryEx.category != null)
                {
                    Category category = categoryEx.category;
                    if (Categories != null)
                    {
                        foreach (var subCategoryData in Categories)
                        {
                            subCategoryData.PartitionKey = subCategoryData.Id;
                            subCategoryData.ParentCategory = category.Id;
                            subCategoryData.Level = category.Level + 1;
                            await AddCategory(subCategoryData);
                        }
                    }
                    if (Questions != null)
                    {
                        QuestionService questionService = new(Db!);
                        foreach (var questionData in Questions)
                        {
                            questionData.ParentCategory = category.Id;
                            await questionService.AddQuestion(questionData);
                        }
                    }
                    // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                    // Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", aResponse.Resource.Id, aResponse.RequestCharge);
                }
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<CategoryEx> AddNewCategory(Category category)
        {
            var (PartitionKey, Id, Title, ParentCategory, Kind, Level, Variations, Questions) = category;
            var myContainer = await container();
            string msg = string.Empty;
            try
            {
                // Check if the id already exists
                ItemResponse<Category> aResponse =
                    await myContainer!.ReadItemAsync<Category>(
                        Id,
                        new PartitionKey(PartitionKey)
                    );
                msg = $"Category in database with Id: {Id} already exists"; //, aResponse.Resource.Id
                Console.WriteLine(msg);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                try
                {
                    // Check if the title already exists
                    HttpStatusCode statusCode = await CheckDuplicate(Title);
                    msg = $"Category in database with Title: {Title} already exists";
                    Console.WriteLine(msg);
                }
                catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    // Create an item in container.Note we provide the value of the partition key for this item
                    ItemResponse<Category> aResponse =
                        await myContainer!.CreateItemAsync(
                            category,
                            new PartitionKey(PartitionKey)
                        );
                    // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                    Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", aResponse.Resource.Id, aResponse.RequestCharge);
                    return new CategoryEx(aResponse.Resource, "");
                }
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
                msg = ex.Message;
            }
            return new CategoryEx(null, msg);
        }

        public async Task<CategoryEx> CreateCategory(CategoryDto categoryDto)
        {
            var (Id, PartitionKey) = categoryDto;
            var myContainer = await container();
            var c = new Category(categoryDto);
            CategoryEx categoryEx = await AddNewCategory(c);

            // update parentCategory
            categoryDto.Modified = categoryDto.Archived;
            await UpdateHasSubCategories(categoryDto);

            return categoryEx;
        }



        public async Task<CategoryEx> UpdateCategory(CategoryDto categoryDto)
        {
            var myContainer = await container();
            string msg = string.Empty;
            try
            {
                var (PartitionKey, Id, ParentCategory, Title, Level, Kind, Variations) = categoryDto;
                // Read the item to see if it exists.  
                ItemResponse<Category> aResponse =
                    await myContainer!.ReadItemAsync<Category>(
                        Id,
                        new PartitionKey(PartitionKey)
                    );
                Category category = aResponse.Resource;
                // Update the item fields
                category.Title = Title;
                category.Kind = Kind;
                category.Variations = Variations;
                category.ParentCategory = ParentCategory;
                //category.Modified = new WhoWhen(categoryDto.Modified!.NickName);

                aResponse = await myContainer.ReplaceItemAsync(category, category.Id, new PartitionKey(category.PartitionKey));
                Console.WriteLine("Updated Category [{0},{1}].\n \tBody is now: {2}\n", category.Title, category.Id, category);

                // update parentCategory
                categoryDto.Modified = categoryDto.Archived;
                await UpdateHasSubCategories(categoryDto);

                return new CategoryEx(category, msg);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                msg = $"Category Id: {categoryDto.Id} NotFound in database."; //, aResponse.RequestCharge);
                Console.WriteLine(msg); //, aResponse.RequestCharge);
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                msg = ex.Message;
                Console.WriteLine(ex.Message);
            }
            return new CategoryEx(null, msg);
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

        public async Task<Category> UpdateHasSubCategories(CategoryDto categoryDto)
        {
            var (PartitionKey, Id, ParentCategory, Title, Level, Kind, Variations) = categoryDto;
            var myContainer = await container();
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<Category> aResponse =
                    await myContainer!.ReadItemAsync<Category>(
                        ParentCategory,
                        new PartitionKey(PartitionKey)
                    );
                Category category = aResponse.Resource;

                var sql = $"SELECT value count(1) FROM c WHERE c.Type = 'category' " +
                    "AND c.partitionKey='{PartitionKey} " +
                    "AND Parentcategory='{ParentCategory}' " + 
                    "AND IS_NULL(c.Archived)";
                int num = await CountItems(myContainer, sql);
                Console.WriteLine($"============================ num: {num}");

                category.HasSubCategories = num > 0;
                category.Modified = new WhoWhen(categoryDto.Modified!);

                aResponse = await myContainer.ReplaceItemAsync(category, category.Id, new PartitionKey(category.PartitionKey));
                Console.WriteLine("Updated Category [{0},{1}].\n \tBody is now: {2}\n", category.Title, category.Id, category);
                return category;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Category item {0}/{1} NotFound in database.\n", categoryDto.PartitionKey, categoryDto.Id); //, aResponse.RequestCharge);
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
            return null;
        }


        public async Task<int> CountItems(Container myContainer, string sqlQuery)
        {
            int count = 0;
            var query = myContainer.GetItemQueryIterator<int>(new QueryDefinition(sqlQuery));
            while (query.HasMoreResults)
            {
                FeedResponse<int> response = await query.ReadNextAsync();
                count += response.Resource.FirstOrDefault();
            }
            return count;
        }

        public async Task<CategoryEx> GetCategory(CategoryKey categoryKey)
        {
            var (partitionKey, id) = categoryKey;
            string msg = string.Empty;
            var myContainer = await container();
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<Category> aResponse =
                    await myContainer.ReadItemAsync<Category>(
                        id,
                        new PartitionKey(partitionKey)
                    );
                Category category = aResponse.Resource;
                return new CategoryEx(category, msg);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                msg = $"Category {partitionKey}/{id} NotFound in database.";
                Console.WriteLine(msg); //, aResponse.RequestCharge);
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
            return new CategoryEx(null, msg);
        }

        public async Task<CategoryEx> DeleteCategory(CategoryDto categoryDto)
        {
            var myContainer = await container();
            string msg = string.Empty;
            try
            {
                // Read the item to see if it exists.
                
                ItemResponse<Category> aResponse =
                    await myContainer!.ReadItemAsync<Category>(
                        categoryDto.Id,
                        new PartitionKey(categoryDto.PartitionKey)
                    );
                Category category = aResponse.Resource;
                category.Archived = new WhoWhen(categoryDto.Archived!.NickName);
                aResponse = await myContainer.ReplaceItemAsync(category, category.Id, new PartitionKey(category.PartitionKey));
                msg = $"Updated Question {category.PartitionKey}/{category.Id}. {category.Title}";
                Console.WriteLine(msg);

                // update parentCategory
                categoryDto.Modified = categoryDto.Archived;
                await UpdateHasSubCategories(categoryDto);

                return new CategoryEx(aResponse.Resource, msg);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                msg =$"Category {categoryDto.Id} NotFound in database."; //, aResponse.RequestCharge);
                Console.WriteLine(msg);
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
                msg = ex.Message;
            }
            return new CategoryEx(null, msg);
        }

        public async Task<CategoryListEx> GetCatsUpTheTree(CategoryKey categoryKey)
        {
            string message = string.Empty;
            try {
                Category? category;
                string? parentCategory;
                List<Category> list = [];
                do
                {
                    CategoryEx categoryEx = await GetCategory(categoryKey, false, 0, null);
                    Console.WriteLine("---------------------------------------------------");
                    Console.WriteLine(JsonConvert.SerializeObject(categoryEx)); 
                    category = categoryEx.category;
                    if (category != null)
                    {
                        list.Add(category);
                        parentCategory = category.ParentCategory;
                        // partitionKey is the same as Id
                        categoryKey = new CategoryKey(category.ParentCategory, category.ParentCategory);
                    }
                    else
                    {
                        message = categoryEx.msg;
                        parentCategory = null;
                    }
                } while (parentCategory != null);
                return new CategoryListEx(list, message);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return new CategoryListEx(null, message);
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



