
# Slam's TerminalPlus

Don't hate me plz

<br><br>

Overhauls the terminal and allows you to customize many aspects of moons including name, hazard level, price, sorting, descriptions, and more!

### The Moon Catalogue
![moonSS](https://i.imgur.com/7NmSSF9.png "Moon Catalogue")

<br>

### The Store Page
![storeSS](https://i.imgur.com/wzf2lic.png "Store Page")

<br>

### The Scan Page
![scanSS](https://i.imgur.com/0Sr1qgl.jpeg "Scan Page")

<br>

### Sort Settings
![settingSS](https://i.imgur.com/6N1BcRP.png "Sort Settings")

<br>

***

## Config Settings:

### General Settings:
- **Set Default Sorting** for the moon catalogue. Options are "default" (level ID), "name", "prefix", "grade/hazard level", "price", "weather", "difficulty" (equation soon to be customizable, NOT the same as grade), or the reverse of any of them.

- **Pad Prefixes** with zeroes for a more uniform look. For example, "8 Titan", "21 Offense", and "220 Assurance" would become "008 Titan", "021 Offense", and "220 Assurance".

- **Show Clear Weather** in the moon catalogue. If false, the "weather" section of moons with no current weather will be left blank.

- **Evenly List Moons** in the moon catalogue rather than separating them according to their sort. For example, if true, moons will be divided into groups of three instead of being divided by grade, price, etc.

- **Show Full-length Weather Names** in the moon catalogue. Will create second rows to display extra-long weather names. If false, extra-long weather names will be truncated to "Complex". NOTE: All vanilla weathers will fit on the terminal! This is solely for mods that have longer weather names.

- **Show the Detailed Terminal Scan** instead of the vanilla one. Will display a list of every scrap with their values, weights, and properties (whether or not the scrap is two-handed, conductive, a weapon, or battery-powered) along with a summarizing bar-graph. See the image above for an example.

- **Set Custom Difficulty Equation** for when sorting the moon catalogue by difficulty. Choose any collection of nine different variables and give them each weights.
Possible Variables:
    - Enemy Power Count (for daytime, nighttime, and inside)
    - Total Enemy Types (for daytime, nighttime, and inside)
    - Interior Size
    - Current Weather
    - Average Scrap Value

- **Set the Scroll Sensitivity** for the terminal menus. The value roughly equates to lines scrolled per scroll step.

- **Change the Clock Setting** for the in-terminal clock. Can be changed between "normal/full", "hour only", "military time", and "off".

- **Show LGU Upgrades in the Main Store** along with the LGU store, though this became obsolete five minutes after I implemented it since LGU's store got a complete redesign.

### Moon Settings:
- **Enable the Moon Config** for the current moon.

- **Hide the Moon** from the catalogue page (will remain active and usable). On for The Company Building by default.

- **Set a Custom Name** for the moon.

- **Set a Custom Prefix** (the number before the name) for the moon.

- **Set a Custom Grade / Hazard Level** for the moon (i.e. the letter grade).

- **Set a Custom Price** for the moon.

- **Set a Custom Description** for the moon (the flavor text on the main monitor).

- **Set a Custom Info Page** for the moon (i.e. the text when when you enter in "[PlanetName] info").

### Misc. Settings:

- ***Kilroy was here.***

***

## Possible Future Plans (will be a long ways out if they happen)
- Custom (and configurable) bestiary entries
- Selectable terminal menus à la AdvancedCompany or the new LateGameUpgrades store? (only if a lot of people want it)
- Separate mod "Super Company Creator" (or some other equally cheesy name) that lets you build and customize your own terminal pages.
- Some terminal games?

***

# CREDITS

mrov for their [TerminalFormatter](https://thunderstore.io/c/lethal-company/p/mrov/TerminalFormatter/) mod that inspired me and for some compatibility help

IAmBatby for [LethalLevelLoader](https://thunderstore.io/c/lethal-company/p/IAmBatby/LethalLevelLoader/) which I used for testing