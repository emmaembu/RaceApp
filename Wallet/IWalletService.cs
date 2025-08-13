
namespace Wallet
{
    public interface IWalletService
    {
        decimal? CheckIfWalletExists(Guid user);

        decimal GetBalance(Guid user);

        bool TryWithdraw(Guid user, decimal amount);

        void Deposit(Guid user, decimal amount);

        void Refund(Guid user, decimal amount);
    }
}
