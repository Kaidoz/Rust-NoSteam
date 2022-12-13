# Rust-NoSteam
Discord: https://discord.gg/Tn3kzbE

## ⭐ » Donations
- https://www.buymeacoffee.com/kaidoz
- https://boosty.to/kaidoz/single-payment/donation/29238
- https://qiwi.com/n/KAIDOZ
- VISA: 4279380632007755
- BTC: 1DNEbR5Yk6a6NXDuQHB2XGAAjaqL8NXvUc

## 📝️ » Information
- Protected steam players against hack(fake steamid)
- Nosteam players are not displayed in server list to avoid ban
- Kicked players if use developers ids

## 🛠️ » Api and Hooks
#### IsPlayerNoSteam
Check player
```C#
IsSteam(ulong steamid)
IsSteam(Connection connection)
IsSteam(BasePlayer player)
```
##### Example 
```C#
bool IsPlayersSteam(BasePlayer player)
{
    if(Call<bool>("IsSteam", player) == true)
      return true;
    return false;
}
```
### Hooks
#### OnBeginPlayerSession
Returning a non-null value kick player with reason as value.
```C#
object OnBeginPlayerSession(Connection connection, bool isLicense)
{
  string status = isLicense ? "steam" : "nosteam";
  Puts($"{connection.userid} is {status} player c:");
  return null;
}
```
## 🔧 » Supported operating systems
| System  | Status |
|---------|--------|
| Windows |   ✅   |
| Linux   |   ✅   | 

## 🧶 » Credits

[Harmony](https://github.com/pardeike/Harmony) patcher used in the project
