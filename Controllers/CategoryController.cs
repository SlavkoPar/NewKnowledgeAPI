using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using System.ComponentModel.DataAnnotations;
using Knowledge.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Drawing.Printing;
using NewKnowledgeAPI.Model.Categories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Knowledge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    //[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]

    public class CategoryController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        DbService dbService { get; set; }


        public CategoryController(IConfiguration configuration)
        {
            dbService = new DbService(configuration);
            dbService.Initialize.Wait();
            Configuration = configuration;
        }

        // GET api/<FamilyController>
        [HttpGet]
        [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any)] //, VaryByQueryKeys = new[] { "impactlevel", "pii" })]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                Console.WriteLine("GetAllCategories");
                //using (var db = new Db(this.Configuration))
                //{
                //    await db.Initialize;
                //var category = new Category(_Db);
                //List<Category> subCategories = await category.GetAllCategories();
                var categoryService = new CategoryService(dbService);
                List<Category> subCategories = await categoryService.GetAllCategories();
                if (subCategories != null)
                {
                    List<CategoryDto> list = [];
                    foreach (Category cat in subCategories)
                    {
                        list.Add(new CategoryDto(cat));
                    }
                    return Ok(list);
                }
                //}
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{partitionKey}/{parentCategory}")]
        [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "partitionKey", "parentCategory" })]
        public async Task<IActionResult> GetSubCategories(string partitionKey, string parentCategory)
        {
            try
            {
                Console.WriteLine("GetSubCategories", partitionKey, parentCategory); 
                //using (var db = new Db(this.Configuration))
                //{
                //    await db.Initialize;
                //var category = new Category(_Db);
                //List<Category> subCategories = await category.GetSubCategories(partitionKey, parentCategory);
                var categoryService = new CategoryService(dbService);
                List<Category> subCategories = await categoryService.GetSubCategories(partitionKey, parentCategory);
                if (subCategories != null)
                {
                    List<CategoryDto> list = [];
                    foreach (Category cat in subCategories)
                    {
                        list.Add(new CategoryDto(cat));
                    }
                    return Ok(list);
                }
                //}
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("{partitionKey}/{id}/{pageSize}/{includeQuestionId}")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "partitionKey", "id", "pageSize", "includeQuestionId" })]
        public async Task<IActionResult> GetCategory(string partitionKey, string id, int pageSize, string includeQuestionId)
        {
            try
            {
                Console.WriteLine("GetCategory: {0}, {1}, {2}, {3} \n", partitionKey, id, pageSize, includeQuestionId);

                // TODO 1. ovo 2. what does  /partitionKey mean?
                //using(var db = new Db(this.Configuration))
                //{
                //    await db.Initialize;
                // TODO Question.Db = db;
                //var category = new Category(_Db);
                // var container = await Db.GetContainer(this.containerId);
                //Category cat = await category.GetCategory(
                //    partitionKey, id, true, pageSize, includeQuestionId=="null" ? null : includeQuestionId);
                var categoryService = new CategoryService(dbService);
                Category cat = await categoryService.GetCategory(
                       partitionKey, id, true, pageSize, includeQuestionId == "null" ? null : includeQuestionId);
                if (cat != null)
                {
                    return Ok(new CategoryDto(cat));
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] CategoryDto categoryDto)
        {
            try
            {
                Console.WriteLine("===>>> CreateCategory: {0} \n", categoryDto.Title);
                var categoryService = new CategoryService(dbService);
                if (categoryDto.PartitionKey == "null")
                {
                    categoryDto.PartitionKey = categoryDto.Id;
                }
                Category category = await categoryService.CreateCategory(categoryDto);
                if (category != null)
                {
                    return Ok(new CategoryDto(category));
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Put([FromBody] CategoryDto categoryDto)
        {
            try
            {
                Console.WriteLine("===>>> UpdateCategory: {0} \n", categoryDto.Title);
                var categoryService = new CategoryService(dbService);
                Category category = await categoryService.UpdateCategory(categoryDto);
                if (category != null)
                {
                    return Ok(new CategoryDto(category));
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[HttpDelete("{partitionKey}, {id}")]
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete([FromBody] CategoryKey categoryKey) //string PartitionKey, string id)
        {
            try
            {
                Console.WriteLine("===>>> DeleteCategory: {0}/{1} \n", categoryKey.PartitionKey, categoryKey.Id);
                var categoryService = new CategoryService(dbService);
                string result = await categoryService.DeleteCategory(categoryKey);
                if (result == "OK")
                    return Ok(new { msg = result });
                else if (result == "HasSubCategories" || result == "NumOfQuestions")
                    return Ok(new { msg = result });
                else if (result == "NotFound")
                    return NotFound();
                else
                    return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
