export const expressionExplainer = {
  wave: 'You start waving',
  fistpump: 'You start fist-pumping',
  robot: 'You start the robot dance!',
  raiseHand: '',
  clap: '',
  money: '',
  kiss: '',
  tik: 'Those movements are sexy!',
  hammer: 'You know how to show off at the dance floor!',
  tektonik: 'What a great dancer!',
  dontsee: '',
  handsair: '',
  shrug: '',
  disco: '',
  dab: '',
  headexplode: '',
  dance: '',
  snowfall: '',
  hohoho: '',
  crafting: '',
  snowballhit: '',
  snowballthrow: '',
  cry: '',
  hands_in_the_air: ''
}
export const validExpressions = Object.keys(expressionExplainer)

export type validExpression = keyof typeof validExpressions

export function isValidExpression(expression: any): expression is keyof typeof expressionExplainer {
  return validExpressions.includes(expression)
}
