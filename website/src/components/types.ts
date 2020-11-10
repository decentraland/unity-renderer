import { Store } from "redux";

export type InitializeUnityResult = {
  container: HTMLElement;
  instancedJS: Promise<any>;
};

type KernelWebApp = {
  createStore: () => Store<any>;
  initWeb: (container: HTMLElement) => Promise<InitializeUnityResult>;
  loadUnity: (r: InitializeUnityResult) => Promise<boolean>;
};

export type Kernel = typeof window & {
  webApp: KernelWebApp;
};
