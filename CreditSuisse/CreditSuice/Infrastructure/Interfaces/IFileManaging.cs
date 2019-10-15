using CreditSuice.Domain.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CreditSuice.Infrastructure.Interfaces
{
    interface IFileManaging
    {
       void OpenAndReadFile( ref Queue<EventData> StartEventsTable, ref Hashtable FinishEventsTable, ref Boolean isFileManagingProcessing);
    }
}
