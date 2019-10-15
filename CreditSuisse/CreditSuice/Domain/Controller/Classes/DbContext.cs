using CreditSuice.Domain.Model;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CreditSuice.Domain.Controller.Classes
{
    public class DbContext
    {
        LiteDatabase ctx;
        LiteCollection<EventDataDB> events;

        public DbContext()
        {

            if (File.Exists(@"EventStatus.db"))
            {
                File.Delete(@"EventStatus.db");
            }
            ctx= new LiteDatabase(@"EventStatus.db");
            events = ctx.GetCollection<EventDataDB>("events");
        }

        public void InsertData(EventData eventData,bool shouldBeAlert,int deltaTime)
        {
            var eventToSave = new EventDataDB{ id = eventData.id, alert = shouldBeAlert,
                duration = deltaTime, host = eventData.host, type = eventData.type };
            events.Insert(eventToSave);
        }

        public void AddIndex()
        {
            events.EnsureIndex(x => x.alert);
        }               
    }
}
