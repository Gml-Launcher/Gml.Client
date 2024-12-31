using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Gml.Web.Api.Dto.Sentry;

public class App
{
    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("app_start_time")] public DateTime AppStartTime { get; set; }

    [JsonProperty("in_foreground")] public bool InForeground { get; set; }
}

public class Contexts
{
    [JsonProperty("Current Culture")] public CurrentCulture CurrentCulture { get; set; }

    [JsonProperty("Current UI Culture")] public CurrentUICulture CurrentUICulture { get; set; }

    [JsonProperty("Dynamic Code")] public DynamicCode DynamicCode { get; set; }

    [JsonProperty("Memory Info")] public MemoryInfo MemoryInfo { get; set; }

    [JsonProperty("ThreadPool Info")] public ThreadPoolInfo ThreadPoolInfo { get; set; }

    [JsonProperty("app")] public App App { get; set; }

    [JsonProperty("device")] public Device Device { get; set; }

    [JsonProperty("os")] public Os Os { get; set; }

    [JsonProperty("runtime")] public Runtime Runtime { get; set; }

    [JsonProperty("trace")] public Trace Trace { get; set; }
}

public class CurrentCulture
{
    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("display_name")] public string DisplayName { get; set; }

    [JsonProperty("calendar")] public string Calendar { get; set; }
}

public class CurrentUICulture
{
    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("display_name")] public string DisplayName { get; set; }

    [JsonProperty("calendar")] public string Calendar { get; set; }
}

public class DebugMeta
{
    [JsonProperty("images")] public List<Image> Images { get; set; }
}

public class Device
{
    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("timezone")] public string Timezone { get; set; }

    [JsonProperty("timezone_display_name")]
    public string TimezoneDisplayName { get; set; }

    [JsonProperty("boot_time")] public DateTime BootTime { get; set; }
}

public class DynamicCode
{
    [JsonProperty("Compiled")] public bool Compiled { get; set; }

    [JsonProperty("Supported")] public bool Supported { get; set; }
}

public class Exception
{
    [JsonProperty("values")] public List<Value> Values { get; set; }
}

public class Frame
{
    [JsonProperty("filename")] public string Filename { get; set; }

    [JsonProperty("function")] public string Function { get; set; }

    [JsonProperty("lineno")] public int Lineno { get; set; }

    [JsonProperty("colno")] public int Colno { get; set; }

    [JsonProperty("abs_path")] public string AbsPath { get; set; }

    [JsonProperty("in_app")] public bool InApp { get; set; }

    [JsonProperty("package")] public string Package { get; set; }

    [JsonProperty("instruction_addr")] public string InstructionAddr { get; set; }

    [JsonProperty("addr_mode")] public string AddrMode { get; set; }

    [JsonProperty("function_id")] public string FunctionId { get; set; }
}

public class Image
{
    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("debug_id")] public string DebugId { get; set; }

    [JsonProperty("debug_checksum")] public string DebugChecksum { get; set; }

    [JsonProperty("debug_file")] public string DebugFile { get; set; }

    [JsonProperty("code_id")] public string CodeId { get; set; }

    [JsonProperty("code_file")] public string CodeFile { get; set; }
}

public class MemoryInfo
{
    [JsonProperty("allocated_bytes")] public long AllocatedBytes { get; set; }

    [JsonProperty("high_memory_load_threshold_bytes")]
    public long HighMemoryLoadThresholdBytes { get; set; }

    [JsonProperty("total_available_memory_bytes")]
    public long TotalAvailableMemoryBytes { get; set; }

    [JsonProperty("finalization_pending_count")]
    public int FinalizationPendingCount { get; set; }

    [JsonProperty("compacted")] public bool Compacted { get; set; }

    [JsonProperty("concurrent")] public bool Concurrent { get; set; }

    [JsonProperty("pause_durations")] public List<double> PauseDurations { get; set; }
}

public class Modules
{
    [JsonProperty("System.Private.CoreLib")]
    public string SystemPrivateCoreLib { get; set; }

    [JsonProperty("Sentry.Test")] public string SentryTest { get; set; }

    [JsonProperty("System.Runtime")] public string SystemRuntime { get; set; }

    [JsonProperty("Sentry")] public string Sentry { get; set; }

    [JsonProperty("System.Net.Primitives")]
    public string SystemNetPrimitives { get; set; }

    [JsonProperty("System.IO.Compression")]
    public string SystemIOCompression { get; set; }

    [JsonProperty("System.Text.Json")] public string SystemTextJson { get; set; }

    [JsonProperty("System.Collections")] public string SystemCollections { get; set; }

    [JsonProperty("System.Text.RegularExpressions")]
    public string SystemTextRegularExpressions { get; set; }

    [JsonProperty("System.Reflection.Emit.Lightweight")]
    public string SystemReflectionEmitLightweight { get; set; }

    [JsonProperty("System.Threading")] public string SystemThreading { get; set; }

    [JsonProperty("System.Reflection.Emit.ILGeneration")]
    public string SystemReflectionEmitILGeneration { get; set; }

    [JsonProperty("System.Memory")] public string SystemMemory { get; set; }

