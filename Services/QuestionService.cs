using System.Collections.Generic;
using System.Drawing.Printing;
using System.Net;
using Knowledge.Model;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;


namespace Knowledge.Services
{
    public class QuestionService : IDisposable
    {
        public DbService? Db { get; set; } = null;

        private readonly string containerId = "Questions";
        private Container? _container = null;

        public async Task<Container> container()
        {
            _container ??= await Db!.GetContainer(containerId);
            return _container;
        }


        public string? partitionKey { get; set; } = null;
        public QuestionService()
        {
        }

        public QuestionService(DbService Db)
        {
            this.Db = Db;
        }

         
        public async Task<HttpStatusCode> CheckDuplicate(QuestionData questionData)
        {
            var sqlQuery = $"SELECT * FROM c WHERE c.Type = 'question' AND c.Title = '{questionData.title}'";
            QueryDefinition queryDefinition = new(sqlQuery);
            FeedIterator<Question> queryResultSetIterator =
                _container!.GetItemQueryIterator<Question>(queryDefinition);
            if (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Question> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                if (currentResultSet.Count == 0)
                {
                    throw new CosmosException("Question Title already exists", HttpStatusCode.NotFound, 0, "0", 0);
                }
            }
            return HttpStatusCode.OK;
        }

        public async Task AddQuestion(QuestionData questionData)
        {
            var myContainer = await container();
            Question question = new(questionData);
            try
            {
                // Read the item to see if it exists.  
                HttpStatusCode statusCode = await CheckDuplicate(questionData);
                Console.WriteLine("Item in database with id: {0} already exists\n", statusCode);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in container.Note we provide the value of the partition key for this item
                ItemResponse<Question> aResponse =
                    await myContainer!.CreateItemAsync(
                        question,
                        new PartitionKey(question.PartitionKey)
                    );

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", aResponse.Resource.Id, aResponse.RequestCharge);
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<Question> GetQuestion(string partitionKey, string id)
        {
            var myContainer = await container();

            Question? question = null;
            try
            {
                // Read the item to see if it exists.  
                question = await myContainer!.ReadItemAsync<Question>(
                    id,
                    new PartitionKey(partitionKey)
                );
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
            return question;
        }

        public async Task<QuestionsMore> GetQuestions(string parentCategory, int startCursor, int pageSize, string includeQuestionId)
        {
            var myContainer = await container();

            List<Question> questions = [];
            bool hasMore = false;
            try
            {
                // OR c.ParentCategory = ''
                var sqlQuery = $"SELECT * FROM c WHERE c.Type = 'question' AND IS_NULL(c.Archived) AND " +
                    $" c.ParentCategory = '{parentCategory}' ORDER BY c.title OFFSET {startCursor} ";
                sqlQuery += includeQuestionId == null
                    ? $"LIMIT {pageSize}"
                    : $"LIMIT 9999";

                int n = 0;
                bool included = false;

                QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
                FeedIterator<Question> queryResultSetIterator = myContainer!.GetItemQueryIterator<Question>(queryDefinition);
                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Question> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (Question question in currentResultSet)
                    {
                        if (includeQuestionId != null && question.Id == includeQuestionId)
                        {
                            included = true;
                        }
                        questions.Add(question);
                        n++;
                        if (n >= pageSize && (includeQuestionId == null || included))
                        {
                            hasMore = true;
                            return new QuestionsMore(questions, hasMore);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
            return new QuestionsMore(questions, hasMore);
        }

        public async Task<List<QuestDto>> GetQuests(List<string> words, int count)
        {
            var myContainer = await container();
            try
            {
                //SELECT c.Title, c.id 
                //    FROM c 
                //    WHERE c.Type = 'question' AND IS_NULL(c.Archived) AND c.ParentCategory = 'DOMAIN' AND CONTAINS(c.Title, "500") 
                //    ORDER BY c.Title OFFSET 0 LIMIT 30
                var sqlQuery = $"SELECT c.ParentCategory, c.Title, c.id FROM c WHERE c.Type = 'question' AND IS_NULL(c.Archived) AND ";
                if (words.Count == 1)
                {
                    sqlQuery += $" CONTAINS(c.Title, \"{words[0]}\", true) ";
                }
                else
                {
                    sqlQuery += "(";
                    for (var i=0; i < words.Count; i++)
                    {
                        if (i > 0)
                            sqlQuery += " OR ";
                        sqlQuery += $" CONTAINS(c.Title, \"{words[i]}\", true) ";
                    }
                    sqlQuery += ")";

                }
                sqlQuery += $" ORDER BY c.Title OFFSET 0 LIMIT {count}";


                List<QuestDto> quests = [];
                QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
                using (FeedIterator<Question> queryResultSetIterator = 
                    myContainer!.GetItemQueryIterator<Question>(queryDefinition))
                {
                    while (queryResultSetIterator.HasMoreResults)
                    {
                        FeedResponse<Question> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                        foreach (Question question in currentResultSet)
                        {
                            quests.Add(new QuestDto(question));
                        }
                    }
                }
                return quests;
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
            return [];
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
