# Bots Tool

![Bots%20Tool%2037951f1030da4cc09e6480c1b554bdd1/Screen_Shot_2021-07-16_at_00.17.20.png](bots-tool/Screen_Shot_2021-07-16_at_00.17.20.png)

**How to use:**

1. Enter the explorer at any position
2. Open browser console
3. Call a Bots Tool method:
- **A.** `clientDebug.InstantiateBotsAtCoords({amount: number, xCoord: number, yCoord: number, areaWidth: number, areaDepth: number})`: Instantiate bots in an area positioned at the base position of the parcel for which coords where provided
    - Example A: `clientDebug.InstantiateBotsAtCoords({amount: 10})` will instantiate **10 bots** in the **user's current parcel** with a default **spawning area of 16x16** as default values apply when some parameters are not provided.
    - Example B: `clientDebug.InstantiateBotsAtCoords({ amount: 100, xCoord: 91.2, yCoord: 151.2, areaWidth: 36, areaDepth: 50 })` will instantiate **100 bots** in a **36x50 area** positioned at **world coords 91.2, 151.2**

- **B.** `clientDebug.RemoveBot(botId: string)`: removes an instantiated bot. Every bot has its ID as its avatar name.
- **C.** `clientDebug.ClearBots()`: removes all the instantiated bots.
- **D.** `clientDebug.StartBotsRandomizedMovement({ populationNormalizedPercentage: number, waypointsUpdateTime: number, xCoord: number, yCoord: number, areaWidth: number, areaDepth: number })` : starts randomized movement on a % of the population. For the selected population, waypoints are randomized for each bot, inside the provided area at the provided coords. Waypoints are re-randomized at the update time defined.
    - Example: `clientDebug.StartBotsRandomizedMovement({ populationNormalizedPercentage: 0.5, waypointsUpdateTime: 5, xCoord: 91.2, yCoord: 151.2, areaWidth: 36, areaDepth: 50 })` will apply randomized movement on **50% of the population**, the randomized waypoints will be in a **36x50 area** positioned at **world coords 91.2, 151.2**, and the waypoints will be re-randomized **every 5 seconds**
    - Example A: `clientDebug.StartBotsRandomizedMovement({ populationNormalizedPercentage: 1, waypointsUpdateTime: 1})` will apply randomized movement on **100% of the population**, the randomized waypoints will be in a **16x16 area** in the **user’s current parcel** and the waypoints will be re-randomized **every 1 seconds**, as default values apply when some parameters are not provided.
    - Example B: `clientDebug.StartBotsRandomizedMovement({ populationNormalizedPercentage: 0.5, waypointsUpdateTime: 5, xCoord: 91.2, yCoord: 151.2, areaWidth: 36, areaDepth: 50 })` will apply randomized movement on **50% of the population**, the randomized waypoints will be in a **36x50 area** positioned at **world coords 91.2, 151.2**, and the waypoints will be re-randomized **every 5 seconds**
- **E.** `clientDebug.StopBotsMovement()` : stops bots randomized movement
    
    

**Note**: The first batch  of bots takes a bit more because it fetches the catalogue of wearables.

**Note**: When a config parameter is missing on a method call, default values are used

*An example case would be:*

1. enter **ZONE** at **91,151** (we deployed a stadium scene to test with bots)
2. open browser console
3. run `clientDebug.InstantiateBotsAtCoords({ amount: 100, xCoord: 91.2, yCoord: 151.2, areaWidth: 36, areaDepth: 50 })` to instantiate 100 bots inside the stadium. It may take a little while to load as the bots collections and wearables are being randomized and loaded.

Interesting data on the original implementation of this tool and its original usage can be found in the following blogpost wrote at that time: https://decentraland.org/blog/project-updates/100-avatars-in-a-browser-tab/ 