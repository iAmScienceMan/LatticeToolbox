# LatticeToolbox

The little engine I wrote for Project Bogey. It used to live inside the game repo, but it kept growing and getting in the way, so I pulled it out into its own thing.

## The main idea

I write components and systems, and the engine deals with the boring low level stuff for me (entities, prototypes, rendering, text, input). Same idea as RobustToolbox for Space Station 14, just a lot smaller, and mine.

## Build

You need the .NET 10 SDK.

```
dotnet build LatticeToolbox.slnx
```

It usually sits as a submodule inside Project Bogey, but it builds on its own too.
