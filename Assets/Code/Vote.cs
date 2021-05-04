using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum Vote
{ Nay, Yay, Abstain }

public static class VoteExtensions
{
    public static Vote ToVote(this bool boolean)
    {
        return boolean ? global::Vote.Yay :
                         global::Vote.Nay;
    }

    public static bool HasMajority(this IEnumerable<Vote> votes)
    {
        return votes.Where(vote => vote == Vote.Yay).Count() >
               votes.Where(vote => vote == Vote.Nay).Count();
    }

    public static bool IsUnanimous(this IEnumerable<Vote> votes)
    {
        return votes.Count() > 0 && 
               !votes.Contains(global::Vote.Nay);
    }
}
