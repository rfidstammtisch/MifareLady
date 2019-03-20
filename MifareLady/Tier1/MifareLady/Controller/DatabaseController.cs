using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MifareLady.Controller
{
    [RoutePrefix("data")]
    public class DatabaseController : ApiController
    {
        [Route("{store}/{id:int}")]
        public object GetDatastoreObject(string store, int id)
        {
            return null;
        }
    }
}
