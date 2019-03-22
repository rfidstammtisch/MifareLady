using BusinessLogic.Models;
using MifareLady.JsonConvert;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MifareLady.Models
{
    [JsonConverter(typeof(DatasetConverter))]
    public class TransportDataset
    {
        protected AbstractDataset dataset;
        public virtual string Type { get; set; }

        public TransportDataset() { }
        public TransportDataset(AbstractDataset dataset) => this.dataset = dataset;

        public virtual AbstractDataset GetDataset() => dataset;
    }
}