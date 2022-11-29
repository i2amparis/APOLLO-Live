﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Topsis.Adapters.Database.Seed;
using Topsis.Application.Contracts.Database;

namespace Topsis.Adapters.Database.Services
{
    public class MigrationHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<MigrationHostedService> _logger;

        public MigrationHostedService(IServiceProvider serviceProvider,
            IDatabaseService databaseService,
            ILogger<MigrationHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _databaseService = databaseService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("Starting migration.");

            // Create a new scope to retrieve scoped services
            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    var db = BuildContext();
                    db.Database.Migrate();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    throw;
                }
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    // Ensure users and roles.
                    var db = BuildContext();
                    var roles = await db.UserRoles.ToArrayAsync();
                    if (roles.Length == 0)
                    {
                        IdentitySeed.ApplyTo(db);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    throw;
                }
            }

            _logger.LogDebug("Finished migration.");
        }

        private WorkspaceDbContext BuildContext()
        {
            // Db Options.
            var builder = new DbContextOptionsBuilder<WorkspaceDbContext>();
            builder.Setup(_databaseService.GetDatabaseEngine(), _databaseService.GetMigrationConnectionString());

            // Migrate.
            var myDbContext = new WorkspaceDbContext(builder.Options, null);
            return myDbContext;
        }
    }
}
