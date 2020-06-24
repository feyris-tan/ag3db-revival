# What's this?
Back in 2008 a user by the name of Dili released this tool on the now defunct HongFire forum. This tool can be used to share characters created in/for  [_Jinkō Shōjo 3_](https://en.wikipedia.org/wiki/Jink%C5%8D_Sh%C5%8Djo_3) 
The required infrastructure/server was shutdown in around 2013 and since then the tool can not be used anymore.
Fortunately, Dili released an SQL dump and the source code to the client.
I admit, I liked that game (even though it's basically porn) and this tool back in the day.
I'd like to bring AG3DB back to life, as I think that game modding history in general is not very well preserved - especially for a niche game like JS3 in which modding it is actually more fun than playing it.
This repository documents my attempt to revive AG3DB. It started out with an upload of the original code of Dili with some modifications done by me in order to modernize it.

# How to run this?
Dili released only the client source code, not the server source code. The server is re-implemented with a "best guess" approach in Quarkus. See my other repository "azusarkus"

You will need to point the client to an azusarkus instance.

Details TBD...

# What did you change?
- Some parts will be re-written in C#, as I'm not that fluent in Visual Basic. 
- I also removed Dili's artworks for the program, because I do not want to post nudity onto Github. Nowadays, a Github account is kind of like a resume after all.
- The AG3DB Public Release torrent is not seeded completely anymore, and can only be finished to about 99.7%. While the table containing character data is complete - the users table is missing due to the lack of seeders. (If you have a complete copy of that torrent, please contact me!) I will include a java program in this repository to rebuild the users table on a best-effort basis.

# Why did you dig this this up?
- I lurked for quite a long time on Hongfire and other related forums. I think it's time for me to give something back to the community - even though it's more than 10 years too late for this. 
- Also, I'd like to become more familiar with Quarkus.
- Also 2: Some kind of lockdown boredom

# Disclaimer
Dili has nothing to do with this repository! It is not sponsored/endorsed or supported in any way by him. Please do not contact him about this. If you have any questions about this, contact me instead by raising a GitHub issue.
