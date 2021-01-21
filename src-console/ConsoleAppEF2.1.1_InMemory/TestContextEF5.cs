﻿using System.Collections.Generic;
using ConsoleAppEF2.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConsoleApp_net5_0_EF5_InMemory
{
    public class TestContextEF5 : DbContext
    {
        public static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                //.AddFilter("Default", LogLevel.Information)
                .AddFilter("Microsoft", LogLevel.Information)
                //.AddFilter("System", LogLevel.Information)
                //.AddDebug()
                .AddConsole();
        });

        public virtual DbSet<ProductDynamic> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(MyLoggerFactory); // Warning: Do not create a new ILoggerFactory instance each time
            optionsBuilder.EnableSensitiveDataLogging();

            optionsBuilder.UseInMemoryDatabase("TestContextEF5");
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<ProductDynamic>()
                .OwnsOne(typeof(Dictionary<string, object>).Name, x => x.Dict, x =>
                {
                    x.IndexerProperty<string>("Name").IsRequired(false);
                    //x.IndexerProperty<string>("b").IsRequired(false);
                    //x.Property<int?>("TestId").IsRequired(false);
                });
        }
    }
}
