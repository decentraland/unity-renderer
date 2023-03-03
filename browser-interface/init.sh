#!/bin/sh
set -e

cd app/browser-interface
npm install
make watch
