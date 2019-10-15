using CreditSuice.Domain.Controller.Classes;
using CreditSuice.Domain.Controller.Interfaces;
using CreditSuice.Domain.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

[assembly: InternalsVisibleTo("UnitTestProject")]
namespace CreditSuice.Domain.Controller
{    
    public class SearchingLongEvents : ISearchingLongEvents
    {
        internal DbContext dbContext { get; set; }
        private readonly ILogger _logger;
        
        public SearchingLongEvents(DbContext ctx, ILogger<SearchingLongEvents> logger)
        {
            dbContext = ctx;
            _logger =logger;
        }

        public void StartSearching(ref Queue<EventData> StartEventsTable, ref Hashtable FinishEventsTable, ref Boolean isFileManagingProcessing)
        {
            Guid guid = new Guid();
            int safeIterator=0;
            _logger.LogDebug($"StartSearching starts loop id={0}", guid);
            while (StartEventsTable.TryDequeue(out EventData startE) || isFileManagingProcessing)
            {
                if (startE == null)
                {
                    safeIterator++;                   
                    Thread.Sleep(100);
                    if(safeIterator > 25)
                        break;
                    continue;
                }
                _logger.LogDebug($"StartSearching id={0}, parameters: start event id={1}, file is still reading = {2}", guid,startE.id,isFileManagingProcessing);
                //I should log time of execution it to try optimalizated
                try
                {
                    if (FinishEventsTable.ContainsKey(startE.id) && startE != null)
                    {
                        var finishE = (FinishEventsTable[startE.id] as EventData);
                        var deltaTime = performensTime(startE, finishE);
                        var saveWithStatusLongEvent = isTooLong(startE, finishE);
                        var eventToSave = ChooseStartOrFinishEventToSave(startE, finishE);
                        dbContext.InsertData(eventToSave, saveWithStatusLongEvent, (int)deltaTime);
                        deleteUsedRecord(startE, finishE, ref StartEventsTable, ref FinishEventsTable);
                    }
                }
                catch(Exception ex) {
                    _logger.LogError(ex.Message);
                    _logger.LogError(ex.StackTrace);
                }
                
            }
            _logger.LogDebug($"StartSearching ends loop id={0}",guid);
        }        

        internal Boolean isTooLong(EventData startData, EventData finishData)
        {
            var delta = finishData.timestamp - startData.timestamp;
            return delta >= Constants.timeEnoughtToBeLongEvent;
        }

        internal EventData ChooseStartOrFinishEventToSave(EventData startData,EventData finishData)
        {
            return startData.type != null ? startData : finishData; 
        }

        internal long performensTime(EventData startData, EventData finishData)
        {
            var delta = finishData.timestamp - startData.timestamp;
            return delta;
        }

        internal void deleteUsedRecord(EventData startData, EventData finishData, ref Queue<EventData> StartEventsTable, ref Hashtable FinishEventsTable)
        {         
            FinishEventsTable.Remove(finishData.id);
        }    
    }
}
