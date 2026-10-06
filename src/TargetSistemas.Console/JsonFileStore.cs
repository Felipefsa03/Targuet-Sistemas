using System.Text.Json;
using System.Text.Json.Serialization;

namespace TargetSistemas.App;

internal static class JsonFileStore
{
    private static readonly JsonSerializerOptions Options = CriarOpcoes();

    public static T Ler<T>(string caminho)
    {
        if (!File.Exists(caminho))
            throw new FileNotFoundException($"Arquivo não encontrado: {caminho}", caminho);

        var json = File.ReadAllText(caminho);
        return JsonSerializer.Deserialize<T>(json, Options)
               ?? throw new JsonException($"Não foi possível desserializar o arquivo: {caminho}");
    }

    public static void Salvar<T>(string caminho, T dados)
    {
        var diretorio = Path.GetDirectoryName(caminho);
        if (!string.IsNullOrWhiteSpace(diretorio))
            Directory.CreateDirectory(diretorio);

        var json = JsonSerializer.Serialize(dados, Options);
        File.WriteAllText(caminho, json);
    }

    private static JsonSerializerOptions CriarOpcoes()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
