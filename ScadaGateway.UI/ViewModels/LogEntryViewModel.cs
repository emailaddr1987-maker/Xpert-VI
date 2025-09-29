namespace ScadaGateway.UI.ViewModels;

public class LogEntryViewModel
{
    public DateTime Time { get; set; } = DateTime.Now;
    public string Source { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
