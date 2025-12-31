using ExpenseManagementAPI.Models;

namespace ExpenseManagementAPI.Repositories
{
    public interface IPasswordResetRepository
    {
        Task<int> CreateTokenAsync(PasswordResetToken token);
        Task<PasswordResetToken?> GetValidTokenAsync(int userId, string token);
        Task<bool> MarkTokenAsUsedAsync(int tokenId);
        Task<bool> DeleteExpiredTokensAsync(int userId);
    }
}