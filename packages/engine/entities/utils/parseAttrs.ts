import * as BABYLON from 'babylonjs'
import * as GUI from 'babylonjs-gui'

export function parseHorizontalAlignment(position: string): number {
  switch (position) {
    case 'left': {
      return GUI.Control.HORIZONTAL_ALIGNMENT_LEFT
    }
    case 'right': {
      return GUI.Control.HORIZONTAL_ALIGNMENT_RIGHT
    }
    default: {
      return GUI.Control.HORIZONTAL_ALIGNMENT_CENTER
    }
  }
}

export function parseVerticalAlignment(position: string): number {
  switch (position) {
    case 'top': {
      return GUI.Control.VERTICAL_ALIGNMENT_TOP
    }
    case 'bottom': {
      return GUI.Control.VERTICAL_ALIGNMENT_BOTTOM
    }
    default: {
      return GUI.Control.VERTICAL_ALIGNMENT_CENTER
    }
  }
}

export function parseSide(side: string): number {
  switch (side) {
    case 'back': {
      return BABYLON.Mesh.BACKSIDE
    }
    case 'double': {
      return BABYLON.Mesh.DOUBLESIDE
    }
    default: {
      return BABYLON.Mesh.FRONTSIDE
    }
  }
}
