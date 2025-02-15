# TTC-React-Back

## Deploy


```sh
cp .example.env .env
cp src/Ttc.WebApi/appsettings.json src/Ttc.WebApi/appsettings.Release.json

docker-compose up -d --build
```

## Database

Use `docker compose up -d --build` or:

```sh
docker run --name ttc-mysql -p 7202:3306 -e MYSQL_ROOT_PASSWORD=my-secret-pw -d mysql:5.5.60
```


## EF Migrations

Migrations will run at startup of application.

```ps1
cd src/Ttc.DataAccess
dotnet ef database update

# Install
dotnet tool install --global dotnet-ef

# Create
dotnet ef migrations add InitialCreate

# Delete
dotnet ef migrations remove
dotnet ef database drop -f
```


## New Season

Go to Admin > Params and update the "year" param.

Use `SportaMatchesScoresheetsExcelCreation` "UnitTest" to create the Sporta Excels.

ATTN: Check instead: Perhaps fill in on the site directly?

## Snippets

There is a simple GUI for updating NextKlassementSporta/Vttl but not yet to clear the ones from previous year:
(See commit: 3cbdd71)

```sql
UPDATE speler SET NextKlassementSporta=null, NextKlassementVttl=null;

SELECT Id, NaamKort, KlassementSporta, NextKlassementSporta
FROM speler
WHERE gestopt IS NULL AND klassementsporta IS NOT NULL AND KlassementSporta<>'0'
ORDER BY klassementsporta
```

Match date tempering for in between seasons:

```c#
/// <summary>
/// If there is no real life data between seasons,
/// change some match dates to around now for testing purposes
/// </summary>
private static void RandomizeMatchDatesForTestingPurposes(TtcDbContext context)
{
  bool endOfSeason = !context.Matches.Any(match => match.Date > DateTime.Now);
  if (true || endOfSeason)
  {
    int currentFrenoySeason = context.CurrentFrenoySeason;
    var passedMatches = context.Matches
        .Where(x => x.FrenoySeason == currentFrenoySeason)
        //.Where(x => x.Date < DateTime.Today)
        .OrderByDescending(x => x.Date)
        .Take(42);
  
    var timeToAdd = DateTime.Today - passedMatches.First().Date;
    foreach (var match in passedMatches.Take(20))
    {
        match.Date = match.Date.Add(timeToAdd);
    }
  
    var rnd = new Random();
    foreach (var match in passedMatches.Take(20))
    {
        match.Date = DateTime.Today.Add(TimeSpan.FromDays(rnd.Next(1, 20))).AddHours(rnd.Next(10, 20));
        match.Description = "";
        match.AwayScore = null;
        match.HomeScore = null;
        //match.IsSyncedWithFrenoy = true;
        match.WalkOver = false;
  
        context.MatchComments.RemoveRange(match.Comments.ToArray());
        context.MatchGames.RemoveRange(match.Games.ToArray());
        context.MatchPlayers.RemoveRange(match.Players.ToArray());
    }
  }
}
```
