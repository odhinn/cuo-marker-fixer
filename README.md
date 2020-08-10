# UOAM, UOMapper map marker fixer for ClassicUO

Default UOAM and UOMapper map markers aint accurate for standard servers when loaded by ClassicUO World Map.
This little program will convert facet values to correct one.

## Guide

- Download compiled one or just get source code and compile it.
- You can either choose put all *.xml and *.map files into program directory and run MarkerFixer.exe or drag/drop onto MarkerFixer.exe
- It will fix markers.

## Facet Values for Standard Servers

- 0: Felucca
- 1: Trammel
- 2: Ilshenar
- 3: Malas
- 4: Tokuno
- 5: Ter Mur

## Custom Markers

Info provided by [Quick](https://github.com/markdwags) 

Creating your own marker file is simple. Create an empty file and enter it using this format:

`x,y,mapindex,name of marker,iconname,color,zoom level`

`x` and `y` are the coordinates for the marker.

The `mapindex` defines what map the client must be on for the marker to display. `0` for `map0.mul`, `1` for `map1.mul`, etc.

The `name of marker` is just that, the name you want to see in the map or when you hover over the marker.

The `iconname` must match the name of the image in the `Data\Client\MapIcons` folder. If no icon exists, it will display a simple dot using the color defined. Icons can be created as `.cur, .png, .jpg, .ico` files.

For `color`, the supported values are `red, green, blue, purple, black, yellow, white, none`.

`zoom level` defines when the icon will show/hide when zooming.  `0 = furthest` and `9 = closest` so if you had a marker zoom level set to 1 (for example dungeon entrances), it will show at any zoom level except if you're all the way out (at 0). Default level is 3.


## Associated Links

- [ClassicUO](https://www.classicuo.eu)  
- [ServUO](https://www.servuo.com)  
- [Quingis UO](https://www.quingis.com)  
