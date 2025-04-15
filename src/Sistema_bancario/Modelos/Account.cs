namespace BankSim.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string Owner { get; set; }
        public decimal Balance { get; set; }

        public Account(int id, string owner, decimal balance)
        {
            Id = id;
            Owner = owner;
            Balance = balance;
        }
    }
}
