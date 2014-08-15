GreenBean Script
================


GreenBean script is an old (circa 2010), somewhat flawed prototype implementation of the [GameMonkey Script] Virtual Machine in C# / .NET 2.0.

It was designed to be bytecode compatible with the original [GameMonkey Script] VM, and as such includes the same instruction set and types.

There is no compiler implementation, so as a result you have to hand craft the machine bytecode into a "Library" and feed it to the machine. I believe you can load a library from a file if you are able to export one from the C++ version of GM.

As this is very old prototype code, there are many missing features, no tests and no guarantees it'll even work. There are certainly no guarantees of further development on it :)

I'm putting this on Github to give it back to the GameMonkey community to learn from, enhance or abuse in whatever way they desire.



License
----

The MIT License (MIT)

Copyright (c) 2010 Oli Wilkinson

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.


Acknowledgements
---
GreenBean Script is based upon "GameMonkey Script", copyright (c) 2003 Auran Development Ltd.
The website for GameMonkey Script can be found at http://gmscript.com
Many thanks go to Greg Douglas and Matthew Riek for their work on GameMonkey Script now and in the future.



[GameMonkey Script]:http://gmscript.com
