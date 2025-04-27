using System.Net;
using Knowledge.Services;
using Microsoft.Azure.Cosmos;
using NewKnowledgeAPI.Common;
using Newtonsoft.Json;
using NewKnowledgeAPI.A.Answers.Model;
using System.Collections.Generic;


namespace NewKnowledgeAPI.A.Answers
{
    public class AnswerService : IDisposable
    {
        public DbService? Db { get; set; } = null;

        private readonly string containerId = "Answers";
        private Container? _container = null;

        public async Task<Container> container()
        {
            _container ??= await Db!.GetContainer(containerId);
            return _container;
        }


        public string? PartitionKey { get; set; } = null;
        public AnswerService()
        {
        }

        public AnswerService(DbService Db)
        {
            this.Db = Db;
        }
                 
        public async Task<HttpStatusCode> CheckDuplicate(string? Title, string? Id = null)
        {

            var sqlQuery = Title != null
                ? $"SELECT * FROM c WHERE c.Type = 'answer' AND c.Title = '{Title}' AND IS_NULL(c.Archived)"
                : $"SELECT * FROM c WHERE c.Type = 'answer' AND c.Id = '{Id}' AND IS_NULL(c.Archived)";
            QueryDefinition queryDefinition = new(sqlQuery);
            FeedIterator<Answer> queryResultSetIterator =
                _container!.GetItemQueryIterator<Answer>(queryDefinition);
            if (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Answer> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                if (currentResultSet.Count == 0)
                {
                    throw new CosmosException("Answer Title already exists", HttpStatusCode.NotFound, 0, "0", 0);
                }
            }
            return HttpStatusCode.Found;
        }

        public async Task<AnswerEx?> AddAnswer(AnswerData answerData)
        {
            var myContainer = await container();
            //Console.WriteLine(JsonConvert.SerializeObject(answerData));
            string msg = string.Empty;
            try
            {
                Answer answer = new(answerData);
                Console.WriteLine("----->>>>> " + JsonConvert.SerializeObject(answer));
                // Read the item to see if it exists.  
                await CheckDuplicate(answerData.Title);
                msg = $":::::: Item in database with Title: {answerData.Title} already exists";
                Console.WriteLine(msg);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Answer q = new(answerData);
                AnswerEx answerEx = await AddNewAnswer(q);
                return answerEx;
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this reans.
                msg = ex.Message;
                Console.WriteLine(msg);
            }
            return new AnswerEx(null, msg);
        }


        public async Task<AnswerEx> AddNewAnswer(Answer answer)
        {
            var (PartitionKey, Id, Title, ParentGroup, Type, Source, Status) = answer;

            var myContainer = await container();
            string msg = string.Empty;
            try
            {
                // Check if the id already exists
                ItemResponse<Answer> aResponse =
                    await myContainer!.ReadItemAsync<Answer>(
                        Id,
                        new PartitionKey(PartitionKey)
                    );
                msg = $"Answer in database with id: {Id} already exists\n";
                Console.WriteLine(msg);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                try
                {
                    // Check if the title already exists
                    HttpStatusCode statusCode = await CheckDuplicate(Title);
                    msg = $"Answer in database with Title: {Title} already exists";
                    Console.WriteLine(msg);
                }
                catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    ItemResponse<Answer> aResponse =
                    await myContainer!.CreateItemAsync(
                            answer,
                            new PartitionKey(PartitionKey)
                        );
                    // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this reans.
                    Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", aResponse.Resource.Id, aResponse.RequestCharge);
                    return new AnswerEx(aResponse.Resource, "");
                }
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this reans.
                Console.WriteLine(ex.Message);
                msg = ex.Message;
            }
            return new AnswerEx(null, msg);
        }


        public async Task<AnswerEx> CreateAnswer(AnswerDto answerDto)
        {
            var myContainer = await container();
            try
            {
                Answer a = new(answerDto);
                AnswerEx answerEx = await AddNewAnswer(a);
                return answerEx;
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this reans.
                Console.WriteLine(ex.Message);
                return new AnswerEx(null, ex.Message);
            }
        }

