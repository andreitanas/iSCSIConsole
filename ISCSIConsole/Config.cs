using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ISCSIConsole
{
    public class Config
    {
        public ushort Port = 3260;
        public Logging Logging;
        public TargetConfig[] Targets;
    }

    public class TargetConfig
    {
        public string Name;
        public DiskConfig[] Disks;
    }

    public class DiskConfig
    {
        public string Name;
        public DiskKind Kind;
        public object Parameters;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DiskKind
    {
        Raw,
        External,
    }

    public class Logging
    {
        public string File;
        [JsonConverter(typeof(StringEnumConverter))]
        public ISCSI.Server.LogLevel Level;
        public bool LogToConsole;
    }
}
