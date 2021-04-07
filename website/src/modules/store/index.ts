import { Kernel } from "../../components/types";

const kernel = (window as Kernel).webApp;
export const getKernelStore = () => {
  const start = Date.now();
  // create kernel store
  const store = kernel.createStore();
  const container = document.getElementById("gameContainer") as HTMLElement;
  // initialize Unity
  setTimeout(() => {
    kernel
      .initWeb(container)
      .then((response) => {
        console.log("website-initWeb completed at: ", Date.now() - start);
        return kernel.loadUnity(response).then(() => {
          console.log("website-loadUnity completed at: ", Date.now() - start);
        });
      })
      .then(() => console.log("website-initUnity completed"))
      .catch(error => console.error("website-initUnity", error))
  }, 3000); // We delay Unity initialization to avoid hiccups in UI for the more anxious people. This has been shown to improve UX

  return store;
};
