# Rust-NoSteam
### Info
- Protected steam players against hack(fake steamid)
- Nosteam players are not displayed in server list to avoid ban([API-FacePunch](https://api.facepunch.com/api/public/manifest/?public_key=j0VF6sNnzn9rwt9qTZtI02zTYK8PRdN1))
- Kicked players if use developers ids

### Api
#### IsPlayerNoSteam
Check player
```C#
IsPlayerNoSteam(ulong steamid)
```
##### Example 
```C#
[PluginReference("NoSteamHelper")] 
private Plugin NoSteamHelper;

bool IsPlayersSteam(BasePlayer player)
{
    if(NoSteamHelper.Call("IsPlayerNoSteam", player.userID)==null)
      return true;
    return false;
}
```

### Hooks
#### OnSteamAuthFailed
Returning a non-null value will not cancel kick player.
```C#
object OnSteamAuthFailed(Connection connection)
{
  Puts($"{connection.userid} is nosteam player, but it doesn't matter to us c:");
  return null;
}
```

#### CanNewConnection
Returning a non-null value kick player with reason as value.
```C#
object CanNewConnection(Connection connection, bool isSteam)
{
  string status = isSteam ? "steam" : "nosteam";
  Puts($"{connection.userid} is {status} player");
  return null;
}
```

### Credits

[Harmony](https://github.com/pardeike/Harmony) patcher used in the project
