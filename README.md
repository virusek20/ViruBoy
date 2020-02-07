# ViruBoy
ViruBoy is a GameBoy emulator written entirely in C# as an exercise in low level programming and code optimalization. It can currently run several simple games like Tetris or Dr. Mario, homebrew games and passes common GameBoy emulator tests.
## ViruBoyUI
This part of the application implements basic rendering of sprites and tiles similar to a real GameBoy using OpenTK. Unlike the original hardware this emulator renders the whole screen at once, breaking advanced games that modify the screen contents during drawing.
![tetris](https://virusek20.kuubstudios.com/files/2020-02/ViruBoyUI_2020-02-07_17-09-12.png)
## ViruBoyTest
Alongside the emulator itself is also provided a set of additional tests for basic instructions I have made while debugging this emulator, althought the test suite is far from complete.
## TestApp
TestApp is a simple console based debugger that allows settings breakpoints, simple benchmarking and dumping memory during emulation.
## Sources
During the development of this project I have made a collection of various websites and datasheets. If you're trying to write an emulator yourself, make sure to check multiple sources as some of the documents aren't 100% accurate, like `JP (HL)` actually being `JP HL`.
* http://gameboy.mongenel.com/dmg/asmmemmap.html
* http://marc.rawer.de/Gameboy/Docs/GBCPUman.pdf
* https://www.pastraiser.com/cpu/gameboy/gameboy_opcodes.html
* http://www.z80.info/decoding.htm
* http://bgb.bircd.org/pandocs.htm#vramtiledata
* https://github.com/retrio/gb-test-roms