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
            return Scene.The.Banks
                .SelectMany(bank => bank.BankAccounts
                .Where(account => account.User == this));
        }
    }

    public IEnumerable<Craft> Crafts
    { get { return Scene.The.Crafts.Where(craft => craft.Owner == this); } }

    private void Start()
    {
        if (BankAccounts.Count() > 0)
            PrimaryBankAccount = BankAccounts.First();
        else
            PrimaryBankAccount =
                Scene.The.DefaultBank.OpenBankAccount(this);
    }

    private void Update()
    {
        if (PrimaryBankAccount == null && BankAccounts.Count() > 0)
             BankAccounts.First();
    }

    [RequireComponent(typeof(User))]
    public class Script : MonoBehaviour
    {
        public User User { get { return GetComponent<User>(); } }
    }
}
