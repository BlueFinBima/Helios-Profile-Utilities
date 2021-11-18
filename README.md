# Helios Virtual Cockpit Utilities for Profiles
### Introduction
All of the functionality to manipulate Helios Profiles provided in this utility should really be in Profile Editor, however until such time as this happens, this utility might help the creation of profiles.

### Functions

* Load a Helios Profile into memory
* Selection of a Helios Panel (and children) to be extracted
* Insertion of that extracted Helios Panel into a newly loaded profile 
* Extract images used by a profile into a single directory, and point the profile at that directory.  Some cleaning up of file names happens in this process.
* Check a Profile for duplicate Bindings (a Helios Bug)and also removal of duplicate bindings
* Save a Helios profile from memory

### Notes
Only limited testing has taken place with these functions.
Extracted panels will always appear at the top left of the destination Monitor 1.  From there, the panel can be copied, moved, resized as necessary within the Helios Profile Editor. 