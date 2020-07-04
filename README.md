# Unity Behavior Tree Implementation

An example of how to implement behavior trees within Unity.

![Graph example](https://i.imgur.com/s2gPL3m.png)
![Animation example](https://i.imgur.com/Bwpo5cl.gif)

## Features

Context switching to allow many AI to use the same graph simultaneously.
Uses attributes to mark variables as being needed to be saved in each context.
Handles interrupts, using parallel repeaters you can continually check for interrupts and break execution of nodes properly.

## Implementation Details

I'm not super familar with how behavior trees should work, they ultimately seem really annoying to work with.
Though I did want some way of visualizing and editing AI, without re-writing lots of common tasks. Which this system handles just fine.

Each individual AI agent communicates with its internal logic using its Context. The agent informs the context of what rigidbody, controller, animator, etc that it controls within the scene.
The context informs the AI agent of how often it should update. It can be totally improved by adding the capacity for switching from coroutines to Tick()ing every Update or FixedUpdate.


## Dependencies

* SerializableCallback for doing arbitrary callbacks within the graph.
* XNode for editing the graph.

## Thanks to

* https://www.gamasutra.com/blogs/ChrisSimpson/20140717/221339/Behavior_trees_for_AI_How_they_work.php
* https://github.com/jlreymendez/planilo
* https://github.com/azixMcAze/Unity-SerializableDictionary
