using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using HeatMapServer.Models;

namespace HeatMapServer.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public List<HeatMapData> Post([FromBody]PostParam postParam)
        {
            if (postParam == null || postParam.visibleRegion == null 
                || postParam.visibleRegion.latLngBounds == null) return new List<HeatMapData>();
            var bounds = postParam.visibleRegion.latLngBounds;

            var points = Gis.DefaultInstance.HeatMapData
                .Where(
                    // visibleRegionの中に入るHeatMapData抽出
                    m => bounds.southwest.latitude <= m.Latitude && m.Latitude <= bounds.northeast.latitude
                    && bounds.southwest.longitude <= m.Longitude && m.Longitude <= bounds.northeast.longitude);

            if (postParam.maxPoint != null && postParam.maxPoint < points.Count())
                points = points.OrderByDescending(m => m.Intensity).Take(postParam.maxPoint.Value);

            return points.ToList();
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
