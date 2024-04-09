using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeoDistanceTool.Cli;

public sealed class GeoDistanceCommand : Command<GeoDistanceSettings>
{
    protected override int Execute(CommandContext context, GeoDistanceSettings settings, CancellationToken cancellationToken)
    {
        // 1. Проверяем наличие всех параметров
        bool allProvided = settings.Lat1.HasValue && settings.Lon1.HasValue &&
                           settings.Lat2.HasValue && settings.Lon2.HasValue;

        // 2. Проверяем валидность диапазонов (только если параметры переданы)
        bool allValid = allProvided &&
                        IsInRange(settings.Lat1!.Value, -90, 90) &&
                        IsInRange(settings.Lon1!.Value, -180, 180) &&
                        IsInRange(settings.Lat2!.Value, -90, 90) &&
                        IsInRange(settings.Lon2!.Value, -180, 180);

        double l1, n1, l2, n2;

        if (allValid)
        {
            // ✅ Режим прямой передачи параметров
            l1 = settings.Lat1.Value;
            n1 = settings.Lon1.Value;
            l2 = settings.Lat2.Value;
            n2 = settings.Lon2.Value;
        }
        else
        {
            // 🔄 Фоллбэк в интерактивный режим
            if (allProvided)
                AnsiConsole.MarkupLine("[yellow]⚠️ Переданные координаты вне допустимого диапазона. Переключаюсь на интерактивный ввод...[/]");
            else
                AnsiConsole.MarkupLine("[yellow]⚠️ Указаны не все параметры. Переключаюсь на интерактивный ввод...[/]");

            AnsiConsole.MarkupLine("\n[bold blue]🌍 Калькулятор расстояний (интерактивный режим)[/]\n");

            l1 = PromptCoordinate("Широта первой точки", -90, 90);
            n1 = PromptCoordinate("Долгота первой точки", -180, 180);
            l2 = PromptCoordinate("Широта второй точки", -90, 90);
            n2 = PromptCoordinate("Долгота второй точки", -180, 180);
        }

        var dist = GeoCalculator.Calculate(l1, n1, l2, n2);
        AnsiConsole.MarkupLine($"\n[bold green]✅ Расстояние:[/] {dist:F3} км");
        return 0;
    }

    private static bool IsInRange(double val, double min, double max) => val >= min && val <= max;

    private double PromptCoordinate(string label, double min, double max)
    {
        return AnsiConsole.Prompt(new TextPrompt<double>($"[cyan]{label}:[/] ")
            .Validate(value => value >= min && value <= max
                ? ValidationResult.Success()
                : ValidationResult.Error($"Значение должно быть от {min} до {max}")));
    }
}
