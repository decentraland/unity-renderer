#!/bin/bash

for dir in `ls`; do
    if [ -d "$dir" ]; then
        cp scene.json $dir/
        cd $dir; cat builder.json| jq '.scene |[.components |.[] |.data | .mappings | select(. != null) |to_entries]|flatten| map({"file": .key|sub("[^/]+";"models"), "hash": .key|sub("[^/]+";"models")})' > contents.json
        cd ..
    fi
done

