using UnityEngine;

public class DataLogInfo : MonoBehaviour
{
    // Data Log Content
    [TextArea(3, 10)]
    public string logTitle = "First Contact";

    [TextArea(5, 15)]
    public string logContent = "This is your first data log. Use these to learn about the world.";

    public DataLogType logType;
}

public enum DataLogType
{
    Tutorial,
    Story,
    Hint,
    Warning
}