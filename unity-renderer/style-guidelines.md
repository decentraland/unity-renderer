# Coding Guidelines

### Unity Project Code Style
Maintaining a common code style will boost our productivity by reducing the amount of noise in Code Reviews and allowing new members to on-board faster.
When working with the Unity Project these guidelines (to be documented) must be followed and to ease the process we have a pre-commit hook to deal with it automatically.
The repo `https://github.com/decentraland/reshaper-pre-commit-hook` contains both the installing bash script and the pre-commit hook itself.

To install it go to the root of your explorer repository and run the following command on it:
`curl -s https://raw.githubusercontent.com/decentraland/reshaper-pre-commit-hook/master/install-git-hook.sh | bash`

### Notes 

* For all our style needs we are using an [`.editorconfig`](https://editorconfig.org/) file. 

* It's recommended to use a `Format On Save` extension on your IDE of choice so we avoid styling feedback noise on pull requests.

* We don't use the `.resharper` extensions for `.editorconfig`. So if you use `resharper` plugin or `rider` you must set the `resharper` specific style settings to match those of `VS Code` and `VS Community`.


### Rider
You can find a settings export file in the root of the project called "rider_codeStyleDCLSetting". Bear in mind that the conversion between VS and Rider is not 1 on 1 but it's good enough.