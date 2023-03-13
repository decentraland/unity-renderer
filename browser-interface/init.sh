#!/bin/sh
set -e

cd app/browser-interface
npm ci
make watch
