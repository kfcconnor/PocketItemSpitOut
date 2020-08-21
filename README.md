# PocketItemSpitOut
 An Exiled V2 Plugin that allows items dropped by dead people within larry's pocket dimension to be randomly dropped within the facility
## Installation
Place the PocketItemSpitOut.dll into your Exiled/Plugins Folder and the mod will start next time you launch the server
## Config
Config options and example inputs are as follows:

Config Item | Description | Example Input
----------- | ----------- | ------------- 
IsEnabled | Enables or Disables the mod | true
debug | Enables debug messages within the mod | false
allow_tesla | Determins if the tesla gates are a valid drop location | true
allow_drop_on_larry_death | determines if multiple items start to drop after femer breaker | false
min_time_for_drop | the minimum time between drops | 10
max_time_for_drop | the maximum time between drops| 25
chance_for_drop | Determines the chance each check if a drop will happen | 75
items_dropped_on_larry_death | the number of items that start dropping per drop after larry is femer breakered | 10

## Commands
These are commands to help with admin duties and montering the mod:

Command Name | Description | Required Arguements
------------ | ----------- |--------------------
count | Shows current number of items waiting to be dropped | n/a
itemlist | Shows a list of items waiting to be dropped and their place in the list (Used for the drop command) | n/a
drop | forces the drop of the item on at the list place given | list id (i.e 0 for the first item in the list and so on)
pinata | toggles the mode for if larry is killed in the femer breaker | n/a
