using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public bool IsBlocked { get; set; }
        public byte[] Image { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var entry = new Dictionary<string, object>();
            foreach (var property in typeof(Person).GetProperties())
                entry[property.Name] = property.GetValue(person);

            return entry;
        }
    }
}
