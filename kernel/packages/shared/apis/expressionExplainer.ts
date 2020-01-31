export const expressionExplainer = { robot: 'the robot dance!', wave: 'waving', fistpump: 'fist-pumping' }
export const validExpressions = Object.keys(expressionExplainer)

export type validExpression = keyof typeof validExpressions

export function isValidExpression(expression: any): expression is keyof typeof expressionExplainer {
  return validExpressions.includes(expression)
}
