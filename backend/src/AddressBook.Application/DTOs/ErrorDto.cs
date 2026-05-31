namespace AddressBook.Application.DTOs;

/// <summary>
/// エラーレスポンスDTO
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// ユーザーフレンドリーなエラーメッセージ
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// ログ追跡用のユニークなエラーID
    /// </summary>
    public string ErrorId { get; set; } = string.Empty;

    /// <summary>
    /// エラー発生時刻（UTC）
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// バリデーションエラーレスポンスDTO
/// </summary>
public class ValidationErrorResponse
{
    /// <summary>
    /// エラーメッセージ
    /// </summary>
    public string Message { get; set; } = "入力データに誤りがあります。";

    /// <summary>
    /// フィールド別のバリデーションエラー
    /// </summary>
    public Dictionary<string, List<string>> Errors { get; set; } = new();
}
