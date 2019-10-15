using CreditSuice.Domain.Model;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CreditSuice.Domain.Controller.Interfaces
{
    interface ISearchingLongEvents
    {
        void StartSearching(ref Queue<EventData> StartEventsTable, ref Hashtable FinishEventsTable, ref Boolean isFileManagingProcessing);
    }
}
