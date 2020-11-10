import { Kernel } from "../components/types";

const kernel = (window as Kernel).webApp;
export const getKernelStore = () => {
  const start = Date.now();
  // create kernel store
  const store = kernel.createStore();
  const container = document.getElementById("gameContainer") as HTMLElement;
  // initialize Unity
  kernel
    .initWeb(container)
    .then((response) => {
      console.log("website-initWeb completed at: ", Date.now() - start);
      return kernel.loadUnity(response).then(() => {
        console.log("website-loadUnity completed at: ", Date.now() - start);
      });
    })
    .then(() => console.log("website-initUnity completed"));

  return store;
};
