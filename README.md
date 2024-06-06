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
    - If a creature's approaching food and gets eaten, they'll find the next closest food

0.1.1: Expanded creature biology
- Health
    - Creature now dies when health is 0
    - Decreases when creature is starving
    - Increases when creature is well fed
- Energy
    - Affects speed
    - High levels when well fed
    - Low levels when hungry
- Age
    - Negatively affects max speed and turn speed when high enough
    - Steadily decreases health when high enough, ramping up slowly
- Speed
    - Move speed and turn speed decrease when hungry or old
- Reproduction
    - Maturity point created when creatures may reproduce

0.1.2: Basic interface
- Camera
    - Can be panned and zoomed in
    - Follows selected creatures
- Creature selection
    - Shows health and food bars (for now)

0.2.0: Interface + complex creature biologies
- Biology overhaul
    - Energy changes
        - Energy is budgeted judiciously between base living cost, movement, and growth 
    - Movement
        - Max speed is calculated given the creatures energy
        - Base energy cost for moving a certain distance balances high speed/low mass creatures
    - Maturity
        - Creatures now must spend energy maturing before spending the energy to reproduce
        - Creature size changes based on how much energy has been spent maturing
    - Size
        - Creatures stats such as maximum health, stomach capacity, and more are affected by their size
- UI
    - Creature color darkens as they age
    - States added to health (Healthy, injured, dying) and food (Well fed, nourished, hungry, starving)
    - Percent changes displayed for health and food
    
