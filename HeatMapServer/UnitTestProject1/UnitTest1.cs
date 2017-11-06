using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HeatMapServer;
using HeatMapServer.Controllers;
using HeatMapServer.Models;
using System.Web.Script.Serialization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //            var baseUrl = "http://trafficmap.azurewebsites.net/api/values/";
            var baseUrl = "http://localhost:65359/api/values";

            var postParam = new PostParam()
            {
                visibleRegion = new VisibleRegion()
                {
                    latLngBounds = new LatLngBounds()
                    {
                        northeast = new LatLng()
                        {
                            latitude = 35.2446699,
                            longitude = 139.6905788
                        },
                        southwest = new LatLng()
                        {
                            latitude = 35.2242776,
                            longitude = 139.6676699
                        }
                    }
                },
                maxPoint = 100,
            };

            var data = new List<HeatMapData>();
            using (var client = new HttpClient())
            {
                var response = client.PostAsJsonAsync(baseUrl, postParam).Result;
                if(response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    data = new JavaScriptSerializer().Deserialize<List<HeatMapData>>(content);

                }
            }


        }
    }
}
