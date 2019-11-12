## Passports

User intentions:
- See my avatar
- See a picture of my avatar on the top-right corner
- See a catalog of normal items I can wear
- See a catalog of the exclusive items I can wear
- Wear new items and have others see that change reflected
- See other people's avatars

System intentions:
- Get the whole catalog of normal items anyone can wear
    * Selector: `getPlatformCatalog` in file `selectors.ts`
- Retrieve the exclusive items the current user can wear
    * Selector: `getInventory` in file `selectors.ts`
- Retrieve the current user's profile and equiped wearables
    * Selector: `getProfile` with the `auth`'s current ID
- Validate the schema of the catalog and the retrieved profile
    * `validation.ts`
- Retrieve the profile of another user
    * Selector: `getProfile` with the other user's ID

Implementation effects:
- Needs to set the current profile download server
    * Dispatch: `setProfileServer(url: string)` from `actions.ts`
- Validate that the catalog has been sent to the renderer before sending a user's profile
    * See: `sendLoadCatalog` in `sagas.ts` (if not present, check after every `CatalogAction` if it can now send the profile)
- Global lock: Before loading the whole catalog, no user profile requests are allowed
    * See: `fetchUserProfile`
- Needs to query the current user's inventory
    * Dispatch: `fetchUserInventory(userId: string)`
- Needs to retrieve the full user's profile to send to the renderer
    * Selector: `getStoredProfileWithInventory(userId: string)`
