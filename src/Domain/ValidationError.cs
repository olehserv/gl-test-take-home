namespace Domain;

/// <summary>
/// One validation failure: which field failed and why. The <see cref="Message"/>
/// describes the actual reason (not always "required"), so callers can report it.
/// </summary>
public sealed record ValidationError(string Field, string Message);
