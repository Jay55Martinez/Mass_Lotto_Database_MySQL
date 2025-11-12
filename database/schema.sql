-- ============================================================================
-- Mass Lottery Database Schema
-- ============================================================================
-- Database: mass_loto_db
-- Purpose: Track Massachusetts State Lottery scratch-off ticket games,
--          prize structures, odds, and remaining prizes
-- Created: 2025-11-12
-- ============================================================================

-- Clean slate: Drop existing database if present
DROP DATABASE IF EXISTS mass_loto_db;

-- Create the main database
CREATE DATABASE mass_loto_db 
    CHARACTER SET utf8mb4 
    COLLATE utf8mb4_unicode_ci;

USE mass_loto_db;

-- ============================================================================
-- Table: games
-- ============================================================================
-- Stores information about each scratch-off lottery game
-- Each game has a unique identifier and tracks basic game details
-- ============================================================================
CREATE TABLE games (
    massGameId INT AUTO_INCREMENT PRIMARY KEY,
    gameName VARCHAR(100) NOT NULL,
    gameIdentifier VARCHAR(100) NOT NULL UNIQUE,
    startDate DATETIME NOT NULL,
    ticketCost INT NOT NULL, -- In dollars (e.g. 5 -> $5)
    odds VARCHAR(50),
    amountPrinted INT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Index on gameIdentifier for fast lookups
    INDEX idx_game_identifier (gameIdentifier),
    
    -- Index on ticketCost for filtering by price point
    INDEX idx_ticket_cost (ticketCost),
    
    -- Index on startDate for sorting by game age
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
    massGameId INT NOT NULL,
    tierNumber INT NOT NULL,
    prizeAmount INT NOT NULL, -- In dollars (e.g. 1000 -> $1000)
    totalPrizes INT NOT NULL,
    paidPrizes INT NOT NULL DEFAULT 0,
    prizesRemaining INT NOT NULL,
    prizeDescription VARCHAR(100) NOT NULL,
    typeOfWin VARCHAR(50) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Foreign key constraint ensures referential integrity
    FOREIGN KEY (massGameId) REFERENCES games(massGameId) 
        ON DELETE CASCADE  -- If game is deleted, delete all associated prize tiers
        ON UPDATE CASCADE, -- If game ID changes, update prize tiers accordingly
    
    -- Index on massGameId for fast lookups of prizes by game
    INDEX idx_mass_game_id (massGameId),
    
    -- Index on tierNumber for sorting prizes by tier
    INDEX idx_tier_number (tierNumber),
    
    -- Index on prizeAmount for filtering/sorting by prize value
    INDEX idx_prize_amount (prizeAmount),
    
    -- Composite index for common query: prizes remaining by game
    INDEX idx_game_remaining (massGameId, prizesRemaining),
    
    -- Unique constraint: Each game can only have one entry per tier number
    UNIQUE KEY unique_game_tier (massGameId, tierNumber),
    
    -- Check constraint: Paid prizes cannot exceed total prizes
    CONSTRAINT chk_paid_prizes CHECK (paidPrizes <= totalPrizes),
    
    -- Check constraint: Remaining prizes cannot be negative
    CONSTRAINT chk_remaining_prizes CHECK (prizesRemaining >= 0),
    
    -- Check constraint: prizesRemaining should equal totalPrizes - paidPrizes
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