#! /usr/bin/env bash

export TARGET=$1

mv $TARGET $TARGET.gz
gunzip $TARGET.gz

export LOCATION_START_CUT=282
export LOCATION_END_CUT=6975
export FILE_LENGTH=`wc -c $TARGET | grep -oE '[0-9]+'`
export REST_FROM_BOTTOM=`echo "$FILE_LENGTH-$LOCATION_END_CUT" | bc`
export CUTTING=`echo "$LOCATION_END_CUT-$LOCATION_START_CUT" | bc`

echo "Cutting:"
cat $TARGET | head -c $LOCATION_END_CUT | tail -c $CUTTING

head -c $LOCATION_START_CUT $TARGET > $TARGET.new
tail -c $REST_FROM_BOTTOM $TARGET >> $TARGET.new

mv $TARGET.new $TARGET
