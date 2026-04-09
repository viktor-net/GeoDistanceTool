using GeoDistanceTool.Cli;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

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

#region WinAPI для определения владельца консольного окна
[DllImport("kernel32.dll")]
static extern IntPtr GetConsoleWindow();
[DllImport("user32.dll")]
static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

static bool IsRunningInOwnConsoleWindow()
{
    if (!OperatingSystem.IsWindows()) return false;
    var consoleWnd = GetConsoleWindow();
    if (consoleWnd == IntPtr.Zero) return false;
    GetWindowThreadProcessId(consoleWnd, out uint windowOwnerPid);
    return windowOwnerPid == (uint)Environment.ProcessId;
}
#endregion

#region Обработка Ctrl+C
bool wasCancelled = false;
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    wasCancelled = true;  // ← флаг ставим
    AnsiConsole.MarkupLine("\n[yellow]⚠️ Операция отменена пользователем (Ctrl+C).[/]");

    Environment.Exit(130);  // ← ДОБАВЬТЕ ЭТУ СТРОКУ — это главное!
};
#endregion

try
{
    var exitCode = app.Run(args);

    bool shouldPause =
        OperatingSystem.IsWindows() &&
        IsRunningInOwnConsoleWindow() &&
        !Console.IsInputRedirected &&
        !Console.IsOutputRedirected;

    if (shouldPause && !wasCancelled)
    {
        AnsiConsole.MarkupLine("\n[dim]Нажмите любую клавишу для выхода...[/]");
        Console.ReadKey(intercept: true);
    }

    return exitCode;
}
catch (OperationCanceledException)
{
    AnsiConsole.MarkupLine("\n[yellow]⚠️ Операция отменена.[/]");
    return 0;
}
finally
{
    if (wasCancelled &&
        OperatingSystem.IsWindows() &&
        IsRunningInOwnConsoleWindow() &&
        !Console.IsInputRedirected &&
        !Console.IsOutputRedirected)
    {
        AnsiConsole.MarkupLine("[dim]Нажмите любую клавишу для выхода...[/]");
        Console.ReadKey(intercept: true);
    }
}