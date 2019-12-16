using System;
using System.Threading;
using System.Threading.Tasks;
using LearnNetMq.Core;
using NetMQ;

namespace LearnNetMq.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            NetMqVersion();
        }

        static void NetMqVersion()
        {
            // CommandActioner uses an NetMq.Actor internally
            var accountActioner = new AccountActioner();
            var account = new Account(1, "Doron Somech", "112233", 0);
            PrintAccount(account);
            accountActioner.Start();
            Console.WriteLine("Sending account to AccountActioner/Actor");
            accountActioner.SendPayload(account,
                new AccountAction(TransactionType.Credit, 15));
            account = accountActioner.GetPayLoad();
            PrintAccount(account);
            accountActioner.Stop();
            Console.WriteLine();
            Console.WriteLine("Sending account to AccountActioner/Actor");
            accountActioner.SendPayload(account,
                new AccountAction(TransactionType.Credit, 15));
            accountActioner.SendPayload(account,
                new AccountAction(TransactionType.Credit, 15));
            accountActioner.SendPayload(account,
                new AccountAction(TransactionType.Credit, 15));
            PrintAccount(account);
            Console.ReadLine();
        }

        static void PrintAccount(Account account)
        {
            Console.WriteLine("Account now");
            Console.WriteLine(account);
            Console.WriteLine();
        }

        static void OriginalVersion()
        {
            var account = new Account(1, "sacha barber", "112233", 0);
            var syncLock = new object();
            // start two asynchronous tasks that both mutate the account balance
            var task1 = Task.Run(() =>
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;
                Console.WriteLine("Thread Id {0}, Account balance before: {1}",
                    threadId, account.Balance);
                lock (syncLock)
                {
                    Console.WriteLine("Thread Id {0}, Adding 10 to balance",
                        threadId);
                    account.Balance += 10;
                    Console.WriteLine("Thread Id {0}, Account balance after: {1}",
                        threadId, account.Balance);
                }
            });
            var task2 = Task.Run(() =>
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;
                Console.WriteLine("Thread Id {0}, Account balance before: {1}",
                    threadId, account.Balance);
                lock (syncLock)
                {
                    Console.WriteLine("Thread Id {0}, Subtracting 4 from balance",
                        threadId);
                    account.Balance -= 4;
                    Console.WriteLine("Thread Id {0}, Account balance after: {1}",
                        threadId, account.Balance);
                }
            });
            // wait for all tasks to complete
            task1.Wait();
            task2.Wait();
        }
    }
}
