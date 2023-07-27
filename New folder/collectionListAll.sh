#!/bin/bash

#get folder where script is running 
BASEDIR=$(dirname "$0")

#echo ------------------------------
echo $BASEDIR/_COLLECTION
find $BASEDIR/_COLLECTION >$BASEDIR/collectionListAll.txt
#echo ------------------------------
echo $?>>$BASEDIR/collectionListAll.txt
