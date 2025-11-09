namespace PatientApp.SharedKernel.Results;

public sealed class Error
{
    public string Message { get; }

    public Error(string? message)
    {
        Message = message ?? string.Empty;
    }

    public override string ToString() => Message;
}