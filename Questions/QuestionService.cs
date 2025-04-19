using System.Net;
using Knowledge.Services;
using Microsoft.Azure.Cosmos;
using NewKnowledgeAPI.Categories.Model;
using NewKnowledgeAPI.Common;
using NewKnowledgeAPI.Questions.Model;
using Newtonsoft.Json;


namespace NewKnowledgeAPI.Questions
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
                 
        public async Task<HttpStatusCode> CheckDuplicate(string? Title, string? Id = null)
        {

            var sqlQuery = Title != null
                ? $"SELECT * FROM c WHERE c.Type = 'question' AND c.Title = '{Title}' AND IS_NULL(c.Archived)"
                : $"SELECT * FROM c WHERE c.Type = 'question' AND c.Id = '{Id}' AND IS_NULL(c.Archived)";
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
            string msg = string.Empty;
            try
            {
                Question question = new(questionData);
                Console.WriteLine("----->>>>> " + JsonConvert.SerializeObject(question));
                // Read the item to see if it exists.  
                await CheckDuplicate(questionData.Title);
                msg = $":::::: Item in database with Title: {questionData.Title} already exists";
                Console.WriteLine(msg);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Question q = new(questionData);
                QuestionEx questionEx = await AddNewQuestion(q);
                return questionEx;
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                msg = ex.Message;
                Console.WriteLine(msg);
            }
            return new QuestionEx(null, msg);
        }


        public async Task<QuestionEx> AddNewQuestion(Question question)
        {
            var (PartitionKey, Id, Title, ParentQuestion, Kind, Level, Variations, Questions) = question;
            var myContainer = await container();
            string msg = string.Empty;
            try
            {
                // Check if the id already exists
                ItemResponse<Question> aResponse =
                    await myContainer!.ReadItemAsync<Question>(
                        Id,
                        new PartitionKey(PartitionKey)
                    );
                msg = $"Question in database with id: {Id} already exists\n";
                Console.WriteLine(msg);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                try
                {
                    // Check if the title already exists
                    HttpStatusCode statusCode = await CheckDuplicate(Title);
                    msg = $"Question in database with Title: {Title} already exists";
                    Console.WriteLine(msg);
                }
                catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    ItemResponse<Question> aResponse =
                    await myContainer!.CreateItemAsync(
                            question,
                            new PartitionKey(PartitionKey)
                        );
                    // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                    Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", aResponse.Resource.Id, aResponse.RequestCharge);
                    return new QuestionEx(aResponse.Resource, "");
                }
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
                msg = ex.Message;
            }
            return new QuestionEx(null, msg);
        }


        public async Task<QuestionEx> CreateQuestion(QuestionDto questionDto)
        {
            var myContainer = await container();
            try
            {
                Question q = new(questionDto);
                QuestionEx questionEx = await AddNewQuestion(q);
                return questionEx;
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
                Console.WriteLine($"*****************************  {PartitionKey}/{Id}");
                // Read the item to see if it exists.  
                question = await myContainer.ReadItemAsync<Question>(
                    Id,
                    new PartitionKey(PartitionKey)
                );
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                msg = "NotFound";
                Console.WriteLine(msg);
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                msg = ex.Message;
                Console.WriteLine(msg);
            }
            Console.WriteLine(JsonConvert.SerializeObject(question));
            Console.WriteLine("*****************************");
            return new QuestionEx(question, msg);
        }

       

        public async Task<QuestionEx> UpdateQuestion(QuestionDto questionDto)
        {
            var myContainer = await container();
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<Question> aResponse =
                    await myContainer!.ReadItemAsync<Question>(
                        questionDto.Id,
                        new PartitionKey(questionDto.PartitionKey)
                    );
                Question question = aResponse.Resource;
                var doUpdate = true;
                if (!question.Title.Equals(questionDto.Title, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        HttpStatusCode statusCode = await CheckDuplicate(questionDto.Title);
                        doUpdate = false;
                        var msg = $"Question with Title: \"{questionDto.Title}\" already exists in database.";
                        Console.WriteLine(msg);
                        return new QuestionEx(null, msg);
                    }
                    catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        question.Title = questionDto.Title;
                    }
                }
                if (doUpdate)
                {
                    question.Source = questionDto.Source;
                    question.Status = questionDto.Status;
                    question.Modified = new WhoWhen(questionDto.Modified!);
                    aResponse = await myContainer.ReplaceItemAsync(question, question.Id, new PartitionKey(question.PartitionKey));
                    Console.WriteLine($"Updated Question \"{question.Id}\" / \"{question.Title}\"");
                    return new QuestionEx(aResponse.Resource, "");
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                var msg = $"Question Id: \"{questionDto.Id}\" Not Found in database.";
                Console.WriteLine(msg); 
                return new QuestionEx(null, msg);
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
            return new QuestionEx(null, "Server Problem");
        }

        public async Task<QuestionEx> DeleteQuestion(QuestionDto questionDto)
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

                //duplicateTitle = true;
                //if (!question.Title.Equals(questionDto.Title, StringComparison.OrdinalIgnoreCase))
                //{
                //    HttpStatusCode statusCode = await CheckDuplicate(questionDto.Title);
                //}
                // TODO check if is it already Archived
                question.Archived = new WhoWhen(questionDto.Archived!.NickName);

                aResponse = await myContainer.ReplaceItemAsync(question, question.Id, new PartitionKey(question.PartitionKey));
                Console.WriteLine("Updated Question [{0},{1}].\n \tBody is now: {2}\n", question.Title, question.Id, question);
                return new QuestionEx(aResponse.Resource, "");
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                var msg = $"Question item {questionDto.Id} NotFound in database.";
                if (duplicateTitle)
                {
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

        /*
        public async Task<string> DeleteQuestion(QuestionDto questionDto)
        {
            var myContainer = await container();
            try
            {
                // Read the item to see if it exists.

                ItemResponse<Question> aResponse =
                    await myContainer!.ReadItemAsync<Question>(
                        questionDto.Id,
                        new PartitionKey(questionDto.PartitionKey)
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
        */

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
                        Console.WriteLine(">>>>>>>> question is: {0}", JsonConvert.SerializeObject(question));
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
