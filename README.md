GreenBean Script Prototype
===

GreenBean script is an old (circa 2010) and flawed prototype implementation of the [GameMonkey Script] Virtual Machine in C# / .NET 2.0.

It was designed to be *bytecode compatible* with the original [GameMonkey Script] VM, and as such includes the same instruction set and types as the VM at the time.

There is no compiler implementation, so as a result you have to hand craft the machine bytecode into a "Library" and feed it to the machine. You can load a library from a file if you are able to export one from the C++ version of GM.

As this is very old prototype code, there are many missing features, no tests and no guarantees it'll even work. There are certainly no guarantees of further development on it :)

I'm putting this on Github to give it to the GameMonkey community to learn from, enhance or abuse in whatever way they desire.

## Missing Features ##
At this point it's easier to say what's missing than what's there...

- You cannot write GameMonkey Script code and compile it as there is no compiler
- Most of the standard machine library is missing
- Missing operators and extensions (fork, inc/dec ops, endon, maybe others)
- Signal/Block/Yield not implemented
- Performance is horrendously bad (matrix.gm runs ~4s in C++ and ~13s in .NET on my machine)
- Huge inconsistencies all over
- Barely any test in place

## Refactor & Revival ##


After posting this old code on Github, I decided to start refactoring the code into something useful. I've added some simple regression tests that forced the implementation of a couple of operators and the fixing of some fairly horrible bugs that existed in the original commit.



## MIT License ##

Like the original [GameMonkey Script], GreenBean Script is released under the MIT license.


## Acknowledgements ##

GreenBean Script is based upon "GameMonkey Script", copyright (c) 2003 Auran Development Ltd.
The website for GameMonkey Script can be found at http://gmscript.com
Many thanks go to Greg Douglas and Matthew Riek for their work on GameMonkey Script now and in the future.



[GameMonkey Script]:http://gmscript.com
