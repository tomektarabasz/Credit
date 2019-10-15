using System;
using System.Collections;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace CreditSuice.Infrastructure
{
    using CreditSuice.Domain.Model;
    using CreditSuice.Infrastructure.Interfaces;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;

    public class FileManaging: IFileManaging
    {
        public string pathSource { get; set; }
        private readonly ILogger _logger;
        public FileManaging(ILogger<FileManaging> logger)
        {
            _logger = logger;
        }
        public void OpenAndReadFile(ref Queue<EventData> StartEventsTable, ref Hashtable FinishEventsTable, ref Boolean isFileManagingProcessing)
        {
            isFileManagingProcessing = true;
            Console.WriteLine("Please write path to file:");
            pathSource = Console.ReadLine();
            _logger.LogDebug("Users inputs of path:" + pathSource);
            using (FileStream fs = File.Open(pathSource, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string line;                
                while ((line = sr.ReadLine()) != null)
                {
                    if (line!="{" && line != "}")
                    {
                        try
                        {
                            var eventData = JsonConvert.DeserializeObject<EventData>(line);
                            if (eventData.state == Constants.StartedState)
                            {
                                StartEventsTable.Enqueue(eventData);
                                _logger.LogDebug("New item was queued in StartEventsTable. All numbers in queue= " + StartEventsTable.Count);
                            }
                            else if (eventData.state == Constants.FinishedState)
                            {
                                FinishEventsTable.Add(eventData.id, eventData);
                                _logger.LogDebug("New item was queued in StartEventsTable. All numbers in queue= " + StartEventsTable.Count);
                            }
                            else
                            {
                                _logger.LogWarning($"Line of json file is not proper object. It is {0}", line);
                            }
                        }
                        catch (Exception ex)
                        {
                            isFileManagingProcessing = false;
                            _logger.LogError(ex.Message);
                            _logger.LogError(ex.StackTrace);
                        }
                    }
                    
                    Console.WriteLine(line);
                }
            }
            isFileManagingProcessing = false;
        }
    }
}
