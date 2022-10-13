# corelib

This is the Corelib for [TinyDotNet](git@github.com:TomatOrg/tinydotnet.git).

## Goals

The main goal of this is to provide a mostly complete standard library for TinyDotNet and TomatOS, we do so trying to use as little native methods as possible, and by re-using as much code from CoreCLR as possible.

The corelib is planned to only include:
- Collections (generic and concurrent)
- Basic IO abstractions and utilities
    - Stream
    - Path
    - etc.
- Span/Memory + extensions
- Only the bare minimum for globalization
    * No culture info or anything alike
- Threading
- Task subsystem

We don't plan to include:
- File IO
- Networking
- Full globalization

Note that while we don't plan to include support does not mean we won't make it as a separate module in the future, and while TomatOS would not use it, maybe a hosted TinyDotNet will.

## Compatability

Right now it has no compatibility with CoreCLR, some of the objects are inherently different, and we don't plan on adding compatibility for now because we are focused on other stuff.


