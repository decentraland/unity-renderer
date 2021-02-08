#### Please read the guidelines in https://github.com/decentraland/standards

# Security

Sign all your commits with GPG

- https://github.com/blog/2144-gpg-signature-verification
- https://help.github.com/articles/generating-a-new-gpg-key/
- https://help.github.com/articles/signing-commits-using-gpg/

# Commit messages

We do [semantic commits](https://seesparkbox.com/foundry/semantic_commit_messages)

```
feat: add hat wobble
^--^  ^------------^
|     |
|     +-> Summary in present tense.
|
+-------> Type: chore, docs, feat, fix, refactor, style, or test.
```


Examples:

```
chore: add Oyster build script
```
```
docs: explain hat wobble
```
```
feat: add beta sequence, implements #332
```
```
fix: remove broken confirmation message, closes #123
```
```
refactor: share logic between 4d3d3d3 and flarhgunnstow
```
```
style: convert tabs to spaces
```
```
test: ensure Tayne retains clothing
```

## Allowed `<type>` values:
   * `feat` new feature
   * `fix` bug fix
   * `docs` changes to the documentation
   * `style` formatting, linting, etc; no production code change
   * `refactor` refactoring production code, eg. renaming a variable
   * `test` adding missing tests, refactoring tests; no production code change
   * `chore` updating build tasks etc; no production code change
   
# Merge commits

Avoid `Merge branch 'a' into 'master'` commit messages. Rebase when possible

# Code style

Every commit should be linted using the command `npm run lint:fix`

For TSLint refer to: https://github.com/decentraland/standards/blob/master/standards/tslint-configuration.md

For TypeScript (and react specific) guidelines refer to https://github.com/decentraland/standards/blob/master/standards/react-redux.md
