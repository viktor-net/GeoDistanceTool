using System;
using Xunit;
using GeoDistanceTool.Cli;

namespace GeoDistanceTool.Tests;

/// <summary>
/// Тесты для основного алгоритма расчёта расстояния (формула гаверсинусов).
/// Проверяют корректность математических вычислений для различных сценариев.
/// </summary>
public class GeoCalculatorTests
{
    private const double ToleranceKm = 0.001;

    /// <summary>
    /// Проверяет, что расстояние между одинаковыми координатами равно 0 км.
    /// </summary>
    [Fact]
    public void Calculate_SameCoordinates_ReturnsZero()
    {
        var distance = GeoCalculator.Calculate(55.75, 37.62, 55.75, 37.62);
        Assert.Equal(0, distance, tolerance: ToleranceKm);
    }

    /// <summary>
    /// Проверяет расчёт расстояния между Москвой и Санкт-Петербургом.
    /// Ожидаемое расстояние: ~635 км (по большому кругу).
    /// </summary>
    [Fact]
    public void Calculate_MoscowToSpb_ReturnsApprox635Km()
    {
        var distance = GeoCalculator.Calculate(55.75, 37.62, 59.93, 30.33);
        Assert.InRange(distance, 630, 640);
    }

    /// <summary>
    /// Проверяет расчёт расстояния между противоположными точками на экваторе (антиподы).
    /// Ожидаемое расстояние: половина длины экватора (π × R ≈ 20015 км).
    /// </summary>
    [Fact]
    public void Calculate_Antipodes_ReturnsHalfCircumference()
    {
        var distance = GeoCalculator.Calculate(0, 0, 0, 180);
        var expected = Math.PI * 6371.0;
        Assert.Equal(expected, distance, tolerance: 1.0);
    }

    /// <summary>
    /// Проверяет расчёт расстояния от Северного до Южного полюса вдоль одного меридиана.
    /// Ожидаемое расстояние: половина длины меридиана (π × R ≈ 20015 км).
    /// </summary>
    [Fact]
    public void Calculate_Poles_ReturnsHalfMeridianLength()
    {
        var distance = GeoCalculator.Calculate(90, 0, -90, 0);
        var expected = Math.PI * 6371.0;
        Assert.Equal(expected, distance, tolerance: 1.0);
    }
}