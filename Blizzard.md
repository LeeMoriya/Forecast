# End-Of-Cycle Blizzard

In Downpour's snow mode, the end-of-cycle DeathRain is replaced with a new Blizzard effect that ramps up in intensity 
as the time after the end of the cycle increases. This effect exposes the player to the effects of the cold, increasing 
their 'Exposure' the longer they are caught in it.

## Exposure

As the Player's exposure increases, their movement will become more restricted. The exact exposure value is hidden
when not in Debug mode, instead the Player's coldness is indicated in a few ways:

From low exposure to high exposure, the player will be afflicted with:

- Shivering
- Slower movement speed
- Short stuns (losing grip on poles)
- Exhaustion (Passing out on the ground)

A light vignette is also present around the edges of the screen which grows more visible the higher the Player's exposure is.

## Death

When the Player's exposure is maxed out, you will begin to hear a beating drum that grows louder and faster the 
longer the Player's is maxed, eventually resulting in death.

## Interiors

While being caught out in the Blizzard is extremely dangerous, interiors are not completely safe from the lowering 
temperatures. As the cycle ticks further and further past its end, the ambient temperature will also lower.
This means that while you will be able to retreat indoors for a time, your exposure will not fully reset to zero, meaning 
you'll be able to spend less and less time in the Blizzard as time progresses.

Eventually the ambient temperature will drop so low that you can reach your maximum exposure even indoors, resulting in
the Player's death if they are unable to make it to a shelter.

## Wind

Whilst outdoors, the Blizzard is accompanied by a strong wind that will push objects and the player in one direction. 
At the time of writing the Blizzard will only blow to the left, however in the future this can be either direction.
The direction the wind will blow at the end of the cycle will be the same as the direction the snowflakes fall during
the cycle. If the snowflakes aren't falling in a particular direction then one will be chosen randomly.

This will be something to keep in mind as you play through the cycle, as the wind may make some jumps impossible if you
are travelling against the wind, and much easier if you're travelling with it.
