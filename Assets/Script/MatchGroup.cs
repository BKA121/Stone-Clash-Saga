using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchGroup
{
    public HashSet<StoneBehaviour> MatchedStones { get; private set; } // HashSet luu mot match
    public MatchType MatchType { get; set; } // Loai match

    //public StoneBehaviour SpecialStoneCandidate { get; set; } // Vi tri thay stone special

    public MatchGroup(IEnumerable<StoneBehaviour> stones, MatchType type)
    {
        MatchedStones = new HashSet<StoneBehaviour>(stones);
        MatchType = type;
        //SpecialStoneCandidate = candidate; 
    }
}
