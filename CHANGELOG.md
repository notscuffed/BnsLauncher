# 2020-09-04 [0.1.1]
- Added profile priority to allow profile reordering

# 2020-08-09 [0.1.0]
- Fixed launcher always showed username instead of display name

# 2020-08-04 [0.0.9]
- Fixed starting without account would still try to log in last logged in account (and probably other bugs releated to not clearing old environment variables)
- Added ability to add custom titles for accounts
- Added setting to hide private server ip in profiles

# 2020-07-20 [0.0.8]
- Added ability to not select any account when starting game

# 2020-07-20 [0.0.7]
- Added account system/support for loginhelper (automatically log in and put pin)
- Added automatic profile reloading

# 2020-07-13 [0.0.5]
- Improved profile system
- Added ability to specify client path/arguments in profile
- Added support for binloader (you need to get the plugin yourself)

# 2020-07-10 [0.0.4]
- Added new GG bypass

# 2020-06-07 [0.0.3]
- Added WSAConnect ip rewrite option to bnspatch dll
- Added logging tab and logging from named pipe
- Enabled TOI by default in sample profile
- Fixed clicking same button that didn't switch tab didn't do anything
- Fixed clicking links in about tab didn't do anything
- Fixed reloading profiles lost process list under profile

# 2020-06-03 [0.0.2]
- Fixed changing tab lost tracked processes
- Fixed sample.xml was missing /@value at the end of the query for Lobby gate address, port, np-address and np-port causing it to not actually edit those xml values...