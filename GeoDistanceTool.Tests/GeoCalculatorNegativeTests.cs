using GeoDistanceTool.Cli;
using System;
using Xunit;

namespace GeoDistanceTool.Tests;

/// <summary>
/// Негативные тесты для GeoCalculator.
/// Проверяют устойчивость алгоритма к некорректным входным данным:
/// NaN, Infinity, значения за пределами допустимых диапазонов.
/// </summary>
public class GeoCalculatorNegativeTests
{
    private const double ToleranceKm = 0.001;

    /// <summary>
    /// Проверяет, что если хотя бы одна координата равна NaN,
    /// результат расчёта также будет NaN (стандартное поведение IEEE 754).
    /// </summary>
    [Theory]
    [InlineData(double.NaN, 0, 0, 0)]
    [InlineData(0, double.NaN, 0, 0)]
    [InlineData(0, 0, double.NaN, 0)]
    [InlineData(0, 0, 0, double.NaN)]
    public void Calculate_AnyNaN_ReturnsNaN(double lat1, double lon1, double lat2, double lon2)
    {
        var distance = GeoCalculator.Calculate(lat1, lon1, lat2, lon2);
        Assert.True(double.IsNaN(distance));
    }

    /// <summary>
    /// Проверяет, что алгоритм не выбрасывает исключения при наличии
    /// бесконечных значений (PositiveInfinity/NegativeInfinity).
    /// Математические операции с Infinity не падают, а возвращают Infinity или NaN.
    /// </summary>
    [Theory]
    [InlineData(double.PositiveInfinity, 0, 0, 0)]
    [InlineData(0, double.NegativeInfinity, 0, 0)]
    [InlineData(0, 0, double.PositiveInfinity, 0)]
    [InlineData(0, 0, 0, double.NegativeInfinity)]
    public void Calculate_AnyInfinity_DoesNotThrow(double lat1, double lon1, double lat2, double lon2)
    {
        var ex = Record.Exception(() => GeoCalculator.Calculate(lat1, lon1, lat2, lon2));
        Assert.Null(ex);
    }

    /// <summary>
    /// Проверяет, что GeoCalculator не валидирует диапазон координат
    /// (это ответственность Command.Execute). Алгоритм должен просто
    /// выполнить математический расчёт, даже если координаты нереалистичны.
    /// </summary>
    [Theory]
    [InlineData(1000, 0, 0, 0)]
    [InlineData(0, 1000, 0, 0)]
    [InlineData(0, 0, -1000, 0)]
    [InlineData(0, 0, 0, -1000)]
    public void Calculate_CoordinatesOutOfRange_DoesNotCrash(double lat1, double lon1, double lat2, double lon2)
    {
        var ex = Record.Exception(() => GeoCalculator.Calculate(lat1, lon1, lat2, lon2));
        Assert.Null(ex);
    }

    /// <summary>
    /// Проверяет корректную обработку перехода через линию перемены дат (180° долготы).
    /// Точки (0°, 179°) и (0°, -179°) находятся на расстоянии ~222 км (2 градуса),
    /// а не ~40000 км (если бы считали "в лоб" без учёта цикличности долготы).
    /// </summary>
    [Fact]
    public void Calculate_LongitudeWrapAround180_WorksCorrectly()
    {
        var dist1 = GeoCalculator.Calculate(0, 179, 0, -179);
        var dist2 = GeoCalculator.Calculate(0, 180, 0, -180);

        Assert.True(dist1 < 250, "Расстояние должно быть ~222 км (2 градуса)");
        Assert.Equal(0, dist2, tolerance: ToleranceKm);
    }
}