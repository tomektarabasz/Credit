using Microsoft.VisualStudio.TestTools.UnitTesting;
using CreditSuice.Domain.Controller;
using CreditSuice.Domain.Controller.Classes;
using Microsoft.Extensions.Logging;
using CreditSuice.Domain.Model;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

namespace UnitTestProject
{
    [TestClass]
    public class SearchLongEventsTests
    {
        internal ILogger<SearchingLongEvents> _logger { get; set; }
        internal ILoggerFactory _loggerFactory { get; set; }
        internal DbContext _dbContext { get; set; }
        internal SearchingLongEvents _searchingLongEvents { get; set; }
        public SearchLongEventsTests()
        {
            _loggerFactory = new LoggerFactory();
            _logger = new Logger<SearchingLongEvents>(_loggerFactory);
            _dbContext = new DbContext();
            _searchingLongEvents = new SearchingLongEvents(_dbContext, _logger);
        }

        [TestMethod]
        public void isToLongTest()
        {
            EventData startE = new EventData() { id = "trollollo", timestamp = 10 };
            EventData finishE = new EventData() { id = "trollollo", timestamp = 11 };
            bool[] answare = new bool[2];
            bool[] expectAnsware = {false, true};
            answare[0] = _searchingLongEvents.isTooLong(startE, finishE);
            finishE.timestamp = 15;
            answare[1] = _searchingLongEvents.isTooLong(startE, finishE);
            
            CollectionAssert.AreEqual(expectAnsware, answare);           
        }
        [TestMethod]
        public void isProperDeleteEventFormFinishHashTable()
        {
            EventData startE = new EventData() { id = "trollollo", timestamp = 10 };
            var startQueueEvent = new Queue<EventData>();
            startQueueEvent.Enqueue(startE);
            EventData finishE = new EventData() { id = "trollollo", timestamp = 11 };
            var finishEventHashTable = new Hashtable();
            finishEventHashTable.Add(finishE.id,finishE);
            bool isFileManagingProcessing = true;
            var taskFactory = new TaskFactory();
            var task1 = taskFactory.StartNew(()=>_searchingLongEvents.StartSearching(ref startQueueEvent, ref finishEventHashTable,ref isFileManagingProcessing ));
            var task2 = taskFactory.StartNew(()=>_searchingLongEvents.StartSearching(ref startQueueEvent, ref finishEventHashTable, ref isFileManagingProcessing ));
            var task3 = taskFactory.StartNew(() => { Thread.Sleep(1000);isFileManagingProcessing = false; });
            Task.WaitAll(new[] {task1, task2, task3 });
            Assert.IsFalse(finishEventHashTable.Count>0);
        }
        [TestMethod]
        public void checkIfSearchingEventsTreadsWorkTillEndOfReadingFile()
        {
            EventData startE = new EventData() { id = "trollollo", timestamp = 10 };
            var startQueueEvent = new Queue<EventData>();
            startQueueEvent.Enqueue(startE);
            EventData finishE = new EventData() { id = "trollollo", timestamp = 11 };
            var finishEventHashTable = new Hashtable();
            finishEventHashTable.Add(finishE.id, finishE);
            ChangeFlag(ref isFileManagingProcessing);
            var taskFactory = new TaskFactory();
            Stopwatch stopwatch = Stopwatch.StartNew();
            var task3 = taskFactory.StartNew(() => { Thread.Sleep(2000); ChangeFlag(ref isFileManagingProcessing); });
            var task1 = taskFactory.StartNew(() => _searchingLongEvents.StartSearching(ref startQueueEvent, ref finishEventHashTable, ref isFileManagingProcessing));
            var task2 = taskFactory.StartNew(() => _searchingLongEvents.StartSearching(ref startQueueEvent, ref finishEventHashTable, ref isFileManagingProcessing));
                        
            Task.WaitAll(new[] {task1,task2,task3});
            stopwatch.Stop();
            Assert.IsFalse(stopwatch.ElapsedMilliseconds<1000);
        }
        public bool isFileManagingProcessing = false;
        public void ChangeFlag(ref bool flag)
        {
            flag = !flag;
        }
    }
}
