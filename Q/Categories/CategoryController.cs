﻿using Knowledge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewKnowledgeAPI.Q.Categories.Model;
using Newtonsoft.Json;
using System.Collections.Generic;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NewKnowledgeAPI.Q.Categories
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
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

       

        /*
        [HttpGet("{partitionKey}/{id}")]
        //[ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "partitionKey", "id" })]
        public async Task<IActionResult> GetSubCategories(string partitionKey, string id)
        {
            try
            {
                Console.WriteLine("GetSubCategories {0}/{1}", partitionKey, id);
                //using (var db = new Db(this.Configuration))
                //{
                //    await db.Initialize;
                //var category = new Category(_Db);
                var categoryRowService = new CategoryRowService(dbService);
                List<CategoryRow> subCategories = await categoryRowService.GetSubCategories(partitionKey, id);
                //Console.WriteLine(JsonConvert.SerializeObject(subCategories.Select( c => c.Title).ToList()));
                subCategories.Sort(CategoryRow.Comparer);
                //Console.WriteLine(JsonConvert.SerializeObject(subCategories.Select(c => c.Title).ToList()));
                if (subCategories != null)
                {
                    List<CategoryRowDto> list = [];
                    foreach (CategoryRow cat in subCategories)
                    {
                        list.Add(new CategoryRowDto(cat));
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
        */

        [HttpGet("{partitionKey}/{id}/{pageSize}/{includeQuestionId}")]
        //[ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "partitionKey", "id", "pageSize", "includeQuestionId" })]
        public async Task<IActionResult> GetCategory(string partitionKey, string id, int pageSize, string includeQuestionId)
        {
            try
            {
                CategoryKey categoryKey = new (partitionKey, id);
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
                CategoryEx categoryEx = await categoryService.GetCategory(
                       categoryKey, true, 
                       pageSize, 
                       includeQuestionId == "null" ? null : includeQuestionId
                );
                if (categoryEx.category != null)
                {
                    return Ok(new CategoryDtoEx(categoryEx));
                }
                return NotFound(new CategoryDtoEx(categoryEx));
            }
            catch (Exception ex)
            {
                return BadRequest(new CategoryDtoEx(ex.Message));
            }
        }

        [HttpGet("{partitionKey}/{id}/{hidrate}")]
        //[ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "partitionKey", "id" })]
        public async Task<IActionResult> GetCategoryHidrated(string partitionKey, string id, bool hidrate)
        {
            // hidrate collections except questions
            try
            {
                CategoryKey categoryKey = new(partitionKey, id);
                // TODO what does  /partitionKey mean?
                //using (var db = new Db(this.Configuration))
                //{
                //await db.Initialize;
                //var category = new Category(db);
                var categoryService = new CategoryService(dbService);
                CategoryEx categoryEx = await categoryService.GetCategory(categoryKey, hidrate, 0, null);
                if (categoryEx.category != null)
                {
                    return Ok(new CategoryDtoEx(categoryEx));
                }
                //}
                return NotFound(new CategoryDtoEx(categoryEx));
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
                CategoryEx categoryEx = await categoryService.CreateCategory(categoryDto);
                return Ok(new CategoryDtoEx(categoryEx));
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
                CategoryEx categoryEx = await categoryService.UpdateCategory(categoryDto);
                if (categoryEx != null)
                {
                    return Ok(new CategoryDtoEx(categoryEx));
                }
                return NotFound(new CategoryDtoEx("Jok Found"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[HttpDelete("{partitionKey}, {id}")]
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete([FromBody] CategoryDto categoryDto) //string PartitionKey, string id)
        {
            try
            {
                Console.WriteLine("===>>> DeleteCategory: {0}/{1} \n", categoryDto.PartitionKey, categoryDto.Id);
                var categoryService = new CategoryService(dbService);
                CategoryEx categoryEx = await categoryService.DeleteCategory(categoryDto);

                if (categoryEx.category != null)
                {
                    return Ok(new CategoryDtoEx(categoryEx));
                }
                return NotFound(categoryEx);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
