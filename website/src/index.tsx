import * as React from "react";
import * as ReactDOM from "react-dom";
import { Provider } from "react-redux";
import { getKernelStore } from "./store";
import App from "./components/App";

let INITIAL_RENDER = true

ReactDOM.render(
  <React.StrictMode>
    <Provider store={getKernelStore()}>
      <App />
    </Provider>
  </React.StrictMode>,
  document.getElementById("root"),
  () => {
    if (INITIAL_RENDER) {
      INITIAL_RENDER = false
      const initial = document.getElementById("root-loading")
      if (initial) {
        initial.style.opacity = '0'
        setTimeout(() => initial.style.display = 'none', 300)
      }
    }
  }
);
