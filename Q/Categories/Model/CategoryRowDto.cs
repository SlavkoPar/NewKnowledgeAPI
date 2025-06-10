﻿using Microsoft.AspNetCore.OutputCaching;
using NewKnowledgeAPI.Common;
using NewKnowledgeAPI.Q.Questions.Model;
using Newtonsoft.Json;
using System;

namespace NewKnowledgeAPI.Q.Categories.Model
{
    public class CategoryRowDto: IDisposable
    {
        public string PartitionKey { get; set; }
        public string Id { get; set; }
        public string? RootId { get; set; }
        public string ParentCategory { get; set; }

        public string Title { get; set; }

        public int Kind { get; set; }
        public int Level { get; set; }
        public int NumOfQuestions { get; set; }
        public bool HasSubCategories { get; set; }
        public string? Link { get; set; }
        public string Header { get; set; }
        public List<string>? Variations { get; set; }

        public List<CategoryRowDto>? SubCategories { get; set; }
        public bool? IsExpanded { get; set; }

        public CategoryRowDto(CategoryRow categoryRow)
        {
            var (partitionKey, id, parentCategory, title, link, header, level, kind,
                hasSubCategories, subCategories,
                hasMoreQuestions, numOfQuestions, _, variations, isExpanded, rootId) = categoryRow;
            Id = id;
            PartitionKey = partitionKey;
            Title = title;
            Kind = kind;
            ParentCategory = parentCategory;
            Level = level;
            NumOfQuestions = numOfQuestions;
            HasSubCategories = hasSubCategories;
            SubCategories = subCategories.Select(row => new CategoryRowDto(row)).ToList();
            Variations = variations ?? [];
            Link = link;
            Header = header;
            IsExpanded = isExpanded;
            RootId = rootId;
        }

        public void Dispose()
        {
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



