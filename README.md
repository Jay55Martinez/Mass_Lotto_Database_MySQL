# Mass Lottery Database System

A containerized MySQL database system that automatically fetches, stores, and tracks Massachusetts State Lottery scratch-off ticket information. The system pulls live data from the Massachusetts Lottery API and populates a structured database with game details, prize tiers, odds, remaining prizes, and historical data.

## Overview

This project provides a complete solution for analyzing Massachusetts lottery scratch-off games by:
- Fetching real-time game data from the official Massachusetts Lottery API
- Storing comprehensive game information including ticket costs, odds, and start dates
- Tracking all prize tiers for each game with detailed information about total prizes, paid prizes, and remaining prizes
- Running in Docker containers for easy deployment and portability
- Automatically initializing the database schema on first run

## Features

- **Automated Data Collection**: Fetches all active scratch-off games from the Massachusetts Lottery API
- **Comprehensive Prize Tracking**: Stores detailed prize tier information including:
  - Prize amounts and descriptions
  - Total prizes available
  - Prizes already paid out
  - Remaining prizes
  - Percentage of prizes remaining
- **Docker Support**: Fully containerized with Docker Compose for easy setup
- **Environment-Based Configuration**: Secure connection string management via environment variables
- **Transaction Safety**: Database operations use transactions to ensure data integrity
- **Database Views**: Pre-built SQL views for easy prize analysis

## Technology Stack

- **Language**: C# (.NET 9.0)
- **Database**: MySQL 8.0
- **Containerization**: Docker & Docker Compose
- **ORM/Data Access**: MySql.Data (ADO.NET)
- **Configuration**: Microsoft.Extensions.Configuration, DotNetEnv
- **API**: Massachusetts Lottery Public API

## Requirements

### For Docker Deployment (Recommended)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows/Mac) or Docker Engine (Linux)
- Docker Compose (included with Docker Desktop)
- 2GB minimum available RAM
- 1GB minimum available disk space

