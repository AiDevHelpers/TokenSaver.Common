namespace AiDevTools.TokenSaver.Common.Core.Contracts;

/// <summary>Severity for captured debugger events and problems.</summary>
public enum DebugSeverity
{
    Info,
    Warning,
    Error,
    Fatal
}

/// <summary>Captures and queries structured debugger events recorded by AiDevTools.TokenSaver.</summary>
public interface IDebuggerEventCapture
{
    void RecordToolExecution(string toolName, long durationMs, string? clientName = null,
        string? clientVersion = null, string? modelHint = null, bool isError = false, string? errorMessage = null);

    void RecordException(Exception exception, string? toolName = null, string? clientName = null, string? sessionId = null);

    void RecordStateChange(string stateChange, string? sessionId = null, string? clientName = null,
        Dictionary<string, object>? data = null);

    void RecordMetric(string metricName, double value, string? unit = null, string? toolName = null, string? clientName = null);

    IReadOnlyList<DebuggerEvent> GetAllEvents(int? limitCount = null);

    IReadOnlyList<DebuggerEvent> QueryEvents(DebuggerEventType? eventType = null, DebugSeverity? severity = null,
        string? clientName = null, string? toolName = null, DateTime? startTime = null, DateTime? endTime = null,
        int? limitCount = null);

    DebuggerEventStats GetEventStats();

    void Clear();
}

public sealed record DebuggerEvent(
    Guid Id,
    DateTime Timestamp,
    DebuggerEventType EventType,
    DebugSeverity Severity,
    DebuggerCallerInfo CallerInfo,
    string? ToolName,
    TimeSpan? Duration,
    Dictionary<string, object> Data,
    string? StackTrace,
    string? SessionId);

public sealed record DebuggerCallerInfo(string? ClientName, string? ClientVersion, string? ModelHint, string? IdeType);

public enum DebuggerEventType
{
    ToolExecution,
    Exception,
    StateChange,
    Metric,
    SessionEvent,
    Other
}

public sealed record DebuggerEventStats(
    int TotalEventCount,
    Dictionary<DebuggerEventType, int> EventCountByType,
    Dictionary<DebugSeverity, int> EventCountBySeverity,
    Dictionary<string, int> EventCountByClient,
    DateTime? OldestEventTime,
    DateTime? NewestEventTime);

/// <summary>Captures and queries code and runtime problems analyzed by AiDevTools.TokenSaver.</summary>
public interface IDebuggerProblemCapture
{
    void RecordProblem(string problemType, string title, string? filePath = null, int? lineNumber = null,
        int? columnNumber = null, string? details = null, string? suggestion = null, string? toolName = null,
        string? clientName = null, string? ruleId = null);

    IReadOnlyList<DebuggerProblem> GetAllProblems(int? limitCount = null);

    IReadOnlyList<DebuggerProblem> QueryProblems(string? problemType = null, DebugSeverity? severity = null,
        string? filePath = null, string? toolName = null, string? clientName = null, DateTime? startTime = null,
        DateTime? endTime = null, int? limitCount = null);

    IReadOnlyDictionary<string, IReadOnlyList<DebuggerProblem>> GetProblemsByFile(string? filterProblemType = null);

    IReadOnlyDictionary<DebugSeverity, IReadOnlyList<DebuggerProblem>> GetProblemsBySeverity(string? filterProblemType = null);

    DebuggerProblemStats GetProblemStats();

    void Clear();
}

public sealed record DebuggerProblem(
    Guid Id,
    DateTime Timestamp,
    string ProblemType,
    DebugSeverity Severity,
    string Title,
    string? FilePath,
    int? LineNumber,
    int? ColumnNumber,
    string? Details,
    string? Suggestion,
    string? ToolName,
    string? ClientName,
    string? RuleId,
    bool IsResolved = false);

public sealed record DebuggerProblemStats(
    int TotalProblemCount,
    int ErrorCount,
    int WarningCount,
    Dictionary<string, int> ProblemCountByType,
    Dictionary<string, int> ProblemCountByFile,
    IReadOnlyList<string> AffectedFiles,
    DateTime? OldestProblemTime,
    DateTime? NewestProblemTime);
