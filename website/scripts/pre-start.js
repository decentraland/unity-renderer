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
const kernelPath = path.resolve( __dirname + "/../../kernel");
files
  .map((p) => ({
    dest: path.resolve( __dirname + "/../" + p.dest),
    origin: path.resolve(kernelPath + "/" + p.origin),
  }))
  .map((p) => {
    if (fs.existsSync(p.dest)) {
      console.log("removing old version of ", path.basename(p.dest));
      const lstat = fs.lstatSync(p.dest)
      if (lstat.isDirectory()) {
        fs.rmdirSync(p.dest, { recursive:true } );
      } else {
        fs.unlinkSync(p.dest);
      }
    }
    return p;
  })
  .map(({ origin, dest, type }) => {
    console.log(`linking ${origin} -> ${dest}`);
    fs.symlinkSync(origin, dest, 'junction');
  });

console.log("\nREPLACE KERNEL VERSION\n");
require("./hash_generator");