    [JsonProperty("System.Reflection.Primitives")]
    public string SystemReflectionPrimitives { get; set; }

    [JsonProperty("System.Linq")] public string SystemLinq { get; set; }

    [JsonProperty("System.Console")] public string SystemConsole { get; set; }

    [JsonProperty("System.Text.Encoding.Extensions")]
    public string SystemTextEncodingExtensions { get; set; }

    [JsonProperty("System.Runtime.InteropServices")]
    public string SystemRuntimeInteropServices { get; set; }

    [JsonProperty("System.Diagnostics.Process")]
    public string SystemDiagnosticsProcess { get; set; }

    [JsonProperty("System.Private.Uri")] public string SystemPrivateUri { get; set; }

    [JsonProperty("System.ComponentModel.Primitives")]
    public string SystemComponentModelPrimitives { get; set; }

    [JsonProperty("Microsoft.Win32.Primitives")]
    public string MicrosoftWin32Primitives { get; set; }

    [JsonProperty("System.Diagnostics.StackTrace")]
    public string SystemDiagnosticsStackTrace { get; set; }

    [JsonProperty("System.Net.Http")] public string SystemNetHttp { get; set; }

    [JsonProperty("System.Diagnostics.DiagnosticSource")]
    public string SystemDiagnosticsDiagnosticSource { get; set; }

    [JsonProperty("System.Collections.Concurrent")]
    public string SystemCollectionsConcurrent { get; set; }

    [JsonProperty("System.Reflection.Metadata")]
    public string SystemReflectionMetadata { get; set; }

    [JsonProperty("System.Threading.Thread")]
    public string SystemThreadingThread { get; set; }

    [JsonProperty("System.Threading.ThreadPool")]
    public string SystemThreadingThreadPool { get; set; }

    [JsonProperty("System.Collections.Immutable")]
    public string SystemCollectionsImmutable { get; set; }

    [JsonProperty("System.IO.MemoryMappedFiles")]
    public string SystemIOMemoryMappedFiles { get; set; }

    [JsonProperty("System.Linq.Expressions")]
    public string SystemLinqExpressions { get; set; }
}

public class Os
{
    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("raw_description")] public string RawDescription { get; set; }
}

public class Package
{
    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("version")] public string Version { get; set; }
}

public class Request
{
    [JsonProperty("query_string")] public string QueryString { get; set; }
}

public class SentryModulesDto
{
    [JsonProperty("modules")] public Modules Modules { get; set; }

    [JsonProperty("event_id")] public string EventId { get; set; }

    [JsonProperty("timestamp")] public DateTime Timestamp { get; set; }

    [JsonProperty("platform")] public string Platform { get; set; }

    [JsonProperty("server_name")] public string ServerName { get; set; }

    [JsonProperty("release")] public string Release { get; set; }

    [JsonProperty("exception")] public Exception Exception { get; set; }

    [JsonProperty("threads")] public Threads Threads { get; set; }

    [JsonProperty("level")] public string Level { get; set; }

    [JsonProperty("request")] public Request Request { get; set; }

    [JsonProperty("contexts")] public Contexts Contexts { get; set; }

    [JsonProperty("user")] public User User { get; set; }

    [JsonProperty("environment")] public string Environment { get; set; }

    [JsonProperty("sdk")] public Sdk Sdk { get; set; }

    [JsonProperty("debug_meta")] public DebugMeta DebugMeta { get; set; }
}

public class Runtime
{
    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("version")] public string Version { get; set; }

    [JsonProperty("raw_description")] public string RawDescription { get; set; }

    [JsonProperty("identifier")] public string Identifier { get; set; }
}

public class Stacktrace
{
    [JsonProperty("frames")] public List<Frame> Frames { get; set; }
}

public class ThreadPoolInfo
{
    [JsonProperty("min_worker_threads")] public int MinWorkerThreads { get; set; }

    [JsonProperty("min_completion_port_threads")]
    public int MinCompletionPortThreads { get; set; }

    [JsonProperty("max_worker_threads")] public int MaxWorkerThreads { get; set; }

    [JsonProperty("max_completion_port_threads")]
    public int MaxCompletionPortThreads { get; set; }

    [JsonProperty("available_worker_threads")]
    public int AvailableWorkerThreads { get; set; }

    [JsonProperty("available_completion_port_threads")]
    public int AvailableCompletionPortThreads { get; set; }
}

public class Threads
{
    [JsonProperty("values")] public List<Value> Values { get; set; }
}

public class User
{
    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("username")] public string Username { get; set; }

    [JsonProperty("ip_address")] public string IpAddress { get; set; }
}

public class Value
{
    [JsonProperty("type")] public string Type { get; set; }

    [JsonProperty("value")] public string ValueData { get; set; }

    [JsonProperty("module")] public string Module { get; set; }

    [JsonProperty("thread_id")] public int ThreadId { get; set; }

    [JsonProperty("id")] public int Id { get; set; }

    [JsonProperty("crashed")] public bool Crashed { get; set; }

    [JsonProperty("current")] public bool Current { get; set; }

    [JsonProperty("stacktrace")] public Stacktrace Stacktrace { get; set; }
}
