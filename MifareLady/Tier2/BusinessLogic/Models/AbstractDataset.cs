using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Models
{
    public abstract class AbstractDataset
    {
        public virtual Dictionary<string, object> ToDictionary()
        {
            var entry = new Dictionary<string, object>();
            foreach (var property in typeof(Person).GetProperties())
            {
                var value = property.GetValue(this);
                if (value == null)
                    continue;

                entry[property.Name] = value;
            }

            return entry;
        }
    }
}
