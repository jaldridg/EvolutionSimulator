# Evolution Simulator
I've always wanted to make an ecosystem and watch creatures adapt and evolve. Here's what I made:

## Version history
0.0.1: Initial commit

0.0.2: Nav mesh implemented

0.0.3: Creatures wander around

0.0.4: Food Introduced
- Creatures now have the need for food and will die without it
- Creatures will wander when full and search for food when hungry

0.1.0: Base world
- More adaptive world
    - Starting creatures responds to world size
    - Added setting for food spawn rate and creature spawn density
- Smarter creatures
    - Creatures will wander shorter distances. This means they can respond to their needs faster
    - Creatures now move towards closest food when hungry