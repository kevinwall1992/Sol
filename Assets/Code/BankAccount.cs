using UnityEngine;
using System.Collections;

public class BankAccount : MonoBehaviour
{
    public User User;

    public float Balance;

    public float Withdraw(float credits)
    {
        Balance -= credits;

        return credits;
    }

    public void Deposit(float credits)
    {
        Balance += credits;
    }
}
