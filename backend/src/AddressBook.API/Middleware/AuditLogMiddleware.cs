using System.Security.Claims;
using AddressBook.Application.Interfaces;

namespace AddressBook.API.Middleware;

/// <summary>
/// データアクセス・変更操作を自動的に監査ログに記録するミドルウェア
/// </summary>
public class AuditLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLogMiddleware> _logger;

    public AuditLogMiddleware(RequestDelegate next, ILogger<AuditLogMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
    {
        // リクエストを処理
        await _next(context);

        // 監査ログ対象のリクエストかチェック
        if (ShouldAudit(context))
        {
            await LogRequestAsync(context, auditLogService);
        }
    }

    private bool ShouldAudit(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;
        var method = context.Request.Method.ToUpper();

        // 監査対象: /api/contacts へのデータ変更操作（POST, PUT, DELETE）
        // および /api/auth へのログイン・ログアウト操作
        if (path.StartsWith("/api/contacts"))
        {
            return method is "POST" or "PUT" or "DELETE" or "GET";
        }

        if (path.StartsWith("/api/auth"))
        {
            return method == "POST"; // ログイン・ログアウト
        }

        return false;
    }

    private async Task LogRequestAsync(HttpContext context, IAuditLogService auditLogService)
    {
        try
        {
            var userId = GetUserId(context);
            var action = DetermineAction(context);
            var resourceType = DetermineResourceType(context);
            var resourceId = ExtractResourceId(context);
            var ipAddress = GetIpAddress(context);
            var userAgent = GetUserAgent(context);

            await auditLogService.LogAsync(userId, action, resourceType, resourceId, ipAddress, userAgent);
        }
        catch (Exception ex)
        {
            // 監査ログの記録エラーはメイン処理に影響を与えない
            _logger.LogError(ex, "監査ログミドルウェアでエラーが発生しました");
        }
    }

    private Guid? GetUserId(HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private string DetermineAction(HttpContext context)
    {
        var method = context.Request.Method.ToUpper();
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

        if (path.Contains("/auth/login"))
        {
            return "Login";
        }

        if (path.Contains("/auth/logout"))
        {
            return "Logout";
        }

        return method switch
        {
            "GET" => "Read",
            "POST" => "Create",
            "PUT" => "Update",
            "DELETE" => "Delete",
            _ => "Unknown"
        };
    }

    private string DetermineResourceType(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

        if (path.Contains("/contacts"))
        {
            return "Contact";
        }

        if (path.Contains("/auth"))
        {
            return "Session";
        }

        return "Unknown";
    }

    private Guid? ExtractResourceId(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // /api/contacts/{id} のようなパスからIDを抽出
        if (segments.Length >= 3 && segments[1] == "contacts")
        {
            if (Guid.TryParse(segments[2], out var resourceId))
            {
                return resourceId;
            }
        }

        return null;
    }

    private string? GetIpAddress(HttpContext context)
    {
        // X-Forwarded-For ヘッダーをチェック（プロキシ経由の場合）
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // 複数のIPがある場合は最初のもの（クライアントのIP）を使用
            return forwardedFor.Split(',')[0].Trim();
        }

        // X-Real-IP ヘッダーをチェック
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // RemoteIpAddress を使用
        return context.Connection.RemoteIpAddress?.ToString();
    }

    private string? GetUserAgent(HttpContext context)
    {
        return context.Request.Headers["User-Agent"].FirstOrDefault();
    }
}

/// <summary>
/// AuditLogMiddleware の拡張メソッド
/// </summary>
public static class AuditLogMiddlewareExtensions
{
    public static IApplicationBuilder UseAuditLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuditLogMiddleware>();
    }
}
