import { Store } from "redux";

export type InitializeUnityResult = {
  container: HTMLElement;
};

type KernelWebApp = {
  createStore: () => Store<any>;
  initWeb: (container: HTMLElement) => Promise<InitializeUnityResult>;
  loadUnity: (r: InitializeUnityResult) => Promise<boolean>;
  utils: {
    isBadWord: (word: string) => boolean;
    filterInvalidNameCharacters: (name: string) => string;
  };
};

export type Kernel = typeof window & {
  webApp: KernelWebApp;
};
