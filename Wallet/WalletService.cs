using System.Collections.Concurrent;
using PawRace.Models.DTO;
namespace Wallet
{
    public class WalletService : IWalletService
    {
        private readonly ConcurrentDictionary<Guid, InMemoryWallet> _wallets = new();
        private const decimal InitialBalance = 100M;

        public WalletService()
        {
        }

        public decimal? CheckIfWalletExists(Guid user)
        {
            if(_wallets.TryGetValue(user,out var wallet))
            {
               return wallet.Balance;
            }

            return null;
    
        }

        public decimal GetBalance(Guid user)
        {
            if (_wallets.TryGetValue(user, out var wallet))
            {
                return wallet.Balance;
            }

            var newWallet = new InMemoryWallet(user);
            _wallets[user] = newWallet;
            
            return newWallet.Balance;
        }

        public bool TryWithdraw(Guid user, decimal amount)
        {
            if(_wallets.TryGetValue(user, out var wallet))
            {
                lock(wallet)
                {
                    if(wallet.Balance >= amount)
                    {
                        wallet.Balance -= amount;
                        return true;
                    }
                }
            }

            return false;
        }
        public void Deposit(Guid user, decimal amount)
        {

            var wallet = _wallets.GetOrAdd(user, id => new InMemoryWallet(id));

            lock (wallet)
            {
                wallet.Balance += amount;
            }
        }

        public void Refund(Guid user, decimal amount)
        {
            if (_wallets.TryGetValue(user, out var wallet))
            {
                lock (wallet)
                {
                    wallet.Balance += amount;
                }
            }
        }
    }
}
