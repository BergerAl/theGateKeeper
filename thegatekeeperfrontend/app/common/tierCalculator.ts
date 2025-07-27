type Tier =
  | "BRONZE"
  | "SILVER"
  | "GOLD"
  | "PLATINUM"
  | "EMERALD"
  | "DIAMOND"
  | "MASTER"
  | "GRANDMASTER"
  | "CHALLENGER";

type Rank = "IV" | "III" | "II" | "I";

interface LeaguePosition {
  tier: Tier;
  rank: Rank;
  leaguePoints: number;
}

const tierPoints: Record<Tier, number> = {
  BRONZE: 400,
  SILVER: 800,
  GOLD: 1200,
  PLATINUM: 1600,
  EMERALD: 2000,
  DIAMOND: 2400,
  MASTER: 2800,
  GRANDMASTER: 3200,
  CHALLENGER: 3600,
};

const rankPoints: Record<Rank, number> = {
  IV: 0,
  III: 100,
  II: 200,
  I: 300,
};

export function getTierRankLeaguePointsFromTotal(totalPoints: number): LeaguePosition {
  // Find the highest tier that does not exceed totalPoints
  let foundTier: Tier = "BRONZE";
  for (const tier of Object.keys(tierPoints) as Tier[]) {
    if (tierPoints[tier] <= totalPoints) {
      foundTier = tier;
    }
  }

  let pointsAfterTier = totalPoints - tierPoints[foundTier];

  // Find the highest rank that does not exceed pointsAfterTier
  let foundRank: Rank = "IV";
  for (const rank of Object.keys(rankPoints) as Rank[]) {
    if (rankPoints[rank] <= pointsAfterTier) {
      foundRank = rank;
    }
  }

  let leaguePoints = pointsAfterTier - rankPoints[foundRank];
  if (leaguePoints < 0) leaguePoints = 0;

  return {
    tier: foundTier,
    rank: foundRank,
    leaguePoints,
  };
}