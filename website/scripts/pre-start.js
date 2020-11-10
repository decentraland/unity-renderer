"use strict";

const fs = require("fs");
const path = require("path");

let files = [
  { origin: "/static/default-profile", dest: "./public/default-profile" },
  { origin: "/static/dist/website.js", dest: "./public/website.js" },
  { origin: "/static/loader", dest: "./public/loader" },
  { origin: "/static/systems", dest: "./public/systems" },
  { origin: "/static/unity", dest: "./public/unity" },
  { origin: "/static/voice-chat-codec", dest: "./public/voice-chat-codec" },
];

console.log("\nREFRESH KERNEL FILES\n");
const kernelPath = path.resolve("../kernel");
files
  .map((p) => ({
    dest: path.resolve(p.dest),
    origin: path.resolve(kernelPath + "/" + p.origin),
  }))
  .map((p) => {
    if (fs.existsSync(p.dest)) {
      console.log("removing old version of ", path.basename(p.dest));
      fs.unlinkSync(p.dest);
    }
    return p;
  })
  .map(({ origin, dest }) => {
    console.log(`linking ${origin} -> ${dest}`);
    fs.symlinkSync(origin, dest);
  });

console.log("\nREPLACE KERNEL VERSION\n");
let content = "";
if (fs.existsSync(".env")) {
  content = fs.readFileSync(".env").toString();
  content = content.replace(/REACT_APP_EXPLORER_VERSION.*/, "");
  content = content
    .split("\n")
    .filter((line) => line)
    .join("\n");
}
content += "\nREACT_APP_EXPLORER_VERSION=" + Date.now().toString();
fs.writeFileSync(".env", content);
