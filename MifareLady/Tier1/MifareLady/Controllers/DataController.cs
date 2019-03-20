﻿using BusinessLogic.Controller;
using BusinessLogic.Models;
using System.Web.Http;

namespace MifareLady.Controllers
{
    [RoutePrefix("data")]
    public class DataController : ApiController
    {
        [HttpGet]
        [Route("{store}/{id:int}")]
        public object GetDatastoreObject(string store, int id)
        {
            var datasetController = new DatasetController();
            return datasetController.GetDataset(store, id);
        }

        [HttpGet]
        [Route("{store}")]
        public object GetDatastoreObjects(string store)
        {
            var datasetController = new DatasetController();
            return datasetController.GetDatasets(store);
        }

        [HttpPost]
        [Route("{store}")]
        public void SaveDatastoreObject(AbstractDataset dataset)
        {
            var datasetController = new DatasetController();
            datasetController.SaveDataset(dataset);
        }

        [HttpDelete]
        [Route("{store}")]
        public void DeleteDatastoreObject(AbstractDataset dataset)
        {
            // TODO: Think about deleting with id
            var datasetController = new DatasetController();
            datasetController.DeleteDataset(dataset);
        }
    }
}