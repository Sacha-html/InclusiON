using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;

namespace InclusiON.Infrastructure.Templates
{
    /// <summary>
    /// Servicio para cargar y procesar templates HTML de emails.
    /// Los templates se leen de la carpeta Templates/Emails como embedded resources.
    /// Soporta placeholders {{Key}} y secciones condicionales {{#Key}}...{{/Key}}.
    /// </summary>
    public static class EmailTemplateService
    {
        private static readonly ConcurrentDictionary<string, string> _cache = new();

        /// <summary>
        /// Carga un template y reemplaza los placeholders con los valores proporcionados.
        /// </summary>
        /// <param name="templateName">Nombre del template sin extension (ej: "invitation")</param>
        /// <param name="replacements">Diccionario de placeholder -> valor</param>
        public static string Render(string templateName, Dictionary<string, string?> replacements)
        {
            var template = LoadTemplate(templateName);
            return ApplyReplacements(template, replacements);
        }

        private static string LoadTemplate(string templateName)
        {
            return _cache.GetOrAdd(templateName, name =>
            {
                // Buscar el template en la carpeta Templates/Emails relativa al assembly
                var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
                var templatePath = Path.Combine(assemblyDir, "Templates", "Emails", $"{name}.html");

                if (!File.Exists(templatePath))
                {
                    throw new FileNotFoundException($"Email template '{name}' no encontrado en: {templatePath}");
                }

                return File.ReadAllText(templatePath);
            });
        }

        private static string ApplyReplacements(string template, Dictionary<string, string?> replacements)
        {
            var result = template;

            // Procesar secciones condicionales: {{#Key}}contenido{{/Key}}
            // Si el valor existe y no es null/empty, muestra el contenido (con sus placeholders)
            // Si no, elimina toda la seccion
            result = Regex.Replace(result, @"\{\{#(\w+)\}\}(.*?)\{\{/\1\}\}", match =>
            {
                var key = match.Groups[1].Value;
                if (replacements.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
                {
                    return match.Groups[2].Value;
                }
                return string.Empty;
            }, RegexOptions.Singleline);

            // Reemplazar placeholders simples: {{Key}}
            foreach (var (key, value) in replacements)
            {
                result = result.Replace($"{{{{{key}}}}}", value ?? string.Empty);
            }

            return result;
        }
    }
}
