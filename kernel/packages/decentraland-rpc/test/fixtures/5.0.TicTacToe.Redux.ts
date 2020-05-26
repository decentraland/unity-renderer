import { test, future } from './support/ClientHelpers'
import { MessageBusClient } from './support/MessageBusClient'
import { Test } from './support/ClientCommons'

type GameSymbol = 'x' | 'o' | null

enum TicTacToeAction {
  PLACE = 'placeSymbol',
  RESTART = 'restart',
  SYNC = 'sync',
  SET_SYMBOL = 'setSymbol'
}

interface ITicTacToeState {
  board: GameSymbol[]
  mySymbol: GameSymbol
}

interface IGenericAction {
  type: TicTacToeAction
  payload?: any
}

const initialState: ITicTacToeState = {
  board: [null, null, null, null, null, null, null, null, null],
  mySymbol: null
}

let state = initialState

function reducer(state: ITicTacToeState = initialState, action: IGenericAction): ITicTacToeState {
  const { type, payload } = action

  switch (type) {
    case TicTacToeAction.SYNC:
      return {
        ...state,
        board: payload.board
      }

    case TicTacToeAction.RESTART:
      return {
        ...initialState
      }

    case TicTacToeAction.PLACE:
      return {
        ...state,
        board: Object.assign([], state.board, {
          [payload.index]: payload.symbol
        })
      }

    case TicTacToeAction.SET_SYMBOL:
      return {
        ...state,
        mySymbol: payload.symbol
      }
  }

  return state
}

function handleAction(action: IGenericAction) {
  state = reducer(state, action)
}

const winingCombinations = [
  [0, 1, 2], // 1 row
  [3, 4, 5], // 2 row
  [6, 7, 8], // 3 row

  [0, 3, 6], // 1 col
  [1, 4, 7], // 2 col
  [2, 5, 8], // 3 col
  // tslint:disable-next-line
  [0, 4, 8], // nw - se
  // tslint:disable-next-line
  [6, 4, 2] // sw - ne
]

const getWinner = () =>
  ['x', 'o'].find($ =>
    winingCombinations.some(combination => combination.every(position => state.board[position] === $))
  )

test(async ScriptingClient => {
  const { Test, TicTacToeBoard } = (await ScriptingClient.loadAPIs(['Test', 'TicTacToeBoard'])) as {
    Test: Test
    TicTacToeBoard: any
  }

  const futureWinner = future()

  const messageBus = await MessageBusClient.acquireChannel(ScriptingClient, 'rtc://tictactoe.signaling.com')

  TicTacToeBoard.onChooseSymbol(({ symbol }: { symbol: GameSymbol }) => {
    handleAction({
      type: TicTacToeAction.SET_SYMBOL,
      payload: {
        symbol
      }
    })
  })

  TicTacToeBoard.onClickPosition(({ position }: { position: number }) => {
    messageBus.emit('set_at', position, state.mySymbol)
  })

  messageBus.on('set_at', (index: number, symbol: GameSymbol) => {
    handleAction({
      type: TicTacToeAction.PLACE,
      payload: {
        index,
        symbol
      }
    })

    const winner = getWinner()

    if (winner !== undefined) {
      // tslint:disable-next-line
      Test.pass(winner)
      futureWinner.resolve(winner)
    }
  })

  await TicTacToeBoard.iAmConnected()

  // wait every command to execute
  console.log('the winner is', await futureWinner)
})
