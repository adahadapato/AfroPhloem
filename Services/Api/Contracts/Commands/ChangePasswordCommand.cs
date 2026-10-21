namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>
/// Body of <c>PUT /api/me/password</c>. Same shape as the API's <c>ChangePasswordCommand</c>.
/// The API signs the user out on every device afterwards.
/// </summary>
public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword, string ConfirmNewPassword);
