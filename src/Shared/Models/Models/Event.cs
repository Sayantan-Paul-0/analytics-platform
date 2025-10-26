namespace Models;
/// <summary>
/// Represents an analytics event from various sources
/// </summary>
public class AnalyticsEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public string EventType { get; set; } = string.Empty;
    public string Source {get; set; } = string.Empty; //IoT,Web,mobile
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Payload { get; set; } = new ();
    public string UserId {get; set; } = string.Empty;
    public string SessionId {get; set; } = string.Empty;
    public Dictionary <string, string> Metadata {get; set; } = new ();
}

///<summary>
/// Event types supported by the system
/// </summary>

public static class EventTypes
{
    public const string PageView = "page_view";
    public const string ButtonClick = "button_click";
    public const string ApiCall = "api_call";
    public const string SensorReading = "sensor_reading";
    public const string UserAction = "user_action";
    public const string SystemMetric = "system_metric";
}

///<summary>
/// Event sources
/// </summary>

public static class EventSources
{
    public const string IoT = "iot";
    public const string Web = "web";
    public const string Mobile = "mobile";
    public const string Backend = "backend";
}
