// tslint:disable-next-line:whitespace
{
  const clip = new AudioClip('carnivalrides.ogg')
  const cube = new Entity()
  const source = new AudioSource(clip)

  cube.addComponentOrReplace(
    new Transform({
      position: new Vector3(5, 1, 5)
    })
  )
  cube.getComponentOrCreate(BoxShape)
  cube.addComponentOrReplace(
    new OnPointerDown(() => {
      cube
        .getComponent(Transform)
        .position.addInPlaceFromFloats(Math.random() * 8 + 1, Math.random() * 8 + 1, Math.random() * 8 + 1)
    })
  )
  source.playing = true
  source.loop = true
  cube.addComponentOrReplace(source)

  engine.addEntity(cube)
}

{
  const clip = new AudioClip('button.ogg')

  const cube = new Entity()
  const source = new AudioSource(clip)
  source.loop = false

  cube.addComponentOrReplace(
    new Transform({
      position: new Vector3(5, 1, 2),
      scale: new Vector3(0.2, 0.2, 0.2)
    })
  )

  cube.getComponentOrCreate(BoxShape)

  cube.addComponentOrReplace(
    new OnPointerDown(() => {
      source.playOnce()
    })
  )

  cube.addComponentOrReplace(source)

  engine.addEntity(cube)
}
