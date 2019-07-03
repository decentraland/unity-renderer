import { Component } from 'decentraland-ecs/src'
import { MutationProb, MutationMaxSpreads } from './Params';

export enum GeneType {
  speed,
  temperature, // -60 to 100
  ears,
  eyes,
  feet,
  mouth,
  nose,
  tail,
  wings,
  COUNT
}


@Component("genome")
export class Genome {
  genes: number[]

  constructor(genes: number[]) {
    this.genes = genes
  }

  CopyFrom(otherGenome: Genome) {
    for (var i = 0; i < this.genes.length; i++) {
      if (i == otherGenome.genes.length) return

      this.genes[i] = otherGenome.genes[i]
    }
  }

  // Based on a mutation probability param
  Mutate() {
    for (var i = 0; i < this.genes.length; i++) {
      if (Math.random() < MutationProb) {
        this.genes[i] += (Math.random() - 0.5) * MutationMaxSpreads[i]
        this.genes[i] = +this.genes[i].toFixed(2)
      }
    }
  }
}
