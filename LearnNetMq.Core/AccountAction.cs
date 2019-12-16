namespace LearnNetMq.Core
{
    public class AccountAction
    {
        public AccountAction(TransactionType transactionType, decimal amount)
        {
            TransactionType = transactionType;
            Amount = amount;
        }

        public TransactionType TransactionType { get; set; }
        public decimal Amount { get; set; }
    }
}