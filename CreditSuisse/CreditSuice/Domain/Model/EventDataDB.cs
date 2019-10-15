using System;
using System.Collections.Generic;
using System.Text;

namespace CreditSuice.Domain.Model
{
    public class EventDataDB
    {
        public string id { get; set; }
        public bool alert { get; set; }
        public string type { get; set; }
        public string host { get; set; }
        public long duration { get; set; }
    }
}
