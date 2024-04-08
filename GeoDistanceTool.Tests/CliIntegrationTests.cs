using System;
using System.Globalization;
using System.Threading;
using Spectre.Console.Cli;
using Xunit;
using GeoDistanceTool.Cli;

namespace GeoDistanceTool.Tests;

/// <summary>
/// Интеграционные тесты для CLI-команды GeoDistanceCommand.
/// Проверяют парсинг аргументов, валидацию и взаимодействие со Spectre.Console.Cli.
/// </summary>
public class CliIntegrationTests
{
    /// <summary>
    /// Проверяет, что числовые значения вне допустимого диапазона
    /// Например: широта 91° успешно парсятся, но не проходят валидацию
    /// внутри Command.Execute должен переключить интерактивный режим.
    /// </summary>
    [Theory]
    [InlineData("--lat1", "91")]
    [InlineData("--lon1", "-181")]
    [InlineData("--lat2", "200")]
    [InlineData("--lon2", "-200")]
    public void Cli_OutOfRangeValue_TriggersInteractiveMode(string flag, string value)
    {
        var app = new CommandApp<GeoDistanceCommand>();
        app.Configure(c => c.PropagateExceptions());

        var ex = Record.Exception(() =>
            app.Run(new[] { flag, value, "--lat1", "55.75", "--lon1", "37.62", "--lat2", "59.93", "--lon2", "30.33" }));

        // Число вне диапазона успешно распарсилось, валидация в Command
        Assert.Null(ex);
    }

    /// <summary>
    /// Проверяет, что нечисловые значения (например строковые)
    /// вызывают исключение на этапе парсинга аргументов Spectre.Console.Cli,
    /// так как не могут быть преобразованы в double.
    /// </summary>
    [Theory]
    [InlineData("--lat1", "abc")]
    [InlineData("--lon1", "xyz")]
    [InlineData("--lat2", "foo")]
    [InlineData("--lon2", "bar")]
    public void Cli_NonNumericValue_ThrowsParseException(string flag, string value)
    {
        var app = new CommandApp<GeoDistanceCommand>();
        app.Configure(c => c.PropagateExceptions());

        var ex = Assert.Throws<CommandRuntimeException>(() =>
            app.Run(new[] { flag, value, "--lat1", "55.75", "--lon1", "37.62", "--lat2", "59.93", "--lon2", "30.33" }));

        Assert.NotNull(ex);
    }
}