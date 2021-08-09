using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class User : MonoBehaviour
{
    public string Username, Password;

    public BankAccount PrimaryBankAccount;

    public IEnumerable<BankAccount> BankAccounts
    {
        get
        {
            return The.Banks
                .SelectMany(bank => bank.BankAccounts
                .Where(account => account.User == this));
        }
    }

    public IEnumerable<Craft> Crafts
    { get { return The.Crafts.Where(craft => craft.GetOwner() == this); } }

    private void Start()
    {
        if (BankAccounts.Count() > 0)
            PrimaryBankAccount = BankAccounts.First();
        else
            PrimaryBankAccount =
                The.DefaultBank.OpenBankAccount(this);
    }

    private void Update()
    {
        if (PrimaryBankAccount == null && BankAccounts.Count() > 0)
             BankAccounts.First();
    }

    public Inventory GetInventory(Craft craft)
    {
        return craft.GetInventory(this);
    }

    public Inventory GetInventory(Station station)
    {
        return station.Craft.GetInventory(this);
    }


    [RequireComponent(typeof(User))]
    public class Script : MonoBehaviour
    {
        public User User { get { return GetComponent<User>(); } }
    }
}