### For Local Development
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [MySQL 8.0](https://dev.mysql.com/downloads/mysql/) or higher
- Git (for cloning the repository)
- A text editor or IDE (Visual Studio, VS Code, Rider, etc.)

## Project Structure

```
Mass_Lotto_Database_MySQL/
├── src/
│   ├── Models/              # Data models (Game, PrizeTier)
│   ├── Services/            # Business logic (API and Database services)
│   ├── Utils/               # Utility functions
│   ├── Program.cs           # Application entry point
│   ├── src.csproj           # Project configuration
│   ├── Dockerfile           # Docker build instructions
│   └── appsettings.json     # Application configuration
├── database/
│   ├── schema.sql           # Database schema and initialization
│   └── best_odds_script.sql # Analysis queries
├── docker-compose.yml       # Docker orchestration
├── .env                     # Environment variables (not in Git)
├── .env.example             # Environment variables template
└── README.md                # This file
```

## Build Instructions

### Option 1: Docker Deployment (Recommended)

1. **Clone the repository**
   ```bash
   git clone https://github.com/Jay55Martinez/Mass_Lotto_Database_MySQL.git
   cd Mass_Lotto_Database_MySQL
   ```

2. **Configure environment variables**
   
   Edit the `docker-compose.yml` file and update the MySQL password:
   ```yaml
   MYSQL_ROOT_PASSWORD: your_secure_password  # Change this
   ```
   
   Also update the connection string in the loader service:
   ```yaml
   DB_CONNECTION_STRING: "server=mysql;user=root;password=your_secure_password;database=mass_lotto_db;AllowUserVariables=True;UseAffectedRows=False"
   ```

3. **Build and run the containers**
   ```bash
   docker compose up --build
   ```

4. **Monitor the process**
   
   You should see:
   - MySQL container starting and initializing the database
   - Database schema being created
   - Loader container connecting to the database
   - Games being fetched and inserted from the API

5. **Access the database**
   
   Connect to MySQL using your preferred client:
   - **Host**: `localhost`
   - **Port**: `3306`
   - **User**: `root`
   - **Password**: (the password you set in docker-compose.yml)
   - **Database**: `mass_lotto_db`

6. **Stop the containers**
   ```bash
   docker compose down
   ```

### Option 2: Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/Jay55Martinez/Mass_Lotto_Database_MySQL.git
   cd Mass_Lotto_Database_MySQL
   ```

2. **Set up MySQL database**
   
   Install MySQL 8.0 and create the database:
   ```sql
   mysql -u root -p < database/schema.sql
   ```

3. **Configure environment variables**
   
   Copy the example environment file:
   ```bash
   cp .env.example .env
   ```
   
   Edit `.env` with your database credentials:
   ```
   DB_CONNECTION_STRING=server=localhost;user=root;password=YOUR_PASSWORD;database=mass_lotto_db;AllowUserVariables=True;UseAffectedRows=False
   ```

4. **Install dependencies and build**
   ```bash
   cd src
   dotnet restore
   dotnet build
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Verify the data**
   
   Connect to your MySQL database and query the data:
   ```sql
   USE mass_lotto_db;
   SELECT * FROM games;
   SELECT * FROM game_prize_summary LIMIT 10;
   ```

## Usage Examples

### Query Active Games
```sql
SELECT gameName, ticketCost, odds, startDate 
FROM games 
ORDER BY ticketCost DESC;
```

### Find Games with Best Odds
```sql
SELECT * FROM game_prize_summary 
WHERE percentRemaining > 50 
ORDER BY prizeAmount DESC, percentRemaining DESC;
```

### Check Top Prizes Remaining
```sql
SELECT 
    gameName, 
    prizeAmount, 
    prizesRemaining, 
    totalPrizes,
    percentRemaining
FROM game_prize_summary
WHERE tierNumber = 1 AND prizesRemaining > 0
ORDER BY prizeAmount DESC;
```

### Games by Ticket Cost
```sql
SELECT ticketCost, COUNT(*) as gameCount, AVG(CAST(odds AS DECIMAL(10,2))) as avgOdds
FROM games
GROUP BY ticketCost
ORDER BY ticketCost;
```

## Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `DB_CONNECTION_STRING` | MySQL connection string | `server=localhost;user=root;password=pass;database=mass_lotto_db;AllowUserVariables=True;UseAffectedRows=False` |

## Database Schema

### `games` Table
Stores core game information:
- `massGameId` (Primary Key) - Official Mass Lottery game ID
- `gameName` - Name of the scratch-off game
- `gameIdentifier` - Unique game identifier
- `startDate` - When the game launched
- `ticketCost` - Cost per ticket in dollars
- `odds` - Overall odds of winning
- `amountPrinted` - Total tickets printed

### `prizeTiers` Table
Stores prize tier details for each game:
- `prizeTierId` (Primary Key) - Auto-incremented ID
- `massGameId` (Foreign Key) - References games table
- `tierNumber` - Prize tier level (1 = top prize)
- `prizeAmount` - Dollar amount of prize
- `totalPrizes` - Total prizes at this tier
- `paidPrizes` - Prizes already claimed
- `prizesRemaining` - Prizes still available
- `prizeDescription` - Description of the prize
- `typeOfWin` - Type of winning condition

### `game_prize_summary` View
Combined view for easy analysis with calculated percentage of prizes remaining.

## API Reference

The application uses the Massachusetts Lottery Public API:
- **Base URL**: `https://www.masslottery.com/api/v1/instant-game-prizes`
- **Game Details**: `https://www.masslottery.com/api/v1/instant-game-prizes?gameID={id}`

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is for educational purposes. Lottery data is provided by the Massachusetts State Lottery Commission.

## Acknowledgments

- Massachusetts State Lottery Commission for providing the public API
- .NET Community for excellent documentation and tools

## Contact

**Repository**: [https://github.com/Jay55Martinez/Mass_Lotto_Database_MySQL](https://github.com/Jay55Martinez/Mass_Lotto_Database_MySQL)

**Issues**: [https://github.com/Jay55Martinez/Mass_Lotto_Database_MySQL/issues](https://github.com/Jay55Martinez/Mass_Lotto_Database_MySQL/issues)
