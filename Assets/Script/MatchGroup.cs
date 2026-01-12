using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchGroup
{
    public bool isHorizontalMatch;
    public HashSet<StoneBehaviour> MatchedStones { get; private set; } // HashSet luu mot match
    public MatchType MatchType { get; set; } // Loai match

    public StoneBehaviour SpecialStoneCandidate { get; set; } // Vi tri thay stone special

    public MatchGroup(IEnumerable<StoneBehaviour> stones, MatchType type, StoneBehaviour candidate, bool isHorizontalMatch)
    {
        MatchedStones = new HashSet<StoneBehaviour>(stones);
        MatchType = type;
        this.isHorizontalMatch = isHorizontalMatch;
        SpecialStoneCandidate = candidate; 
    }
}
