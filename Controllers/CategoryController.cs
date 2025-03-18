using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Knowledge.Model;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using System.ComponentModel.DataAnnotations;
using Knowledge.Services;
using Microsoft.AspNetCore.Cors;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Knowledge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        [EnableCors("AllowAllOrigins")]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
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
        public async Task<IActionResult> GetSubCategories(string partitionKey, string parentCategory)
        {
            try
            {
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
        public async Task<IActionResult> GetCategory(string partitionKey, string id, int pageSize, string includeQuestionId)
        {
            try
            {
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
                    // TODO treba li ovo svuda
                    //Category.Db = null;
                    //Question.Db = null;
                //}
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // POST api/<FamilyController>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<FamilyController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<FamilyController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
