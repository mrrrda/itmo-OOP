using System.Text.Json;

namespace MessagingSystem;

public class MessagingSystemConfiguration
{
    public static void ParseTo(ApplicationService applicationService, string path)
    {
        if (applicationService is null)
            throw new ArgumentNullException(nameof(applicationService), "Invalid application service");

        new FileInfo(path);

        string fullPath = System.IO.Path.Combine(path, "messagingSystem.json");

        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

        string json = JsonSerializer.Serialize(applicationService, jsonOptions);
        File.WriteAllText(fullPath, json);
    }

    public static ApplicationService ParseFrom(string path)
    {
        new FileInfo(path);

        ApplicationService? applicationService = JsonSerializer.Deserialize<ApplicationService>(File.ReadAllText(path));

        if (applicationService is null)
            throw new ArgumentException();

        return applicationService;
    }
}
