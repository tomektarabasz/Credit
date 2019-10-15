using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CreditSuice.Domain.Controller;
using CreditSuice.Domain.Controller.Classes;
using CreditSuice.Domain.Controller.Interfaces;
using CreditSuice.Domain.Model;
using CreditSuice.Infrastructure;
using CreditSuice.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("SearchLongEventsTests")]

namespace CreditSuice
{
    class Program
    {
        internal static int numberOfThreads { get {return Constants.numberOfThreads; } }

        static void Main(string[] args)
        {
            var serviceProvider = ServiceProviderAssembly();
            var configuration = ConfigurationAssembly();
            Console.WriteLine("########### CreditSuisse is the bast please to work ###########");
            Console.WriteLine(String.Format("Configuration from appsettings: time of to long event processing = {0} ms. Number of threads = {1} ",configuration.GetSection("timeEnoughtToBeLongEvent").Value,configuration.GetSection("numberOfThreads").Value));
            Console.WriteLine(String.Format("Feel free to customize it!",configuration.GetSection("timeEnoughtToBeLongEvent").Value,configuration.GetSection("numberOfThreads").Value));

            Queue<EventData> StartEventsTable = new Queue<EventData>();
            Hashtable FinishEventsTable = new Hashtable();
            Boolean isFileManagingProcessing = false;

            TaskFactory taskFactory = new TaskFactory();
            Task[] taskArray = new Task[numberOfThreads];
            taskArray[0]=taskFactory.StartNew(() =>
            {
                var fileManaging = serviceProvider.GetService<IFileManaging>();
                fileManaging.OpenAndReadFile(ref StartEventsTable, ref FinishEventsTable, ref isFileManagingProcessing);
            });
            for (int i=1;i<numberOfThreads; i++)
            {
                taskArray[i] = taskFactory.StartNew(() =>
                {                    
                    var searcher = serviceProvider.GetService<ISearchingLongEvents>();
                    searcher.StartSearching(ref StartEventsTable, ref FinishEventsTable, ref isFileManagingProcessing);
                });
            }
            Task.WaitAll(taskArray);
            Console.ReadKey();
        }

        internal static ServiceProvider ServiceProviderAssembly()
        {
            return new ServiceCollection()
            .AddSingleton<IFileManaging, FileManaging>()
            .AddSingleton<DbContext>()
            .AddScoped<ISearchingLongEvents,SearchingLongEvents>()
            .AddLogging(configure=>configure.AddConsole())
            .BuildServiceProvider();            
        }
        internal static IConfigurationRoot ConfigurationAssembly()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            Int32.TryParse(configuration.GetSection("timeEnoughtToBeLongEvent").Value,out Constants.timeEnoughtToBeLongEvent);
            Int32.TryParse(configuration.GetSection("numberOfThreads").Value,out Constants.timeEnoughtToBeLongEvent);
            return configuration;            
        }        
    }
}
