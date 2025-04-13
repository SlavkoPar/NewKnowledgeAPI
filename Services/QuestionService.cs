using System.Collections.Generic;
using System.Drawing.Printing;
using System.Net;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using NewKnowledgeAPI.Model.Questions;
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


        public string? PartitionKey { get; set; } = null;
        public QuestionService()
        {
        }

        public QuestionService(DbService Db)
        {
            this.Db = Db;
        }
                 
        public async Task<HttpStatusCode> CheckDuplicate(string Title) //QuestionData questionData)
        {
            var sqlQuery = $"SELECT * FROM c WHERE c.Type = 'question' AND c.Title = '{Title}'";
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
            return HttpStatusCode.Found;
        }

        public async Task<QuestionEx?> AddQuestion(QuestionData questionData)
        {
            var myContainer = await container();
            //Console.WriteLine(JsonConvert.SerializeObject(questionData));
            try
            {
                Question question = new(questionData);
                Console.WriteLine("----->>>>> " + JsonConvert.SerializeObject(question));
                // Read the item to see if it exists.  
                await CheckDuplicate(questionData.Title);
                var msg = $":::::: Item in database with Title: {questionData.Title} already exists";
                Console.WriteLine(msg);
                return new QuestionEx(null, msg);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Question question = new(questionData);
                ItemResponse<Question> aResponse =
                    await myContainer.CreateItemAsync(
                        question,
                        new PartitionKey(question.PartitionKey)
                    );
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", aResponse.Resource.Id, aResponse.RequestCharge);
                return new QuestionEx(aResponse.Resource, "");
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
                return new QuestionEx(null, ex.Message);
            }
        }


        public async Task<QuestionEx> GetQuestion(string PartitionKey, string Id)
        {
            var myContainer = await container();

            Question? question = null;
            string msg = "";
            try
            {
                // Read the item to see if it exists.  
                question = await myContainer.ReadItemAsync<Question>(
                    Id,
                    new PartitionKey(PartitionKey)
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
            return new QuestionEx(question, msg);
        }

        public async Task<QuestionEx?> CreateQuestion(QuestionDto questionDto)
        {
            var myContainer = await container();
            try
            {
                // Read the item to see if it exists.  
                HttpStatusCode statusCode = await CheckDuplicate(questionDto.Title);

                /*
                 *  TODO Proveri generisani Id duplicate
                 */

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                //var msg = $"Created item in database with id: {aResponse.Resource.Id} Operation consumed {aResponse.RequestCharge} RUs.\n", , );
                //Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", aResponse.Resource.Id, aResponse.RequestCharge);
                var msg = $"Created item in database with Title: {questionDto.Title}";
                Console.WriteLine(msg);
                return new QuestionEx(null, msg);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Question question = new(questionDto);
                // Create an item in container.Note we provide the value of the partition key for this item
                ItemResponse<Question> aResponse =
                    await myContainer!.CreateItemAsync(
                        question,
                        new PartitionKey(question.PartitionKey)
                    );
                return new QuestionEx(null, $"Item in database with Title: {questionDto.Title} already exists\n");
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
                return new QuestionEx(null, ex.Message);
            }
        }

        public async Task<QuestionEx?> UpdateQuestion(QuestionDto questionDto)
        {
            var myContainer = await container();
            var duplicateTitle = false;
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<Question> aResponse =
                    await myContainer!.ReadItemAsync<Question>(
                        questionDto.Id,
                        new PartitionKey(questionDto.PartitionKey)
                    );
                Question question = aResponse.Resource;

                duplicateTitle = true;
                if (!question.Title.Equals(questionDto.Title, StringComparison.OrdinalIgnoreCase))
                {
                    HttpStatusCode statusCode = await CheckDuplicate(questionDto.Title);
                }

                question.Title = questionDto.Title;
                question.Source = questionDto.Source;
                question.Status = questionDto.Status;

                aResponse = await myContainer.ReplaceItemAsync<Question>(question, question.Id, new PartitionKey(question.PartitionKey));
                Console.WriteLine("Updated Question [{0},{1}].\n \tBody is now: {2}\n", question.Title, question.Id, question);
                new QuestionEx(aResponse.Resource, "");
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                var msg = $"Question item {questionDto.Id} NotFound in database.";
                if (duplicateTitle) {
                    msg = $"Question Title: {questionDto.Title} aleready exists in database.";
                }
                Console.WriteLine(msg); //, aResponse.RequestCharge);
                return new QuestionEx(null, msg);

            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
            return new QuestionEx(null, "Server Problem");
        }

        public async Task<string> DeleteQuestion(QuestionKey questionKey)
        {
            var myContainer = await container();
            try
            {
                // Read the item to see if it exists.

                ItemResponse<Question> aResponse =
                    await myContainer!.ReadItemAsync<Question>(
                        questionKey.Id,
                        new PartitionKey(questionKey.PartitionKey)
                    );
                Question question = aResponse.Resource;
                aResponse = await myContainer.DeleteItemAsync<Question>(question.Id, new PartitionKey(question.PartitionKey));
                Console.WriteLine("Deleted Question [{0},{1}].\n \tBody is now: {2}\n", question.Title, question.Id, question);
                return "OK";
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Question item {0} NotFound in database.\n", questionKey.Id); //, aResponse.RequestCharge);
                return "NotFound";
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
                return ex.Message;
            }
        }

 
        public async Task<QuestionsMore> GetQuestions(string parentCategory, int startCursor, int pageSize, string includeQuestionId)
        {
            var myContainer = await container();
            try
            {
                // OR c.ParentCategory = ''
                string sqlQuery = $"SELECT * FROM c WHERE c.Type = 'question' AND IS_NULL(c.Archived) AND " +
                    $" c.ParentCategory = '{parentCategory}' ORDER BY c.Title OFFSET {startCursor} ";
                sqlQuery += includeQuestionId == "null"
                    ? $"LIMIT {pageSize}"
                    : $"LIMIT 9999";

                Console.WriteLine("************ sqlQuery{0}", sqlQuery);

                int n = 0;
                bool included = false;

                List<Question> questions = [];
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
                        Console.WriteLine("Id je {0}", question.Id);
                        questions.Add(question);
                        n++;
                        if (n >= pageSize && (includeQuestionId == null || included))
                        {
                            return new QuestionsMore(questions, true);
                        }
                    }
                    return new QuestionsMore(questions, false);
                }
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
            return new QuestionsMore([], false);
        }

        public async Task<List<QuestDto>> GetQuests(List<string> words, int count)
        {
            var myContainer = await container();
            try
            {
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
