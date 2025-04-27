using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using Knowledge.Services;
using Microsoft.AspNetCore.Authorization;
using NewKnowledgeAPI.Q.Categories.Model;
using NewKnowledgeAPI.Q.Questions.Model;
using NewKnowledgeAPI.A.Answers.Model;
using NewKnowledgeAPI.A.Answers;
using NewKnowledgeAPI.Q.Categories;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NewKnowledgeAPI.Q.Questions
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class QuestionAnswerController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        DbService dbService { get; set; }

        public QuestionAnswerController(IConfiguration configuration)
        {
            dbService = new DbService(configuration);
            dbService.Initialize.Wait();
            Configuration = configuration;
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AssignAnswer([FromBody] AssignedAnswerDto assignedAnswerDto)
        {
            try
            {
                Console.WriteLine("*********=====>>>>>>>>>>>>>>>>>>>> assignedAnswerDto");
                Console.WriteLine(JsonConvert.SerializeObject(assignedAnswerDto));

                var categoryService = new CategoryService(dbService);
                var answerService = new AnswerService(dbService);
                var questionService = new QuestionService(dbService);

                QuestionDtoEx questionDtoEx = await questionService.AssignAnswer(assignedAnswerDto, categoryService, answerService);
                var (questionDto, msg) = questionDtoEx;
                Console.WriteLine("*********=====>>>>>> questionEx");
                Console.WriteLine(JsonConvert.SerializeObject(questionDtoEx));
                return questionDto != null 
                    ? Ok(questionDtoEx) 
                    : NotFound(questionDtoEx);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
