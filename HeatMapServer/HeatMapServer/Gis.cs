using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace HeatMapServer
{
    public class Gis
    {
        private static Gis mGis = new Gis();
        private static List<HeatMapData> _HeatMapData = new List<HeatMapData>();

        public static Gis DefaultInstance
        {
            get
            {
                return mGis;
            }
        }

        public List<HeatMapData> HeatMapData
        {
            get { return _HeatMapData; }
        }

        // HeatMapDataのキャッシュファイル名
        private static readonly string HeatMapCoordinatesFile = "HeatMapCoordinates";

        public static bool InitHeatmapData()
        {
            var path = HttpContext.Current.Server.MapPath("~/App_Data");
            IFormatter formatter = new BinaryFormatter();

            if (File.Exists(path + "/" + HeatMapCoordinatesFile))
            {
                try
                {
                    // 以前Serializeしたファイルがあればそれを使う
                    using (Stream stream = new FileStream(path + "/" + HeatMapCoordinatesFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        _HeatMapData = (List<HeatMapData>)formatter.Deserialize(stream);
                    }
                    Console.WriteLine("Read: {0}", path + "/" + HeatMapCoordinatesFile);

                    foreach(var point in _HeatMapData.Where(m=>m.Name.Contains("野比")))
                    {
                        var a = point;
                    }

                    return true;
                }catch(Exception e)
                {
                    Console.WriteLine("Error thrown while reading {0}: {1}", path + "/" + HeatMapCoordinatesFile, e.Message);
                    return false;
                }

            }

            // GISファイルを読み込み
            var di = new System.IO.DirectoryInfo(path);

            foreach (var f in di.GetFiles("*.xml", System.IO.SearchOption.TopDirectoryOnly))
            {
                using (var file = new FileStream(f.FullName, FileMode.Open))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Dataset), "http://nlftp.mlit.go.jp/ksj/schemas/ksj-app");
                    var points = (Dataset)serializer.Deserialize(file);
                    foreach(var stop in points.BusStop)
                    {
                        var point = points.Point.Where(m => m.Id == stop.position.hrefWoSharp).FirstOrDefault();
                        _HeatMapData.Add(/*Jgd2000ToWGS84*/(        // 国土交通省のデータはすでに世界測地系で提供されているようだ
                            new HeatMapData()
                        {
                            Latitude = point.coordinate[0],
                            Longitude = point.coordinate[1],
                            Name = stop.busStopName,
                        }));
                    }
                }
                Console.WriteLine("Read: {0}", f.FullName);
            }
            Console.WriteLine("Read GIS files complete");

            // 次回用にSerializeしたファイルを保存
            using (Stream stream = new FileStream(path + "/" + HeatMapCoordinatesFile, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                formatter.Serialize(stream, _HeatMapData);
            }
            Console.WriteLine("Wrote: {0}", path + "/" + HeatMapCoordinatesFile);

            return true;
        }

        public static bool SetRandomIntensity()
        {
            var rnd = new Random();

            _HeatMapData = _HeatMapData.ConvertAll(m => m.SetIntensity(rnd.Next(256)));

            return true;
        }

        /// <summary>
        /// 日本測地系の位置情報から世界測地系(WGS84)の位置情報に変換を行なう
        /// </summary>
        /// <param name="latTokyo">緯度(日本測地系)</param>
        /// <param name="lngTokyo">経度(日本測地系)</param>
        /// <returns></returns>
        public static HeatMapData Jgd2000ToWGS84(HeatMapData Jgd2000)
        {
            return new HeatMapData()
            {
                Name = Jgd2000.Name,
                Intensity = Jgd2000.Intensity,
                Latitude = Jgd2000.Latitude - 0.00010695d * Jgd2000.Latitude + 0.000017464d * Jgd2000.Longitude + 0.0046017d,
                Longitude = Jgd2000.Longitude - 0.000046038d * Jgd2000.Latitude - 0.000083043d * Jgd2000.Longitude + 0.010040d,
            };
        }
    }


    [Serializable]
    public class HeatMapData
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Name { get; set; }
        public int Intensity { get; set; }

        public HeatMapData SetIntensity(int intensity)
        {
            this.Intensity = intensity;
            return this;
        }
    }

    [XmlRoot("Dataset")]
    public class Dataset
    {
        [XmlElement(ElementName = "description", Namespace = "http://www.opengis.net/gml/3.2")]
        public string description { get; set; }

        [XmlElement(ElementName = "boundedBy", Namespace = "http://www.opengis.net/gml/3.2")]
        public boundedBy boundedBy { get; set; }

        [XmlElement(ElementName = "Point", Namespace = "http://www.opengis.net/gml/3.2")]
        public List<Point> Point { get; set; }

        [XmlElement(ElementName = "BusStop", Namespace = "http://nlftp.mlit.go.jp/ksj/schemas/ksj-app")]
        public List<BusStop> BusStop { get; set; }
    }

    [XmlRoot("boundedBy")]
    public class boundedBy
    {
        [XmlElement("EnvelopeWithTimePeriod")]
        public EnvelopeWithTimePeriod EnvelopeWithTimePeriod { get; set; }
    }

    [XmlRoot("EnvelopeWithTimePeriod")]
    public class EnvelopeWithTimePeriod
    {
        [XmlAttribute("srsName")]
        public string srsName { get; set; }

        [XmlAttribute("frame")]
        public string frame { get; set; }

        [XmlElement("lowerCorner")]
        public string lowerCorner { get; set; }

        [XmlElement("upperCorner")]
        public string upperCorner { get; set; }

        [XmlElement("beginPosition")]
        public beginPosition beginPosition { get; set; }

        [XmlElement("endPosition")]
        public endPosition endPosition { get; set; }
    }

    [XmlRoot("beginPosition")]
    public class beginPosition
    {
        [XmlAttribute("calendarEraName")]
        public string calendarEraName { get; set; }

        [XmlText]
        public string BeginPosition { get; set; }
    }

    [XmlRoot("endPosition")]
    public class endPosition
    {
        [XmlAttribute("indeterminatePosition")]
        public string indeterminatePosition { get; set; }
    }

    [XmlRoot("Point")]
    public class Point
    {
        [XmlAttribute(AttributeName = "id", Namespace = "http://www.opengis.net/gml/3.2", Form = XmlSchemaForm.Qualified)]
        public string Id { get; set; }

        [XmlElement("pos")]
        public string Pos { get; set; }

        [XmlElement("coordinate")]
        public double[] coordinate
        {
            get
            {
                return Pos.Split(' ').Select(n => double.Parse(n)).ToArray();
            }
        }
    }

    [XmlRoot("BusStop")]
    public class BusStop
    {
        [XmlAttribute(AttributeName = "id", Namespace = "http://www.opengis.net/gml/3.2", Form = XmlSchemaForm.Qualified)]
        public string Id { get; set; }

        [XmlElement("position")]
        public position position { get; set; }

        [XmlElement("busStopName")]
        public string busStopName { get; set; }

        [XmlElement("busRouteInformation")]
        public List<busRouteInformation> busRouteInformations { get; set; }
    }

    [XmlRoot("position")]
    public class position
    {
        [XmlAttribute(AttributeName = "href", Namespace = "http://www.w3.org/1999/xlink", Form = XmlSchemaForm.Qualified)]
        public string href { get; set; }

        [XmlElement("hrefWoSharp")]
        public string hrefWoSharp
        {
            get
            {
                return href.Replace("#", "");
            }
        }
    }

    [XmlRoot("busRouteInformation")]
    public class busRouteInformation
    {
        [XmlElement("BusRouteInformation")]
        public BusRouteInformation BusRouteInformation { get; set; }
    }

    [XmlRoot("BusRouteInformation")]
    public class BusRouteInformation
    {
        [XmlElement("busType")]
        public int busType { get; set; }

        [XmlElement("busOperationCompany")]
        public string busOperationCompany { get; set; }

        [XmlElement("busLineName")]
        public string busLineName { get; set; }
    }

}