        public async Task<AnswerEx> GetAnswer(string PartitionKey, string Id)
        {
            var myContainer = await container();
            Answer? answer = null;
            string msg = string.Empty;
            try
            {
                Console.WriteLine($"*****************************  {PartitionKey}/{Id}");
                // Read the item to see if it exists.  
                answer = await myContainer.ReadItemAsync<Answer>(
                    Id,
                    new PartitionKey(PartitionKey)
                );
                return new AnswerEx(answer, msg);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                msg = "NotFound";
                Console.WriteLine(msg);
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this reans.
                msg = ex.Message;
                Console.WriteLine(msg);
            }
            Console.WriteLine(JsonConvert.SerializeObject(answer));
            Console.WriteLine("*****************************");
            return new AnswerEx(null, msg);
        }

       

        public async Task<AnswerEx> UpdateAnswer(AnswerDto answerDto)
        {
            var myContainer = await container();
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<Answer> aResponse =
                    await myContainer!.ReadItemAsync<Answer>(
                        answerDto.Id,
                        new PartitionKey(answerDto.PartitionKey)
                    );
                Answer answer = aResponse.Resource;
                var doUpdate = true;
                if (!answer.Title.Equals(answerDto.Title, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        HttpStatusCode statusCode = await CheckDuplicate(answerDto.Title);
                        doUpdate = false;
                        var msg = $"Answer with Title: \"{answerDto.Title}\" already exists in database.";
                        Console.WriteLine(msg);
                        return new AnswerEx(null, msg);
                    }
                    catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        answer.Title = answerDto.Title;
                    }
                }
                if (doUpdate)
                {
                    answer.Source = answerDto.Source;
                    answer.Status = answerDto.Status;
                    answer.Modified = new WhoWhen(answerDto.Modified!);
                    aResponse = await myContainer.ReplaceItemAsync(answer, answer.Id, new PartitionKey(answer.PartitionKey));
                    Console.WriteLine($"Updated Answer \"{answer.Id}\" / \"{answer.Title}\"");
                    return new AnswerEx(aResponse.Resource, "");
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                var msg = $"Answer Id: \"{answerDto.Id}\" Not Found in database.";
                Console.WriteLine(msg); 
                return new AnswerEx(null, msg);
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this reans.
                Console.WriteLine(ex.Message);
            }
            return new AnswerEx(null, "Server Problem");
        }

        public async Task<AnswerEx> DeleteAnswer(AnswerDto answerDto)
        {
            var myContainer = await container();
            var duplicateTitle = false;
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<Answer> aResponse =
                    await myContainer!.ReadItemAsync<Answer>(
                        answerDto.Id,
                        new PartitionKey(answerDto.PartitionKey)
                    );
                Answer answer = aResponse.Resource;

                //duplicateTitle = true;
                //if (!answer.Title.Equals(answerDto.Title, StringComparison.OrdinalIgnoreCase))
                //{
                //    HttpStatusCode statusCode = await CheckDuplicate(answerDto.Title);
                //}
                // TODO check if is it already Archived
                answer.Archived = new WhoWhen(answerDto.Archived!.NickName);

                aResponse = await myContainer.ReplaceItemAsync(answer, answer.Id, new PartitionKey(answer.PartitionKey));
                Console.WriteLine("Updated Answer [{0},{1}].\n \tBody is now: {2}\n", answer.Title, answer.Id, answer);
                return new AnswerEx(aResponse.Resource, "");
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                var msg = $"Answer item {answerDto.Id} NotFound in database.";
                if (duplicateTitle)
                {
                    msg = $"Answer Title: {answerDto.Title} aleready exists in database.";
                }
                Console.WriteLine(msg); //, aResponse.RequestCharge);
                return new AnswerEx(null, msg);
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this reans.
                Console.WriteLine(ex.Message);
            }
            return new AnswerEx(null, "Server Problem");
        }

        /*
        public async Task<string> DeleteAnswer(AnswerDto answerDto)
        {
            var myContainer = await container();
            try
            {
                // Read the item to see if it exists.

                ItemResponse<Answer> aResponse =
                    await myContainer!.ReadItemAsync<Answer>(
                        answerDto.Id,
                        new PartitionKey(answerDto.PartitionKey)
                    );
                Answer answer = aResponse.Resource;
                aResponse = await myContainer.DeleteItemAsync<Answer>(answer.Id, new PartitionKey(answer.PartitionKey));
                Console.WriteLine("Deleted Answer [{0},{1}].\n \tBody is now: {2}\n", answer.Title, answer.Id, answer);
                return "OK";
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Answer item {0} NotFound in database.\n", answerKey.Id); //, aResponse.RequestCharge);
                return "NotFound";
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this reans.
                Console.WriteLine(ex.Message);
                return ex.Message;
            }
        }
        */

        public async Task<AnswersMore> GetAnswers(string parentGroup, int startCursor, int pageSize, string includeAnswerId)
        {
            var myContainer = await container();
            try
            {
                // OR c.ParentGroup = ''
                string sqlQuery = $"SELECT * FROM c WHERE c.Type = 'answer' AND IS_NULL(c.Archived) AND " +
                    $" c.ParentGroup = '{parentGroup}' ORDER BY c.Title OFFSET {startCursor} ";
                sqlQuery += includeAnswerId == "null"
                    ? $"LIMIT {pageSize}"
                    : $"LIMIT 9999";

                Console.WriteLine("************ sqlQuery{0}", sqlQuery);

                int n = 0;
                bool included = false;

                List<Answer> answers = [];
                QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
                FeedIterator<Answer> queryResultSetIterator = myContainer!.GetItemQueryIterator<Answer>(queryDefinition);
                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Answer> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (Answer answer in currentResultSet)
                    {
                        if (includeAnswerId != null && answer.Id == includeAnswerId)
                        {
                            included = true;
                        }
                        Console.WriteLine(">>>>>>>> answer is: {0}", JsonConvert.SerializeObject(answer));
                        answers.Add(answer);
                        n++;
                        if (n >= pageSize && (includeAnswerId == null || included))
                        {
                            return new AnswersMore(answers, true);
                        }
                    }
                    return new AnswersMore(answers, false);
                }
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this reans.
                Console.WriteLine(ex.Message);
            }
            return new AnswersMore([], false);
        }

        public async Task<List<ShortAnswerDto>> GetShortAnswers(List<string> words, int count)
        {
            var myContainer = await container();
            try
            {
                var sqlQuery = $"SELECT c.partitionKey, c.ParentGroup, c.Title, c.id FROM c WHERE c.Type = 'answer' AND IS_NULL(c.Archived) AND ";
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


                List<ShortAnswerDto> shortAnswers = [];
                QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
                using (FeedIterator<ShortAnswer> queryResultSetIterator = 
                    myContainer!.GetItemQueryIterator<ShortAnswer>(queryDefinition))
                {
                    while (queryResultSetIterator.HasMoreResults)
                    {
                        FeedResponse<ShortAnswer> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                        foreach (ShortAnswer shortAnswer in currentResultSet)
                        {
                            shortAnswers.Add(new ShortAnswerDto(shortAnswer));
                        }
                    }
                }
                return shortAnswers;
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this reans.
                Console.WriteLine(ex.Message);
            }
            return [];
        }


        public async Task<Dictionary<string, string>> GetTitles(List<string> answerIds)
        {
            var myContainer = await container();
            try
            {
                string str = string.Join(",", answerIds.ToArray());

                // OR c.ParentGroup = ''
                string sqlQuery = $"SELECT c.id, c.Title FROM c " + 
                    $" WHERE c.Type = 'answer' AND IS_NULL(c.Archived) AND " +
                    $" c.Id IN ({str}) ORDER BY c.Title OFFSET LIMIT 100";

                //Console.WriteLine("************ sqlQuery{0}", sqlQuery);

                List<AnswerTitle> answerTitles = [];
                QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
                FeedIterator<AnswerTitle> queryResultSetIterator = myContainer!.GetItemQueryIterator<AnswerTitle>(queryDefinition);
                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<AnswerTitle> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (AnswerTitle answerTitle in currentResultSet)
                    {
                        //Console.WriteLine(">>>>>>>> answer is: {0}", JsonConvert.SerializeObject(answer));
                        answerTitles.Add(answerTitle);
                    }
                    return answerTitles.ToDictionary(x => x.Id, x => x.Title);
                }
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this reans.
                Console.WriteLine(ex.Message);
            }
            return answerIds.Select(x => (new AnswerTitle(x, "unk"))).ToDictionary(x => x.Id, x => x.Title);
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
