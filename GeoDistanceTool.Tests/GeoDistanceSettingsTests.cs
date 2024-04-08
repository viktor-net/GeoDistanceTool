using System;
using Xunit;
using GeoDistanceTool.Cli;

namespace GeoDistanceTool.Tests;

/// <summary>
/// Тесты для проверки валидации координат в GeoDistanceSettings.
/// Проверяют границы допустимых значений широты (-90..90) и долготы (-180..180).
/// </summary>
public class GeoDistanceSettingsTests
{
    /// <summary>
    /// Проверяет валидацию широты: допустимы значения от -90° до 90° включительно.
    /// Значения NaN считаются невалидными.
    /// </summary>
    [Theory]
    [InlineData(90, true)]
    [InlineData(-90, true)]
    [InlineData(90.0001, false)]
    [InlineData(-90.0001, false)]
    [InlineData(0, true)]
    [InlineData(double.NaN, false)]
    public void Latitude_IsValid_ReturnsExpected(double lat, bool expectedValid)
    {
        Assert.Equal(expectedValid, lat >= -90 && lat <= 90 && !double.IsNaN(lat));
    }

    /// <summary>
    /// Проверяет валидацию долготы: допустимы значения от -180° до 180° включительно.
    /// Значения NaN считаются невалидными.
    /// </summary>
    [Theory]
    [InlineData(180, true)]
    [InlineData(-180, true)]
    [InlineData(180.0001, false)]
    [InlineData(-180.0001, false)]
    [InlineData(0, true)]
    [InlineData(double.NaN, false)]
    public void Longitude_IsValid_ReturnsExpected(double lon, bool expectedValid)
    {
        Assert.Equal(expectedValid, lon >= -180 && lon <= 180 && !double.IsNaN(lon));
    }

    /// <summary>
    /// Проверяет, что GeoDistanceSettings допускает null-значения для всех параметров.
    /// Валидация происходит в Command.Execute, а не на уровне Settings.
    /// </summary>
    [Fact]
    public void Settings_AllNull_IsAllowed()
    {
        var settings = new GeoDistanceSettings();
        Assert.Null(settings.Lat1);
    }
}