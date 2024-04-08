using System.ComponentModel;
using System.Text;
using GeoDistanceTool.Cli;
using Spectre.Console;
using Spectre.Console.Cli;

# region Для рендеринга эмодзи в консоли
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;
#endregion

var app = new CommandApp<GeoDistanceCommand>();

# region Пример
// При --help
app.Configure(config =>
{
    config.SetApplicationName("GeoDistanceTool");
    config.AddExample(new[] { "--lat1", "55.75", "--lon1", "37.62", "--lat2", "59.93", "--lon2", "30.33" });
});
#endregion

try
{
    return app.Run(args);
}
catch (OperationCanceledException)
{
    AnsiConsole.MarkupLine("\n[yellow]⚠️ Операция отменена пользователем (Ctrl+C).[/]");
    return 0;
}