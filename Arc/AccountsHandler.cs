using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Atomic.Arc
{
    public interface IAccountsHandler
    {
        IEnumerable<Account> Accounts { get; }
        string GetUsername(string accountNumber);
        void LoadAccounts();
        void AddAccount(string username, string accountNumber);
        void RemoveAccount(Account account);
    }
    public class AccountsHandler : IAccountsHandler
    {
        private readonly ArcConfig config;
        private List<Account> accounts;
        private readonly object busy = new object();
        private readonly IArcLog arcLog;

        public AccountsHandler(ArcConfig config, IArcLog arcLog)
        {
            this.config = config;
            this.arcLog = arcLog;
            LoadAccounts();
        }

        public string GetUsername(string accountNumber)
        {
            lock(busy)
            {
                return accounts.FirstOrDefault(x => x.AccountNumber == accountNumber).Username;
            }
        }

        public void LoadAccounts()
        {
            arcLog.Log("Loading Account data");
            lock (busy)
            {
                accounts = File.Exists(config.AccountsFilename)
                    ? JsonConvert.DeserializeObject<List<Account>>(File.ReadAllText(config.AccountsFilename))
                    : new List<Account>();
                arcLog.Log($"Loaded {accounts.Count} accounts");
            }
        }

        private void SaveAccounts()
        {
            Console.WriteLine("Saving accounts");
            File.WriteAllText(config.AccountsFilename, JsonConvert.SerializeObject(accounts));
            Console.WriteLine("Accounts saved");
        }

        public void AddAccount(string username, string accountNumber)
        {
            Console.WriteLine($"Adding account - {accountNumber}({username})");
            lock (busy)
            {
                var account = new Account
                {
                    AccountNumber = accountNumber,
                    Username = username
                };
                accounts.Add(account);
                SaveAccounts();
            }
        }

        public void RemoveAccount(Account account)
        {
            Console.WriteLine($"Removing account - {account.AccountNumber}({account.Username})");
            lock (busy)
            {
                accounts.Remove(account);
                SaveAccounts();
            }
        }

        public IEnumerable<Account> Accounts
            => accounts;
    }

    public class Account
    {
        public string Username { get; set; } = "Unknown account";
        public string AccountNumber { get; set; }
    }
}
