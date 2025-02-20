# GigaSharp

GigaSharp is a .NET recreation of the old Clube de Gigachad discord bots (N-Chan and, potentially coming in the future under a new name, BS Bot), as well as the new development location of any new bot ideas the group has.

These old bots were abandoned due to major structural flaws in their programming that made maintaining their use and adding new features extremely tiresome and often times not worth it. Additionally, the active collaborators in maintaining these bots did not enjoy development in python (the language they were written in): In speculating about the benefits of re-making these bots, the idea of using C# for their recreations was floated, and upon understanding that this was entirely feasible, the group decided to commit to the idea.

This readme may later be updated with deeper information about each bot's use and functionality, but for now, it will serve mostly as log of **stable** versions of the repository, as a fallback in the event that progress on working on these projects causes major bugs.

# Stable Changelog

* **2025-01-12: Version 0.1, commit 50ef898**

First stable progress release of the project. Capable of logging into a bot and has a functional 5-minute pinging loop to keep the application running indefinitely on Render. Also contains non-functional progress on a slash command system.

* **2025-01-17: Version 0.2, commit d14b574**

Most basic n-chan functionalities complete: Fully functional web scraping all necessary book data from nhentai, functional 404 detection system, functional slash command interface, functional book discord message embed creation, and ping bot, ping nhentai, book, random, and exists commands added.

* **2025-02-17: Version 0.3, commit 8549d9b**

Basic, stable database added (but very rudimentary and not without its problems as highlighted in the issues tab). Books are now retrieved from the database when possible rather than webscraped. Read command also added and fully functional.

* **2025-02-20: Version 1.0, commit b74aec0**

First stable, long-running, deployable version of the project. All basic n-chan functionalities finished, functional, and tested. Book of the day and book rating systems added. Commands made global, loose local commands deleted. It may also be possible to use n-chan commands in DMs if the user installs the application into their account.
