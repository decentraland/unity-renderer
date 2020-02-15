content="["
for dir in `find ../static/loader/tutorial-scene -type f`; do
    if [[ $dir != *".DS_Store"*  && $dir != *"generate_contents"* && $dir != *"tutorialSceneContents.json"* ]]; then
        path=${dir:32}
        content+="{\"file\": \"$path\", \"hash\": \"$path\"},"
    fi
done
content=${content%?}"]"
echo $content > ../packages/decentraland-loader/lifecycle/tutorial/tutorialSceneContents.json