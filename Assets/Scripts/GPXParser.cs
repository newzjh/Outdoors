using System;
using System.Linq;
using System.Xml;
using System.Collections.Generic;
using GoShared;
using System.Diagnostics;
using System.IO;


public class GpxParser
{

    public static List<Track> ParseGPXContent(string content)
    {
        XmlDocument doc = new XmlDocument();
        try
        {
            doc.LoadXml(content);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e);
        }

        return ParseGPXContent(doc);
    }


    public static List<Track> ParseGPXContent(Stream s)
    {
        XmlDocument doc = new XmlDocument();
        try
        {
            doc.Load(s);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e);
        }

        return ParseGPXContent(doc);
    }

    public static List<Track> ParseGPXContent(XmlDocument doc)
    {
        var tracks = new List<Track>();

        //获取根节点
        XmlNode gpxnode = null;
        foreach(XmlNode xmlRootElement in doc.ChildNodes)
        {
            if (xmlRootElement.Name == "gpx")
                gpxnode = xmlRootElement;
        }
        if (gpxnode == null)
        {
            return null;
        }

        List<XmlNode> trknodes = new List<XmlNode>();
        foreach (XmlNode xmlSubElement in gpxnode.ChildNodes)
        {
            if (xmlSubElement.Name == "trk")
            {
                trknodes.Add(xmlSubElement);
            }
        }

        foreach(XmlNode trknode in trknodes)
        {
            Track track = new Track();
            foreach (XmlNode trksegnode in trknode.ChildNodes)
            {
                if (trksegnode.Name != "trkseg")
                    continue;

                foreach(XmlNode trkptnode in trksegnode.ChildNodes)
                {
                    if (trkptnode.Name != "trkpt")
                        continue;

                    Coordinates onepoint = new Coordinates(0,0);
                    foreach (XmlAttribute attr in trkptnode.Attributes)
                    {
                        if (attr.Name == "lat")
                            double.TryParse(attr.Value, out onepoint.latitude);
                        else if (attr.Name == "lon")
                            double.TryParse(attr.Value, out onepoint.longitude);
                    }

                    foreach(XmlNode trkptsubnode in trkptnode.ChildNodes)
                    {
                        if (trkptsubnode.Name == "ele")
                            double.TryParse(trkptsubnode.InnerText, out onepoint.altitude);
                        //else if (trkptsubnode.Name == "time")
                        //    onepoint.time = trkptsubnode.InnerText;
                    }

                    track.points.Add(onepoint);
                }
            }

            tracks.Add(track);
        }


        return tracks;
    }
}