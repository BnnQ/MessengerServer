using System.Text.Json;
using System.Text.Json.Serialization;

namespace MessengerServer.Extensions;

public static class ObjectExtensions
{
    public static Dictionary<string, string> SerializeToDictionary(this object obj)
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        return obj.GetType()
            .GetProperties()
            .ToDictionary(
                p => p.Name,
                p =>
                {
                    var value = p.GetValue(obj);
                    if (value == null)
                        return "null";

                    if (value is string s)
                        return s;

                    if (value is Enum e)
                        return e.ToString();

                    var serializedValue = JsonSerializer.Serialize(value, jsonSerializerOptions);
                    return serializedValue;
                }
            );
    }


    private static bool IsValidJson(string stringValue)
    {
        if (string.IsNullOrWhiteSpace(stringValue) || stringValue.Length < 2)
            return false;

        var trimValue = stringValue.Trim();

        if ((trimValue.StartsWith("{") && trimValue.EndsWith("}")) || //For object
            (trimValue.StartsWith("[") && trimValue.EndsWith("]"))) //For array
            try
            {
                var obj = JsonSerializer.Deserialize<object>(trimValue);
                return true;
            }
            catch
            {
                return false;
            }

        return false;
    }
}