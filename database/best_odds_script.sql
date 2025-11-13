USE mass_loto_db;

SELECT *
FROM games
ORDER BY amountPrinted DESC
LIMIT 1;

-- Returns all the games estimatedNewOdds with all work shown
SELECT 
    g.massGameId,
    g.gameName,
    g.odds,
    SUM(pt.paidPrizes) AS totalPaidedTickets,
    SUM(pt.totalPrizes) AS actualTotalPrizes,
    SUM(pt.prizesRemaining) AS remainingPrizes,
    SUM(pt.paidPrizes) / SUM(pt.totalPrizes) AS percentTicketsSold,
    ROUND(SUM(pt.totalPrizes) * (SUM(pt.paidPrizes) / SUM(pt.totalPrizes)), 0) AS estimatedTicketsSold,
    g.amountPrinted,
    g.amountPrinted - ROUND((SUM(pt.totalPrizes) * (SUM(pt.paidPrizes) / SUM(pt.totalPrizes))), 0) AS remainingTickets,
    SUM(pt.prizesRemaining) / (g.amountPrinted - ROUND((SUM(pt.totalPrizes) * (SUM(pt.paidPrizes) / SUM(pt.totalPrizes))), 0)) AS estimatedNewOdds
FROM games g
JOIN prizeTiers pt ON g.massGameId = pt.massGameId
GROUP BY g.massGameId, g.gameName
ORDER BY estimatedNewOdds DESC;

-- Returns the game with the highest estimatedNewOdds
SELECT 
	g.gameName,
    SUM(pt.prizesRemaining) / (g.amountPrinted - ROUND((SUM(pt.totalPrizes) * (SUM(pt.paidPrizes) / SUM(pt.totalPrizes))), 0)) AS estimatedNewOdds
FROM games g
JOIN prizeTiers pt ON g.massGameID = pt.massGameId
GROUP BY g.massGameId, g.gameName
ORDER BY estimatedNewOdds DESC
LIMIT 1;
