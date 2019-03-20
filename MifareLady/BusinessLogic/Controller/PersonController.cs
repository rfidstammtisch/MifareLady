using BusinessLogic.Models;
using NLog;
using SQLiteAdapter.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Controller
{
    public class PersonController
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public void SavePerson(Person person)
        {
            if (person == null)
            {
                Log.Warn("[PersonController.SavePerson] try to save empty person");
                return;
            }

            // TODO: set Id as Primary-Field
            // actual the first property is primary if Store does not exist
            StoreProvider.UpdateStore(typeof(Person).Name, person.ToDictionary());
        }

        public void DeletePerson(Person person)
        {
            if (person == null)
            {
                Log.Warn("[PersonController.SavePerson] try to delete empty person");
                return;
            }

            StoreProvider.DeleteEntry(typeof(Person).Name, person.ToDictionary());
        }

        public Person GetPerson(int id)
        {
            StoreProvider.GetCompleteStore(typeof(Person).Name);

            return null;
        }
    }
}
