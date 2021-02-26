## How to create typescript workers

You can check the files used for our simple gif-processor worker (`packages/gif-processor`):
1. Create relevant folder inside `packages/` and put a .ts file for the main thread and another .ts for the worker (gif-processor example: `processor.ts` and `worker.ts`)

2. Copy the tsconfig.json file from `packages/gif-processor` into the new folder, rename and edit its "OutDir" value to point to a new folder inside `/static/`

3. Duplicate the `targets/engine/gif-processor.json` file, rename it and change its "file" value to point to your worker .ts file

4. Inside the `Makefile` file create a new path constant (like the one for `GIF_PROCESSOR`) and set the path to your compiled worker file (the folder you created in step 2 plus 'worker.ts'). Then add that new constant inside `build-essentials` like `GIF_PROCESSOR`

5. Inside the `Makefile` add the following 2 lines with your own paths:
```
static/gif-processor/worker.js: packages/gif-processor/*.ts
	@$(COMPILER) targets/engine/gif-processor.json
```
6. You should be able to import your package that uses the Worker anywhere. Beware of the [limitations when passing data to/from workers](https://developer.mozilla.org/en-US/docs/Web/API/Web_Workers_API/Structured_clone_algorithm) and also consider [passing the data as Transferable objects](https://developer.mozilla.org/en-US/docs/Web/API/Transferable) to improve performance.