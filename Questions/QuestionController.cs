using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using Knowledge.Services;
using Microsoft.AspNetCore.Authorization;
using NewKnowledgeAPI.Categories.Model;
using NewKnowledgeAPI.Questions.Model;
using NewKnowledgeAPI.Categories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NewKnowledgeAPI.Questions
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class QuestionController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        DbService dbService { get; set; }

        public QuestionController(IConfiguration configuration)
        {
            dbService = new DbService(configuration);
            dbService.Initialize.Wait();
            Configuration = configuration;
        }


        [HttpGet("{partitionKey}/{parentCategory}/{startCursor}/{pageSize}/{includeQuestionId}")]
        public async Task<IActionResult> GetQuestions(string partitionKey, string parentCategory, int startCursor, int pageSize, string? includeQuestionId)
        {
            string message = string.Empty;
            try
            {
                var categoryService = new CategoryService(dbService);
                CategoryKey categoryKey = new CategoryKey(partitionKey, parentCategory);
                CategoryEx categoryEx = await categoryService.GetCategory(categoryKey);
                var (category, msg) = categoryEx;
                if (category != null)
                {
                    var questionService = new QuestionService(dbService);
                    QuestionsMore questionsMore = await questionService.GetQuestions(parentCategory, startCursor, pageSize, includeQuestionId);
                    Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>> Count {0}", questionsMore.questions.Count);
                    CategoryDto categoryDto = new(categoryKey, questionsMore);
                    categoryDto.Title = category.Title;
                    return Ok(new CategoryDtoEx(categoryDto, msg));
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return Ok(new CategoryDtoEx(message));

        }

        [HttpGet("{partitionKey}/{id}")]
        public async Task<IActionResult> GetQuestion(string partitionKey, string id)
        {
            try
            {
                var categoryService = new CategoryService(dbService);
                var questionService = new QuestionService(dbService);
                QuestionEx questionEx = await questionService.GetQuestion(partitionKey, id);
                var (question, msg) = questionEx;
                if (question == null)
                    return NotFound(new QuestionDtoEx(questionEx));
                CategoryKey categoryKey = new(partitionKey, question.ParentCategory!);
                // get category Title
                CategoryEx categoryEx = await categoryService.GetCategory(categoryKey);
                var (category, message) = categoryEx;
                question.CategoryTitle = category != null 
                    ? category.Title
                    : "NotFound Category";
                return Ok(new QuestionDtoEx(questionEx));
            }
            catch (Exception ex)
            {
                return BadRequest(new QuestionDtoEx(ex.Message));
            }
        }

        [HttpGet("{filter}/{count}/{nesto}")]
        public async Task<IActionResult> GetQuests(string filter, int count, string nesto)
        {
            Console.WriteLine("GetQuests", filter, count, nesto);
            try
            {
                var words = filter //.ToLower()
                                .Replace("?", "")
                                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                .Where(w => w.Length > 2)
                                .ToList();
                var questionService = new QuestionService(dbService);
                List<QuestDto> quests = await questionService.GetQuests(words, count);
                return Ok(quests);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] QuestionDto questionDto)
        {
            try
            {
                Console.WriteLine("*********=====>>>>>> questionDto");
                Console.WriteLine(JsonConvert.SerializeObject(questionDto));

                var categoryService = new CategoryService(dbService);
                var questionService = new QuestionService(dbService);

                QuestionEx questionEx = await questionService.CreateQuestion(questionDto);
                Console.WriteLine("*********=====>>>>>> questionEx");
                Console.WriteLine(JsonConvert.SerializeObject(questionEx));
                var question = questionEx.question;
                if (question != null)
                {
                    //Category category = new Category(questionEx.question);
                    questionDto.Modified = questionDto.Created; // to be used for category
                    await categoryService.UpdateNumOfQuestions(questionDto, 1);
                }
                // Console.WriteLine("^^^^^^^^^^^ questionEx" + JsonConvert.SerializeObject(questionEx));
                return Ok(new QuestionDtoEx(questionEx));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Put([FromBody] QuestionDto questionDto)
        {
            try
            {
                Console.WriteLine("===>>> UpdateQuestion: {0} \n", questionDto.Title);
                var questionService = new QuestionService(dbService);

                QuestionEx questionEx = await questionService.UpdateQuestion(questionDto);
                if (questionEx!.question != null)
                    return Ok(new QuestionDtoEx(questionEx));
                return NotFound(new QuestionDtoEx(questionEx));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete([FromBody] QuestionDto questionDto) //string PartitionKey, string id)
        {
            try
            {
                Console.WriteLine("===>>> DeleteQuestion: {0}/{1} \n", questionDto.PartitionKey, questionDto.Id);
                var categoryService = new CategoryService(dbService);
                var questionService = new QuestionService(dbService);
                QuestionEx questionEx = await questionService.DeleteQuestion(questionDto);
                if (questionEx!.question != null)
                {
                    questionDto.Modified = questionDto.Archived;
                    await categoryService.UpdateNumOfQuestions(questionDto, -1);
                    return Ok(new QuestionDtoEx(questionEx));
                }
                return NotFound(questionEx);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
