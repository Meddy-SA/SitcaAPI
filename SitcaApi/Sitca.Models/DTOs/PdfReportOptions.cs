namespace Sitca.Models.DTOs;

public class PdfReportOptions
{
  public string HtmlContent { get; set; } = null!;
  public string Orientation { get; set; } = "default";
  public PdfMargins Margins { get; set; } = null!;
  public bool AddPageNumbers { get; set; } = true;
  public string Header { get; set; } = null!;
  public string Footer { get; set; } = null!;
  public string[] FontPaths { get; set; } = [];
}
