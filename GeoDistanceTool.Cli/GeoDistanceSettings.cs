using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace GeoDistanceTool.Cli;

public class GeoDistanceSettings : CommandSettings
{
    [CommandOption("--lat1")]
    [Description("Широта первой точки (-90 до 90)")]
    public double? Lat1 { get; init; }

    [CommandOption("--lon1")]
    [Description("Долгота первой точки (-180 до 180)")]
    public double? Lon1 { get; init; }

    [CommandOption("--lat2")]
    [Description("Широта второй точки (-90 до 90)")]
    public double? Lat2 { get; init; }

    [CommandOption("--lon2")]
    [Description("Долгота второй точки (-180 до 180)")]
    public double? Lon2 { get; init; }
}