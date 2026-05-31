using System.Net;
using System.Text.Json;
using AddressBook.Application.DTOs;

namespace AddressBook.API.Middleware;

/// <summary>
/// グローバル例外ハンドラーミドルウェア
/// すべての未処理例外をキャッチし、エラーIDを付与してログに記録する
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // エラーIDを生成（ログ追跡用）
        var errorId = $"err_{DateTime.UtcNow:yyyyMMdd}_{Guid.NewGuid():N}";
        var timestamp = DateTime.UtcNow;

        // 構造化ログでエラーを記録
        _logger.LogError(
            exception,
            "Unhandled exception occurred. ErrorId: {ErrorId}, Path: {Path}, Method: {Method}, UserId: {UserId}, IpAddress: {IpAddress}",
            errorId,
            context.Request.Path,
            context.Request.Method,
            context.User?.Identity?.Name ?? "Anonymous",
            context.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
        );

        // レスポンスの設定
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // エラーレスポンスの作成
        var errorResponse = new ErrorResponse
        {
            Message = "システムエラーが発生しました。もう一度お試しください。",
            ErrorId = errorId,
            Timestamp = timestamp
        };

        // JSONシリアライズオプション
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        var json = JsonSerializer.Serialize(errorResponse, options);
        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// グローバル例外ハンドラーミドルウェアの拡張メソッド
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
