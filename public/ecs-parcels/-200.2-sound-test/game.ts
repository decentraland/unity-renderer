{
  const clip = new AudioClip('carnivalrides.ogg')
  const cube = new Entity()
  const source = new AudioSource(clip)

  cube.set(
    new Transform({
      position: new Vector3(5, 1, 5)
    })
  )
  cube.getOrCreate(BoxShape)
  cube.set(
    new OnClick(() => {
      cube.get(Transform).position.set(Math.random() * 8 + 1, Math.random() * 8 + 1, Math.random() * 8 + 1)
    })
  )
  source.playing = true
  source.loop = true
  cube.set(source)

  engine.addEntity(cube)
}

{
  const clip = new AudioClip('button.ogg')

  const cube = new Entity()
  const source = new AudioSource(clip)
  source.loop = false

  cube.set(
    new Transform({
      position: new Vector3(5, 1, 2),
      scale: new Vector3(0.2, 0.2, 0.2)
    })
  )

  cube.getOrCreate(BoxShape)

  cube.set(
    new OnClick(() => {
      source.playOnce()
    })
  )

  cube.set(source)

  engine.addEntity(cube)
}
