# loopctl
Windows Store App Loopback Exempt Control (Make your apps can access localhost)

## 3-Step Usage
1. Run `loopctl.exe`. You will get `loopctl-applist.csv` file, which looks like this:
```
Loopback,App,SID
0,DisallowedApp,S-1-15-2-...
1,AllowedApp,S-1-15-2-...
0,AnotherApp,S-1-15-2-...
```
2. Open `loopctl-applist.csv`, Modify the first column,  
`1 = ALLOW app access localhost`, `0 = DISALLOW app access localhost`.
3. Run `loopctl.exe` again, changes will apply. Besides, `loopctl-applist.csv` will be removed.
