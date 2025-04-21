using System.Collections.Concurrent;
using BankSim.Models;
using System.Threading;

namespace BankSim.Services
{
    public class BankService
    {
        private readonly ConcurrentDictionary<int, Account> _accounts;
        private readonly object _lock = new();

        public BankService(ConcurrentDictionary<int, Account> accounts)
        {
            _accounts = accounts;
        }

        public bool Transfer(int fromId, int toId, decimal amount)
        {
            if (!_accounts.ContainsKey(fromId) || !_accounts.ContainsKey(toId) || amount <= 0)
                return false;

            lock (_lock)
            {
                var fromAccount = _accounts[fromId];
                var toAccount = _accounts[toId];

                if (fromAccount.Balance < amount)
                    return false;

                fromAccount.Balance -= amount;
                toAccount.Balance += amount;

                return true;
            }
        }

        public void AddAccount(Account account)
        {
            _accounts.TryAdd(account.Id, account);
        }

        public IEnumerable<Account> GetAllAccounts() => _accounts.Values;

        public bool RemoveAccount(int id)
        {
            return _accounts.TryRemove(id, out _);
        }

        public decimal GetBalance(int accountId)
        {
            if (_accounts.TryGetValue(accountId, out var account))
            {
                return account.Balance;
            }
            throw new ArgumentException($"La cuenta con ID {accountId} no existe.");
        }
    }


}
