
## v1.0.0
- **Official Release!!!**
- Fixed weather not syncing correctly
- Fixed config and/or terminal creating moon duplicates
- **New Scan page!**
    - Option to toggle between vanilla scan and detailed scan
    - Detailed scan includes:
	- A bar graph of current item statistics
	- A detailed list of all scrap including their weight, value, and additional status properties
	- The additional status notes include two-handed carry, conductivity, usable weapon, and battery-powered
	- I tried my diggety-dang darndest to get "noisemaker" as a status property but nothing in the code seems to connect them. If you know how, reach out to me on github!
    - An unnecessarily large graphic overhaul into a retro "floppy disk" aesthetic
    - A secondary "scan ship" page specifically for equipment (enter "scan ship" instead of just "scan". When in space or at the Company, using just "scan" will scan the ship for scrap, not equipment).
- Not only are duplicate entries now removed, if the list of moons changes for any reason (most likely adding/removing mods), it will grab the deprecated config values and carry them over to the new config.
- **Customizable difficulty equation!** You can now choose variables and adjust weights for the "difficulty" catalogue sort option
- Added/improved sorting options
- More moon catalogue options
    - Option for toggling clear weather between not displaying or displaying as "Clear"
    - Option for absolute spacing regardless of current sorting (line breaks every three moons)
    - Option for toggling long weather names between displaying the full name or "Complex"
    - "reverse" is now its own standalone keyword, enter it to reverse the current listing
- Small overhaul to the route confirmation page
- Fixed the "out of array" errors (hopefully, I haven't been able to recreate them so I restructured some places I thought might be the problem)
- Added a price override option for compatibility with other price-changing mods. Enter "-1" as the price in the config to bypass it and use the non-TerminalPlus value.
- Remade the logo a lot cleaner (it was bugging me)
- Probably some other small changes/additions (I did so much I've honestly forgotten)
- Kilroy is here.

## v0.2.2
- Fixed small problem with the difficulty algorhythm
- Kilroy was here.

## v0.2.1
- Actually uploaded the right version this time (hopefully)
- Kilroy was here.

## v0.2.0
- Fixed the empty space below pages
- Hid clear weather from the catalogue, now a config option
- Added "list" and "current" sort options which flip the current page order
- Fixed modded moon prices not showing up for a bunch of reasons
- Fixed long weather names breaking the formatting
- Added "even listing" config option that separates moons into groups of three, regardless of the sorting
- Fixed some keyword conflicts
- Kilroy was here.

## v0.1.2

- Okay NOW they should work
- Kilroy was here.

## v0.1.1

- (Hopefully fixed store images)
- Kilroy was here.

## v0.1.0

- Test release
- Kilroy was here.