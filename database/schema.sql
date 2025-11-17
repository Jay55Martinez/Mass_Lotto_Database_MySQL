-- ============================================================================
-- Mass Lottery Database Schema
-- ============================================================================
-- Database: mass_lotto_db
-- Purpose: Track Massachusetts State Lottery scratch-off ticket games,
--          prize structures, odds, and remaining prizes
-- Created: 2025-11-12
-- ============================================================================

-- Clean slate: Drop existing database if present
DROP DATABASE IF EXISTS mass_lotto_db;

-- Create the main database
CREATE DATABASE mass_lotto_db 
    CHARACTER SET utf8mb4 
    COLLATE utf8mb4_unicode_ci;

USE mass_lotto_db;

-- ============================================================================
-- Table: games
-- ============================================================================
-- Stores information about each scratch-off lottery game
-- Each game has a unique identifier and tracks basic game details
-- ============================================================================
CREATE TABLE games (
    -- Primary key: Manual ID (no AUTO_INCREMENT)
    -- This will be the game ID from the Massachusetts Lottery API
    massGameId INT PRIMARY KEY,
    
    gameName VARCHAR(100) NOT NULL,
    gameIdentifier VARCHAR(100) NOT NULL UNIQUE,
    startDate DATETIME NOT NULL,
    ticketCost INT NOT NULL,
    odds VARCHAR(50),
    amountPrinted INT,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_game_identifier (gameIdentifier),
    INDEX idx_ticket_cost (ticketCost),
    INDEX idx_start_date (startDate)
) ENGINE=InnoDB;

-- ============================================================================
-- Table: prizeTiers
-- ============================================================================
-- Stores prize tier information for each game
-- Each game has multiple prize tiers (e.g., top prize, second prize, etc.)
-- Tracks total prizes, paid prizes, and remaining prizes for each tier
-- ============================================================================
CREATE TABLE prizeTiers (
    prizeTierId INT AUTO_INCREMENT PRIMARY KEY,
    
    -- Foreign key to games table
    massGameId INT NOT NULL,
    
    tierNumber INT NOT NULL,
    prizeAmount INT NOT NULL,
    totalPrizes INT NOT NULL,
    paidPrizes INT NOT NULL DEFAULT 0,
    prizesRemaining INT NOT NULL,
    prizeDescription VARCHAR(100) NOT NULL,
    typeOfWin VARCHAR(50) NOT NULL,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (massGameId) REFERENCES games(massGameId) 
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    
    INDEX idx_mass_game_id (massGameId),
    INDEX idx_tier_number (tierNumber),
    INDEX idx_prize_amount (prizeAmount),
    INDEX idx_game_remaining (massGameId, prizesRemaining),
    
    UNIQUE KEY unique_game_tier (massGameId, tierNumber),
    
    CONSTRAINT chk_paid_prizes CHECK (paidPrizes <= totalPrizes),
    CONSTRAINT chk_remaining_prizes CHECK (prizesRemaining >= 0),
    CONSTRAINT chk_prizes_balance CHECK (prizesRemaining = totalPrizes - paidPrizes)
) ENGINE=InnoDB;

-- ============================================================================
-- Optional: Create a view for easy prize analysis
-- ============================================================================
-- This view combines game and prize information for easier querying
CREATE VIEW game_prize_summary AS
SELECT 
    g.massGameId,
    g.gameName,
    g.gameIdentifier,
    g.ticketCost,
    g.odds,
    g.startDate,
    pt.tierNumber,
    pt.prizeAmount,
    pt.totalPrizes,
    pt.paidPrizes,
    pt.prizesRemaining,
    pt.prizeDescription,
    pt.typeOfWin,
    ROUND((pt.prizesRemaining / pt.totalPrizes * 100), 2) AS percentRemaining
FROM games g
JOIN prizeTiers pt ON g.massGameId = pt.massGameId
ORDER BY g.ticketCost, g.gameName, pt.tierNumber;

-- ============================================================================
-- End of schema
-- ============================================================================