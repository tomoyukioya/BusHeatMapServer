using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeatMapServer.Models
{
    public class PostParam
    {
        public VisibleRegion visibleRegion { get; set; }
        public int? maxPoint { get; set; }
    }

    public class VisibleRegion
    {
        public LatLng farLeft { get; set;}
        public LatLng farRight { get; set; }
        public LatLng nearLeft { get; set; }
        public LatLng nearRight { get; set; }
        public LatLngBounds latLngBounds { get; set; }

    }

    public class LatLng
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
    }

    public class LatLngBounds
    {
        public LatLng northeast { get; set;}
        public LatLng southwest { get; set; }
    }
}