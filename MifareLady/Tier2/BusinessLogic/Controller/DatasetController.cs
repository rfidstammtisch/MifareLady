﻿using BusinessLogic.Models;
using NLog;
using SQLiteAdapter.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Controller
{
    public class DatasetController
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public void SaveDataset(AbstractDataset dataset)
        {
            if (dataset == null)
            {
                Log.Warn("[PersonController.SavePerson] try to save empty dataset");
                return;
            }

            // TODO: set Id as Primary-Field
            // actual the first property is primary if Store does not exist
            StoreProvider.UpdateStore(dataset.GetType().Name, dataset.ToDictionary());
        }

        public void DeleteDataset(AbstractDataset dataset)
        {
            if (dataset == null)
            {
                Log.Warn("[PersonController.DeleteDataset] try to delete empty dataset");
                return;
            }

            StoreProvider.DeleteEntry(dataset.GetType().Name, dataset.ToDictionary());
        }

        public List<AbstractDataset> GetDatasets(string datasetStore)
        {
            var dbDatasets = StoreProvider.GetCompleteStore(datasetStore);

            var datasets = new List<AbstractDataset>();
            var datasetType = Type.GetType($"BusinessLogic.Models.{datasetStore}");

            foreach (var dbDataset in dbDatasets)
            {
                var dataset = Activator.CreateInstance(datasetType);
                foreach (var key in dbDataset.Keys)
                {
                    var property = datasetType.GetProperty(key);
                    if (property == null)
                        continue;

                    var value = dbDataset[key] == DBNull.Value ? null : dbDataset[key];
                    if (property.PropertyType == typeof(bool))
                        switch (value)
                        {
                            case "0":
                                value = false;
                                break;
                            case "1":
                                value = true;
                                break;
                        }

                    value = Convert.ChangeType(value, property.PropertyType);

                    property.SetValue(dataset, value);
                }
                datasets.Add(dataset as AbstractDataset);
            }

            return datasets;
        }

        public List<AbstractDataset> GetDataset(string datasetStore, string column, object value)
        {
            // TODO: think about search criterias
            var dbDatasets = StoreProvider.Search(datasetStore, column, value);
            var datasetType = Type.GetType($"BusinessLogic.Models.{datasetStore}");

            var datasets = new List<AbstractDataset>();
            foreach (var dbDataset in dbDatasets ?? new List<Dictionary<string, object>>())
            {
                var dataset = Activator.CreateInstance(datasetType);
                foreach (var key in dbDataset.Keys)
                {
                    var property = datasetType.GetProperty(key);
                    if (property == null)
                        continue;

                    if (dbDataset[key] != DBNull.Value)
                    {
                        if (property.PropertyType == typeof(bool))
                            property.SetValue(dataset, !dbDataset[key].Equals(0));
                        else
                        {
                            var result = Convert.ChangeType(dbDataset[key], property.PropertyType);
                            property.SetValue(dataset, result);
                        }
                    }
                }

                datasets.Add(dataset as AbstractDataset);
            }

            return datasets;
        }
    }
}
