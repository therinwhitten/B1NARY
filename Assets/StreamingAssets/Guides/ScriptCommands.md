**This covers all the data about script commands. For more info about script
blocks, view ScriptBlocks.md**

All commands in the system uses a line-by-line command system, with blocks being 
the exception. This includes dialogue. Here are most examples covered by the system:

1. `{command: argument1, argument2 }`
2. `{command}`
3. `Speaker::`
4. `[Emotion]`
5. `// Comment`

Any brackets such as `{`, `}`, and `[` being hit while playing will prevent the 
document from further playing. For a reason why, see *ScriptBlocks.md*


# Basic Commands
Empty spaces are simply skipped.

## Speaker Re-assignment `Speaker::`
This is commonly used to specify which character will speak the next line. This
will be displayed ingame if it manages to stop and allows player input. For a
workaround to allow characters to change emotion, make sure to switch speaker, 
do what you want to do, and switch back to the next speaker.

### Example:
> Asterella::  
> I am alive!

This will change the speaker and speaks the line: *I am alive!*.

> Speaker1::  
> I like pineapple on pizza!  
> Speaker2::  
> [Angry]  
> Speaker1::  
> [Surprised]  
> What's with that face?  

Speaker1 says some war crimes, and this displeases Speaker2; Thus, they will
display an angry expression. Because the script simply changes the emotion when
Speaker2 is changed, it will not show to the user, but the command will still
be processed, as it doesn't allow it to be viewed since it is done in a single 
frame.

## Expressions : `[Expression]`
This is used for changing the expression on the current focused character that
is found in `Speaker::`. This assumes the Live2D is not a empty character (more
on that later) and has an appropriately assigned expression/emotion list.

## Comments : `// Comment`
This is a feature that is present in most programming languages! This allows you
to simply add footnotes, add explanation to what the things below what it does, 
or add some kind of TODO line.

When the system reads your script, all lines will be removed after where the `//` 
is. So the system is not reading `[Expression] // Giga chad is hot`, it insteads
reads `[Expression]`. This works for both having comments in a full line, or on
a command. As said above, this will become an empty line via the system once it
reads the line that is fully taken by a comment.

### Example:
> Speaker1::  
> I like chicken nuggies on my pizza! // No he doesn't.  
> Speaker2::  
> // Best boi  
> Why u lyin, ima tumblr shame u :grinning:

When the system reads this, it will see as this instead: 

> Speaker1::  
> I like chicken nuggies on my pizza!  
> Speaker2::  
>  
> Why u lyin, ima tumblr shame u :grinning:

## Commands : `{Command: Argument1, Argument2}`
This controls the main flow of the game, such as managing character position,
adding additive to the dialogue, among other things.

**There are many ways to make a command, here is a few examples:**

1. `{Command}`
2. `{Command: Argument1}`
3. `{Command: Argument1, Argument2}`

### Additive Text : `{additive: on/off}`
This is a `boolean`, as true or false, or active and inactive, where if it is
enabled, it will add text from the next line down instead of clearing it of text
and replacing it normally.

### Creating Characters
There are multiple ways to create a person in the world.

#### Initiating a Character : `{initiateChar: gameObjectName, positionX, characterName}`
This will try to take a character in the CharacterLayer and enable it for use.
This of course, will require you to have that character in the scene to use this
command.

This command is helpful since it does not use [extra resources creating the object](https://forum.unity.com/threads/performance-differences-of-gameobject-setactive-vs-destroy.221625/)
when spawning the object in during use. This doesn't matter too much on loading,
but having a smooth transition during play should be planned.

#### Summoning a Character : `{summonChar: gameObjectName, positionX, characterName}`
This will try to get a character prefab in `Assets/Resources/Characters/Prefabs/` 
folder, and put it into the scene to play with.

Due to engine limitations, you can only put character prefabs as a single
folder instead of having subfolders.

#### Spawning a Character : `{spawnChar: gameObjectName, positionX, characterName}`
This will try to initialize a character. If it doesn't detect the character, it
will try to summon the character. It will only break if both doesn't work.

#### Spawning an Empty Character : `{spawnEmpty: characterName}`
This spawns an empty character; What this means is that this character has no
models or animations, simply just an invisible voice box in the character layers.

This is mainly useful for memory/CPU management when you want to use something
like a narrator.