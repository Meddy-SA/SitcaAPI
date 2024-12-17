namespace Sitca.Models.Enums;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class MultiLanguageDisplayAttribute : Attribute
{
  public string Spanish { get; set; }
  public string English { get; set; }

  public MultiLanguageDisplayAttribute(string spanish, string english)
  {
    Spanish = spanish;
    English = english;
  }
}
