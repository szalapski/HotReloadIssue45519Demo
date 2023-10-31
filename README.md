See https://github.com/dotnet/aspnetcore/issues/51781

To reproduce the problem:  In Windows, run 

`dotnet watch --verbose --project ./TradeCars/Server`

The site should start up.  You'll see `Hot reload capabilities: .` which portends the failure.  Edit anything in Index.razor to watch hot reload fail.
