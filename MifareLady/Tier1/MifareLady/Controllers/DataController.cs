using BusinessLogic.Controller;
using BusinessLogic.Models;
using MifareLady.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace MifareLady.Controllers
{
    [RoutePrefix("data")]
    public class DataController : ApiController
    {
        [HttpGet]
        [Route("{store}/column/{column}/value/{vlaue}")]
        public List<TransportDataset> GetDatastoreObject(string store, string column, string value)
        {
            var datasetController = new DatasetController();
            var datasets = datasetController.GetDataset(store, column, value);

            var result = new List<TransportDataset>();
            foreach (var dataset in datasets)
                if (dataset is Person)
                    result.Add(new TransportPerson(dataset as Person));

            return result;
        }

        [HttpGet]
        [Route("{store}")]
        public List<TransportDataset> GetDatastoreObjects(string store)
        {
            var datasetController = new DatasetController();
            var datasets = datasetController.GetDatasets(store);

            var result = new List<TransportDataset>();
            foreach (var dataset in datasets)
                if (dataset is Person)
                    result.Add(new TransportPerson(dataset as Person));

            return result;
        }

        [HttpPost]
        [Route("{store}")]
        public void SaveDatastoreObject(string store, TransportDataset dataset)
        {
            var datasetController = new DatasetController();
            datasetController.SaveDataset(dataset.GetDataset());
        }

        [HttpDelete]
        [Route("{store}")]
        public void DeleteDatastoreObject(string store, TransportDataset dataset)
        {
            // TODO: Think about deleting with id
            var datasetController = new DatasetController();
            datasetController.DeleteDataset(dataset.GetDataset());
        }
    }
}
