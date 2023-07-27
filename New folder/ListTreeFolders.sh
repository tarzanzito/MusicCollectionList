#!/bin/bash

BASEDIR=$(dirname "$0")
OUTPUTFILE=$1

echo $BASEDIR
echo $OUTPUTFILE

if [ -f $OUTPUTFILE ]; then
	rm $OUTPUTFILE
fi

find $BASEDIR >$OUTPUTFILE

#chmod +x ListTreelFolders.sh