#!/bin/sh
set -e

ls
cd app/browser-interface
npm install
make watch
