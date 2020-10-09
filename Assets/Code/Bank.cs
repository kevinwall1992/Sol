using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bank : User.Script
{
    public string Name;

    public IEnumerable<BankAccount> BankAccounts
    { get { return GetComponentsInChildren<BankAccount>(); } }

    public BankAccount OpenBankAccount(User user, float initial_deposit = 0)
    {
        BankAccount bank_account = GameObject.Instantiate(BankAccountPrefab);
        bank_account.transform.SetParent(transform);

        bank_account.User = user;
        bank_account.Balance = initial_deposit;

        return bank_account;
    }


    public BankAccount BankAccountPrefab;
}
