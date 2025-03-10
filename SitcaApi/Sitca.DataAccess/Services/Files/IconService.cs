using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Sitca.DataAccess.Services.Files;

public class IconService : IIconService
{
    private readonly Dictionary<string, string> _iconCache = new Dictionary<string, string>();
    private readonly IWebHostEnvironment _environment;

    public IconService(IWebHostEnvironment environment)
    {
        _environment = environment;
        InitializeIcons();
    }

    private void InitializeIcons()
    {
        // Puedes cargar desde archivos físicos solo una vez al iniciar la aplicación
        var iconPath = Path.Combine(_environment.WebRootPath, "icons");

        // Convertir las imágenes a base64 y almacenarlas en caché
        _iconCache["pdf"] = ConvertImageToBase64(Path.Combine(iconPath, "pdf-icon.png"));
        _iconCache["word"] = ConvertImageToBase64(Path.Combine(iconPath, "word-icon.png"));
        _iconCache["excel"] = ConvertImageToBase64(Path.Combine(iconPath, "excel-icon.png"));
        _iconCache["ppt"] = ConvertImageToBase64(Path.Combine(iconPath, "ppt-icon.png"));
        _iconCache["image"] = ConvertImageToBase64(Path.Combine(iconPath, "image-icon.png"));
    }

    private string ConvertImageToBase64(string imagePath)
    {
        if (!File.Exists(imagePath))
            return _iconCache.GetValueOrDefault("default");

        byte[] imageBytes = File.ReadAllBytes(imagePath);
        string base64String = Convert.ToBase64String(imageBytes);
        string extension = Path.GetExtension(imagePath).ToLower();
        string mimeType = GetMimeTypeForExtension(extension);

        return $"data:{mimeType};base64,{base64String}";
    }

    private string GetMimeTypeForExtension(string extension)
    {
        return extension switch
        {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            _ => "image/png",
        };
    }

    public string GetIconForFileType(string fileExtension)
    {
        if (string.IsNullOrEmpty(fileExtension))
            return _iconCache["default"];

        fileExtension = fileExtension.TrimStart('.').ToLower();

        return fileExtension switch
        {
            "pdf" => _iconCache["pdf"],
            "doc" or "docx" => _iconCache["word"],
            "xls" or "xlsx" => _iconCache["excel"],
            "ppt" or "pptx" => _iconCache["ppt"],
            "jpg" or "jpeg" or "png" or "jfig" => _iconCache["image"],
            _ => _iconCache["default"],
        };
    }
}
