namespace InvoiceManagementSystem.API.DTOs;

/// <summary>
/// ApiResponse — Format standard i përgjigjes për të gjitha endpoint-et.
/// Përdoret për të kthyer të dhëna, mesazhe suksesi ose gabime në mënyrë uniforme.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public IEnumerable<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}

/// <summary>
/// Variant pa tip gjeneric — për përgjigje pa të dhëna (p.sh. DELETE).
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse OkNoData(string? message = null) =>
        new() { Success = true, Message = message };

    public static new ApiResponse Fail(string message, IEnumerable<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}
