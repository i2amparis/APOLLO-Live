﻿using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Topsis.Adapters.Database;

namespace Topsis.Web.Infrastructure
{
    public class DbContextTransactionPageFilter : IAsyncPageFilter
    {
        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<WorkspaceDbContext>();

            try
            {
                await dbContext.BeginTransactionAsync();

                var actionExecuted = await next();
                if (actionExecuted.Exception != null && !actionExecuted.ExceptionHandled)
                {
                    dbContext.RollbackTransaction();
                }
                else
                {
                    await dbContext.CommitTransactionAsync();
                }
            }
            catch (Exception)
            {
                dbContext.RollbackTransaction();
                throw;
            }
        }
    }
}
